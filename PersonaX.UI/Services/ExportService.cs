using System.IO.Compression;
using System.Text.Json;

namespace PersonaX.UI.Services
{
    public class ExportService : IExportService
    {
        private readonly IEncryptionService _encryptionService;
        private readonly AuditLogRepository _auditLogRepository;

        public ExportService(IEncryptionService encryptionService, AuditLogRepository auditLogRepository)
        {
            _encryptionService = encryptionService;
            _auditLogRepository = auditLogRepository;
        }

        public async Task<string> ExportAsync(string passphrase, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(passphrase))
            {
                throw new ArgumentException("Passphrase darf nicht leer sein.", nameof(passphrase));
            }

            if (!File.Exists(Constants.PiiDatabasePath))
            {
                throw new FileNotFoundException("Die PII-Datenbank wurde nicht gefunden.", Constants.PiiDatabasePath);
            }

            Directory.CreateDirectory(Constants.ExportPath);

            var tempRoot = Path.Combine(FileSystem.CacheDirectory, $"export-{Guid.NewGuid():N}");
            var payloadDirectory = Path.Combine(tempRoot, "payload");
            Directory.CreateDirectory(payloadDirectory);

            try
            {
                File.Copy(Constants.PiiDatabasePath, Path.Combine(payloadDirectory, Constants.PiiDatabaseFilename), true);

                if (Directory.Exists(Constants.MediaRootPath))
                {
                    CopyDirectory(Constants.MediaRootPath, Path.Combine(payloadDirectory, "Media"));
                }

                var manifest = new ExportManifest
                {
                    CreatedAtUtc = DateTime.UtcNow,
                    DatabaseFile = Constants.PiiDatabaseFilename,
                    MediaFolder = Directory.Exists(Constants.MediaRootPath) ? "Media" : null
                };

                await File.WriteAllTextAsync(
                    Path.Combine(payloadDirectory, "manifest.json"),
                    JsonSerializer.Serialize(manifest),
                    cancellationToken);

                var zipPath = Path.Combine(tempRoot, "payload.zip");
                ZipFile.CreateFromDirectory(payloadDirectory, zipPath, CompressionLevel.Optimal, false);

                var salt = _encryptionService.GenerateSalt();
                var key = _encryptionService.DeriveMasterKeyFromPin(passphrase, salt);
                var zipBytes = await File.ReadAllBytesAsync(zipPath, cancellationToken);
                var encrypted = _encryptionService.EncryptAesGcm(zipBytes, key);

                var outputPath = Path.Combine(Constants.ExportPath, $"personax-backup-{DateTime.UtcNow:yyyyMMddHHmmss}.pxbackup");
                await using var output = File.Create(outputPath);
                await output.WriteAsync(salt, cancellationToken);
                await output.WriteAsync(encrypted.iv, cancellationToken);
                await output.WriteAsync(encrypted.tag, cancellationToken);
                await output.WriteAsync(encrypted.ciphertext, cancellationToken);
                await output.FlushAsync(cancellationToken);

                await _auditLogRepository.LogAsync(Models.AuditAction.Exported, "Backup", 0, Path.GetFileName(outputPath));
                return outputPath;
            }
            finally
            {
                if (Directory.Exists(tempRoot))
                {
                    Directory.Delete(tempRoot, true);
                }
            }
        }

        public async Task ImportAsync(string encryptedBackupPath, string passphrase, CancellationToken cancellationToken = default)
        {
            if (!File.Exists(encryptedBackupPath))
            {
                throw new FileNotFoundException("Backup-Datei wurde nicht gefunden.", encryptedBackupPath);
            }

            if (string.IsNullOrWhiteSpace(passphrase))
            {
                throw new ArgumentException("Passphrase darf nicht leer sein.", nameof(passphrase));
            }

            var envelope = await File.ReadAllBytesAsync(encryptedBackupPath, cancellationToken);
            if (envelope.Length <= 44)
            {
                throw new InvalidOperationException("Backup-Datei ist ungültig.");
            }

            var salt = envelope[..16];
            var iv = envelope[16..28];
            var tag = envelope[28..44];
            var ciphertext = envelope[44..];
            var key = _encryptionService.DeriveMasterKeyFromPin(passphrase, salt);
            var zipBytes = _encryptionService.DecryptAesGcm(ciphertext, key, iv, tag);

            var tempRoot = Path.Combine(FileSystem.CacheDirectory, $"import-{Guid.NewGuid():N}");
            Directory.CreateDirectory(tempRoot);

            try
            {
                var zipPath = Path.Combine(tempRoot, "payload.zip");
                await File.WriteAllBytesAsync(zipPath, zipBytes, cancellationToken);

                var extractDirectory = Path.Combine(tempRoot, "payload");
                ZipFile.ExtractToDirectory(zipPath, extractDirectory, true);

                var dbSource = Path.Combine(extractDirectory, Constants.PiiDatabaseFilename);
                if (!File.Exists(dbSource))
                {
                    throw new InvalidOperationException("Backup enthält keine PII-Datenbank.");
                }

                File.Copy(dbSource, Constants.PiiDatabasePath, true);

                var mediaSource = Path.Combine(extractDirectory, "Media");
                if (Directory.Exists(mediaSource))
                {
                    if (Directory.Exists(Constants.MediaRootPath))
                    {
                        Directory.Delete(Constants.MediaRootPath, true);
                    }

                    CopyDirectory(mediaSource, Constants.MediaRootPath);
                }

                await _auditLogRepository.LogAsync(Models.AuditAction.Imported, "Backup", 0, Path.GetFileName(encryptedBackupPath));
            }
            finally
            {
                if (Directory.Exists(tempRoot))
                {
                    Directory.Delete(tempRoot, true);
                }
            }
        }

        private static void CopyDirectory(string sourceDirectory, string destinationDirectory)
        {
            Directory.CreateDirectory(destinationDirectory);

            foreach (var file in Directory.GetFiles(sourceDirectory))
            {
                File.Copy(file, Path.Combine(destinationDirectory, Path.GetFileName(file)), true);
            }

            foreach (var directory in Directory.GetDirectories(sourceDirectory))
            {
                CopyDirectory(directory, Path.Combine(destinationDirectory, Path.GetFileName(directory)));
            }
        }

        private sealed class ExportManifest
        {
            public DateTime CreatedAtUtc { get; set; }
            public string DatabaseFile { get; set; } = string.Empty;
            public string? MediaFolder { get; set; }
        }
    }
}
