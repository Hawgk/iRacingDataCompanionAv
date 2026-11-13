using IRDCav.BestLaps;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IRDCav.Services
{
    /// <summary>
    /// The player's all-time best lap with the current car at the current track, and the best lap
    /// of its class there. Loaded from the best lap store when the car or track changes, and saved
    /// whenever a faster lap is offered. Runs on the telemetry thread.
    /// </summary>
    internal sealed class PersonalBest
    {
        private readonly IBestLapStore _store;
        private CarAtTrack _car = CarAtTrack.Unknown;
        private IReadOnlyList<string> _carClasses = Array.Empty<string>();

        public PersonalBest(IBestLapStore store)
        {
            _store = store;
        }

        /// <summary>Seconds; 0 when there is none.</summary>
        public float LapTime { get; private set; }

        /// <summary>Sector times of the best lap; 0 for an unknown sector.</summary>
        public float[] Sectors { get; private set; } = SectorTimes.Unknown();

        /// <summary>The fastest lap of any car driven in the current class at this track; 0 when there is none.</summary>
        public float ClassBest { get; private set; }

        public void UseCar(CarAtTrack car)
        {
            _car = car;
            LapTime = 0;
            Sectors = SectorTimes.Unknown();
            _carClasses = Array.Empty<string>();
            if (car.IsKnown && _store.TryLoad(car, out BestLapRecord? record))
            {
                Apply(record);
            }
            ClassBest = car.IsKnown ? _store.ClassBest(car) : 0;
        }

        /// <summary>Takes over and saves a lap that beats the personal best; slower laps are ignored.</summary>
        public void Offer(float lapTime, float[] sectors)
        {
            if (lapTime <= 0 || (LapTime > 0 && lapTime >= LapTime))
            {
                return;
            }
            LapTime = lapTime;
            Sectors = sectors;
            ClassBest = ClassBest > 0 ? Math.Min(ClassBest, lapTime) : lapTime;
            if (_car.IsKnown)
            {
                _carClasses = WithCurrentClass();
                _store.Save(Record());
            }
        }

        private void Apply(BestLapRecord record)
        {
            LapTime = record.LapTime;
            Sectors = SectorTimes.FromSaved(record.Sectors);
            _carClasses = record.CarClasses;
            if (WithCurrentClass().Count > _carClasses.Count)
            {
                // Noted right away, so the class best finds this car from now on.
                _carClasses = WithCurrentClass();
                _store.Save(record with { CarClasses = _carClasses });
            }
        }

        private BestLapRecord Record()
        {
            return new BestLapRecord
            {
                Track = _car.Track,
                TrackConfig = _car.TrackConfig,
                Car = _car.Car,
                CarClasses = _carClasses,
                LapTime = LapTime,
                Sectors = Sectors,
                Recorded = DateTime.UtcNow,
            };
        }

        private IReadOnlyList<string> WithCurrentClass() =>
            _car.CarClass.Length == 0 || _carClasses.Contains(_car.CarClass)
                ? _carClasses
                : _carClasses.Append(_car.CarClass).ToArray();
    }
}
