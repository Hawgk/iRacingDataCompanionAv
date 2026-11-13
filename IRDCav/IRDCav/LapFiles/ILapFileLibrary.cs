using System;

namespace IRDCav.LapFiles
{
    /// <summary>The best laps iRacing keeps for the player, per track and car.</summary>
    public interface ILapFileLibrary
    {
        /// <summary>The best lap of this car at this track; false when there is none or it can't be read.</summary>
        bool TryLoadBest(LapFileKey key, out IRacingLapFile lap);

        /// <summary>When the best lap file was last written; <see cref="DateTime.MinValue"/> when there is none.</summary>
        DateTime LastWritten(LapFileKey key);

        /// <summary>The fastest best lap at the key's track of any of the player's cars in this class; 0 when there is none.</summary>
        float ClassBest(LapFileKey key, int carClassId);
    }
}
