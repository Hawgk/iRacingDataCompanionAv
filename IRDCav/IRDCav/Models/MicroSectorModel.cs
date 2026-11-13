using System;

namespace IRDCav.Models
{
    /// <summary>
    /// Remembers when a car last passed each microsector boundary, so a gap can be measured the
    /// way race timing does it: as the time between two cars passing the same point. Every
    /// boundary acts as a timing loop. A boundary crossed between two ticks gets a pass time
    /// interpolated by distance. A jump (tow, reset, a gap in the data) forgets all pass times,
    /// because the car didn't drive through the points in between.
    /// </summary>
    public class MicroSectorModel
    {
        /// <summary>
        /// Boundaries are about this far apart; between two of them the car is taken to drive at an
        /// even speed. Replaying a Suzuka session at 7.5 ticks per second, 35 m measured half of all
        /// gaps within 3 ms and 95 % within 30 ms. Lime Rock gets about 69 boundaries, the
        /// Nordschleife about 595.
        /// </summary>
        public const double TargetLengthMeters = 35;

        /// <summary>Also the count when the track length is unknown.</summary>
        public const int MinCount = 20;

        // Larger time steps are a pause, a session change or a reconnect, not driving.
        private const double MaxElapsedPerTick = 1.0;

        // Moving further in one tick is a jump; several missed ticks at full speed still fit.
        private const int MaxBoundariesPerTick = 4;

        private readonly double[] _passTimes;
        private float _lapDistPct = -1;
        private double _time;

        public MicroSectorModel() : this(MinCount)
        {
        }

        public MicroSectorModel(int count)
        {
            _passTimes = new double[Math.Max(count, MinCount)];
            Array.Fill(_passTimes, double.NaN);
        }

        /// <summary>The number of microsectors for a track; <see cref="MinCount"/> when its length is unknown.</summary>
        public static int CountFor(double trackLengthMeters) =>
            Math.Max(MinCount, (int)Math.Round(trackLengthMeters / TargetLengthMeters));

        public int Count => _passTimes.Length;

        /// <summary>Feeds one telemetry step and notes the time of every boundary crossed in it.</summary>
        public void Record(TimingTick tick, float lapDistPct, float lastLapDistPct)
        {
            float distance = ForwardDistance(lastLapDistPct, lapDistPct);
            if (IsDriving(tick, lapDistPct, lastLapDistPct, distance))
            {
                PassBoundaries(tick, lastLapDistPct, distance);
            }
            else
            {
                Array.Fill(_passTimes, double.NaN);
            }
            _lapDistPct = lapDistPct;
            _time = tick.SessionTime;
        }

        /// <summary>
        /// How long ago the car passed <paramref name="lapDistPct"/> on its way to where it is now,
        /// at most half a lap back; false when it is ahead of the car or the car didn't drive past it.
        /// </summary>
        public bool TrySecondsSincePassing(float lapDistPct, double sessionTime, out float seconds)
        {
            seconds = 0;
            if (_lapDistPct < 0 || lapDistPct < 0 || ForwardDistance(lapDistPct, _lapDistPct) < 0 ||
                !TryPassTime(Math.Clamp(lapDistPct, 0, 0.99999f) * Count, out double passTime))
            {
                return false;
            }
            seconds = (float)(sessionTime - passTime);
            return seconds >= 0;
        }

        /// <summary>Between the two boundaries around the point; within the car's own microsector, between its last boundary and now.</summary>
        private bool TryPassTime(double position, out double passTime)
        {
            int boundary = (int)position;
            double start = _passTimes[boundary];
            double carPosition = _lapDistPct * Count;
            bool isInCarsMicroSector = (int)carPosition == boundary && carPosition >= position;

            double end = isInCarsMicroSector ? _time : _passTimes[(boundary + 1) % Count];
            double endPosition = isInCarsMicroSector ? carPosition : boundary + 1;
            passTime = start + (end - start) * Share(position - boundary, endPosition - boundary);
            return !double.IsNaN(start) && !double.IsNaN(end) && end >= start;
        }

        private void PassBoundaries(TimingTick tick, float lastLapDistPct, float distance)
        {
            double start = lastLapDistPct * Count;
            double end = start + distance * Count;
            for (int boundary = (int)Math.Floor(start) + 1; boundary <= (int)Math.Floor(end); boundary++)
            {
                double share = (boundary - start) / (end - start);
                _passTimes[boundary % Count] = tick.PreviousTime + tick.ElapsedTime * share;
            }
        }

        private bool IsDriving(TimingTick tick, float lapDistPct, float lastLapDistPct, float distance) =>
            lapDistPct >= 0 && lastLapDistPct >= 0 &&
            tick.ElapsedTime >= 0 && tick.ElapsedTime <= MaxElapsedPerTick &&
            distance >= 0 && distance <= (float)MaxBoundariesPerTick / Count;

        private static double Share(double part, double whole) => whole > 0 ? Math.Clamp(part / whole, 0, 1) : 0;

        /// <summary>Signed distance from one lap fraction to another, the short way round.</summary>
        private static float ForwardDistance(float from, float to)
        {
            float distance = to - from;
            if (distance < -0.5f)
            {
                return distance + 1.0f;
            }
            return distance > 0.5f ? distance - 1.0f : distance;
        }
    }
}
