using Avalonia.Threading;
using IRDCav.Models;
using System;
using System.Collections.Generic;
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
        private static readonly TimeSpan RefreshInterval = TimeSpan.FromMilliseconds(33);

        // A calmer relative is easier to read at a glance; the data behind it is still computed at full rate.
        private const double RelativesUpdatesPerSecond = 4;

        private readonly OverlayStateStore _store;
        private readonly InputTraceBuffer _inputs;
        private readonly OverlayViewModels _viewModels;
        private readonly DispatcherTimer _timer;
        private readonly Stopwatch _clock = Stopwatch.StartNew();
        private readonly RefreshCadence _relativesCadence = new(RelativesUpdatesPerSecond);

        private OverlayState? _appliedState;
        private IReadOnlyList<RaceRow>? _appliedRelatives;
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
            RefreshState();
            RefreshRelatives();
            RefreshInputs();
        }

        private void RefreshState()
        {
            OverlayState state = _store.Current;
            if (ReferenceEquals(state, _appliedState))
            {
                return;
            }

            _appliedState = state;
            ApplyVisibility(state.ShowsOverlays);
            _viewModels.Results.SessionInfo = state.Session;
            _viewModels.Results.RaceDataList.Update(state.Standings);
            _viewModels.Fuel.SessionInfo = state.Session;
            _viewModels.Fuel.FuelData = state.Fuel;
        }

        private void RefreshRelatives()
        {
            TimeSpan now = _clock.Elapsed;
            IReadOnlyList<RaceRow> rows = _store.Current.Relatives;
            if (!_relativesCadence.IsDue(now) || ReferenceEquals(rows, _appliedRelatives))
            {
                return;
            }

            _appliedRelatives = rows;
            _relativesCadence.MarkUpdated(now);
            _viewModels.Relatives.RaceDataList.Update(rows);
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

        private void ApplyVisibility(bool isVisible)
        {
            _viewModels.Results.IsVisible = isVisible;
            _viewModels.Relatives.IsVisible = isVisible;
            _viewModels.Fuel.IsVisible = isVisible;
            _viewModels.InputGraph.IsVisible = isVisible;
        }
    }
}
