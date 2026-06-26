using CommunityToolkit.Mvvm.Input;
using PersonaX.UI.Models;

namespace PersonaX.UI.PageModels
{
    public interface IProjectTaskPageModel
    {
        IAsyncRelayCommand<ProjectTask> NavigateToTaskCommand { get; }
        bool IsBusy { get; }
    }
}