using PersonaX.UI.Models;

namespace PersonaX.UI.Services
{
    public class MediaService : IMediaService
    {
        private readonly IEncryptionService _encryptionService;
        private readonly IKeyStoreService _keyStoreService;
        private readonly MediaRepository _mediaRepository;

        public MediaService(IEncryptionService encryptionService, IKeyStoreService keyStoreService, MediaRepository mediaRepository)
        {
            _encryptionService = encryptionService;
            _keyStoreService = keyStoreService;
            _mediaRepository = mediaRepository;
        }

        public async Task<MediaItem> CapturePhotoAsync(int personId, CancellationToken cancellationToken = default)
        {
            var photo = await MediaPicker.Default.CapturePhotoAsync();
            if (photo is null)
            {
                throw new InvalidOperationException("Fotoaufnahme wurde abgebrochen.");
            }

            return await ImportMediaAsync(personId, photo, MediaType.Photo, cancellationToken);
        }

        public async Task<MediaItem> CaptureVideoAsync(int personId, CancellationToken cancellationToken = default)
        {
            var video = await MediaPicker.Default.CaptureVideoAsync();
            if (video is null)
            {
                throw new InvalidOperationException("Videoaufnahme wurde abgebrochen.");
            }

            return await ImportMediaAsync(personId, video, MediaType.Video, cancellationToken);
        }

        public Task<MediaItem> CaptureAudioAsync(int personId, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException("Audioaufnahme benötigt eine plattformspezifische Implementierung und ist noch nicht aktiviert.");
        }

        public async Task<MediaItem> ImportMediaAsync(int personId, FileResult fileResult, MediaType mediaType, CancellationToken cancellationToken = default)
        {
            var key = await _keyStoreService.GetFileEncryptionKeyAsync();
            if (key is null)
            {
                throw new InvalidOperationException("Die App ist gesperrt. Kein Dateischlüssel verfügbar.");
            }

            var personMediaDirectory = Path.Combine(Constants.MediaRootPath, personId.ToString());
            Directory.CreateDirectory(personMediaDirectory);

            var encryptedFileName = $"{Guid.NewGuid():N}.enc";
            var encryptedFilePath = Path.Combine(personMediaDirectory, encryptedFileName);

            await using var inputStream = await fileResult.OpenReadAsync();
            using var memoryStream = new MemoryStream();
            await inputStream.CopyToAsync(memoryStream, cancellationToken);

            var encrypted = _encryptionService.EncryptAesGcm(memoryStream.ToArray(), key);
            await File.WriteAllBytesAsync(encryptedFilePath, encrypted.ciphertext, cancellationToken);

            var mediaItem = new MediaItem
            {
                PersonID = personId,
                Type = mediaType,
                OriginalFileName = fileResult.FileName,
                EncryptedFilePath = Path.GetRelativePath(Constants.MediaRootPath, encryptedFilePath),
                MimeType = GetMimeType(fileResult.FileName, mediaType),
                IV = Convert.ToBase64String(encrypted.iv),
                Tag = Convert.ToBase64String(encrypted.tag),
                CreatedAt = DateTime.UtcNow
            };

            await _mediaRepository.SaveItemAsync(mediaItem);
            return mediaItem;
        }

        public async Task<string> CreateDecryptedCopyAsync(MediaItem mediaItem, CancellationToken cancellationToken = default)
        {
            var key = await _keyStoreService.GetFileEncryptionKeyAsync();
            if (key is null)
            {
                throw new InvalidOperationException("Die App ist gesperrt. Kein Dateischlüssel verfügbar.");
            }

            var encryptedPath = Path.Combine(Constants.MediaRootPath, mediaItem.EncryptedFilePath);
            if (!File.Exists(encryptedPath))
            {
                throw new FileNotFoundException("Verschlüsselte Mediendatei wurde nicht gefunden.", encryptedPath);
            }

            var ciphertext = await File.ReadAllBytesAsync(encryptedPath, cancellationToken);
            var plaintext = _encryptionService.DecryptAesGcm(
                ciphertext,
                key,
                Convert.FromBase64String(mediaItem.IV),
                Convert.FromBase64String(mediaItem.Tag));

            var tempFileName = $"{Guid.NewGuid():N}{Path.GetExtension(mediaItem.OriginalFileName)}";
            var tempFilePath = Path.Combine(FileSystem.CacheDirectory, tempFileName);
            await File.WriteAllBytesAsync(tempFilePath, plaintext, cancellationToken);
            return tempFilePath;
        }

        public async Task DeleteMediaAsync(MediaItem mediaItem, CancellationToken cancellationToken = default)
        {
            await _mediaRepository.DeleteItemAsync(mediaItem);

            var encryptedPath = Path.Combine(Constants.MediaRootPath, mediaItem.EncryptedFilePath);
            if (File.Exists(encryptedPath))
            {
                File.Delete(encryptedPath);
            }

            await Task.CompletedTask;
        }

        private static string GetMimeType(string fileName, MediaType mediaType)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".mp4" => "video/mp4",
                ".mov" => "video/quicktime",
                ".m4a" => "audio/m4a",
                ".wav" => "audio/wav",
                _ => mediaType switch
                {
                    MediaType.Photo => "image/*",
                    MediaType.Video => "video/*",
                    MediaType.Audio => "audio/*",
                    _ => "application/octet-stream"
                }
            };
        }
    }
}
