namespace PersonaX.UI.Pages
{
    public partial class LockPage : ContentPage
    {
        public LockPage(PageModels.LockPageModel model)
        {
            BindingContext = model;
            InitializeComponent();
        }
    }
}
