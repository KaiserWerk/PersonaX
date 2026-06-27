using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PersonaX.UI.Data;
using PersonaX.UI.Models;
using PersonaX.UI.Services;
using System.Collections.ObjectModel;

namespace PersonaX.UI.PageModels
{
    public class PeopleDetailPageModel : ObservableObject, IQueryAttributable
    {
        private readonly PeopleRepository _peopleRepository;
        private readonly IMediaService _mediaService;
        private Person? _person;
        private string _firstName = string.Empty;
        private string _lastName = string.Empty;
        private string _email = string.Empty;
        private string _phoneNumber = string.Empty;
        private DateTime _dateOfBirth = DateTime.Today;
        private string _notes = string.Empty;
        private string _street = string.Empty;
        private string _city = string.Empty;
        private string _state = string.Empty;
        private string _postalCode = string.Empty;
        private string _country = string.Empty;
        private bool _isBusy;
        private bool _isRecordingAudio;
        private ObservableCollection<MediaItem> _mediaItems = [];

        public string FirstName
        {
            get => _firstName;
            set => SetProperty(ref _firstName, value);
        }

        public string LastName
        {
            get => _lastName;
            set => SetProperty(ref _lastName, value);
        }

        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        public string PhoneNumber
        {
            get => _phoneNumber;
            set => SetProperty(ref _phoneNumber, value);
        }

        public DateTime DateOfBirth
        {
            get => _dateOfBirth;
            set => SetProperty(ref _dateOfBirth, value);
        }

        public string Notes
        {
            get => _notes;
            set => SetProperty(ref _notes, value);
        }

        public string Street
        {
            get => _street;
            set => SetProperty(ref _street, value);
        }

        public string City
        {
            get => _city;
            set => SetProperty(ref _city, value);
        }

        public string State
        {
            get => _state;
            set => SetProperty(ref _state, value);
        }

        public string PostalCode
        {
            get => _postalCode;
            set => SetProperty(ref _postalCode, value);
        }

        public string Country
        {
            get => _country;
            set => SetProperty(ref _country, value);
        }

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        public ObservableCollection<MediaItem> MediaItems
        {
            get => _mediaItems;
            set => SetProperty(ref _mediaItems, value);
        }

        public bool IsRecordingAudio
        {
            get => _isRecordingAudio;
            set
            {
                if (SetProperty(ref _isRecordingAudio, value))
                {
                    OnPropertyChanged(nameof(AudioRecordingButtonText));
                }
            }
        }

        public string AudioRecordingButtonText => IsRecordingAudio ? "Audioaufnahme stoppen" : "Audio aufnehmen";

        public bool CanDelete => _person is not null && _person.ID != 0;
        public bool CanManageMedia => _person is not null && _person.ID > 0;
        public IRelayCommand AppearingCommand { get; }
        public IAsyncRelayCommand SaveCommand { get; }
        public IAsyncRelayCommand DeleteCommand { get; }
        public IAsyncRelayCommand AddPhotoCommand { get; }
        public IAsyncRelayCommand AddVideoCommand { get; }
        public IAsyncRelayCommand ToggleAudioRecordingCommand { get; }
        public IAsyncRelayCommand<MediaItem> DeleteMediaCommand { get; }

        public PeopleDetailPageModel(PeopleRepository peopleRepository, IMediaService mediaService)
        {
            _peopleRepository = peopleRepository;
            _mediaService = mediaService;
            AppearingCommand = new RelayCommand(Appearing);
            SaveCommand = new AsyncRelayCommand(SaveAsync);
            DeleteCommand = new AsyncRelayCommand(DeleteAsync, () => CanDelete);
            AddPhotoCommand = new AsyncRelayCommand(AddPhotoAsync, () => CanManageMedia);
            AddVideoCommand = new AsyncRelayCommand(AddVideoAsync, () => CanManageMedia);
            ToggleAudioRecordingCommand = new AsyncRelayCommand(ToggleAudioRecordingAsync, () => CanManageMedia);
            DeleteMediaCommand = new AsyncRelayCommand<MediaItem>(DeleteMediaAsync);
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("id", out var value) && int.TryParse(value?.ToString(), out var id))
            {
                LoadAsync(id).FireAndForget();
                return;
            }

            _person = new Person();
            FirstName = string.Empty;
            LastName = string.Empty;
            Email = string.Empty;
            PhoneNumber = string.Empty;
            DateOfBirth = DateTime.Today;
            Notes = string.Empty;
            Street = string.Empty;
            City = string.Empty;
            State = string.Empty;
            PostalCode = string.Empty;
            Country = string.Empty;
            IsRecordingAudio = false;
            MediaItems = [];
            DeleteCommand.NotifyCanExecuteChanged();
            AddPhotoCommand.NotifyCanExecuteChanged();
            AddVideoCommand.NotifyCanExecuteChanged();
            ToggleAudioRecordingCommand.NotifyCanExecuteChanged();
            OnPropertyChanged(nameof(CanDelete));
            OnPropertyChanged(nameof(CanManageMedia));
        }

        private void Appearing()
        {
        }

