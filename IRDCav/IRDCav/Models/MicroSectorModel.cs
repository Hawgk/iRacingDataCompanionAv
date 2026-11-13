using System;
using System.Collections.Generic;

namespace IRDCav.Models
{
    /// <summary>
    /// Records per-car microsector times and uses the car's best microsectors
    /// ("optimal lap") to convert a track-distance gap into a time gap.
    /// </summary>
    public class MicroSectorModel
    {
        public const int MICROSECTOR_COUNT = 20;
        public const float MICROSECTOR_LENGTH = 1.0f / MICROSECTOR_COUNT;

        // Moving further than one sector per tick means a teleport (tow, reset, pit exit, lapDistPct glitch).
        private const float MAX_DISTANCE_PER_TICK = MICROSECTOR_LENGTH;
        // Larger time steps indicate a pause, session change or reconnect.
        private const double MAX_ELAPSED_PER_TICK = 1.0;
        private const int NO_SECTOR = -1;

        private int _currentSector = NO_SECTOR;
        private double _currentTime = 0;
        private bool _isCurrentSectorValid = false;
        private readonly float[] _currentLap = new float[MICROSECTOR_COUNT];
        private float[] _lastLap = new float[MICROSECTOR_COUNT];

        public int RecordedSectors { get; private set; }

        /// <summary>Microsector times of the lap in progress; 0 for a microsector not (yet) driven without interruption.</summary>
        public IReadOnlyList<float> CurrentLap => _currentLap;

        /// <summary>Microsector times of the lap that last crossed the start/finish line; never changed afterwards.</summary>
        public IReadOnlyList<float> LastLap => _lastLap;

        /// <summary>Increases each time the car crosses the start/finish line.</summary>
        public int CompletedLaps { get; private set; }

        /// <summary>Sum of all best microsector times. Only meaningful when <see cref="IsComplete"/>.</summary>
        public float OptimalLapTime { get; private set; }

        /// <summary>Best time of each microsector (0 if never completed).</summary>
        public float[] BestMicroSectors { get; } = new float[MICROSECTOR_COUNT];

        public bool IsComplete => RecordedSectors == MICROSECTOR_COUNT;

        public static int GetSectorId(float lapDistPct)
        {
            int id = (int)(lapDistPct * MICROSECTOR_COUNT);
            return Math.Clamp(id, 0, MICROSECTOR_COUNT - 1);
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

            float fraction = (lapDistPct - sectorId * MICROSECTOR_LENGTH) / MICROSECTOR_LENGTH;
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

        private static bool IsForwardStep(float distance) =>
            distance >= 0 && distance <= MAX_DISTANCE_PER_TICK;

        private static int NextSector(int sectorId) => (sectorId + 1) % MICROSECTOR_COUNT;

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
            float boundary = (_currentSector + 1) * MICROSECTOR_LENGTH;
            double fractionBefore = Math.Clamp((boundary - lastLapDistPct) / distance, 0.0f, 1.0f);

            _currentTime += elapsedTime * fractionBefore;
            if (_isCurrentSectorValid)
            {
                CommitSector(_currentSector, (float)_currentTime);
                _currentLap[_currentSector] = (float)_currentTime;
            }
            if (nextSector == 0)
            {
                CompleteLap();
            }

            StartSector(nextSector, elapsedTime * (1.0 - fractionBefore), isValid: true);
        }

        private void CompleteLap()
        {
            _lastLap = (float[])_currentLap.Clone();
            Array.Clear(_currentLap);
            CompletedLaps++;
        }

        private void StartSector(int sectorId, double initialTime, bool isValid)
        {
            _currentSector = sectorId;
            _currentTime = initialTime;
            _isCurrentSectorValid = isValid;
        }

        /// <summary>Also discards the lap in progress: after a teleport its microsectors no longer belong to one lap.</summary>
        private void Invalidate(float lapDistPct)
        {
            Array.Clear(_currentLap);
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
