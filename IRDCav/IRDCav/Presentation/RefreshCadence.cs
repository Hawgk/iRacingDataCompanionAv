using System;

namespace IRDCav.Presentation
{
    /// <summary>
    /// Limits how often a view is updated. Due times follow a fixed grid, so a timer that ticks
    /// slightly late still reaches the full rate on average. An update more than half an interval
    /// late restarts the grid, so two updates never follow each other closely.
    /// </summary>
    internal sealed class RefreshCadence
    {
        private readonly TimeSpan _interval;
        private TimeSpan _nextDue = TimeSpan.Zero;

        public RefreshCadence(double updatesPerSecond)
        {
            _interval = TimeSpan.FromSeconds(1 / updatesPerSecond);
        }

        public bool IsDue(TimeSpan now) => now >= _nextDue;

        public void MarkUpdated(TimeSpan now)
        {
            TimeSpan next = _nextDue + _interval;
            _nextDue = next - now >= _interval / 2 ? next : now + _interval;
        }
    }
}
