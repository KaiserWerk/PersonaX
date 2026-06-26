using Microsoft.Extensions.DependencyInjection;

namespace PersonaX.UI
{
    public partial class App : Application
    {
        private ILockService? _lockService;

        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }

        protected override void OnStart()
        {
            base.OnStart();

            // Get LockService from DI container
            _lockService = Handler?.MauiContext?.Services.GetService<ILockService>();

            if (_lockService != null)
            {
                // Subscribe to lock events
                _lockService.Locked += OnAppLocked;
                _lockService.Unlocked += OnAppUnlocked;

                // Start auto-lock timer if app is unlocked
                if (!_lockService.IsLocked)
                {
                    _lockService.StartAutoLockTimer();
                }
            }
        }

        protected override void OnSleep()
        {
            base.OnSleep();

            // Lock the app when it goes to background
            _lockService?.LockAsync().FireAndForget();
        }

        protected override void OnResume()
        {
            base.OnResume();

            // App is already locked by OnSleep, navigation to LockPage will be handled by shell
        }

        private void OnAppLocked(object? sender, EventArgs e)
        {
            // Navigate to lock screen
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Shell.Current.GoToAsync("//LockPage");
            });
        }

        private void OnAppUnlocked(object? sender, EventArgs e)
        {
            // Navigate back to main page after unlock
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Shell.Current.GoToAsync("//main");
            });
        }
    }

    /// <summary>
    /// Extension for fire-and-forget async calls.
    /// </summary>
    public static class AppTaskExtensions
    {
        public static void FireAndForget(this Task task)
        {
            task.ContinueWith(t =>
            {
                if (t.IsFaulted && t.Exception != null)
                {
                    Console.WriteLine(t.Exception);
                }
            }, TaskScheduler.Default);
        }
    }
}
