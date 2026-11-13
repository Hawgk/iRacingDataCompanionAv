using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.Input;
using System;

namespace IRDCav.ViewModels
{
    /// <summary>The tray menu.</summary>
    public partial class ApplicationViewModel : ViewModelBase
    {
        private readonly Action _openSettings;

        public ApplicationViewModel(Action openSettings)
        {
            _openSettings = openSettings;
        }

        [RelayCommand]
        private void OpenSettings() => _openSettings();

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
