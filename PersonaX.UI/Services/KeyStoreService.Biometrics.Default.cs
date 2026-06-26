#if !ANDROID && !IOS
namespace PersonaX.UI.Services
{
    public partial class KeyStoreService
    {
        private partial Task<bool> AuthenticateBiometricPromptAsync()
            => Task.FromResult(false);

        private partial Task<bool> GetPlatformBiometricsAvailabilityAsync()
            => Task.FromResult(false);
    }
}
#endif