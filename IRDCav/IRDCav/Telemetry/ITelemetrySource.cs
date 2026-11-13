using System;

namespace IRDCav.Telemetry
{
    /// <summary>
    /// Source of live sim data. Events are raised on the source's own background thread.
    /// </summary>
    public interface ITelemetrySource
    {
        event Action? Connected;
        event Action? Disconnected;
        event Action? Stopped;
        event Action<ITelemetryFrame>? FrameReceived;

        void Start();
        void Stop();
    }
}
