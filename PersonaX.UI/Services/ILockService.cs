namespace PersonaX.UI.Services
{
    /// <summary>
    /// Service for managing app lock state, PIN verification, and auto-lock.
    /// </summary>
    public interface ILockService
    {
        /// <summary>
        /// Checks if the app is currently locked.
        /// </summary>
        bool IsLocked { get; }

        /// <summary>
        /// Gets the number of failed unlock attempts.
        /// </summary>
        int FailedAttempts { get; }

        /// <summary>
        /// Maximum allowed failed attempts before wipe (optional, disabled by default).
        /// </summary>
        int MaxFailedAttempts { get; set; }

        /// <summary>
        /// Auto-lock timeout in minutes (default: 5).
        /// </summary>
        int AutoLockTimeoutMinutes { get; set; }

        /// <summary>
        /// Event fired when the app transitions to locked state.
        /// </summary>
        event EventHandler? Locked;

        /// <summary>
        /// Event fired when the app is unlocked.
        /// </summary>
        event EventHandler? Unlocked;

        /// <summary>
        /// Attempts to unlock the app with a PIN.
        /// </summary>
        Task<bool> UnlockWithPinAsync(string pin);

        /// <summary>
        /// Attempts to unlock the app with biometrics.
        /// </summary>
        Task<bool> UnlockWithBiometricsAsync();

        /// <summary>
        /// Locks the app immediately.
        /// </summary>
        Task LockAsync();

        /// <summary>
        /// Notifies the service of user activity to reset the auto-lock timer.
        /// </summary>
        void NotifyUserActivity();

        /// <summary>
        /// Starts the auto-lock timer.
        /// </summary>
        void StartAutoLockTimer();

        /// <summary>
        /// Stops the auto-lock timer.
        /// </summary>
        void StopAutoLockTimer();

        /// <summary>
        /// Resets failed attempt counter.
        /// </summary>
        void ResetFailedAttempts();

        /// <summary>
        /// Performs emergency wipe (if enabled and max attempts exceeded).
        /// </summary>
        Task WipeDataAsync();
    }
}
