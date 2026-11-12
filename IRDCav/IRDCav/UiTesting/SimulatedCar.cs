using IRDCav.Models;
using System;

namespace IRDCav.UiTesting
{
    /// <summary>One car driving laps at a pace derived from its class and the driver's rating.</summary>
    internal sealed class SimulatedCar
    {
        private const float RatingPaceFactor = 0.00001f;
        private const int RatingReference = 3000;
        private const double LapTimeVariation = 0.01;
        private const int PitStopLapInterval = 5;
        private const float PitLaneLength = 0.04f;

        private readonly Random _random;
        private readonly double _baseLapTime;
        private double _lapTime;
        private double _timeOnLap;

        public SimulatedCar(DemoDriver driver, int id, double raceSeconds)
        {
            _random = new Random(id);
            _baseLapTime = driver.CarClass.LapTime * (1 + (RatingReference - driver.IRating) * RatingPaceFactor);
            _lapTime = NextLapTime();
            Model = CreateModel(driver, id);
            Distance = raceSeconds / _baseLapTime + _random.NextDouble() * 0.05;
            Model.LastLapTime = (float)NextLapTime();
            Model.FastestLapTime = (float)(_baseLapTime * (1 - LapTimeVariation / 2));
            UpdateModel();
        }

        public RaceDataModel Model { get; }

        /// <summary>Laps driven since the start, including the fraction of the current lap.</summary>
        public double Distance { get; private set; }

        public double ReferenceLapTime => _baseLapTime;

        public void Advance(double seconds)
        {
            _timeOnLap += seconds;
            Distance += seconds / _lapTime;
            if ((int)Distance > Model.Lap)
            {
                CompleteLap();
            }
            UpdateModel();
        }

        private void CompleteLap()
        {
            double overshoot = (Distance - Math.Floor(Distance)) * _lapTime;
            float lapTime = (float)(_timeOnLap - overshoot);
            Model.LastLapTime = lapTime;
            Model.FastestLapTime = Math.Min(Model.FastestLapTime, lapTime);
            _timeOnLap = overshoot;
            _lapTime = NextLapTime();
        }

        private void UpdateModel()
        {
            Model.Lap = (int)Distance;
            Model.LapsCompleted = Model.Lap;
            Model.LapDistPct = (float)(Distance - Model.Lap);
            Model.OnPitRoad = IsInPitLane();
        }

        /// <summary>Every car pits every few laps, staggered by id, so pit styling is visible regularly.</summary>
        private bool IsInPitLane()
        {
            bool isPitLap = Model.Lap % PitStopLapInterval == Model.Id % PitStopLapInterval;
            return isPitLap && (Model.LapDistPct < PitLaneLength || Model.LapDistPct > 1 - PitLaneLength);
        }

        private double NextLapTime() => _baseLapTime * (1 + (_random.NextDouble() - 0.5) * LapTimeVariation);

        private static double SafetyRatingOf(DemoDriver driver) => 1.5 + driver.IRating % 287 / 100.0;

        private static RaceDataModel CreateModel(DemoDriver driver, int id) => new()
        {
            Id = id,
            IsMe = driver.Name == DemoRoster.PlayerName,
            ConsiderForRelative = true,
            Name = driver.Name,
            CarNumber = driver.CarNumber,
            ClassStr = driver.CarClass.Name,
            ClassColor = driver.CarClass.Color,
            License = ((float)driver.IRating / 1000).ToString("0.0") + "k",
            SafetyRating = $"{driver.License.Class} {SafetyRatingOf(driver):0.00}",
            LicenseColor = driver.License.Color,
            EstLapTime = driver.CarClass.LapTime,
        };
    }
}
