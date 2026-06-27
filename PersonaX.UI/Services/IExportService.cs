namespace PersonaX.UI.Services
{
    public interface IExportService
    {
        Task<string> ExportAsync(CancellationToken cancellationToken = default);
        Task ImportAsync(string backupPath, CancellationToken cancellationToken = default);
    }
}
