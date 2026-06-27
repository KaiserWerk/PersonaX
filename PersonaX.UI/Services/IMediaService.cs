using PersonaX.UI.Models;

namespace PersonaX.UI.Services
{
    public interface IMediaService
    {
        Task<MediaItem> CapturePhotoAsync(int personId, CancellationToken cancellationToken = default);
        Task<MediaItem> CaptureVideoAsync(int personId, CancellationToken cancellationToken = default);
        Task<MediaItem> ImportAudioAsync(int personId, CancellationToken cancellationToken = default);
        Task StartAudioRecordingAsync(int personId, CancellationToken cancellationToken = default);
        Task<MediaItem> StopAudioRecordingAsync(CancellationToken cancellationToken = default);
        Task<MediaItem> ImportMediaAsync(int personId, FileResult fileResult, MediaType mediaType, CancellationToken cancellationToken = default);
        Task DeleteMediaAsync(MediaItem mediaItem, CancellationToken cancellationToken = default);
        Task<MediaItem> CaptureAudioAsync(int personId, CancellationToken cancellationToken = default);
    }
}
