using IRDCav.Telemetry;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IRDCav.Services
{
    /// <summary>A sector the player finished: the lap time and the delta to the best lap at both of its ends.</summary>
    /// <param name="EndsLap">The last sector; its end values are taken at the line and refined once iRacing reports the lap time.</param>
    internal readonly record struct SectorResult(
        int Sector, float StartTime, float EndTime, float StartDelta, float EndDelta, bool IsDeltaValid, bool EndsLap)
    {
        public float Time => EndTime - StartTime;
        public float Delta => EndDelta - StartDelta;
    }

    /// <summary>
    /// Times the player's sectors at iRacing's sector boundaries. Where a boundary lies between two
    /// ticks, the lap time and iRacing's delta to the all-time best lap are interpolated. A sector's
    /// delta is how much that delta changed across the sector, the same comparison iRacing's own
    /// sector display makes. A jump (tow, reset, pit lane) stops timing until the next boundary.
    /// </summary>
    internal sealed class SectorClock
    {
        public const int NoSector = -1;

        // A larger step between two ticks is a jump rather than driving.
        private const float MaxStep = 0.05f;

        private float[] _starts = Array.Empty<float>();
        private PlayerLapTelemetry _last;
        private bool _hasLast;
        private bool _isSectorTimed;
        private double _lapStart;
        private float _sectorStartTime;
        private float _sectorStartDelta;
        private bool _isSectorStartDeltaValid;

        /// <summary>The sector being driven, or <see cref="NoSector"/>.</summary>
        public int DrivenSector { get; private set; } = NoSector;

        public void Configure(IReadOnlyList<float> sectorStarts)
        {
            _starts = sectorStarts.ToArray();
            Reset();
        }

        public void Reset()
        {
            _hasLast = false;
            _isSectorTimed = false;
            DrivenSector = NoSector;
        }

        public IReadOnlyList<SectorResult> Update(PlayerLapTelemetry now)
        {
            IReadOnlyList<SectorResult> results = _hasLast ? Advance(_last, now) : Restart(now);
            _last = now;
            _hasLast = true;
            return results;
        }

        private IReadOnlyList<SectorResult> Advance(PlayerLapTelemetry last, PlayerLapTelemetry now)
        {
            float step = now.LapDistPct - last.LapDistPct;
            if (_starts.Length == 0 || now.LapDistPct < 0 || last.LapDistPct < 0)
            {
                return Restart(now);
            }
            if (step >= 0 && step <= MaxStep)
            {
                return CrossBoundaries(last, now);
            }
            return step + 1 <= MaxStep ? CrossLine(last, now) : Restart(now);
        }

        private IReadOnlyList<SectorResult> CrossBoundaries(PlayerLapTelemetry last, PlayerLapTelemetry now)
        {
            var results = new List<SectorResult>();
            for (int sector = 1; sector < _starts.Length; sector++)
            {
                float boundary = _starts[sector];
                if (last.LapDistPct < boundary && now.LapDistPct >= boundary)
                {
                    float share = (boundary - last.LapDistPct) / (now.LapDistPct - last.LapDistPct);
                    float time = (float)(SessionTimeAt(last, now, share) - _lapStart);
                    float delta = Lerp(last.DeltaToBestLap, now.DeltaToBestLap, share);
                    bool isDeltaValid = last.IsDeltaToBestLapValid && now.IsDeltaToBestLapValid;
                    AddFinished(results, time, delta, isDeltaValid, endsLap: false);
                    StartSector(sector, time, delta, isDeltaValid);
                }
            }
            return results;
        }

        /// <summary>The moment of the line is interpolated by distance; iRacing restarts its delta from 0 there.</summary>
        private IReadOnlyList<SectorResult> CrossLine(PlayerLapTelemetry last, PlayerLapTelemetry now)
        {
            float before = 1 - last.LapDistPct;
            double line = SessionTimeAt(last, now, before / (before + now.LapDistPct));

            var results = new List<SectorResult>();
            AddFinished(results, (float)(line - _lapStart), last.DeltaToBestLap, last.IsDeltaToBestLapValid, endsLap: true);
            _lapStart = line;
            StartSector(0, 0, 0, isDeltaValid: true);
            return results;
        }

        /// <summary>
        /// Times are measured from the line on the session clock. iRacing's own lap clock runs a few
        /// hundredths off it, so it only stands in for a lap whose start wasn't seen.
        /// </summary>
        private IReadOnlyList<SectorResult> Restart(PlayerLapTelemetry now)
        {
            _isSectorTimed = false;
            _lapStart = now.SessionTime - now.CurrentLapTime;
            DrivenSector = now.LapDistPct >= 0 ? SectorAt(now.LapDistPct) : NoSector;
            return Array.Empty<SectorResult>();
        }

        private static double SessionTimeAt(PlayerLapTelemetry last, PlayerLapTelemetry now, float share) =>
            last.SessionTime + (now.SessionTime - last.SessionTime) * share;

        private void AddFinished(List<SectorResult> results, float time, float delta, bool isDeltaValid, bool endsLap)
        {
            if (_isSectorTimed)
            {
                results.Add(new SectorResult(
                    DrivenSector, _sectorStartTime, time, _sectorStartDelta, delta, _isSectorStartDeltaValid && isDeltaValid, endsLap));
            }
        }

        private void StartSector(int sector, float time, float delta, bool isDeltaValid)
        {
            DrivenSector = sector;
            _isSectorTimed = true;
            _sectorStartTime = time;
            _sectorStartDelta = delta;
            _isSectorStartDeltaValid = isDeltaValid;
        }

        private int SectorAt(float lapDistPct)
        {
            int sector = Array.FindLastIndex(_starts, start => start <= lapDistPct);
            return sector >= 0 ? sector : NoSector;
        }

        private static float Lerp(float from, float to, float share) => from + (to - from) * share;
    }
}
