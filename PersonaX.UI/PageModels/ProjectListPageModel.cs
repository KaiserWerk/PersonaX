using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PersonaX.UI.Data;
using PersonaX.UI.Models;
using PersonaX.UI.Services;

namespace PersonaX.UI.PageModels
{
    public partial class ProjectListPageModel : ObservableObject
    {
        private readonly ProjectRepository _projectRepository;
        private readonly ILockService _lockService;

        [ObservableProperty]
        private List<Project> _projects = [];

        [ObservableProperty]
        private Project? selectedProject;

        public ProjectListPageModel(ProjectRepository projectRepository, ILockService lockService)
        {
            _projectRepository = projectRepository;
            _lockService = lockService;
        }

        [RelayCommand]
        private async Task Appearing()
        {
            _lockService.NotifyUserActivity();
            Projects = await _projectRepository.ListAsync();
        }

        [RelayCommand]
        Task? NavigateToProject(Project project)
        {
            _lockService.NotifyUserActivity();
            return project is null ? Task.CompletedTask : Shell.Current.GoToAsync($"project?id={project.ID}");
        }

        [RelayCommand]
        async Task AddProject()
        {
            _lockService.NotifyUserActivity();
            await Shell.Current.GoToAsync($"project");
        }
    }
}