using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PersonaX.UI.Services;

namespace PersonaX.UI.PageModels
{
    public class LockPageModel : ObservableObject
    {
        private readonly ILockService _lockService;
        private string _pin = string.Empty;
        private string _errorMessage = string.Empty;
        private bool _isUnlocking;
        private bool _showSetupMode;
        private string _confirmPin = string.Empty;
        private int _failedAttempts;
        private bool _isBiometricsAvailable;
        private bool _isBiometricsEnabled;

        public string Pin
        {
            get => _pin;
            set => SetProperty(ref _pin, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                if (SetProperty(ref _errorMessage, value))
                {
                    OnPropertyChanged(nameof(HasErrorMessage));
                }
            }
        }

        public bool IsUnlocking
        {
            get => _isUnlocking;
            set
            {
                if (SetProperty(ref _isUnlocking, value))
                {
                    OnPropertyChanged(nameof(IsUnlockButtonEnabled));
                }
            }
        }

        public bool ShowSetupMode
        {
            get => _showSetupMode;
            set
            {
                if (SetProperty(ref _showSetupMode, value))
                {
                    OnPropertyChanged(nameof(ShowUnlockMode));
                    OnPropertyChanged(nameof(TitleText));
                    OnPropertyChanged(nameof(PrimaryActionText));
                }
            }
        }

        public string ConfirmPin
        {
            get => _confirmPin;
            set => SetProperty(ref _confirmPin, value);
        }

        public int FailedAttempts
        {
            get => _failedAttempts;
            set => SetProperty(ref _failedAttempts, value);
        }

        public bool IsBiometricsAvailable
        {
            get => _isBiometricsAvailable;
            set
            {
                if (SetProperty(ref _isBiometricsAvailable, value))
                {
                    OnPropertyChanged(nameof(ShowBiometricsButton));
                    OnPropertyChanged(nameof(ShowBiometricsSetupButton));
                    OnPropertyChanged(nameof(BiometricsStatusText));
                }
            }
        }

        public bool IsBiometricsEnabled
        {
            get => _isBiometricsEnabled;
            set
            {
                if (SetProperty(ref _isBiometricsEnabled, value))
                {
                    OnPropertyChanged(nameof(ShowBiometricsButton));
                    OnPropertyChanged(nameof(ShowBiometricsSetupButton));
                    OnPropertyChanged(nameof(BiometricsStatusText));
                }
            }
        }

        public bool ShowUnlockMode => !ShowSetupMode;
        public bool HasErrorMessage => !string.IsNullOrWhiteSpace(ErrorMessage);
        public bool IsUnlockButtonEnabled => !IsUnlocking;
        public string TitleText => ShowSetupMode ? "Setup PIN" : "Enter PIN";
        public string PrimaryActionText => ShowSetupMode ? "Create PIN" : "Unlock";
        public bool ShowBiometricsButton => ShowUnlockMode && IsBiometricsAvailable && IsBiometricsEnabled;
        public bool ShowBiometricsSetupButton => IsBiometricsAvailable && !IsBiometricsEnabled;
        public string BiometricsStatusText => !IsBiometricsAvailable
            ? "Biometrie ist auf diesem Gerät derzeit nicht verfügbar."
            : IsBiometricsEnabled
                ? "Biometrie ist aktiviert und kann zum Entsperren verwendet werden."
                : "Biometrie kann auf diesem Gerät aktiviert werden.";

        public IAsyncRelayCommand AppearingCommand { get; }
        public IAsyncRelayCommand UnlockCommand { get; }
        public IAsyncRelayCommand UnlockWithBiometricsCommand { get; }
        public IAsyncRelayCommand EnableBiometricsCommand { get; }

        public LockPageModel(ILockService lockService)
        {
            _lockService = lockService;
            AppearingCommand = new AsyncRelayCommand(AppearingAsync);
            UnlockCommand = new AsyncRelayCommand(UnlockAsync);
            UnlockWithBiometricsCommand = new AsyncRelayCommand(UnlockWithBiometricsAsync);
            EnableBiometricsCommand = new AsyncRelayCommand(EnableBiometricsAsync);
        }

        private async Task AppearingAsync()
        {
            FailedAttempts = _lockService.FailedAttempts;
            ErrorMessage = string.Empty;
            Pin = string.Empty;
            ConfirmPin = string.Empty;
        }

        private async Task UnlockAsync()
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

        private async Task UnlockWithBiometricsAsync()
        {
            try
            {
                IsUnlocking = true;
                var unlocked = await _lockService.UnlockWithBiometricsAsync();
                if (!unlocked)
                {
                    ErrorMessage = "Biometrische Entsperrung wurde abgebrochen oder ist fehlgeschlagen.";
                }
            }
            finally
            {
                IsUnlocking = false;
            }
        }

        private async Task EnableBiometricsAsync()
        {
            IsBiometricsEnabled = true;
            ErrorMessage = "Biometrie wurde aktiviert.";
        }
    }
}
