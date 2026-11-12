using Avalonia.Threading;
using System;

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

        private readonly OverlayStateStore _store;
        private readonly InputTraceBuffer _inputs;
        private readonly OverlayViewModels _viewModels;
        private readonly DispatcherTimer _timer;

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
            RefreshState();
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
            ApplyVisibility(state.IsConnected);
            _viewModels.Results.SessionInfo = state.Session;
            _viewModels.Results.RaceDataList.Update(state.Standings);
            _viewModels.Relatives.SessionInfo = state.Session;
            _viewModels.Relatives.FuelData = state.Fuel;
            _viewModels.Relatives.RaceDataList.Update(state.Relatives);
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
            _viewModels.InputGraph.IsVisible = isVisible;
        }
    }
}
