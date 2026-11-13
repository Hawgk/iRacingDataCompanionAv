using System;

namespace IRDCav.Models
{
    /// <summary>
    /// Records per-car microsector times and uses the car's best microsectors
    /// ("optimal lap") to convert a track-distance gap into a time gap.
    /// </summary>
    public class MicroSectorModel
    {
        /// <summary>
        /// Microsectors are about this long. Long enough that no car crosses two of them between two
        /// race data ticks, which would discard the sector (7.5 ticks per second: about 12 m at
        /// 330 km/h, 25 m if a tick is missed); short enough that the speed changes little within one.
        /// Replaying a Suzuka lap, 35 m kept half of all interval errors under 4 ms and 95 % under
        /// 35 ms, where 290 m (20 per lap) had 105 ms and 1.3 s. Lime Rock gets about 69 microsectors,
        /// the Nordschleife about 595.
        /// </summary>
        public const double TargetLengthMeters = 35;

        /// <summary>Also the count when the track length is unknown.</summary>
        public const int MinCount = 20;

        // Larger time steps indicate a pause, session change or reconnect.
        private const double MAX_ELAPSED_PER_TICK = 1.0;
        private const int NO_SECTOR = -1;

        private readonly int _count;
        private readonly float _length;
        private int _currentSector = NO_SECTOR;
        private double _currentTime = 0;
        private bool _isCurrentSectorValid = false;

        public MicroSectorModel() : this(MinCount)
        {
        }

        public MicroSectorModel(int count)
        {
            _count = Math.Max(count, MinCount);
            _length = 1.0f / _count;
            BestMicroSectors = new float[_count];
        }

        /// <summary>The number of microsectors for a track; <see cref="MinCount"/> when its length is unknown.</summary>
        public static int CountFor(double trackLengthMeters) =>
            Math.Max(MinCount, (int)Math.Round(trackLengthMeters / TargetLengthMeters));

        public int Count => _count;

        public int RecordedSectors { get; private set; }

        /// <summary>Sum of all best microsector times. Only meaningful when <see cref="IsComplete"/>.</summary>
        public float OptimalLapTime { get; private set; }

        /// <summary>Best time of each microsector (0 if never completed).</summary>
        public float[] BestMicroSectors { get; }

        public bool IsComplete => RecordedSectors == _count;

        private int GetSectorId(float lapDistPct)
        {
            int id = (int)(lapDistPct * _count);
            return Math.Clamp(id, 0, _count - 1);
        }

        /// <summary>
        /// Time gap between this car and the player, based on this car's optimal lap.
        /// Positive: car is ahead of the player. Negative: car is behind.
        /// </summary>
        public bool TryGetInterval(float lapDistPct, float playerLapDistPct, out float interval)
        {
            interval = 0;
            if (!IsComplete || lapDistPct < 0 || playerLapDistPct < 0)
            {
                return false;
            }

            interval = WrapToHalfLap(TimeAt(lapDistPct) - TimeAt(playerLapDistPct));
            return true;
        }

        /// <summary>
        /// Feeds one telemetry step. Elapsed time is attributed to the microsector(s) driven
        /// in this step; when a boundary is crossed the time is split proportionally to the
        /// distance driven on each side of it.
        /// </summary>
        public void Record(double elapsedTime, float lapDistPct, float lastLapDistPct)
        {
            bool isInWorld = lapDistPct >= 0 && lastLapDistPct >= 0;
            float distance = ForwardDistance(lastLapDistPct, lapDistPct);

            if (!isInWorld || !IsRegularTick(elapsedTime) || !IsForwardStep(distance))
            {
                Invalidate(lapDistPct);
                return;
            }

            int sectorId = GetSectorId(lapDistPct);
            EnsureTracking(GetSectorId(lastLapDistPct));

            if (sectorId == _currentSector)
            {
                // Standing still also accumulates time; the car really is spending it in this sector.
                _currentTime += elapsedTime;
            }
            else if (sectorId == NextSector(_currentSector))
            {
                CrossBoundary(elapsedTime, lastLapDistPct, distance);
            }
            else
            {
                Invalidate(lapDistPct);
            }
        }

        private float WrapToHalfLap(float interval)
        {
            if (interval < 0)
            {
                interval += OptimalLapTime;
            }
            return interval > OptimalLapTime / 2 ? interval - OptimalLapTime : interval;
        }

        /// <summary>Time from the start/finish line to <paramref name="lapDistPct"/> along the optimal lap.</summary>
        private float TimeAt(float lapDistPct)
        {
            lapDistPct = Math.Clamp(lapDistPct, 0.0f, 1.0f);
            int sectorId = GetSectorId(lapDistPct);
            float time = 0;

            for (int i = 0; i < sectorId; i++)
            {
                time += BestMicroSectors[i];
            }

            float fraction = (lapDistPct - sectorId * _length) / _length;
            return time + BestMicroSectors[sectorId] * Math.Clamp(fraction, 0.0f, 1.0f);
        }

        private static float ForwardDistance(float from, float to)
        {
            float distance = to - from;
            if (distance < -0.5f)
            {
                return distance + 1.0f;
            }
            return distance > 0.5f ? distance - 1.0f : distance;
        }

        private static bool IsRegularTick(double elapsedTime) =>
            elapsedTime >= 0 && elapsedTime <= MAX_ELAPSED_PER_TICK;

        // Moving further than one microsector per tick means a teleport (tow, reset, pit exit, lapDistPct glitch).
        private bool IsForwardStep(float distance) =>
            distance >= 0 && distance <= _length;

        private int NextSector(int sectorId) => (sectorId + 1) % _count;

        private void EnsureTracking(int lastSectorId)
        {
            if (_currentSector != lastSectorId)
            {
                StartSector(lastSectorId, 0, isValid: false);
            }
        }

        private void CrossBoundary(double elapsedTime, float lastLapDistPct, float distance)
        {
            int nextSector = NextSector(_currentSector);
            float boundary = (_currentSector + 1) * _length;
            double fractionBefore = Math.Clamp((boundary - lastLapDistPct) / distance, 0.0f, 1.0f);

            _currentTime += elapsedTime * fractionBefore;
            if (_isCurrentSectorValid)
            {
                CommitSector(_currentSector, (float)_currentTime);
            }

            StartSector(nextSector, elapsedTime * (1.0 - fractionBefore), isValid: true);
        }

        private void StartSector(int sectorId, double initialTime, bool isValid)
        {
            _currentSector = sectorId;
            _currentTime = initialTime;
            _isCurrentSectorValid = isValid;
        }

        private void Invalidate(float lapDistPct)
        {
            StartSector(lapDistPct >= 0 ? GetSectorId(lapDistPct) : NO_SECTOR, 0, isValid: false);
        }

        private void CommitSector(int sectorId, float time)
        {
            float best = BestMicroSectors[sectorId];
            if (time <= 0 || (best > 0 && time >= best))
            {
                return;
            }

            if (best == 0)
            {
                RecordedSectors++;
            }
            BestMicroSectors[sectorId] = time;
            OptimalLapTime += time - best;
        }
    }
}
