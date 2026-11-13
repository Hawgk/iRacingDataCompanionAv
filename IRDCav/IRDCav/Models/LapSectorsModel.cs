using System;
using System.Collections.Generic;

namespace IRDCav.Models
{
    /// <summary>One sector of the best lap and how the lap in progress compares to it.</summary>
    /// <param name="Number">1 for the sector after the start/finish line.</param>
    /// <param name="BestTime">Seconds on the best lap; 0 when unknown.</param>
    /// <param name="Delta">Seconds against <paramref name="BestTime"/>, rounded to milliseconds; NaN when there is nothing to compare.</param>
    public sealed record SectorModel(int Number, float BestTime, float Delta);

    /// <summary>
    /// The player's all-time best lap with this car at this track in iRacing's sectors of the track,
    /// with the deltas of the lap being driven.
    /// </summary>
    public sealed record LapSectorsModel
    {
        public static readonly LapSectorsModel Empty = new();

        /// <summary>The player's all-time best lap with this car at this track in seconds; 0 when there is none.</summary>
        public float BestLapTime { get; init; }

        /// <summary>The player's all-time best lap at this track with any car of the current class in seconds; 0 when there is none.</summary>
        public float ClassBestLapTime { get; init; }

        /// <summary>The average of the player's last five lap times in seconds, every lap included; 0 before the first lap.</summary>
        public float AverageLapTime { get; init; }

        /// <summary>iRacing's sectors of the track in track order; their number differs from track to track.</summary>
        public IReadOnlyList<SectorModel> Sectors { get; init; } = Array.Empty<SectorModel>();
    }
}
