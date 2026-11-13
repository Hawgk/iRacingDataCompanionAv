using Avalonia.Threading;
using System;
using System.Diagnostics;

namespace IRDCav.Presentation
{
    /// <summary>
    /// Pulls the latest data into the view models at the UI's own pace. Runs on the UI thread
    /// at background priority, so rendering and input always come first, and does nothing
    /// when no new data has been published since the last tick.
    /// </summary>
    internal sealed class OverlayRefresher
    {
        // Every redraw of a window costs CPU, which iRacing needs more. The input trace still gets
        // every telemetry sample, only drawn in steps of two: 15 frames a second still read as motion.
        private static readonly TimeSpan RefreshInterval = TimeSpan.FromSeconds(1.0 / 15);

        // Lists and numbers are easier to read at a glance when they change calmly; the data behind
        // them is still computed at full rate.
        private const double DataUpdatesPerSecond = 4;

        private readonly OverlayStateStore _store;
        private readonly InputTraceBuffer _inputs;
        private readonly OverlayViewModels _viewModels;
        private readonly DispatcherTimer _timer;
        private readonly Stopwatch _clock = Stopwatch.StartNew();
        private readonly RefreshCadence _dataCadence = new(DataUpdatesPerSecond);

        private OverlayState? _appliedState;
        private long _appliedInputVersion = -1;

        public OverlayRefresher(OverlayStateStore store, InputTraceBuffer inputs, OverlayViewModels viewModels)
        {
            _store = store;
            _inputs = inputs;
            _viewModels = viewModels;
            _timer = new DispatcherTimer(RefreshInterval, DispatcherPriority.Background, (_, _) => Refresh());
        }

        public void Start() => _timer.Start();

        public void Stop() => _timer.Stop();

        private void Refresh()
        {
            OverlayState state = _store.Current;
            ApplyVisibility(state.ShowsOverlays);
            RefreshData(state);
            RefreshInputs();
        }

        private void RefreshData(OverlayState state)
        {
            TimeSpan now = _clock.Elapsed;
            if (!_dataCadence.IsDue(now) || ReferenceEquals(state, _appliedState))
            {
                return;
            }

            _appliedState = state;
            _dataCadence.MarkUpdated(now);
            _viewModels.Results.SessionInfo = state.Session;
            _viewModels.Results.RaceDataList.Update(state.Standings);
            _viewModels.Relatives.RaceDataList.Update(state.Relatives);
            _viewModels.Fuel.SessionInfo = state.Session;
            _viewModels.Fuel.FuelData = state.Fuel;
            _viewModels.LapSectors.Apply(state.LapSectors);
        }

        private void RefreshInputs()
        {
            long version = _inputs.Version;
            if (version != _appliedInputVersion && _viewModels.InputGraph.IsVisible)
            {
                _appliedInputVersion = version;
                _viewModels.InputGraph.SetSamples(_inputs.Snapshot());
            }
        }

        /// <summary>Follows every tick, so the overlays appear and disappear without delay. Unchanged values are ignored.</summary>
        private void ApplyVisibility(bool isVisible)
        {
            _viewModels.Results.IsVisible = isVisible;
            _viewModels.Relatives.IsVisible = isVisible;
            _viewModels.Fuel.IsVisible = isVisible;
            _viewModels.InputGraph.IsVisible = isVisible;
            _viewModels.LapSectors.IsVisible = isVisible;
        }
    }
}
