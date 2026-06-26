#if ANDROID
using Android.Media;

namespace PersonaX.UI.Services
{
    public partial class MediaService
    {
        private MediaRecorder? _androidMediaRecorder;

        private partial Task PlatformStartAudioRecordingAsync(string filePath, CancellationToken cancellationToken)
        {
            _androidMediaRecorder?.Release();
            _androidMediaRecorder?.Dispose();

            var recorder = new MediaRecorder();
            recorder.SetAudioSource(AudioSource.Mic);
            recorder.SetOutputFormat(OutputFormat.Mpeg4);
            recorder.SetAudioEncoder(AudioEncoder.Aac);
            recorder.SetAudioSamplingRate(44100);
            recorder.SetAudioEncodingBitRate(128000);
            recorder.SetOutputFile(filePath);
            recorder.Prepare();
            recorder.Start();

            _androidMediaRecorder = recorder;
            return Task.CompletedTask;
        }

        private partial Task PlatformStopAudioRecordingAsync(CancellationToken cancellationToken)
        {
            if (_androidMediaRecorder is null)
            {
                throw new InvalidOperationException("Android AudioRecorder ist nicht initialisiert.");
            }

            _androidMediaRecorder.Stop();
            _androidMediaRecorder.Reset();
            _androidMediaRecorder.Release();
            _androidMediaRecorder.Dispose();
            _androidMediaRecorder = null;

            return Task.CompletedTask;
        }
    }
}
#endif