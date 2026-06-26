using KaiserWerk.PersonaX.App.ViewModels;
using Microsoft.Maui.Controls;

namespace KaiserWerk.PersonaX.App.Views;

[QueryProperty(nameof(this.PersonId), nameof(this.PersonId))]
public partial class CaptureMediaPage : ContentPage
{
    public int PersonId
    {
        set
        {
            var bd = this.BindingContext as CaptureMediaViewModel;
            bd.SetPersonId(value);
        }
    }
    public CaptureMediaPage(CaptureMediaViewModel vm)
	{
		InitializeComponent();
		this.BindingContext = vm;
	}
}