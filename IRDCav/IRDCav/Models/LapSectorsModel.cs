using System.Collections.Generic;
using System.Linq;

namespace IRDCav.Models
{
    /// <summary>One sector of the best lap and how the lap in progress compares to it.</summary>
    /// <param name="Number">1 for the sector after the start/finish line.</param>
    /// <param name="BestTime">Seconds on the best lap; 0 when unknown.</param>
    /// <param name="Delta">Seconds against <paramref name="BestTime"/>, rounded to milliseconds; NaN when there is nothing to compare.</param>
    public sealed record SectorModel(int Number, float BestTime, float Delta);

    /// <summary>The player's best lap with this car at this track split into equal sectors, with the deltas of the lap being driven.</summary>
    public sealed record LapSectorsModel
    {
        public const int SectorCount = 5;

        public static readonly LapSectorsModel Empty = new();

        /// <summary>The player's all-time best lap with this car at this track in seconds; 0 when there is none.</summary>
        public float BestLapTime { get; init; }

        /// <summary>The player's all-time best lap at this track with any car of the current class in seconds; 0 when there is none.</summary>
        public float ClassBestLapTime { get; init; }

        /// <summary>Always <see cref="SectorCount"/> sectors, in track order.</summary>
        public IReadOnlyList<SectorModel> Sectors { get; init; } = Enumerable.Range(1, SectorCount)
            .Select(number => new SectorModel(number, 0, float.NaN))
            .ToArray();
    }
}
