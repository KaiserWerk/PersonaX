using PersonaX.UI.Models;
using PersonaX.UI.PageModels;

namespace PersonaX.UI.Pages
{
    public partial class MainPage : ContentPage
    {
        public MainPage(MainPageModel model)
        {
            InitializeComponent();
            BindingContext = model;
        }
    }
}