using System;

namespace IRDCav.Models
{
    public class MicroSectorModel
    {
        public static int MICROSECTOR_COUNT = 50;
        public static float MICROSECTOR_LENGTH = 1.0f / MICROSECTOR_COUNT;

        public float[] MicroSectors { get; set; } = new float[MICROSECTOR_COUNT];
        public float[] BestMicroSectors { get; set; } = new float[MICROSECTOR_COUNT];

        private static int GetSectorId(float lapDist, bool isFloor)
        {
            float normalizedId = (float)(lapDist * 100 / (100.0f / MICROSECTOR_COUNT));

            if (isFloor)
            {
                return (int)Math.Floor(normalizedId);
            }
            else
            {
                if (lapDist < 1.0f)
                    return (int)Math.Ceiling(normalizedId);
                return (int)Math.Floor(normalizedId);
            }
        }

        // Check if range between microsectors is valid for the interval calculation
        // If any element in the range is zero for both last and best microsectors this returns false.
        private float CalculateIntervalFromSectors(float lapDistPct, int floor, int ceil)
        {
            float interval = 0;

            for (int sectorId = floor; sectorId < ceil; sectorId++)
            {
                if (sectorId < 0 || sectorId >= MICROSECTOR_COUNT)
                {
                    interval = 0;
                    break;
                }
                if (MicroSectors[sectorId] == 0.0f &&
                    BestMicroSectors[sectorId] == 0.0f)
                {
                    interval = 0;
                    break;
                }
                else
                {
                    float sectorToAdd = MicroSectors[sectorId] == 0.0f ? BestMicroSectors[sectorId] : MicroSectors[sectorId];
                    float sectorStart = sectorId * MICROSECTOR_LENGTH;
                    float sectorEnd = (sectorId + 1) * MICROSECTOR_LENGTH;

                    // Lower sector bound. Interpolate between first and second point.
                    if (sectorId == floor)
                    {
                        // Wrap if car went over the finish line
                        int sid = sectorId + 1;
                        if (sid >= MICROSECTOR_COUNT)
                        {
                            sid = 0;
                        }
                        // Select whichever is quicker
                        float sectorToAddNext = MicroSectors[sid] == 0.0f ? BestMicroSectors[sid] : MicroSectors[sid];

                        sectorToAdd = Interpolation.LinearInterpolate(lapDistPct, sectorStart, sectorEnd, sectorToAdd, sectorToAdd + sectorToAddNext);
                    }
                    // Upper sector bound. Interpolate between last and second to last point.
                    else if (sectorId == ceil - 1)
                    {
                        // Wrap if car went over the finish line
                        int sid = sectorId - 1;
                        if (sid < 0)
                        {
                            sid = MICROSECTOR_COUNT - 1;
                        }
                        float sectorToAddPrev = MicroSectors[sid] == 0.0f ? BestMicroSectors[sid] : MicroSectors[sid];

                        sectorToAdd = Interpolation.LinearInterpolate(lapDistPct, sectorEnd, sectorStart, sectorToAddPrev, sectorToAddPrev + sectorToAdd);
                    }

                    interval += sectorToAdd;
                }
            }
            return interval;
        }

        public float GetInterval(float lapDistPct, float playerLapDistPct)
        {
            int floor = 0;
            int ceil = 0;

            if (lapDistPct > playerLapDistPct)
            {
                floor = GetSectorId(playerLapDistPct, true);
                ceil = GetSectorId(lapDistPct, false);
            }
            else
            {
                floor = GetSectorId(lapDistPct, true);
                ceil = GetSectorId(playerLapDistPct, false);
            }

            return CalculateIntervalFromSectors(lapDistPct, floor, ceil);
        }

        public void SetMicroSector(double elapsedTime, float lapDistPct, float lastLapDistPct)
        {
            // Detects when a new sector is started.
            // Elapsed time is then added into buckets.
            // If a sector switch happened in last cycle the elapsed time is split
            // into both buckets depending on the overlap.
            float sectorStart;
            float sectorEnd;
            float correction;
            int sectorIdx = GetSectorId(lapDistPct < 1.0f ? lapDistPct : 0.99f, true);
            int lastSectorIdx = GetSectorId(lastLapDistPct, true);

            if (lastLapDistPct != lapDistPct)
            {
                if (sectorIdx > lastSectorIdx ||
                    (sectorIdx == 0 && sectorIdx < lastSectorIdx))
                {
                    sectorEnd = (lastSectorIdx + 1) * MICROSECTOR_LENGTH;
                    correction = sectorEnd - lastLapDistPct;

                    MicroSectors[lastSectorIdx] += (float)Math.Round(elapsedTime * correction, 3);

                    // No best sector time exists yet or best sector time was slower.
                    if ((MicroSectors[sectorIdx] - BestMicroSectors[sectorIdx]) >= (MicroSectors[sectorIdx] / 2) ||
                        BestMicroSectors[sectorIdx] > MicroSectors[sectorIdx] &&
                        MicroSectors[sectorIdx] != 0)
                    {
                        BestMicroSectors[sectorIdx] = MicroSectors[sectorIdx];
                    }

                    MicroSectors[sectorIdx] = 0;

                    sectorStart = sectorIdx * MICROSECTOR_LENGTH;
                    correction = lapDistPct - sectorStart;

                    MicroSectors[sectorIdx] += (float)Math.Round(elapsedTime * correction, 3);
                } 
                else if (sectorIdx == lastSectorIdx)
                {
                    MicroSectors[sectorIdx] += (float)Math.Round(elapsedTime, 3);
                }
            }
        }
    }
}
