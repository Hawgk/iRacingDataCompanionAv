using IRDCav.BestLaps;
using IRDCav.Models;
using IRDCav.Telemetry;
using System;
using System.Linq;

namespace IRDCav.Services
{
    /// <summary>
    /// Follows the player's laps in equal sectors built from the microsectors and compares them with
    /// the personal best of the car at this track. A lap counts once iRacing reports it as a new
    /// session best, so laps iRacing does not count are ignored; the lap that crossed the line
    /// close to that report is offered to the personal best. Each finished sector gets a delta,
    /// which stays until the car enters that sector again. Runs on the telemetry thread.
    /// </summary>
    internal sealed class LapSectorTracker
    {
        // iRacing reports a new best lap a moment after the line; a lap end and a new best this close belong together.
        private const double LapMatchSeconds = 5;
        private const double Never = double.NegativeInfinity;

        private readonly PersonalBest _personalBest;
        private CarAtTrack _car = CarAtTrack.Unknown;
        private MicroSectorModel? _source;
        private int _completedLaps;
        private float _reportedBest = float.NaN;
        private double _lapEndedAt = Never;
        private double _bestChangedAt = Never;
        private float[] _lastLapSectors = SectorTimes.Unknown();
        private readonly float[] _shownDeltas = SectorTimes.NoDeltas();

        public LapSectorTracker(PersonalBest personalBest)
        {
            _personalBest = personalBest;
        }

        /// <summary>Loads the personal best when the car or the track changes.</summary>
        public void UseCar(CarAtTrack car)
        {
            if (car == _car)
            {
                return;
            }
            _car = car;
            _personalBest.UseCar(car);
            Array.Fill(_shownDeltas, float.NaN);
        }

        /// <summary>Forgets the session's laps and the deltas, e.g. for a new session or connection; keeps the personal best.</summary>
        public void Reset()
        {
            _source = null;
            _reportedBest = float.NaN;
            _lapEndedAt = Never;
            _bestChangedAt = Never;
            Array.Fill(_shownDeltas, float.NaN);
        }

        public LapSectorsModel Update(MicroSectorModel playerSectors, SessionTelemetry session)
        {
            Follow(playerSectors);
            ObserveLapEnd(playerSectors, session.SessionTime);
            ObserveSessionBest(session.BestLapTime, session.SessionTime);
            OfferSessionBest(session.SessionTime);
            UpdateShownDeltas(playerSectors);
            return BuildModel();
        }

        /// <summary>A different car or a fresh model starts counting laps anew.</summary>
        private void Follow(MicroSectorModel playerSectors)
        {
            if (!ReferenceEquals(playerSectors, _source))
            {
                _source = playerSectors;
                _completedLaps = playerSectors.CompletedLaps;
            }
        }

        /// <summary>The last sector ends with the lap, so its delta comes from the lap just finished.</summary>
        private void ObserveLapEnd(MicroSectorModel playerSectors, double now)
        {
            if (playerSectors.CompletedLaps == _completedLaps)
            {
                return;
            }
            _completedLaps = playerSectors.CompletedLaps;
            _lastLapSectors = SectorTimes.Of(playerSectors.LastLap);
            ShowKnownDeltas(SectorTimes.Deltas(_lastLapSectors, _personalBest.Sectors));
            _lapEndedAt = now;
        }

        /// <summary>A session best already set when the app connects counts too, though without its sectors.</summary>
        private void ObserveSessionBest(float bestLapTime, double now)
        {
            float reported = Math.Max(bestLapTime, 0);
            if (reported == _reportedBest)
            {
                return;
            }
            _reportedBest = reported;
            _bestChangedAt = reported > 0 ? now : Never;
        }

        /// <summary>Offers the lap that ended close to the new session best, or only its time if no lap did.</summary>
        private void OfferSessionBest(double now)
        {
            if (_bestChangedAt == Never)
            {
                return;
            }
            if (Math.Abs(_lapEndedAt - _bestChangedAt) <= LapMatchSeconds)
            {
                _personalBest.Offer(_reportedBest, _lastLapSectors);
                _lapEndedAt = Never;
                _bestChangedAt = Never;
            }
            else if (now - _bestChangedAt > LapMatchSeconds)
            {
                _personalBest.Offer(_reportedBest, SectorTimes.Unknown());
                _bestChangedAt = Never;
            }
        }

        /// <summary>Finished sectors of the lap in progress replace their old deltas; the sector being driven has none.</summary>
        private void UpdateShownDeltas(MicroSectorModel playerSectors)
        {
            ShowKnownDeltas(SectorTimes.Deltas(SectorTimes.Of(playerSectors.CurrentLap), _personalBest.Sectors));
            if (SectorTimes.TrySectorOf(playerSectors.DrivenMicroSector, out int drivenSector))
            {
                _shownDeltas[drivenSector] = float.NaN;
            }
        }

        private void ShowKnownDeltas(float[] deltas)
        {
            for (int sector = 0; sector < deltas.Length; sector++)
            {
                if (!float.IsNaN(deltas[sector]))
                {
                    _shownDeltas[sector] = deltas[sector];
                }
            }
        }

        private LapSectorsModel BuildModel() => new()
        {
            BestLapTime = _personalBest.LapTime,
            ClassBestLapTime = _personalBest.ClassBest,
            Sectors = Enumerable.Range(0, LapSectorsModel.SectorCount)
                .Select(sector => new SectorModel(sector + 1, _personalBest.Sectors[sector], _shownDeltas[sector]))
                .ToArray(),
        };
    }
}
