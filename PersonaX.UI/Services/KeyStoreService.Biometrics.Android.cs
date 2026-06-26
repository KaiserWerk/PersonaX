#if ANDROID
using AndroidX.Biometric;
using AndroidX.Core.Content;
using AndroidX.Fragment.App;
using Microsoft.Maui.ApplicationModel;
using Java.Lang;

namespace PersonaX.UI.Services
{
    public partial class KeyStoreService
    {
        private partial Task<bool> AuthenticateBiometricPromptAsync()
        {
            var activity = Platform.CurrentActivity as FragmentActivity;
            if (activity is null)
            {
                return Task.FromResult(false);
            }

            var tcs = new TaskCompletionSource<bool>();
            var executor = ContextCompat.GetMainExecutor(activity);
            var callback = new AndroidBiometricCallback(tcs);
            var prompt = new BiometricPrompt(activity, executor, callback);

            var promptInfo = new BiometricPrompt.PromptInfo.Builder()
                .SetTitle("PersonaX entsperren")
                .SetSubtitle("Biometrische Authentifizierung verwenden")
                .SetNegativeButtonText("Abbrechen")
                .Build();

            prompt.Authenticate(promptInfo);
            return tcs.Task;
        }

        private partial Task<bool> GetPlatformBiometricsAvailabilityAsync()
        {
            var activity = Platform.CurrentActivity;
            if (activity is null)
            {
                return Task.FromResult(false);
            }

            var biometricManager = BiometricManager.From(activity);
            var result = biometricManager.CanAuthenticate(BiometricManager.Authenticators.BiometricWeak | BiometricManager.Authenticators.DeviceCredential);
            return Task.FromResult(result == BiometricManager.BiometricSuccess);
        }

        private sealed class AndroidBiometricCallback(TaskCompletionSource<bool> taskCompletionSource) : BiometricPrompt.AuthenticationCallback
        {
            public override void OnAuthenticationSucceeded(BiometricPrompt.AuthenticationResult result)
            {
                base.OnAuthenticationSucceeded(result);
                taskCompletionSource.TrySetResult(true);
            }

            public override void OnAuthenticationError(int errorCode, ICharSequence? errString)
            {
                base.OnAuthenticationError(errorCode, errString);
                taskCompletionSource.TrySetResult(false);
            }

            public override void OnAuthenticationFailed()
            {
                base.OnAuthenticationFailed();
            }
        }
    }
}
#endif