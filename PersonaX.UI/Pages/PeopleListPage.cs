namespace PersonaX.UI.Pages
{
    public class PeopleListPage : ContentPage
    {
        private readonly PageModels.PeopleListPageModel _model;

        public PeopleListPage(PageModels.PeopleListPageModel model)
        {
            _model = model;
            BindingContext = model;
            Title = "People";

            var collectionView = new CollectionView
            {
                Margin = new Thickness(16),
                SelectionMode = SelectionMode.Single,
                EmptyView = new VerticalStackLayout
                {
                    Padding = 20,
                    Spacing = 10,
                    Children =
                    {
                        new Label { Text = "Noch keine Personen vorhanden.", HorizontalOptions = LayoutOptions.Center },
                        new Label { Text = "Füge eine Person über den Plus-Button hinzu.", HorizontalOptions = LayoutOptions.Center }
                    }
                },
                ItemTemplate = new DataTemplate(() =>
                {
                    var name = new Label { FontSize = 20, FontAttributes = FontAttributes.Bold };
                    name.SetBinding(Label.TextProperty, nameof(Models.Person.FullName));

                    var email = new Label();
                    email.SetBinding(Label.TextProperty, nameof(Models.Person.Email));

                    var phone = new Label();
                    phone.SetBinding(Label.TextProperty, nameof(Models.Person.PhoneNumber));

                    return new Border
                    {
                        Margin = new Thickness(0, 0, 0, 10),
                        Padding = 12,
                        StrokeThickness = 0,
                        Content = new VerticalStackLayout
                        {
                            Spacing = 4,
                            Children = { name, email, phone }
                        }
                    };
                })
            };
            collectionView.SetBinding(ItemsView.ItemsSourceProperty, nameof(PageModels.PeopleListPageModel.People));
            collectionView.SetBinding(SelectableItemsView.SelectedItemProperty, nameof(PageModels.PeopleListPageModel.SelectedPerson));
            collectionView.SelectionChanged += async (_, e) =>
            {
                if (e.CurrentSelection.FirstOrDefault() is Models.Person person)
                {
                    await _model.NavigateToPersonCommand.ExecuteAsync(person);
                    collectionView.SelectedItem = null;
                }
            };

            var refreshView = new RefreshView { Content = collectionView };
            refreshView.SetBinding(RefreshView.IsRefreshingProperty, nameof(PageModels.PeopleListPageModel.IsRefreshing));
            refreshView.SetBinding(RefreshView.CommandProperty, nameof(PageModels.PeopleListPageModel.RefreshCommand));

            var addButton = new Button
            {
                Text = "+",
                FontSize = 24,
                WidthRequest = 56,
                HeightRequest = 56,
                CornerRadius = 28,
                HorizontalOptions = LayoutOptions.End,
                VerticalOptions = LayoutOptions.End,
                Margin = 16
            };
            addButton.SetBinding(Button.CommandProperty, nameof(PageModels.PeopleListPageModel.AddPersonCommand));

            Content = new Grid
            {
                Children =
                {
                    refreshView,
                    addButton
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
