using IRDCav.Models;
using IRDCav.Telemetry;
using IRSDKSharper;
using System.Collections.Generic;
using System.Linq;
using static IRSDKSharper.IRacingSdkSessionInfo.DriverInfoModel;

namespace IRDCav.Services
{
    /// <summary>Builds the header summary from the session info YAML and live telemetry.</summary>
    internal sealed class SessionInfoBuilder
    {
        private const string UnlimitedIncidents = "unlimited";

        public SessionInfoModel Build(IRacingSdkSessionInfo sessionInfo, SessionTelemetry session)
        {
            var weekend = sessionInfo.WeekendInfo;
            List<DriverModel> drivers = sessionInfo.DriverInfo.Drivers ?? new List<DriverModel>();
            DriverModel? player = drivers.Find(driver => driver.CarIdx == sessionInfo.DriverInfo.DriverCarIdx);
            List<DriverModel> playerClass = drivers.Where(driver => driver.CarClassID == player?.CarClassID).ToList();

            return new SessionInfoModel
            {
                AirTemp = FirstWord(weekend.TrackAirTemp) + "°C",
                SurfaceTemp = FirstWord(weekend.TrackSurfaceTemp) + "°C",
                Precipitation = FirstWord(weekend.TrackPrecipitation) + "%",
                Humidity = FirstWord(weekend.TrackRelativeHumidity) + "%",
                DriverClassName = player?.CarClassShortName ?? string.Empty,
                IncidentCount = FormatIncidents(player, weekend.WeekendOptions.IncidentLimit),
                SessionType = SessionType(sessionInfo, session.SessionNum),
                SOF = playerClass.Count > 0 ? (int)playerClass.Average(driver => driver.IRating) : 0,
                DriverCount = playerClass.Count,
                TimeRemain = session.TimeRemain,
            };
        }

        // iRacing reports values with units, e.g. "24.3 C".
        private static string FirstWord(string value) => value.Split(" ")[0];

        private static string FormatIncidents(DriverModel? player, string incidentLimit)
        {
            string limit = incidentLimit == UnlimitedIncidents ? "-" : incidentLimit;
            return $"{player?.CurDriverIncidentCount}/{limit}";
        }

        private static string SessionType(IRacingSdkSessionInfo sessionInfo, int sessionNum)
        {
            if (sessionNum <= 0)
            {
                return string.Empty;
            }
            string sessionName = sessionInfo.SessionInfo.Sessions[sessionNum].SessionName;
            return sessionName.Length > 0 ? sessionName[0].ToString() : string.Empty;
        }
    }
}
