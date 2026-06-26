using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PersonaX.UI.Models;

namespace PersonaX.UI.PageModels
{
    public partial class MainPageModel : ObservableObject, IProjectTaskPageModel
    {
        private bool _isNavigatedTo;
        private bool _dataLoaded;
        private readonly ProjectRepository _projectRepository;
        private readonly TaskRepository _taskRepository;
        private readonly CategoryRepository _categoryRepository;
        private readonly ModalErrorHandler _errorHandler;
        private readonly SeedDataService _seedDataService;
        private readonly ILockService _lockService;

        [ObservableProperty]
        private List<CategoryChartData> _todoCategoryData = [];

        [ObservableProperty]
        private List<Brush> _todoCategoryColors = [];

        [ObservableProperty]
        private List<ProjectTask> _tasks = [];

        [ObservableProperty]
        private List<Project> _projects = [];

        [ObservableProperty]
        bool _isBusy;

        [ObservableProperty]
        bool _isRefreshing;

        [ObservableProperty]
        private string _today = DateTime.Now.ToString("dddd, MMM d");

        [ObservableProperty]
        private Project? selectedProject;

        public bool HasCompletedTasks
            => Tasks?.Any(t => t.IsCompleted) ?? false;

        public MainPageModel(SeedDataService seedDataService, ProjectRepository projectRepository,
            TaskRepository taskRepository, CategoryRepository categoryRepository, ModalErrorHandler errorHandler, ILockService lockService)
        {
            _projectRepository = projectRepository;
            _taskRepository = taskRepository;
            _categoryRepository = categoryRepository;
            _errorHandler = errorHandler;
            _seedDataService = seedDataService;
            _lockService = lockService;
        }

        private async Task LoadData()
        {
            try
            {
                IsBusy = true;

                Projects = await _projectRepository.ListAsync();

                var chartData = new List<CategoryChartData>();
                var chartColors = new List<Brush>();

                var categories = await _categoryRepository.ListAsync();
                foreach (var category in categories)
                {
                    chartColors.Add(category.ColorBrush);

                    var ps = Projects.Where(p => p.CategoryID == category.ID).ToList();
                    int tasksCount = ps.SelectMany(p => p.Tasks).Count();

                    chartData.Add(new(category.Title, tasksCount));
                }

                TodoCategoryData = chartData;
                TodoCategoryColors = chartColors;

                Tasks = await _taskRepository.ListAsync();
            }
            finally
            {
                IsBusy = false;
                OnPropertyChanged(nameof(HasCompletedTasks));
            }
        }

        private async Task InitData(SeedDataService seedDataService)
        {
            bool isSeeded = Preferences.Default.ContainsKey("is_seeded");

            if (!isSeeded)
            {
                await seedDataService.LoadSeedDataAsync();
            }

            Preferences.Default.Set("is_seeded", true);
            await Refresh();
        }

        [RelayCommand]
        private async Task Refresh()
        {
            _lockService.NotifyUserActivity();
            try
            {
                IsRefreshing = true;
                await LoadData();
            }
            catch (Exception e)
            {
                _errorHandler.HandleError(e);
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        [RelayCommand]
        private void NavigatedTo() =>
        {
            _lockService.NotifyUserActivity();
            _isNavigatedTo = true;
        }

        [RelayCommand]
        private void NavigatedFrom() =>
            _isNavigatedTo = false;

        [RelayCommand]
        private async Task Appearing()
        {
            _lockService.NotifyUserActivity();
            if (!_dataLoaded)
            {
                await InitData(_seedDataService);
                _dataLoaded = true;
                await Refresh();
            }
            // This means we are being navigated to
            else if (!_isNavigatedTo)
            {
                await Refresh();
            }
        }

        [RelayCommand]
        private Task TaskCompleted(ProjectTask task)
        {
            _lockService.NotifyUserActivity();
            OnPropertyChanged(nameof(HasCompletedTasks));
            return _taskRepository.SaveItemAsync(task);
        }

        [RelayCommand]
        private Task AddTask()
        {
            _lockService.NotifyUserActivity();
            return Shell.Current.GoToAsync($"task");
        }

        [RelayCommand]
        private Task? NavigateToProject(Project project)
        {
            _lockService.NotifyUserActivity();
            return project is null ? null : Shell.Current.GoToAsync($"project?id={project.ID}");
        }

        [RelayCommand]
        private Task NavigateToTask(ProjectTask task)
        {
            _lockService.NotifyUserActivity();
            return Shell.Current.GoToAsync($"task?id={task.ID}");
        }

        [RelayCommand]
        private async Task CleanTasks()
        {
            _lockService.NotifyUserActivity();
            var completedTasks = Tasks.Where(t => t.IsCompleted).ToList();
            foreach (var task in completedTasks)
            {
                await _taskRepository.DeleteItemAsync(task);
                Tasks.Remove(task);
            }

            OnPropertyChanged(nameof(HasCompletedTasks));
            Tasks = new(Tasks);
            await AppShell.DisplayToastAsync("All cleaned up!");
        }
    }
}