using IRDCav.Models;
using IRDCav.Telemetry;

namespace IRDCav.Services
{
    /// <summary>
    /// Detects driving stints and completed laps of the player and feeds the fuel calculator.
    /// </summary>
    // TODO: Instant resets are not detected as the car stays registered as on track.
    internal sealed class FuelMonitor
    {
        private readonly FuelCalculator _calculator;
        private int _lastLap;

        public FuelMonitor(FuelCalculator calculator)
        {
            _calculator = calculator;
        }

        public bool IsDriving { get; private set; }

        public FuelDataModel Current => _calculator.Current;

        public void Update(SessionTelemetry session, int playerLap)
        {
            if (!session.IsOnTrack)
            {
                IsDriving = false;
                return;
            }

            if (!IsDriving)
            {
                StartStint(session.FuelLevel, playerLap);
            }
            RecordFuel(session, playerLap);
        }

        public void Reset() => _calculator.Reset();

        private void StartStint(float fuelLevel, int playerLap)
        {
            _calculator.StartStint(fuelLevel);
            _lastLap = playerLap;
            IsDriving = true;
        }

        private void RecordFuel(SessionTelemetry session, int playerLap)
        {
            bool isLapCompleted = playerLap > _lastLap;
            _lastLap = playerLap;

            if (isLapCompleted)
            {
                _calculator.CompleteLap(session.FuelLevel, session.TimeRemain);
            }
            else
            {
                _calculator.AddSample(session.FuelLevel, session.TimeRemain);
            }
        }
    }
}
