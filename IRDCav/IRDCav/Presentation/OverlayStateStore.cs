using System.Threading;

namespace IRDCav.Presentation
{
    /// <summary>
    /// Holds only the latest published state. The producer never waits for the consumer and
    /// states the UI did not pick up in time are simply replaced, so nothing can queue up.
    /// </summary>
    public sealed class OverlayStateStore : IOverlayStatePublisher
    {
        private OverlayState _current = OverlayState.Disconnected;

        public OverlayState Current => Volatile.Read(ref _current);

        public void Publish(OverlayState state) => Volatile.Write(ref _current, state);
    }
}
