using Microsoft.Maui.Controls;

namespace KaiserWerk.PersonaX.App
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();
        }
    }
}
