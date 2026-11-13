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
        private IReadOnlyDictionary<int, string> _tireCompoundNames = new Dictionary<int, string>();
        private double _lastSessionTime = NoSessionTime;
        private int _microSectorCount = MicroSectorModel.MinCount;

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
                _cars[i] = new RaceDataModel { MicroSectors = new MicroSectorModel(_microSectorCount) };
            }
            _player = _cars[0];
        }

        /// <summary>
        /// Splits the lap into microsectors of about equal length on every track. A new count starts
        /// all cars' microsectors afresh; the same track keeps them, also across sessions.
        /// </summary>
        public void UseTrackLength(double trackLengthMeters)
        {
            int count = MicroSectorModel.CountFor(trackLengthMeters);
            if (count == _microSectorCount)
            {
                return;
            }
            _microSectorCount = count;
            foreach (RaceDataModel car in _cars)
            {
                car.MicroSectors = new MicroSectorModel(count);
            }
        }

        public void SetPlayer(int carIdx)
        {
            _player.IsMe = false;
            _player = _cars[carIdx];
            _player.IsMe = true;
        }

        public void ApplyTelemetry(IReadOnlyList<CarTelemetry> cars)
        {
            for (int i = 0; i < cars.Count; i++)
            {
                CarTelemetry car = cars[i];
                _cars[car.Id].ApplyTelemetry(car);
                _cars[car.Id].TireCompound = TireCompoundName(car.TireCompound);
            }
        }

        /// <summary>
        /// iRacing lists the compounds of the player's car only; other classes may number
        /// their compounds differently.
        /// </summary>
        public void ApplyTireCompounds(IEnumerable<DriverTireModel> tires) =>
            _tireCompoundNames = tires
                .Where(tire => !string.IsNullOrEmpty(tire.TireCompoundType))
                .GroupBy(tire => tire.TireIndex)
                .ToDictionary(group => group.Key, group => group.First().TireCompoundType);

        /// <summary>The compound's name, its index when the name is unknown, or empty when the car reports none.</summary>
        private string TireCompoundName(int index) => index switch
        {
            < 0 => string.Empty,
            _ => _tireCompoundNames.TryGetValue(index, out string? name) ? name : index.ToString(),
        };

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

        /// <summary>Live car state. Only valid on the telemetry thread; never hand it to the UI.</summary>
        public IReadOnlyCollection<RaceDataModel> Cars => _cars;
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
