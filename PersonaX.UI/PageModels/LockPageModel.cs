using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PersonaX.UI.Services;

namespace PersonaX.UI.PageModels
{
    public partial class LockPageModel : ObservableObject
    {
        private readonly ILockService _lockService;
        private readonly IKeyStoreService _keyStoreService;

        [ObservableProperty]
        private string _pin = string.Empty;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        [ObservableProperty]
        private bool _isUnlocking = false;

        [ObservableProperty]
        private bool _showSetupMode = false;

        [ObservableProperty]
        private string _confirmPin = string.Empty;

        [ObservableProperty]
        private int _failedAttempts = 0;

        public LockPageModel(ILockService lockService, IKeyStoreService keyStoreService)
        {
            _lockService = lockService;
            _keyStoreService = keyStoreService;
        }

        [RelayCommand]
        private async Task Appearing()
        {
            // Check if this is first-time setup
            ShowSetupMode = !await _keyStoreService.IsInitializedAsync();
            FailedAttempts = _lockService.FailedAttempts;
            ErrorMessage = string.Empty;
            Pin = string.Empty;
            ConfirmPin = string.Empty;
        }

        [RelayCommand]
        private async Task Unlock()
        {
            if (IsUnlocking)
                return;

            ErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(Pin))
            {
                ErrorMessage = "Please enter your PIN";
                return;
            }

            try
            {
                IsUnlocking = true;

                if (ShowSetupMode)
                {
                    // Setup mode - initialize PIN
                    if (Pin.Length < 4)
                    {
                        ErrorMessage = "PIN must be at least 4 characters";
                        return;
                    }

                    if (Pin != ConfirmPin)
                    {
                        ErrorMessage = "PINs do not match";
                        return;
                    }

                    await _keyStoreService.InitializeAsync(Pin);
                    await _lockService.UnlockWithPinAsync(Pin);
                    // Navigation handled by LockService.Unlocked event
                }
                else
                {
                    // Unlock mode
                    var unlocked = await _lockService.UnlockWithPinAsync(Pin);

                    if (!unlocked)
                    {
                        FailedAttempts = _lockService.FailedAttempts;
                        ErrorMessage = $"Incorrect PIN. Attempts: {FailedAttempts}";
                        Pin = string.Empty;
                    }
                    // Navigation handled by LockService.Unlocked event
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error: {ex.Message}";
            }
            finally
            {
                IsUnlocking = false;
            }
        }

        [RelayCommand]
        private async Task UnlockWithBiometrics()
        {
            if (!await _keyStoreService.IsBiometricsAvailableAsync())
            {
                ErrorMessage = "Biometric authentication not available";
                return;
            }

            var unlocked = await _lockService.UnlockWithBiometricsAsync();
            if (!unlocked)
            {
                ErrorMessage = "Biometric authentication failed";
            }
        }
    }
}
