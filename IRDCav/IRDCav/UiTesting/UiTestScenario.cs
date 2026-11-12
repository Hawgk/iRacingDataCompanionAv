using IRDCav.Presentation;
using IRDCav.Services;
using IRDCav.Telemetry;

namespace IRDCav.UiTesting
{
    /// <summary>
    /// Turns the simulated race into overlay states. The rows are shaped by the same list
    /// builders as live data, so the overlays show exactly what they would show in a race.
    /// </summary>
    internal sealed class UiTestScenario
    {
        private readonly SimulatedRace _race = new();
        private readonly RelativeListBuilder _relativeBuilder;
        private readonly StandingsListBuilder _standingsBuilder;

        public UiTestScenario(RelativeListBuilder relativeBuilder, StandingsListBuilder standingsBuilder)
        {
            _relativeBuilder = relativeBuilder;
            _standingsBuilder = standingsBuilder;
        }

        public void Advance(double seconds) => _race.Advance(seconds);

        public InputSample CurrentInput() => InputPattern.At(_race.RaceSeconds);

        public OverlayState CurrentState()
        {
            var session = SimulatedSession.Info(_race);
            return new OverlayState
            {
                IsConnected = true,
                Session = session,
                Fuel = SimulatedSession.Fuel(_race),
                Relatives = _relativeBuilder.Build(_race.Cars),
                Standings = _standingsBuilder.Build(_race.Cars, session.SessionType),
            };
        }
    }
}
