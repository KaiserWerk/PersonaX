using KaiserWerk.PersonaX.App.ViewModels;
using Microsoft.Maui.Controls;

namespace KaiserWerk.PersonaX.App.Views;

public partial class UserSettingsPage : ContentPage
{
	public UserSettingsPage(UserSettingsViewModel vm)
	{
		InitializeComponent();
		this.BindingContext = vm;
	}
}