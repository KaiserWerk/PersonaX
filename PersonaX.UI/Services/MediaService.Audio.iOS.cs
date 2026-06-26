#if IOS
using AVFoundation;
using Foundation;

namespace PersonaX.UI.Services
{
    public partial class MediaService
    {
        private AVAudioRecorder? _iosAudioRecorder;

        private partial Task PlatformStartAudioRecordingAsync(string filePath, CancellationToken cancellationToken)
        {
            AVAudioSession.SharedInstance().SetCategory(AVAudioSessionCategory.PlayAndRecord);
            AVAudioSession.SharedInstance().SetActive(true);

            var settings = new AudioSettings
            {
                SampleRate = 44100,
                NumberChannels = 1,
                Format = AudioToolbox.AudioFormatType.MPEG4AAC,
                AudioQuality = AVAudioQuality.High,
                EncoderAudioQualityForVBR = AVAudioQuality.High,
                EncoderBitRate = 128000
            };

            var url = NSUrl.FromFilename(filePath);
            _iosAudioRecorder?.Dispose();
            _iosAudioRecorder = AVAudioRecorder.Create(url, settings, out var error);

            if (error is not null)
            {
                throw new InvalidOperationException(error.LocalizedDescription);
            }

            if (_iosAudioRecorder is null || !_iosAudioRecorder.Record())
            {
                throw new InvalidOperationException("Audioaufnahme konnte auf iOS nicht gestartet werden.");
            }

            return Task.CompletedTask;
        }

        private partial Task PlatformStopAudioRecordingAsync(CancellationToken cancellationToken)
        {
            if (_iosAudioRecorder is null)
            {
                throw new InvalidOperationException("iOS AudioRecorder ist nicht initialisiert.");
            }

            _iosAudioRecorder.Stop();
            _iosAudioRecorder.Dispose();
            _iosAudioRecorder = null;
            AVAudioSession.SharedInstance().SetActive(false);

            return Task.CompletedTask;
        }
    }
}
#endif