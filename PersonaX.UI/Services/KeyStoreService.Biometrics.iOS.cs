#if IOS
using Foundation;
using LocalAuthentication;

namespace PersonaX.UI.Services
{
    public partial class KeyStoreService
    {
        private partial Task<bool> AuthenticateBiometricPromptAsync()
        {
            var context = new LAContext();
            if (!context.CanEvaluatePolicy(LAPolicy.DeviceOwnerAuthenticationWithBiometrics, out var error))
            {
                return Task.FromResult(false);
            }

            var tcs = new TaskCompletionSource<bool>();
            context.EvaluatePolicy(
                LAPolicy.DeviceOwnerAuthenticationWithBiometrics,
                "PersonaX entsperren",
                (success, evaluateError) => tcs.TrySetResult(success));

            return tcs.Task;
        }

        private partial Task<bool> GetPlatformBiometricsAvailabilityAsync()
        {
            var context = new LAContext();
            var available = context.CanEvaluatePolicy(LAPolicy.DeviceOwnerAuthenticationWithBiometrics, out var error);
            return Task.FromResult(available);
        }
    }
}
#endif