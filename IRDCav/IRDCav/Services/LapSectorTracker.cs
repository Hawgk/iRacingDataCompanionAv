using IRDCav.Models;
using IRDCav.Telemetry;
using System;
using System.Linq;

namespace IRDCav.Services
{
    /// <summary>
    /// Follows the player's laps in equal sectors built from the microsectors. The best lap is
    /// iRacing's: when its best lap time changes, the lap that just crossed the line is taken over,
    /// so laps iRacing does not count are ignored. Each finished sector gets a delta to the best lap,
    /// which stays until the car enters that sector again. Runs on the telemetry thread.
    /// </summary>
    internal sealed class LapSectorTracker
    {
        // iRacing reports a new best lap a moment after the line; a lap end and a new best this close belong together.
        private const double LapMatchSeconds = 5;
        private const double Never = double.NegativeInfinity;

        private MicroSectorModel? _source;
        private int _completedLaps;
        private float _reportedBest = float.NaN;
        private double _lapEndedAt = Never;
        private double _bestChangedAt = Never;
        private float[] _lastLapSectors = SectorTimes.Unknown();
        private float[] _bestSectors = SectorTimes.Unknown();
        private readonly float[] _shownDeltas = SectorTimes.NoDeltas();

        /// <summary>Forgets the best lap and the deltas, e.g. for a new session or connection.</summary>
        public void Reset()
        {
            _source = null;
            _reportedBest = float.NaN;
            _lapEndedAt = Never;
            _bestChangedAt = Never;
            _bestSectors = SectorTimes.Unknown();
            Array.Fill(_shownDeltas, float.NaN);
        }

        public LapSectorsModel Update(MicroSectorModel playerSectors, SessionTelemetry session)
        {
            Follow(playerSectors);
            ObserveLapEnd(playerSectors, session.SessionTime);
            ObserveBestLap(session.BestLapTime, session.SessionTime);
            AdoptBestLap(session.SessionTime);
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
            ShowKnownDeltas(SectorTimes.Deltas(_lastLapSectors, _bestSectors));
            _lapEndedAt = now;
        }

        /// <summary>The first value after a reset is only noted; a best lap set before that has no recorded sectors.</summary>
        private void ObserveBestLap(float bestLapTime, double now)
        {
            float reported = Math.Max(bestLapTime, 0);
            if (float.IsNaN(_reportedBest) || reported == _reportedBest)
            {
                _reportedBest = reported;
                return;
            }
            _reportedBest = reported;
            _bestChangedAt = reported > 0 ? now : Never;
            if (reported == 0)
            {
                _bestSectors = SectorTimes.Unknown();
            }
        }

        /// <summary>Takes over the lap that ended close to the change of the best time, or forgets the old sectors if none did.</summary>
        private void AdoptBestLap(double now)
        {
            if (_bestChangedAt == Never)
            {
                return;
            }
            if (Math.Abs(_lapEndedAt - _bestChangedAt) <= LapMatchSeconds)
            {
                _bestSectors = _lastLapSectors;
                _lapEndedAt = Never;
                _bestChangedAt = Never;
            }
            else if (now - _bestChangedAt > LapMatchSeconds)
            {
                _bestSectors = SectorTimes.Unknown();
                _bestChangedAt = Never;
            }
        }

        /// <summary>Finished sectors of the lap in progress replace their old deltas; the sector being driven has none.</summary>
        private void UpdateShownDeltas(MicroSectorModel playerSectors)
        {
            ShowKnownDeltas(SectorTimes.Deltas(SectorTimes.Of(playerSectors.CurrentLap), _bestSectors));
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
            BestLapTime = _reportedBest > 0 ? _reportedBest : 0,
            Sectors = Enumerable.Range(0, LapSectorsModel.SectorCount)
                .Select(sector => new SectorModel(sector + 1, _bestSectors[sector], _shownDeltas[sector]))
                .ToArray(),
        };
    }
}
