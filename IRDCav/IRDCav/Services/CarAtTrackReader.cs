using IRDCav.BestLaps;
using IRSDKSharper;

namespace IRDCav.Services
{
    /// <summary>Reads the player's car and the track from the session info YAML.</summary>
    internal static class CarAtTrackReader
    {
        public static CarAtTrack Read(IRacingSdkSessionInfo sessionInfo)
        {
            var weekend = sessionInfo.WeekendInfo;
            var player = sessionInfo.DriverInfo.Drivers?.Find(driver => driver.CarIdx == sessionInfo.DriverInfo.DriverCarIdx);
            if (weekend == null || player == null)
            {
                return CarAtTrack.Unknown;
            }
            return new CarAtTrack(
                weekend.TrackDisplayName ?? string.Empty,
                weekend.TrackConfigName ?? string.Empty,
                player.CarScreenName ?? string.Empty,
                player.CarClassShortName ?? string.Empty);
        }
    }
}
