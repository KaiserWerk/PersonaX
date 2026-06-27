using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PersonaX.UI.Data;
using PersonaX.UI.Models;
namespace PersonaX.UI.PageModels
{
    public class PeopleListPageModel : ObservableObject
    {
        private readonly PeopleRepository _peopleRepository;
        private List<Person> _people = [];
        private Person? _selectedPerson;
        private bool _isRefreshing;

        public List<Person> People
        {
            get => _people;
            set => SetProperty(ref _people, value);
        }

        public Person? SelectedPerson
        {
            get => _selectedPerson;
            set => SetProperty(ref _selectedPerson, value);
        }

        public bool IsRefreshing
        {
            get => _isRefreshing;
            set => SetProperty(ref _isRefreshing, value);
        }

        public IAsyncRelayCommand AppearingCommand { get; }
        public IAsyncRelayCommand RefreshCommand { get; }
        public IAsyncRelayCommand AddPersonCommand { get; }
        public IAsyncRelayCommand<Person> NavigateToPersonCommand { get; }

        public PeopleListPageModel(PeopleRepository peopleRepository)
        {
            _peopleRepository = peopleRepository;
            AppearingCommand = new AsyncRelayCommand(AppearingAsync);
            RefreshCommand = new AsyncRelayCommand(RefreshAsync);
            AddPersonCommand = new AsyncRelayCommand(AddPersonAsync);
            NavigateToPersonCommand = new AsyncRelayCommand<Person>(NavigateToPersonAsync);
        }

        private async Task AppearingAsync()
        {
            await LoadPeople();
        }

        private async Task RefreshAsync()
        {
            try
            {
                IsRefreshing = true;
                await LoadPeople();
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        private async Task AddPersonAsync()
        {
            await Shell.Current.GoToAsync("PersonDetail");
        }

        private async Task NavigateToPersonAsync(Person? person)
        {
            if (person == null)
                return;

            await Shell.Current.GoToAsync($"PersonDetail?id={person.ID}");
        }

        private async Task LoadPeople()
        {
            try
            {
                People = await _peopleRepository.ListAsync();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to load people: {ex.Message}", "OK");
            }
        }
    }
}
