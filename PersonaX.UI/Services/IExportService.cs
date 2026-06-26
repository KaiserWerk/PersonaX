namespace PersonaX.UI.Services
{
    public interface IExportService
    {
        Task<string> ExportAsync(string passphrase, CancellationToken cancellationToken = default);
        Task ImportAsync(string encryptedBackupPath, string passphrase, CancellationToken cancellationToken = default);
    }
}
