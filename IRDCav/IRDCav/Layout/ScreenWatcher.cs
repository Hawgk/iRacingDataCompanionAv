using Avalonia.Controls;
using Avalonia.Threading;
using System;
using System.Linq;

namespace IRDCav.Layout
{
    /// <summary>
    /// Raises <see cref="Changed"/> when monitors are added, removed, rearranged or rescaled.
    /// Avalonia 11.1 has no event for this, so the configuration is compared every few seconds,
    /// which costs next to nothing.
    /// </summary>
    public sealed class ScreenWatcher
    {
        private static readonly TimeSpan CheckInterval = TimeSpan.FromSeconds(2);

        private readonly Screens _screens;
        private readonly DispatcherTimer _timer;
        private string _configuration;

        public ScreenWatcher(Screens screens)
        {
            _screens = screens;
            _configuration = DescribeConfiguration();
            _timer = new DispatcherTimer(CheckInterval, DispatcherPriority.Background, (_, _) => Check());
        }

        public event Action? Changed;

        public void Start() => _timer.Start();

        public void Stop() => _timer.Stop();

        private void Check()
        {
            string configuration = DescribeConfiguration();
            if (configuration != _configuration)
            {
                _configuration = configuration;
                Changed?.Invoke();
            }
        }

        private string DescribeConfiguration() =>
            string.Join(";", _screens.All.Select(screen => $"{screen.Bounds}@{screen.Scaling}"));
    }
}
