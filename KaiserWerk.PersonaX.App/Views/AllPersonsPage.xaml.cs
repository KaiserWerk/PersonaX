using KaiserWerk.PersonaX.App.ViewModels;
using Microsoft.Maui.Controls;

namespace KaiserWerk.PersonaX.App.Views;

public partial class AllPersonsPage : ContentPage
{
    public AllPersonsPage(AllPersonsViewModel vm)
    {
        InitializeComponent();
        this.BindingContext = vm;
    }
}