        private async Task SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(FirstName) && string.IsNullOrWhiteSpace(LastName))
            {
                await Shell.Current.DisplayAlert("Validation", "Bitte Vorname oder Nachname eingeben.", "OK");
                return;
            }

            _person ??= new Person();
            _person.FirstName = FirstName.Trim();
            _person.LastName = LastName.Trim();
            _person.Email = Email.Trim();
            _person.PhoneNumber = PhoneNumber.Trim();
            _person.DateOfBirth = DateOfBirth;
            _person.Notes = Notes?.Trim() ?? string.Empty;
            _person.Address = BuildAddress();

            await _peopleRepository.SaveItemAsync(_person);
            _person = await _peopleRepository.GetAsync(_person.ID) ?? _person;
            MediaItems = new ObservableCollection<MediaItem>(_person.MediaItems);
            DeleteCommand.NotifyCanExecuteChanged();
            AddPhotoCommand.NotifyCanExecuteChanged();
            AddVideoCommand.NotifyCanExecuteChanged();
            ToggleAudioRecordingCommand.NotifyCanExecuteChanged();
            OnPropertyChanged(nameof(CanDelete));
            OnPropertyChanged(nameof(CanManageMedia));
            await AppShell.DisplayToastAsync("Person gespeichert");
        }

        private async Task DeleteAsync()
        {
            if (_person is null || _person.ID == 0)
            {
                await Shell.Current.GoToAsync("..");
                return;
            }

            var confirmed = await Shell.Current.DisplayAlert(
                "Person löschen",
                $"{_person.FullName} wirklich löschen?",
                "Löschen",
                "Abbrechen");

            if (!confirmed)
            {
                return;
            }

            await _peopleRepository.DeleteItemAsync(_person);
            await AppShell.DisplayToastAsync("Person gelöscht");
            await Shell.Current.GoToAsync("..");
        }

        private async Task LoadAsync(int id)
        {
            try
            {
                IsBusy = true;
                _person = await _peopleRepository.GetAsync(id);
                if (_person is null)
                {
                    await Shell.Current.DisplayAlert("Fehler", $"Person mit ID {id} wurde nicht gefunden.", "OK");
                    await Shell.Current.GoToAsync("..");
                    return;
                }

                FirstName = _person.FirstName;
                LastName = _person.LastName;
                Email = _person.Email;
                PhoneNumber = _person.PhoneNumber;
                DateOfBirth = _person.DateOfBirth == default ? DateTime.Today : _person.DateOfBirth;
                Notes = _person.Notes;
                Street = _person.Address?.Street ?? string.Empty;
                City = _person.Address?.City ?? string.Empty;
                State = _person.Address?.State ?? string.Empty;
                PostalCode = _person.Address?.PostalCode ?? string.Empty;
                Country = _person.Address?.Country ?? string.Empty;
                MediaItems = new ObservableCollection<MediaItem>(_person.MediaItems);
                DeleteCommand.NotifyCanExecuteChanged();
                AddPhotoCommand.NotifyCanExecuteChanged();
                AddVideoCommand.NotifyCanExecuteChanged();
                ToggleAudioRecordingCommand.NotifyCanExecuteChanged();
                OnPropertyChanged(nameof(CanDelete));
                OnPropertyChanged(nameof(CanManageMedia));
            }
            finally
            {
                IsBusy = false;
            }
        }

        private Address? BuildAddress()
        {
            if (string.IsNullOrWhiteSpace(Street)
                && string.IsNullOrWhiteSpace(City)
                && string.IsNullOrWhiteSpace(State)
                && string.IsNullOrWhiteSpace(PostalCode)
                && string.IsNullOrWhiteSpace(Country))
            {
                return null;
            }

            return new Address
            {
                ID = _person?.Address?.ID ?? 0,
                PersonID = _person?.ID ?? 0,
                Street = Street.Trim(),
                City = City.Trim(),
                State = State.Trim(),
                PostalCode = PostalCode.Trim(),
                Country = Country.Trim()
            };
        }

        private async Task AddPhotoAsync()
        {
            await AddMediaAsync(() => _mediaService.CapturePhotoAsync(_person!.ID));
        }

        private async Task AddVideoAsync()
        {
            await AddMediaAsync(() => _mediaService.CaptureVideoAsync(_person!.ID));
        }

        private async Task ToggleAudioRecordingAsync()
        {
            if (_person is null || _person.ID == 0)
            {
                await Shell.Current.DisplayAlert("Hinweis", "Bitte die Person zuerst speichern.", "OK");
                return;
            }

            try
            {
                if (!IsRecordingAudio)
                {
                    await _mediaService.StartAudioRecordingAsync(_person.ID);
                    IsRecordingAudio = true;
                    await AppShell.DisplayToastAsync("Audioaufnahme gestartet");
                    return;
                }

                var mediaItem = await _mediaService.StopAudioRecordingAsync();
                IsRecordingAudio = false;
                MediaItems.Insert(0, mediaItem);
                await AppShell.DisplayToastAsync("Audio gespeichert");
            }
            catch (Exception ex)
            {
                IsRecordingAudio = false;
                await Shell.Current.DisplayAlert("Fehler", ex.Message, "OK");
            }
        }

        private async Task DeleteMediaAsync(MediaItem? mediaItem)
        {
            if (mediaItem is null)
            {
                return;
            }

            var confirmed = await Shell.Current.DisplayAlert(
                "Medium löschen",
                $"{mediaItem.OriginalFileName} wirklich löschen?",
                "Löschen",
                "Abbrechen");

            if (!confirmed)
            {
                return;
            }

            await _mediaService.DeleteMediaAsync(mediaItem);
            MediaItems.Remove(mediaItem);
        }

        private async Task AddMediaAsync(Func<Task<MediaItem>> action)
        {
            if (_person is null || _person.ID == 0)
            {
                await Shell.Current.DisplayAlert("Hinweis", "Bitte die Person zuerst speichern.", "OK");
                return;
            }

            try
            {
                var mediaItem = await action();
                MediaItems.Insert(0, mediaItem);
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Fehler", ex.Message, "OK");
            }
        }
    }

    internal static class PeopleDetailTaskExtensions
    {
        public static void FireAndForget(this Task task)
        {
            task.ContinueWith(static t =>
            {
                if (t.Exception is not null)
                {
                    System.Diagnostics.Debug.WriteLine(t.Exception);
                }
            }, TaskScheduler.Default);
        }
    }
}
