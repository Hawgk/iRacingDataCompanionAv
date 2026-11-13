using IRDCav.Telemetry;

namespace IRDCav.Services
{
    internal sealed record TelemetryServices(
        ITelemetrySource Source,
        RaceDataController RaceData,
        IRatingEstimator IRatingEstimator,
        FuelMonitor Fuel,
        SessionInfoBuilder SessionInfoBuilder,
        RelativeListBuilder RelativeBuilder,
        StandingsListBuilder StandingsBuilder);
}
