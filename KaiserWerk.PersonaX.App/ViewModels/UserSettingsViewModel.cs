using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Storage;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace KaiserWerk.PersonaX.App.ViewModels;

public class UserSettingsViewModel
{
    private readonly string file = Path.Join(FileSystem.AppDataDirectory, "PersonaX", "persona_x.db");
    public Command DownloadDbFileCommand { get; }

    public UserSettingsViewModel()
    {
        this.DownloadDbFileCommand = new Command(async () => await this.DownloadDbFileCommandExecute());
    }

    private async Task DownloadDbFileCommandExecute()
    {
        using var stream = new MemoryStream(await File.ReadAllBytesAsync(this.file));
        var fileSaverResult = await FileSaver.Default.SaveAsync("persona_x.db", stream, CancellationToken.None);
        if (fileSaverResult.IsSuccessful)
        {
            await Toast.Make($"The file was saved successfully to location: {fileSaverResult.FilePath}").Show(CancellationToken.None);
        }
        else
        {
            await Toast.Make($"The file was not saved successfully with error: {fileSaverResult.Exception.Message}").Show(CancellationToken.None);
        }
    }
}
