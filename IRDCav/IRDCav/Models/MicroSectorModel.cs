using System;

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

        // A tick in which a car moves further than this is treated as a teleport
        // (tow, reset, pit exit, lapDistPct glitch) and invalidates the running sector.
        private const float MAX_DISTANCE_PER_TICK = MICROSECTOR_LENGTH;
        // Larger time steps indicate a pause, session change or reconnect.
        private const double MAX_ELAPSED_PER_TICK = 1.0;

        private int _currentSector = -1;
        private double _currentTime = 0;
        private bool _currentValid = false;

        /// <summary>Number of microsectors that have a best time recorded.</summary>
        public int RecordedSectors { get; private set; }

        /// <summary>Sum of all best microsector times. Only meaningful when all sectors are recorded.</summary>
        public float OptimalLapTime { get; private set; }

        /// <summary>Last fully driven time of each microsector (0 if never completed).</summary>
        public float[] MicroSectors { get; } = new float[MICROSECTOR_COUNT];

        /// <summary>Best time of each microsector (0 if never completed).</summary>
        public float[] BestMicroSectors { get; } = new float[MICROSECTOR_COUNT];

        public bool IsComplete => RecordedSectors == MICROSECTOR_COUNT;

        public static int GetSectorId(float lapDistPct)
        {
            int id = (int)(lapDistPct * MICROSECTOR_COUNT);
            return Math.Clamp(id, 0, MICROSECTOR_COUNT - 1);
        }

        /// <summary>
        /// Time from the start/finish line to <paramref name="lapDistPct"/> along the optimal lap.
        /// </summary>
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

        /// <summary>
        /// Time gap between this car and the player, based on this car's optimal lap.
        /// Positive: car is ahead of the player. Negative: car is behind.
        /// Returns 0 if not enough data is available.
        /// </summary>
        public float GetInterval(float lapDistPct, float playerLapDistPct)
        {
            if (!IsComplete || OptimalLapTime <= 0 ||
                lapDistPct < 0 || playerLapDistPct < 0)
            {
                return 0;
            }

            // Time this car needs to drive forward from the player's position to its own position.
            float interval = TimeAt(lapDistPct) - TimeAt(playerLapDistPct);
            if (interval < 0)
            {
                interval += OptimalLapTime;
            }

            // More than half a lap ahead means the car is actually behind.
            if (interval > OptimalLapTime / 2)
            {
                interval -= OptimalLapTime;
            }

            return interval;
        }

        /// <summary>
        /// Feeds one telemetry step. Elapsed time is attributed to the microsector(s) driven
        /// in this step; when a boundary is crossed the time is split proportionally to the
        /// distance driven on each side of it.
        /// </summary>
        public void SetMicroSector(double elapsedTime, float lapDistPct, float lastLapDistPct)
        {
            if (lapDistPct < 0 || lastLapDistPct < 0 ||
                elapsedTime < 0 || elapsedTime > MAX_ELAPSED_PER_TICK)
            {
                Invalidate(lapDistPct);
                return;
            }

            // Forward distance driven, accounting for crossing the start/finish line.
            float distance = lapDistPct - lastLapDistPct;
            if (distance < -0.5f)
            {
                distance += 1.0f;
            }
            else if (distance > 0.5f)
            {
                distance -= 1.0f;
            }

            if (distance < 0 || distance > MAX_DISTANCE_PER_TICK)
            {
                // Driving backwards or teleported.
                Invalidate(lapDistPct);
                return;
            }

            int sectorId = GetSectorId(lapDistPct);
            int lastSectorId = GetSectorId(lastLapDistPct);

            if (_currentSector != lastSectorId)
            {
                // Lost track of the car (first sample, or after an invalidation).
                _currentSector = lastSectorId;
                _currentTime = 0;
                _currentValid = false;
            }

            if (sectorId == lastSectorId)
            {
                // Standing still also accumulates time; the car really is spending it in this sector.
                _currentTime += elapsedTime;
            }
            else if (sectorId == (lastSectorId + 1) % MICROSECTOR_COUNT)
            {
                float boundary = (lastSectorId + 1) * MICROSECTOR_LENGTH;
                double fractionBefore = Math.Clamp((boundary - lastLapDistPct) / distance, 0.0f, 1.0f);

                _currentTime += elapsedTime * fractionBefore;
                if (_currentValid)
                {
                    CommitSector(lastSectorId, (float)_currentTime);
                }

                _currentSector = sectorId;
                _currentTime = elapsedTime * (1.0 - fractionBefore);
                _currentValid = true;
            }
            else
            {
                Invalidate(lapDistPct);
            }
        }

        private void Invalidate(float lapDistPct)
        {
            _currentSector = lapDistPct >= 0 ? GetSectorId(lapDistPct) : -1;
            _currentTime = 0;
            _currentValid = false;
        }

        private void CommitSector(int sectorId, float time)
        {
            if (time <= 0)
            {
                return;
            }

            MicroSectors[sectorId] = time;

            if (BestMicroSectors[sectorId] == 0)
            {
                BestMicroSectors[sectorId] = time;
                RecordedSectors++;
            }
            else if (time < BestMicroSectors[sectorId])
            {
                BestMicroSectors[sectorId] = time;
            }
            else
            {
                return;
            }

            float optimal = 0;
            foreach (float best in BestMicroSectors)
            {
                optimal += best;
            }
            OptimalLapTime = optimal;
        }
    }
}
