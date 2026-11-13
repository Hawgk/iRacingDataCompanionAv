using IRDCav.Models;
using IRDCav.Telemetry;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IRDCav.Services
{
    /// <summary>
    /// The sector view's data: iRacing's best lap of the car at the track with its sectors, and the
    /// delta of every finished sector, which stays until the car enters that sector again. iRacing
    /// compares with a new best at once but writes it to its file later; when a finished lap shows
    /// that iRacing's reference has changed, the recent lap it matches becomes the reference.
    /// Runs on the telemetry thread.
    /// </summary>
    internal sealed class LapSectorTracker
    {
        // A lap time and the reference iRacing implies for it agree within this, in seconds.
        private const float SameLapTolerance = 0.05f;
        private const double LastLapTimeWaitSeconds = 5;
        private const int RememberedLaps = 3;

        private readonly ReferenceLap _reference;
        private readonly SectorClock _clock;
        private readonly List<DrivenLap> _recentLaps = new();
        private LapSectorsSetup _setup = LapSectorsSetup.Unknown;
        private float[] _shownDeltas = Array.Empty<float>();
        private float[] _lapSectorTimes = Array.Empty<float>();
        private PendingLap _pending = PendingLap.None;

        public LapSectorTracker(ReferenceLap reference, SectorClock clock)
        {
            _reference = reference;
            _clock = clock;
        }

        /// <summary>Loads iRacing's best lap when the car, the track or its sectors change.</summary>
        public void UseCar(LapSectorsSetup setup)
        {
            if (setup.IsSameAs(_setup))
            {
                return;
            }
            _setup = setup;
            _reference.Load(setup.Key, setup.SessionCarClassId, setup.SectorStarts.Count);
            _clock.Configure(setup.SectorStarts);
            ForgetLaps();
        }

        /// <summary>Forgets the driven laps, e.g. for a new session or connection; keeps the reference.</summary>
        public void Reset()
        {
            _clock.Reset();
            ForgetLaps();
        }

        public LapSectorsModel Update(PlayerLapTelemetry lap)
        {
            foreach (SectorResult result in _clock.Update(lap))
            {
                Record(result, lap);
            }
            ClearDrivenSectorDelta();
            ResolvePendingLap(lap);
            return BuildModel();
        }

        private void ForgetLaps()
        {
            int sectorCount = _setup.SectorStarts.Count;
            _shownDeltas = Enumerable.Repeat(float.NaN, sectorCount).ToArray();
            _lapSectorTimes = new float[sectorCount];
            _recentLaps.Clear();
            _pending = PendingLap.None;
        }

        private void Record(SectorResult result, PlayerLapTelemetry lap)
        {
            _lapSectorTimes[result.Sector] = result.Time;
            _shownDeltas[result.Sector] = result.IsDeltaValid ? MathF.Round(result.Delta, 3) : float.NaN;
            if (result.EndsLap)
            {
                _pending = new PendingLap(result, _lapSectorTimes, lap.LastLapTime, lap.SessionTime, IsWaiting: true);
                _lapSectorTimes = new float[_lapSectorTimes.Length];
            }
        }

        private void ClearDrivenSectorDelta()
        {
            if (_clock.DrivenSector >= 0 && _clock.DrivenSector < _shownDeltas.Length)
            {
                _shownDeltas[_clock.DrivenSector] = float.NaN;
            }
        }

        /// <summary>iRacing reports the lap time a moment after the line.</summary>
        private void ResolvePendingLap(PlayerLapTelemetry lap)
        {
            if (!_pending.IsWaiting)
            {
                return;
            }
            if (lap.LastLapTime > 0 && lap.LastLapTime != _pending.LastLapTimeAtLine)
            {
                Resolve(_pending, lap.LastLapTime);
                _pending = PendingLap.None;
            }
            else if (lap.SessionTime - _pending.LineCrossedAt > LastLapTimeWaitSeconds)
            {
                _pending = PendingLap.None;
            }
        }

        private void Resolve(PendingLap pending, float lapTime)
        {
            SectorResult lastSector = pending.LastSector;
            pending.SectorTimes[lastSector.Sector] = lapTime - lastSector.StartTime;
            _reference.ReloadIfChanged();
            if (lastSector.IsDeltaValid)
            {
                FollowIRacingReference(lapTime, lastSector);
            }
            Remember(new DrivenLap(lapTime, pending.SectorTimes));
        }

        /// <summary>
        /// The lap time minus iRacing's delta at the line is the reference iRacing compared with. If
        /// it is ours, the last sector's delta becomes exact; otherwise a recent lap became the new best.
        /// </summary>
        private void FollowIRacingReference(float lapTime, SectorResult lastSector)
        {
            float iRacingReference = lapTime - lastSector.EndDelta;
            if (IsSameLapTime(iRacingReference, _reference.LapTime))
            {
                float exactDelta = lapTime - _reference.LapTime - lastSector.StartDelta;
                _shownDeltas[lastSector.Sector] = MathF.Round(exactDelta, 3);
                return;
            }

            int match = _recentLaps.FindIndex(recent => IsSameLapTime(iRacingReference, recent.LapTime));
            if (match >= 0)
            {
                _reference.Adopt(_recentLaps[match].LapTime, _recentLaps[match].SectorTimes);
            }
            else
            {
                _reference.Adopt(iRacingReference, Array.Empty<float>());
            }
        }

        private void Remember(DrivenLap lap)
        {
            _recentLaps.Insert(0, lap);
            if (_recentLaps.Count > RememberedLaps)
            {
                _recentLaps.RemoveAt(_recentLaps.Count - 1);
            }
        }

        private static bool IsSameLapTime(float lapTime, float other) => other > 0 && Math.Abs(lapTime - other) <= SameLapTolerance;

        private LapSectorsModel BuildModel() => new()
        {
            BestLapTime = _reference.LapTime,
            ClassBestLapTime = _reference.ClassBest,
            Sectors = Enumerable.Range(0, _shownDeltas.Length)
                .Select(sector => new SectorModel(sector + 1, _reference.SectorTimes[sector], _shownDeltas[sector]))
                .ToArray(),
        };

        private sealed record DrivenLap(float LapTime, float[] SectorTimes);

        private readonly record struct PendingLap(
            SectorResult LastSector, float[] SectorTimes, float LastLapTimeAtLine, double LineCrossedAt, bool IsWaiting)
        {
            public static readonly PendingLap None = new(default, Array.Empty<float>(), 0, 0, IsWaiting: false);
        }
    }
}
