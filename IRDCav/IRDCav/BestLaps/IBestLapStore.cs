using System.Diagnostics.CodeAnalysis;

namespace IRDCav.BestLaps
{
    /// <summary>The player's best laps per track and car.</summary>
    public interface IBestLapStore
    {
        /// <summary>The saved best lap of this car at this track; false when there is none.</summary>
        bool TryLoad(CarAtTrack car, [NotNullWhen(true)] out BestLapRecord? record);

        /// <summary>The fastest saved lap time at this car's track of any car driven in its class; 0 when there is none.</summary>
        float ClassBest(CarAtTrack car);

        /// <summary>Replaces the saved best lap of the record's car at its track.</summary>
        void Save(BestLapRecord record);
    }
}
