using IRDCav.ViewModels.Settings;
using System;

namespace IRDCav.Views.Settings
{
    /// <summary>Opens the settings window, or brings the open one to the front.</summary>
    internal sealed class SettingsWindowLauncher
    {
        private readonly Func<SettingsViewModel> _createViewModel;
        private SettingsWindow? _window;

        public SettingsWindowLauncher(Func<SettingsViewModel> createViewModel)
        {
            _createViewModel = createViewModel;
        }

        public void Show()
        {
            if (_window != null)
            {
                _window.Activate();
                return;
            }
            _window = new SettingsWindow { DataContext = _createViewModel() };
            _window.Closed += (_, _) => _window = null;
            _window.Show();
        }
    }
}
