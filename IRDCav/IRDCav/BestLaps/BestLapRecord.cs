using System;
using System.Collections.Generic;

namespace IRDCav.BestLaps
{
    /// <summary>The player's best lap with one car at one track, as saved in its file.</summary>
    public sealed record BestLapRecord
    {
        public string Track { get; init; } = string.Empty;
        public string TrackConfig { get; init; } = string.Empty;
        public string Car { get; init; } = string.Empty;

        /// <summary>Every class the car has been driven in at this track; used to find the best lap of a class.</summary>
        public IReadOnlyList<string> CarClasses { get; init; } = Array.Empty<string>();

        /// <summary>iRacing's lap time in seconds.</summary>
        public float LapTime { get; init; }

        /// <summary>Sector times in seconds, from the start/finish line on; 0 for a sector that was not recorded.</summary>
        public IReadOnlyList<float> Sectors { get; init; } = Array.Empty<float>();

        /// <summary>When the lap was driven, in UTC.</summary>
        public DateTime Recorded { get; init; }
    }
}
