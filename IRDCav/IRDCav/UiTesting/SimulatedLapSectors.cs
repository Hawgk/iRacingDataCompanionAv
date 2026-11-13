using IRDCav.Models;
using System;
using System.Linq;

namespace IRDCav.UiTesting
{
    /// <summary>
    /// Sector view data of the simulated player: a fixed best lap and deltas that differ per lap and
    /// sector, with gains, losses and an exact tie. Like live, a delta stays until its sector is driven again.
    /// </summary>
    internal static class SimulatedLapSectors
    {
        // Six sectors of different length, like iRacing's sectors of a real track.
        private static readonly float[] SectorShares = { 0.155f, 0.21f, 0.11f, 0.16f, 0.18f, 0.185f };
        private const float MaxDelta = 0.4f;
        private const int TiedSector = 2;
        private const float ClassAdvantage = 0.613f;
        private const float AveragePaceLoss = 1.284f;

        public static LapSectorsModel From(SimulatedRace race)
        {
            RaceDataModel player = race.Player;
            int drivenSector = DrivenSector(player.LapDistPct);
            float[] best = SectorShares.Select(share => player.EstLapTime * share).ToArray();
            return new LapSectorsModel
            {
                BestLapTime = best.Sum(),
                ClassBestLapTime = best.Sum() - ClassAdvantage,
                AverageLapTime = best.Sum() + AveragePaceLoss,
                Sectors = Enumerable.Range(0, SectorShares.Length)
                    .Select(sector => new SectorModel(sector + 1, best[sector], ShownDelta(player.Lap, sector, drivenSector)))
                    .ToArray(),
            };
        }

        private static int DrivenSector(float lapDistPct)
        {
            float sectorEnd = 0;
            for (int sector = 0; sector < SectorShares.Length; sector++)
            {
                sectorEnd += SectorShares[sector];
                if (lapDistPct < sectorEnd)
                {
                    return sector;
                }
            }
            return SectorShares.Length - 1;
        }

        /// <summary>Sectors already driven in this lap show its delta, those still ahead the previous lap's.</summary>
        private static float ShownDelta(int lap, int sector, int drivenSector) => sector == drivenSector
            ? float.NaN
            : Delta(sector < drivenSector ? lap : lap - 1, sector);

        private static float Delta(int lap, int sector) =>
            sector == TiedSector && lap % 2 == 0 ? 0 : MathF.Round(MaxDelta * MathF.Sin(lap * 7 + sector * 3), 3);
    }
}
