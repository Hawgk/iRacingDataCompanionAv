using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using IRDCav.ViewModels;
using IRDCav.Views;

namespace IRDCav
{
    public partial class App : Application
    {
        DataCollector? _dc = null;
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public void OnExit(object sender, ControlledApplicationLifetimeExitEventArgs e)
        {
            _dc?.Terminate();
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
                desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;
                DataContext = new ApplicationViewModel();

                DataViewModel standingViewModel = new DataViewModel();
                RelativesViewModel relativesViewModel = new RelativesViewModel();
                InputGraphViewModel inputGraphViewModel = new InputGraphViewModel();

                _dc = new DataCollector(standingViewModel, relativesViewModel, inputGraphViewModel);

                ResultsWindow resultsWindow = new ResultsWindow
                {
                    DataContext = standingViewModel,
                };

                RelativesWindow relativesWindow = new RelativesWindow
                {
                    DataContext = relativesViewModel,
                };

                InputGraphWindow inputGraphWindow = new InputGraphWindow
                {
                    DataContext = inputGraphViewModel,
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}