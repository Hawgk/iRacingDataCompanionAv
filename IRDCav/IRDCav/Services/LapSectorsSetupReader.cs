using IRDCav.LapFiles;
using IRSDKSharper;
using System;
using System.Linq;

namespace IRDCav.Services
{
    /// <summary>Reads the player's car, the track and iRacing's sector boundaries from the session info YAML.</summary>
    internal static class LapSectorsSetupReader
    {
        public static LapSectorsSetup Read(IRacingSdkSessionInfo sessionInfo)
        {
            var weekend = sessionInfo.WeekendInfo;
            var driverInfo = sessionInfo.DriverInfo;
            var player = driverInfo.Drivers?.Find(driver => driver.CarIdx == driverInfo.DriverCarIdx);
            if (weekend == null || player == null)
            {
                return LapSectorsSetup.Unknown;
            }

            var key = new LapFileKey(weekend.TrackName ?? string.Empty, player.CarPath ?? string.Empty, driverInfo.DriverUserID);
            return new LapSectorsSetup(key, player.CarClassID, SectorStarts(sessionInfo));
        }

        private static float[] SectorStarts(IRacingSdkSessionInfo sessionInfo) =>
            sessionInfo.SplitTimeInfo?.Sectors?
                .OrderBy(sector => sector.SectorNum)
                .Select(sector => (float)sector.SectorStartPct)
                .ToArray()
            ?? Array.Empty<float>();
    }
}
