using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace IRDCav.Models
{
    public enum MicroSectorStatus
    {
        Initialized = -1,
        NotComplete = 0,
        Complete = 1,
    }

    public struct MicroSector
    {
        public float Value;
        public MicroSectorStatus Status;
    }

    public class MicroSectorModel
    {
        private int _recordedSectors = 0;
        public static int MICROSECTOR_COUNT = 20;
        public static float MICROSECTOR_LENGTH = 1.0f / MICROSECTOR_COUNT;

        public int RecordedSectors {
            get => _recordedSectors;
            set 
            {
                if (value <= MICROSECTOR_COUNT)
                {
                    _recordedSectors = value;
                }
            } 
        }

        public float OptimalLapTime { get; set; }

        public MicroSector[] MicroSectors = new MicroSector[MICROSECTOR_COUNT];
        public MicroSector[] BestMicroSectors = new MicroSector[MICROSECTOR_COUNT];

        public MicroSectorModel()
        {
            for (int i = 0; i < MICROSECTOR_COUNT; i++)
            {
                MicroSectors[i] = new MicroSector()
                {
                    Value = 0,
                    Status = MicroSectorStatus.Initialized,
                };
                BestMicroSectors[i] = new MicroSector()
                {
                    Value = 0,
                    Status = MicroSectorStatus.Initialized,
                };
            }
        }

        private static int GetSectorId(float lapDist, bool isFloor)
        {
            lapDist = lapDist >= 1.0f ? 0.9999f : lapDist;
            lapDist = lapDist < 0.0f ? 0.0001f : lapDist;
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

        private float CalculateIntervalFromSectors(float lapDistPct, float playerLapDistPct, int floor, int ceil)
        {
            float interval = 0;
            int virtualCeil = floor > ceil ? ceil + MICROSECTOR_COUNT : ceil;

            //for (int sectorId = floor; sectorId < virtualCeil; sectorId++)
            Parallel.For(floor, virtualCeil, delegate (int sectorId, ParallelLoopState state)
            {
                int virtualSectorId = sectorId < MICROSECTOR_COUNT ? sectorId : sectorId - MICROSECTOR_COUNT;

                if (virtualSectorId < 0 || virtualSectorId >= MICROSECTOR_COUNT)
                {
                    interval = 0;
                    state.Break();
                    //break;
                }
                else
                {
                    float sectorStart = virtualSectorId * MICROSECTOR_LENGTH;
                    float sectorEnd = (virtualSectorId + 1) * MICROSECTOR_LENGTH;
                    float timeStart = 0.0f;
                    float timeEnd = BestMicroSectors[virtualSectorId].Value;
                    float lowerRefDist = lapDistPct < playerLapDistPct && floor < ceil ? lapDistPct : playerLapDistPct;
                    float upperRefDist = lapDistPct >= playerLapDistPct || floor >= ceil ? lapDistPct : playerLapDistPct;

                    // Both cars are in same sector.
                    if (floor == ceil - 1)
                    {
                        sectorEnd = upperRefDist - sectorStart;
                        sectorStart = lowerRefDist - sectorStart;
                        timeStart = timeEnd * (sectorStart / MICROSECTOR_LENGTH);
                        timeEnd = timeEnd * (sectorEnd / MICROSECTOR_LENGTH);
                    }
                    // Lower sector bound. Interpolate between first and second point.
                    else if (sectorId == floor && sectorId != ceil - 1)
                    {
                        sectorStart = lowerRefDist - sectorStart;
                        timeStart = timeEnd * (sectorStart / MICROSECTOR_LENGTH);
                    }
                    // Upper sector bound. Interpolate between last and second to last point.
                    else if (sectorId == virtualCeil - 1)
                    {
                        sectorEnd = upperRefDist - sectorStart;
                        timeEnd = timeEnd * (sectorEnd / MICROSECTOR_LENGTH);
                    }

                    interval += (timeEnd - timeStart);
                }
            });
            return interval;
        }

        public float GetInterval(float lapDistPct, float playerLapDistPct)
        {
            float interval = 0;

            if (RecordedSectors == MICROSECTOR_COUNT)
            {
                bool wrap = Math.Abs(lapDistPct - playerLapDistPct) >= 0.5f;
                int floor = 0;
                int ceil = 0;

                // Check if car is in front of player
                if (lapDistPct > playerLapDistPct || wrap)
                {
                    floor = GetSectorId(playerLapDistPct, true);
                    ceil = GetSectorId(lapDistPct, false);
                    interval = CalculateIntervalFromSectors(lapDistPct, playerLapDistPct, floor, ceil);
                }
                else
                {
                    floor = GetSectorId(lapDistPct, true);
                    ceil = GetSectorId(playerLapDistPct, false);
                    interval = -CalculateIntervalFromSectors(lapDistPct, playerLapDistPct, floor, ceil);
                }

                if (interval > OptimalLapTime / 2)
                {
                    interval -= OptimalLapTime;
                }
                else if (-interval > OptimalLapTime / 2)
                {
                    interval += OptimalLapTime;
                }
            }

            return interval;
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
            int lastSectorIdx = GetSectorId(lastLapDistPct < 0.0f ? 0.01f : lastLapDistPct, true);

            if (lastLapDistPct != lapDistPct)
            {
                if (sectorIdx > lastSectorIdx ||
                    (sectorIdx == 0 && sectorIdx < lastSectorIdx))
                {
                    sectorEnd = (lastSectorIdx + 1) * MICROSECTOR_LENGTH;
                    correction = (sectorEnd - lastLapDistPct) / MICROSECTOR_LENGTH;

                    MicroSectors[lastSectorIdx].Value += (float)Math.Round(elapsedTime * correction, 3);
                    // Only change status if MicroSector is not initialized. This prevents
                    // incomplete sectors to be used in interval calculation. Incomplete sectors
                    // are set to NotComplete.
                    if (MicroSectors[lastSectorIdx].Status != MicroSectorStatus.Initialized)
                    {
                        MicroSectors[lastSectorIdx].Status = MicroSectorStatus.Complete;
                        if (RecordedSectors < MICROSECTOR_COUNT)
                        {
                            OptimalLapTime += MicroSectors[sectorIdx].Value;
                        }
                    }
                    else
                    {
                        MicroSectors[lastSectorIdx].Status = MicroSectorStatus.NotComplete;
                    }

                    if (MicroSectors[sectorIdx].Status == MicroSectorStatus.Complete &&
                        (BestMicroSectors[sectorIdx].Value == 0.0f || BestMicroSectors[sectorIdx].Value > MicroSectors[sectorIdx].Value))
                    {
                        if (BestMicroSectors[sectorIdx].Value > MicroSectors[sectorIdx].Value)
                        {
                            OptimalLapTime -= BestMicroSectors[sectorIdx].Value - MicroSectors[sectorIdx].Value;
                        }
                        BestMicroSectors[sectorIdx].Value = MicroSectors[sectorIdx].Value;
                        BestMicroSectors[sectorIdx].Status = MicroSectorStatus.Complete;
                        RecordedSectors++;
                    }

                    MicroSectors[sectorIdx].Value = 0;

                    sectorStart = sectorIdx * MICROSECTOR_LENGTH;
                    correction = (lapDistPct - sectorStart) / MICROSECTOR_LENGTH;

                    MicroSectors[sectorIdx].Value += (float)Math.Round(elapsedTime * correction, 3);
                    MicroSectors[sectorIdx].Status = MicroSectorStatus.NotComplete;
                } 
                else if (sectorIdx == lastSectorIdx)
                {
                    MicroSectors[sectorIdx].Value += (float)Math.Round(elapsedTime, 3);

                    // Only change status if MicroSector is not initialized. This prevents
                    // incomplete sectors to be used in interval calculation.
                    if (MicroSectors[sectorIdx].Status != MicroSectorStatus.Initialized)
                    {
                        MicroSectors[sectorIdx].Status = MicroSectorStatus.NotComplete;
                    }
                }
            }
        }
    }
}
