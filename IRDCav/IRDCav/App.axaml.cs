using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using IRDCav.LapFiles;
using IRDCav.Configuration;
using IRDCav.Layout;
using IRDCav.Presentation;
using IRDCav.Services;
using IRDCav.Telemetry;
using IRDCav.UiTesting;
using IRDCav.ViewModels;
using IRDCav.Views;
using IRDCav.ViewModels.Settings;
using IRDCav.Views.Settings;
using IRDCav.Views.Theme;
using System;
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
                OverlayViewModels viewModels = CreateViewModels();
                LiveOverlayConfiguration configuration = LoadConfiguration(viewModels);
                var settings = new SettingsWindowLauncher(() => new SettingsViewModel(configuration, OverlayFonts.AvailableNames()));
                DataContext = new ApplicationViewModel(settings.Show);
                CreateWindows(viewModels, settings.Show);

                var store = new OverlayStateStore();
                var inputs = new InputTraceBuffer(InputTraceSampleCount);
                _refresher = new OverlayRefresher(store, inputs, viewModels);
                _feed = StartupOptions.Parse(desktop.Args ?? Array.Empty<string>()).IsUiTestingMode
                    ? CreateUiTestFeed(store, inputs)
                    : CreateProcessor(store, inputs);

                desktop.Exit += (_, _) => OnExit();
                _screenWatcher?.Start();
                _refresher.Start();
                _feed.Start();
            }

            base.OnFrameworkInitializationCompleted();
        }

        /// <summary>Applies config.json to the theme and view models; must run before the windows are created.</summary>
        private LiveOverlayConfiguration LoadConfiguration(OverlayViewModels viewModels)
        {
            var file = new JsonOverlayConfigurationFile(JsonOverlayConfigurationFile.DefaultFilePath);
            file.CreateIfMissing();
            OverlayTheme.AddTabularDigits(this);
            var configuration = new LiveOverlayConfiguration(file, new OverlayConfigurationApplier(this, viewModels));
            configuration.Load(file.Read());
            return configuration;
        }

        private static OverlayViewModels CreateViewModels() => new(
            new ResultsViewModel(),
            new RelativesViewModel(),
            new FuelViewModel(),
            new InputGraphViewModel(InputTraceSampleCount),
            new LapSectorsViewModel());

        // Windows show and hide themselves through their IsVisible binding.
        private void CreateWindows(OverlayViewModels viewModels, Action openSettings)
        {
            var results = new ResultsWindow { DataContext = viewModels.Results };
            var relatives = new RelativesWindow { DataContext = viewModels.Relatives };
            var fuel = new FuelWindow { DataContext = viewModels.Fuel };
            var inputGraph = new InputGraphWindow { DataContext = viewModels.InputGraph };
            var lapSectors = new LapSectorsWindow { DataContext = viewModels.LapSectors };

            _screenWatcher = new ScreenWatcher(results.Screens);
            var layout = new OverlayLayoutContext(new JsonOverlayLayoutStore(JsonOverlayLayoutStore.DefaultFilePath), _screenWatcher);
            OverlayWindowBehavior.Attach(results, DefaultLayouts.Results, layout);
            OverlayWindowBehavior.Attach(relatives, DefaultLayouts.Relatives, layout);
            OverlayWindowBehavior.Attach(fuel, DefaultLayouts.Fuel, layout);
            OverlayWindowBehavior.Attach(inputGraph, DefaultLayouts.InputGraph, layout);
            OverlayWindowBehavior.Attach(lapSectors, DefaultLayouts.LapSectors, layout);
            foreach (Window window in new Window[] { results, relatives, fuel, inputGraph, lapSectors })
            {
                SettingsHotkey.Attach(window, openSettings);
            }
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
                IRatingEstimator: new IRatingEstimator(),
                Fuel: new FuelMonitor(new FuelCalculator()),
                SessionInfoBuilder: new SessionInfoBuilder(),
                RelativeBuilder: CreateRelativeBuilder(),
                StandingsBuilder: CreateStandingsBuilder(),
                LapSectors: CreateLapSectorTracker());

            return new TelemetryProcessor(services, publisher, inputs);
        }

        /// <summary>Simulated race instead of iRacing, to check the overlays without the sim running.</summary>
        private static UiTestFeed CreateUiTestFeed(IOverlayStatePublisher publisher, IInputSink inputs)
        {
            var race = new SimulatedRace(new IRatingEstimator());
            var scenario = new UiTestScenario(race, CreateRelativeBuilder(), CreateStandingsBuilder());
            return new UiTestFeed(scenario, publisher, inputs);
        }

        /// <summary>The best laps and their sectors come from the lap files iRacing keeps.</summary>
        private static LapSectorTracker CreateLapSectorTracker() =>
            new(new ReferenceLap(new IRacingLapFileLibrary(IRacingLapFileLibrary.DefaultDirectory)), new SectorClock(), new RecentLapAverage());

        private static RelativeListBuilder CreateRelativeBuilder() => new(RelativeRowCount);

        private static StandingsListBuilder CreateStandingsBuilder() =>
            new(StandingsPlayerWindowSize, StandingsLeadersPerClass);
    }
}
