using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.Input;
using Avalonia;

namespace IRDCav.ViewModels
{
    public partial class ApplicationViewModel : ViewModelBase
    {
        [RelayCommand]
        private static void Exit()
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime application)
            {
                application.Shutdown();
            }
        }
    }
}
