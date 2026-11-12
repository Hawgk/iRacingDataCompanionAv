using IRSDKSharper;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace IRDCav.Telemetry
{
    /// <summary>
    /// Read access to the current telemetry tick. Values are read lazily, so callers
    /// only pay for what they use. Only valid during the <see cref="ITelemetrySource.FrameReceived"/> callback;
    /// the list returned by <see cref="ReadCars"/> is reused and overwritten by the next call.
    /// </summary>
    public interface ITelemetryFrame
    {
        InputSample ReadInputs();
        SessionTelemetry ReadSession();
        IReadOnlyList<CarTelemetry> ReadCars();
        bool TryGetSessionInfo([NotNullWhen(true)] out IRacingSdkSessionInfo? sessionInfo);

        /// <summary>Increases whenever the session info YAML changes; derived data can be cached until then.</summary>
        int SessionInfoVersion { get; }
    }
}
