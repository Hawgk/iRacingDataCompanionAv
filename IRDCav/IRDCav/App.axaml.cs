using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using IRDCav.Layout;
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

        private IOverlayDataFeed? _feed;
        private OverlayRefresher? _refresher;
        private ScreenWatcher? _screenWatcher;

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
                _feed = CreateProcessor(store, inputs);

                desktop.Exit += (_, _) => OnExit();
                _screenWatcher?.Start();
                _refresher.Start();
                _feed.Start();
            }

            base.OnFrameworkInitializationCompleted();
        }

        // Windows show and hide themselves through their IsVisible binding.
        private void CreateWindows(OverlayViewModels viewModels)
        {
            var results = new ResultsWindow { DataContext = viewModels.Results };
            var relatives = new RelativesWindow { DataContext = viewModels.Relatives };
            var inputGraph = new InputGraphWindow { DataContext = viewModels.InputGraph };

            _screenWatcher = new ScreenWatcher(results.Screens);
            var layout = new OverlayLayoutContext(new JsonOverlayLayoutStore(JsonOverlayLayoutStore.DefaultFilePath), _screenWatcher);
            OverlayWindowBehavior.Attach(results, DefaultLayouts.Results, layout);
            OverlayWindowBehavior.Attach(relatives, DefaultLayouts.Relatives, layout);
            OverlayWindowBehavior.Attach(inputGraph, DefaultLayouts.InputGraph, layout);
        }

        private void OnExit()
        {
            _feed?.Stop();
            _refresher?.Stop();
            _screenWatcher?.Stop();
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
