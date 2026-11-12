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
        // At 30 telemetry frames per second this is about 3.3 seconds of input history.
        private const int InputTraceSampleCount = 100;

        private TelemetryProcessor? _processor;
        private OverlayRefresher? _refresher;

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

                var viewModels = new OverlayViewModels(
                    new ResultsViewModel(), new RelativesViewModel(), new InputGraphViewModel(InputTraceSampleCount));
                CreateWindows(viewModels);

                var store = new OverlayStateStore();
                var inputs = new InputTraceBuffer(InputTraceSampleCount);
                _refresher = new OverlayRefresher(store, inputs, viewModels);
                _processor = CreateProcessor(store, inputs);

                desktop.Exit += (_, _) => OnExit();
                _refresher.Start();
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

        private void OnExit()
        {
            _processor?.Stop();
            _refresher?.Stop();
        }

        private static TelemetryProcessor CreateProcessor(IOverlayStatePublisher publisher, IInputSink inputs)
        {
            var services = new TelemetryServices(
                Source: new IRacingTelemetrySource(),
                RaceData: new RaceDataController(new IntervalCalculator()),
                Fuel: new FuelMonitor(new FuelCalculator()),
                SessionInfoBuilder: new SessionInfoBuilder(),
                RelativeBuilder: new RelativeListBuilder(RelativeRowCount),
                StandingsBuilder: new StandingsListBuilder(StandingsPlayerWindowSize, StandingsLeadersPerClass));

            return new TelemetryProcessor(services, publisher, inputs);
        }
    }
}
