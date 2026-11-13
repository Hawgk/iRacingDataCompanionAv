using IRDCav.Models;
using IRDCav.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IRDCav.UiTesting
{
    /// <summary>
    /// A race already in progress. Positions, intervals and fastest laps are derived from the
    /// distance each car has driven, so the field overtakes, laps and pits over time.
    /// </summary>
    internal sealed class SimulatedRace
    {
        public const double StartSeconds = 20 * 60;

        private readonly List<SimulatedCar> _cars;
        private readonly SimulatedCar _player;
        private readonly IRatingEstimator _iRatingEstimator;

        public SimulatedRace(IRatingEstimator iRatingEstimator)
        {
            _iRatingEstimator = iRatingEstimator;
            _cars = DemoRoster.Drivers.Select((driver, id) => new SimulatedCar(driver, id, StartSeconds)).ToList();
            _player = _cars.First(car => car.Model.IsMe);
            Cars = _cars.Select(car => car.Model).ToList();
            UpdateStandings();
        }

        public IReadOnlyCollection<RaceDataModel> Cars { get; }

        public double RaceSeconds { get; private set; } = StartSeconds;

        public RaceDataModel Player => _player.Model;

        public void Advance(double seconds)
        {
            RaceSeconds += seconds;
            foreach (SimulatedCar car in _cars)
            {
                car.Advance(seconds);
            }
            UpdateStandings();
        }

        private void UpdateStandings()
        {
            UpdatePositions();
            UpdateIntervals();
            MarkClassFastestLaps();
            _iRatingEstimator.Update(Cars, isRace: true);
        }

        private void UpdatePositions()
        {
            var carsPerClass = new Dictionary<string, int>();
            int position = 0;
            foreach (RaceDataModel car in _cars.OrderByDescending(car => car.Distance).Select(car => car.Model))
            {
                car.Position = ++position;
                car.ClassPosition = carsPerClass[car.ClassStr] = carsPerClass.GetValueOrDefault(car.ClassStr) + 1;
            }
        }

        /// <summary>Same sign convention as the live calculation: positive intervals are ahead of the player.</summary>
        private void UpdateIntervals()
        {
            foreach (SimulatedCar car in _cars)
            {
                car.Model.Interval = WrapToHalfLap(car.Model.LapDistPct - Player.LapDistPct) * (float)_player.ReferenceLapTime;
                car.Model.LapDelta = (int)Math.Round(car.Distance - _player.Distance);
            }
        }

        private static float WrapToHalfLap(float distance) => distance switch
        {
            >= 0.5f => distance - 1,
            < -0.5f => distance + 1,
            _ => distance,
        };

        private void MarkClassFastestLaps()
        {
            foreach (var classCars in _cars.Select(car => car.Model).GroupBy(car => car.ClassStr))
            {
                RaceDataModel fastest = classCars.MinBy(car => car.FastestLapTime)!;
                foreach (RaceDataModel car in classCars)
                {
                    car.IsFastest = car == fastest;
                }
            }
        }
    }
}
