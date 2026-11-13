using IRDCav.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IRDCav.Services
{
    /// <summary>Sector times of one lap, built from its microsectors. A time of 0 means the sector is unknown.</summary>
    internal static class SectorTimes
    {
        private const int SectorCount = LapSectorsModel.SectorCount;
        private const int MicroSectorsPerSector = MicroSectorModel.MICROSECTOR_COUNT / SectorCount;

        public static float[] Unknown() => new float[SectorCount];

        public static float[] NoDeltas() => Enumerable.Repeat(float.NaN, SectorCount).ToArray();

        /// <summary>Saved sector times; all unknown when their number doesn't match, e.g. from another version.</summary>
        public static float[] FromSaved(IReadOnlyList<float> sectors) =>
            sectors.Count == SectorCount ? sectors.Select(time => Math.Max(time, 0)).ToArray() : Unknown();

        /// <summary>Sums the microsectors of each sector; 0 for a sector with a microsector that was not driven.</summary>
        public static float[] Of(IReadOnlyList<float> microSectors)
        {
            float[] sectors = Unknown();
            for (int sector = 0; sector < SectorCount; sector++)
            {
                IEnumerable<float> parts = microSectors.Skip(sector * MicroSectorsPerSector).Take(MicroSectorsPerSector);
                sectors[sector] = parts.All(time => time > 0) ? parts.Sum() : 0;
            }
            return sectors;
        }

        /// <summary>Time lost (positive) or gained (negative) per sector, in whole milliseconds; NaN where a time is unknown.</summary>
        public static float[] Deltas(float[] times, float[] best) =>
            Enumerable.Range(0, SectorCount)
                .Select(sector => times[sector] > 0 && best[sector] > 0
                    ? MathF.Round(times[sector] - best[sector], 3)
                    : float.NaN)
                .ToArray();

        /// <summary>The sector a microsector belongs to; false for -1, which stands for "not on track".</summary>
        public static bool TrySectorOf(int microSector, out int sector)
        {
            sector = microSector / MicroSectorsPerSector;
            return microSector >= 0 && sector < SectorCount;
        }
    }
}
