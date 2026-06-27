using PersonaX.UI.Models;

namespace PersonaX.UI.Services
{
    public partial class MediaService : IMediaService
    {
        private readonly IEncryptionService _encryptionService;
        private readonly MediaRepository _mediaRepository;
        private AudioRecordingSession? _audioRecordingSession;

        public MediaService(IEncryptionService encryptionService, MediaRepository mediaRepository)
        {
            _encryptionService = encryptionService;
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
            return RecordAudioAsync(personId, cancellationToken);
        }

        public async Task<MediaItem> ImportAudioAsync(int personId, CancellationToken cancellationToken = default)
        {
            var file = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Audiodatei auswählen"
            });

            if (file is null)
            {
                throw new InvalidOperationException("Audioimport wurde abgebrochen.");
            }

            return await ImportMediaAsync(personId, file, MediaType.Audio, cancellationToken);
        }

        public async Task<MediaItem> ImportMediaAsync(int personId, FileResult fileResult, MediaType mediaType, CancellationToken cancellationToken = default)
        {
            var personMediaDirectory = Path.Combine(Constants.MediaRootPath, personId.ToString());
            Directory.CreateDirectory(personMediaDirectory);

            var encryptedFileName = $"{Guid.NewGuid():N}.enc";
            var encryptedFilePath = Path.Combine(personMediaDirectory, encryptedFileName);

            await using var inputStream = await fileResult.OpenReadAsync();
            using var memoryStream = new MemoryStream();
            await inputStream.CopyToAsync(memoryStream, cancellationToken);

            await File.WriteAllBytesAsync(encryptedFilePath, memoryStream.ToArray(), cancellationToken);

            var mediaItem = new MediaItem()
            {
                PersonID = personId,
                Type = mediaType,
                OriginalFileName = fileResult.FileName,
                FilePath = Path.GetRelativePath(Constants.MediaRootPath, encryptedFilePath),
                MimeType = GetMimeType(fileResult.FileName, mediaType),
                CreatedAt = DateTime.UtcNow
            };

            await _mediaRepository.SaveItemAsync(mediaItem);
            return mediaItem;
        }

        public async Task StartAudioRecordingAsync(int personId, CancellationToken cancellationToken = default)
        {
            if (_audioRecordingSession is not null)
            {
                throw new InvalidOperationException("Eine Audioaufnahme läuft bereits.");
            }

            await EnsureMicrophonePermissionAsync();

            var tempFileName = $"audio-{Guid.NewGuid():N}.m4a";
            var tempFilePath = Path.Combine(FileSystem.CacheDirectory, tempFileName);
            _audioRecordingSession = new AudioRecordingSession(personId, tempFilePath, tempFileName);

            await PlatformStartAudioRecordingAsync(tempFilePath, cancellationToken);
        }

        public async Task<MediaItem> StopAudioRecordingAsync(CancellationToken cancellationToken = default)
        {
            if (_audioRecordingSession is null)
            {
                throw new InvalidOperationException("Es läuft keine Audioaufnahme.");
            }

            var session = _audioRecordingSession;

            try
            {
                await PlatformStopAudioRecordingAsync(cancellationToken);
                return await ImportFileAsync(session.PersonId, session.FilePath, session.OriginalFileName, MediaType.Audio, cancellationToken);
            }
            finally
            {
                if (File.Exists(session.FilePath))
                {
                    File.Delete(session.FilePath);
                }

                _audioRecordingSession = null;
            }
        }

        public async Task<string> CreateDecryptedCopyAsync(MediaItem mediaItem, CancellationToken cancellationToken = default)
        {
            return "";
        }

        public async Task DeleteMediaAsync(MediaItem mediaItem, CancellationToken cancellationToken = default)
        {
            await _mediaRepository.DeleteItemAsync(mediaItem);

            var encryptedPath = Path.Combine(Constants.MediaRootPath, mediaItem.FilePath);
            if (File.Exists(encryptedPath))
            {
                File.Delete(encryptedPath);
            }

            await Task.CompletedTask;
        }

        private async Task<MediaItem> RecordAudioAsync(int personId, CancellationToken cancellationToken)
        {
            await StartAudioRecordingAsync(personId, cancellationToken);
            throw new InvalidOperationException("Für direkte Audioaufnahme muss die Start-/Stop-Logik der UI verwendet werden.");
        }

        private async Task EnsureMicrophonePermissionAsync()
        {
            var status = await Permissions.CheckStatusAsync<Permissions.Microphone>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.Microphone>();
            }

            if (status != PermissionStatus.Granted)
            {
                throw new InvalidOperationException("Mikrofonberechtigung wurde nicht erteilt.");
            }
        }

        private async Task<MediaItem> ImportFileAsync(int personId, string filePath, string originalFileName, MediaType mediaType, CancellationToken cancellationToken)
        {
            var personMediaDirectory = Path.Combine(Constants.MediaRootPath, personId.ToString());
            Directory.CreateDirectory(personMediaDirectory);

            var bytes = await File.ReadAllBytesAsync(filePath, cancellationToken);

            var mediaItem = new MediaItem
            {
                PersonID = personId,
                Type = mediaType,
                OriginalFileName = originalFileName,
                FilePath = Path.GetRelativePath(Constants.MediaRootPath, filePath),
                MimeType = GetMimeType(originalFileName, mediaType),
                CreatedAt = DateTime.UtcNow
            };

            await _mediaRepository.SaveItemAsync(mediaItem);
            return mediaItem;
        }

        private sealed record AudioRecordingSession(int PersonId, string FilePath, string OriginalFileName);

        private partial Task PlatformStartAudioRecordingAsync(string filePath, CancellationToken cancellationToken);
        private partial Task PlatformStopAudioRecordingAsync(CancellationToken cancellationToken);

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
