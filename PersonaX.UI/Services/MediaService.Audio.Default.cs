#if !ANDROID && !IOS
namespace PersonaX.UI.Services
{
    public partial class MediaService
    {
        private partial Task PlatformStartAudioRecordingAsync(string filePath, CancellationToken cancellationToken)
        {
            throw new NotSupportedException("Echte Audioaufnahme ist nur auf Android und iOS aktiviert.");
        }

        private partial Task PlatformStopAudioRecordingAsync(CancellationToken cancellationToken)
        {
            throw new NotSupportedException("Echte Audioaufnahme ist nur auf Android und iOS aktiviert.");
        }
    }
}
#endif
