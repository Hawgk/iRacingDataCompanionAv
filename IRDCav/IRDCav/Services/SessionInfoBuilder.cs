using IRDCav.Models;
using IRDCav.Telemetry;
using IRSDKSharper;
using System.Collections.Generic;
using System.Globalization;
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
                AirTemp = OneDecimal(FirstWord(weekend.TrackAirTemp)) + "°",
                SurfaceTemp = OneDecimal(FirstWord(weekend.TrackSurfaceTemp)) + "°",
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

        /// <summary>"21.64" becomes "21.6"; a value that is not a number is kept as it is.</summary>
        private static string OneDecimal(string number) =>
            double.TryParse(number, NumberStyles.Float, CultureInfo.InvariantCulture, out double value)
                ? value.ToString("0.0", CultureInfo.InvariantCulture)
                : number;

        private static string FormatIncidents(DriverModel? player, string incidentLimit)
        {
            string limit = incidentLimit == UnlimitedIncidents ? "-" : incidentLimit;
            return $"{player?.CurDriverIncidentCount}/{limit}";
        }

        private static string SessionType(IRacingSdkSessionInfo sessionInfo, int sessionNum)
        {
            var sessions = sessionInfo.SessionInfo.Sessions;
            if (sessionNum < 0 || sessionNum >= sessions.Count)
            {
                return string.Empty;
            }
            return SessionLetter(sessions[sessionNum].SessionType);
        }

        // iRacing session types include "Practice", "Open Qualify", "Lone Qualify", "Warmup" and "Race".
        private static string SessionLetter(string sessionType) => sessionType switch
        {
            _ when sessionType.Contains("Race") => "R",
            _ when sessionType.Contains("Qualify") => "Q",
            _ => "P",
        };
    }
}
