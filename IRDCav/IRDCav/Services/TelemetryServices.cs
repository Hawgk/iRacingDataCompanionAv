using IRDCav.Telemetry;

namespace IRDCav.Services
{
    internal sealed record TelemetryServices(
        ITelemetrySource Source,
        RaceDataController RaceData,
        FuelMonitor Fuel,
        SessionInfoBuilder SessionInfoBuilder,
        RelativeListBuilder RelativeBuilder,
        StandingsListBuilder StandingsBuilder);
}
