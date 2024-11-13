using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using IRDCav.ViewModels;
using IRDCav.Views;

namespace IRDCav
{
    public partial class App : Application
    {
        DataCollector _dc = null;
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public void OnExit(object sender, ControlledApplicationLifetimeExitEventArgs e)
        {
            _dc.Terminate();
        }

        public override void OnFrameworkInitializationCompleted()
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // Line below is needed to remove Avalonia data validation.
                // Without this line you will get duplicate validations from both Avalonia and CT
                BindingPlugins.DataValidators.RemoveAt(0);
                desktop.Exit += OnExit;

                DataViewModel standingViewModel = new DataViewModel();
                RelativesViewModel relativesViewModel = new RelativesViewModel();

                _dc = new DataCollector(standingViewModel, relativesViewModel);

                desktop.MainWindow = new ResultsWindow
                {
                    DataContext = standingViewModel,
                };

                RelativesWindow relativesWindow = new RelativesWindow
                {
                    DataContext = relativesViewModel,
                };
                relativesWindow.Show();
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}