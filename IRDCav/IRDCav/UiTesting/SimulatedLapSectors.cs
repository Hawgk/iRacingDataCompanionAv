using IRDCav.Models;
using System;
using System.Linq;

namespace IRDCav.UiTesting
{
    /// <summary>
    /// Sector view data of the simulated player: a fixed best lap and deltas that differ per lap and
    /// sector, with gains, losses and an exact tie, for every sector already driven in the current lap.
    /// </summary>
    internal static class SimulatedLapSectors
    {
        private static readonly float[] SectorShares = { 0.22f, 0.18f, 0.21f, 0.19f, 0.20f };
        private const float MaxDelta = 0.4f;
        private const int TiedSector = 2;

        public static LapSectorsModel From(SimulatedRace race)
        {
            RaceDataModel player = race.Player;
            int sectorsDriven = (int)(player.LapDistPct * LapSectorsModel.SectorCount);
            float[] best = SectorShares.Select(share => player.EstLapTime * share).ToArray();
            return new LapSectorsModel
            {
                BestLapTime = best.Sum(),
                Sectors = Enumerable.Range(0, LapSectorsModel.SectorCount)
                    .Select(sector => new SectorModel(sector + 1, best[sector], sector < sectorsDriven ? Delta(player.Lap, sector) : float.NaN))
                    .ToArray(),
            };
        }

        private static float Delta(int lap, int sector) =>
            sector == TiedSector && lap % 2 == 0 ? 0 : MathF.Round(MaxDelta * MathF.Sin(lap * 7 + sector * 3), 3);
    }
}
