using Microsoft.Maui.Controls.Shapes;

namespace PersonaX.UI.Pages
{
    public class LockPage : ContentPage
    {
        private readonly PageModels.LockPageModel _model;

        public LockPage(PageModels.LockPageModel model)
        {
            _model = model;
            BindingContext = model;
            Title = "Unlock";
            Shell.SetNavBarIsVisible(this, false);
            Shell.SetTabBarIsVisible(this, false);

            var titleLabel = new Label
            {
                FontSize = 24,
                FontAttributes = FontAttributes.Bold,
                HorizontalOptions = LayoutOptions.Center
            };
            titleLabel.SetBinding(Label.TextProperty, nameof(PageModels.LockPageModel.TitleText));

            var setupPinEntry = new Entry { Placeholder = "Enter new PIN (min 4 characters)", IsPassword = true, MinimumHeightRequest = 44 };
            setupPinEntry.SetBinding(Entry.TextProperty, nameof(PageModels.LockPageModel.Pin));
            setupPinEntry.SetBinding(IsVisibleProperty, nameof(PageModels.LockPageModel.ShowSetupMode));

            var confirmPinEntry = new Entry { Placeholder = "Confirm PIN", IsPassword = true, MinimumHeightRequest = 44 };
            confirmPinEntry.SetBinding(Entry.TextProperty, nameof(PageModels.LockPageModel.ConfirmPin));
            confirmPinEntry.SetBinding(IsVisibleProperty, nameof(PageModels.LockPageModel.ShowSetupMode));

            var unlockPinEntry = new Entry { Placeholder = "Enter PIN", IsPassword = true, MinimumHeightRequest = 44 };
            unlockPinEntry.SetBinding(Entry.TextProperty, nameof(PageModels.LockPageModel.Pin));
            unlockPinEntry.SetBinding(IsVisibleProperty, nameof(PageModels.LockPageModel.ShowUnlockMode));

            var errorLabel = new Label { TextColor = Colors.Red, HorizontalOptions = LayoutOptions.Center };
            errorLabel.SetBinding(Label.TextProperty, nameof(PageModels.LockPageModel.ErrorMessage));
            errorLabel.SetBinding(IsVisibleProperty, nameof(PageModels.LockPageModel.HasErrorMessage));

            var attemptsLabel = new Label { HorizontalOptions = LayoutOptions.Center, FontSize = 12 };
            attemptsLabel.SetBinding(Label.TextProperty, new Binding(nameof(PageModels.LockPageModel.FailedAttempts), stringFormat: "Failed attempts: {0}"));
            attemptsLabel.SetBinding(IsVisibleProperty, nameof(PageModels.LockPageModel.ShowUnlockMode));

            var unlockButton = new Button { MinimumHeightRequest = 44 };
            unlockButton.SetBinding(Button.TextProperty, nameof(PageModels.LockPageModel.PrimaryActionText));
            unlockButton.SetBinding(Button.CommandProperty, nameof(PageModels.LockPageModel.UnlockCommand));
            unlockButton.SetBinding(IsEnabledProperty, nameof(PageModels.LockPageModel.IsUnlockButtonEnabled));

            Content = new ScrollView
            {
                Content = new VerticalStackLayout
                {
                    Padding = 24,
                    Spacing = 16,
                    VerticalOptions = LayoutOptions.Center,
                    Children =
                    {
                        new Label { Text = "🔒", FontSize = 48, HorizontalOptions = LayoutOptions.Center },
                        titleLabel,
                        setupPinEntry,
                        confirmPinEntry,
                        unlockPinEntry,
                        errorLabel,
                        attemptsLabel,
                        unlockButton
                    }
                }
            };
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _model.AppearingCommand.ExecuteAsync(null);
        }
    }
}
