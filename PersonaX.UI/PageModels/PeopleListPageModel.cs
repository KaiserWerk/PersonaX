using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PersonaX.UI.Data;
using PersonaX.UI.Models;
using PersonaX.UI.Services;

namespace PersonaX.UI.PageModels
{
    public partial class PeopleListPageModel : ObservableObject
    {
        private readonly PeopleRepository _peopleRepository;
        private readonly ILockService _lockService;

        [ObservableProperty]
        private List<Person> _people = [];

        [ObservableProperty]
        private Person? _selectedPerson;

        [ObservableProperty]
        private bool _isRefreshing = false;

        public PeopleListPageModel(PeopleRepository peopleRepository, ILockService lockService)
        {
            _peopleRepository = peopleRepository;
            _lockService = lockService;
        }

        [RelayCommand]
        private async Task Appearing()
        {
            _lockService.NotifyUserActivity();
            await LoadPeople();
        }

        [RelayCommand]
        private async Task Refresh()
        {
            _lockService.NotifyUserActivity();
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

        [RelayCommand]
        private async Task AddPerson()
        {
            _lockService.NotifyUserActivity();
            await Shell.Current.GoToAsync("PersonDetail");
        }

        [RelayCommand]
        private async Task NavigateToPerson(Person person)
        {
            if (person == null)
                return;

            _lockService.NotifyUserActivity();
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
