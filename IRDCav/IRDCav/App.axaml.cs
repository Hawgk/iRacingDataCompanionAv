using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using IRDCav.Presentation;
using IRDCav.Services;
using IRDCav.Telemetry;
using IRDCav.ViewModels;
using IRDCav.Views;
using System.Text;

namespace IRDCav
{
    /// <summary>Composition root: wires telemetry, services, view models and windows together.</summary>
    public partial class App : Application
    {
        private const int RelativeRowCount = 9;
        private const int StandingsPlayerWindowSize = 11;
        private const int StandingsLeadersPerClass = 3;

        private TelemetryProcessor? _processor;

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // Avoid duplicate validations from both Avalonia and the CommunityToolkit.
                BindingPlugins.DataValidators.RemoveAt(0);
                desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;
                DataContext = new ApplicationViewModel();

                var viewModels = new OverlayViewModels(new ResultsViewModel(), new RelativesViewModel(), new InputGraphViewModel());
                CreateWindows(viewModels);

                _processor = CreateProcessor(viewModels);
                desktop.Exit += (_, _) => _processor.Stop();
                _processor.Start();
            }

            base.OnFrameworkInitializationCompleted();
        }

        // Windows show and hide themselves through their IsVisible binding.
        private static void CreateWindows(OverlayViewModels viewModels)
        {
            _ = new ResultsWindow { DataContext = viewModels.Results };
            _ = new RelativesWindow { DataContext = viewModels.Relatives };
            _ = new InputGraphWindow { DataContext = viewModels.InputGraph };
        }

        private static TelemetryProcessor CreateProcessor(OverlayViewModels viewModels)
        {
            var services = new TelemetryServices(
                Source: new IRacingTelemetrySource(),
                RaceData: new RaceDataController(new IntervalCalculator()),
                Fuel: new FuelMonitor(new FuelCalculator()),
                SessionInfoBuilder: new SessionInfoBuilder(),
                RelativeBuilder: new RelativeListBuilder(RelativeRowCount),
                StandingsBuilder: new StandingsListBuilder(StandingsPlayerWindowSize, StandingsLeadersPerClass));

            return new TelemetryProcessor(services, new OverlayPresenter(new AvaloniaUiDispatcher(), viewModels));
        }
    }
}
