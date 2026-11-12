using IRSDKSharper;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace IRDCav.Telemetry
{
    /// <summary>
    /// Read access to the current telemetry tick. Values are read lazily, so callers
    /// only pay for what they use. Only valid during the <see cref="ITelemetrySource.FrameReceived"/> callback.
    /// </summary>
    public interface ITelemetryFrame
    {
        InputSample ReadInputs();
        SessionTelemetry ReadSession();
        IReadOnlyList<CarTelemetry> ReadCars();
        bool TryGetSessionInfo([NotNullWhen(true)] out IRacingSdkSessionInfo? sessionInfo);
    }
}
