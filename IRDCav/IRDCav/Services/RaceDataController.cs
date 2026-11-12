using IRDCav.Models;
using IRDCav.Telemetry;
using IRSDKSharper;
using System.Collections.Generic;
using System.Linq;
using static IRSDKSharper.IRacingSdkSessionInfo.DriverInfoModel;
using static IRSDKSharper.IRacingSdkSessionInfo.SessionInfoModel.SessionModel;

namespace IRDCav.Services
{
    /// <summary>Holds the merged state of all cars, indexed by carIdx.</summary>
    internal sealed class RaceDataController
    {
        private const double NoSessionTime = -1;

        private readonly IntervalCalculator _intervalCalculator;
        private readonly RaceDataModel[] _cars = new RaceDataModel[IRacingSdkConst.MaxNumCars];
        private RaceDataModel _player = new();
        private double _lastSessionTime = NoSessionTime;

        public RaceDataController(IntervalCalculator intervalCalculator)
        {
            _intervalCalculator = intervalCalculator;
            Clear();
        }

        public void Clear()
        {
            _lastSessionTime = NoSessionTime;
            for (int i = 0; i < _cars.Length; i++)
            {
                _cars[i] = new RaceDataModel();
            }
            _player = _cars[0];
        }

        public void SetPlayer(int carIdx)
        {
            _player.IsMe = false;
            _player = _cars[carIdx];
            _player.IsMe = true;
        }

        public void ApplyTelemetry(IEnumerable<CarTelemetry> cars)
        {
            foreach (CarTelemetry car in cars)
            {
                _cars[car.Id].ApplyTelemetry(car);
            }
        }

        public void ApplyPositions(IEnumerable<PositionModel> positions)
        {
            foreach (PositionModel position in positions)
            {
                _cars[position.CarIdx].ApplyPosition(position);
            }
        }

        public void ApplyDrivers(IEnumerable<DriverModel> drivers)
        {
            foreach (DriverModel driver in drivers)
            {
                _cars[driver.CarIdx].ApplyDriver(driver);
            }
        }

        /// <param name="sessionTime">iRacing "SessionTime" (seconds since session start, counts up).</param>
        public void Advance(double sessionTime)
        {
            if (_lastSessionTime != NoSessionTime)
            {
                // Negative or huge steps (session change, replay jump) are rejected by the
                // microsector model, which then simply restarts its current sector.
                _intervalCalculator.Update(_cars, _player, sessionTime - _lastSessionTime);
                MarkClassFastestLaps();
            }
            _lastSessionTime = sessionTime;
        }

        /// <summary>Copies of all cars that are safe to hand to another thread.</summary>
        public IReadOnlyList<RaceDataModel> Snapshot() => _cars.Select(car => car.Snapshot()).ToArray();

        private void MarkClassFastestLaps()
        {
            foreach (RaceDataModel car in _cars)
            {
                car.IsFastest = false;
            }

            var carsWithLapTimeByClass = _cars
                .Where(car => car.ClassStr != string.Empty && car.FastestLapTime > 0)
                .GroupBy(car => car.ClassStr);

            foreach (var classCars in carsWithLapTimeByClass)
            {
                classCars.MinBy(car => car.FastestLapTime)!.IsFastest = true;
            }
        }
    }
}
