using IRDCav.LapFiles;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IRDCav.Services
{
    /// <summary>What the sector view needs to know about the player's car and the track.</summary>
    /// <param name="SessionCarClassId">The car's class in this session; used when its lap file names none.</param>
    /// <param name="SectorStarts">Where iRacing's sectors of the track start, as lap fractions from 0.</param>
    internal sealed record LapSectorsSetup(LapFileKey Key, int SessionCarClassId, IReadOnlyList<float> SectorStarts)
    {
        public static readonly LapSectorsSetup Unknown = new(LapFileKey.Unknown, 0, Array.Empty<float>());

        public bool IsSameAs(LapSectorsSetup other) =>
            Key == other.Key && SessionCarClassId == other.SessionCarClassId && SectorStarts.SequenceEqual(other.SectorStarts);
    }
}
