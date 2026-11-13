using IRDCav.Telemetry;
using static IRSDKSharper.IRacingSdkEnum;
using static IRSDKSharper.IRacingSdkSessionInfo.DriverInfoModel;
using static IRSDKSharper.IRacingSdkSessionInfo.SessionInfoModel.SessionModel;

namespace IRDCav.Models
{
    /// <summary>
    /// Merged live and session data of one car. A default instance doubles as an empty display row.
    /// </summary>
    public class RaceDataModel
    {
        private const string UndefinedSdkColor = "0xundefined";

        public int Id { get; set; }
        public bool IsMe { get; set; }
        public bool IsFastest { get; set; }
        public bool IsPaceCar { get; set; }
        public bool OnPitRoad { get; set; }
        public bool ConsiderForRelative { get; set; }

        public string Name { get; set; } = string.Empty;
        public string ClassStr { get; set; } = string.Empty;
        public string CarNumber { get; set; } = string.Empty;
        public string License { get; set; } = string.Empty;
        /// <summary>"#RRGGBB", or empty when unknown.</summary>
        public string ClassColor { get; set; } = string.Empty;

        /// <summary>"#RRGGBB", or empty when unknown.</summary>
        public string LicenseColor { get; set; } = string.Empty;

        public int IRating { get; set; }

        /// <summary>Estimated iRating change if the race ended now; only valid with <see cref="HasIRatingChange"/>.</summary>
        public int IRatingChange { get; set; }
        public bool HasIRatingChange { get; set; }

        public int Position { get; set; }
        public int ClassPosition { get; set; }
        public int Lap { get; set; }
        public int LapsCompleted { get; set; }

        /// <summary>Laps this car is ahead of (positive) or behind (negative) the player.</summary>
        public int LapDelta { get; set; }

        public bool IsLapAhead => LapDelta > 0;
        public bool IsLapBehind => LapDelta < 0;

        public float LapDistPct { get; set; }
        public float LastLapDistPct { get; set; }
        public float LastLapTime { get; set; }
        public float EstLapTime { get; set; }
        public float FastestLapTime { get; set; }
        public float Interval { get; set; }
        public float Gap { get; set; }

        public TrkSurf TrackSurface { get; set; }
        public TrkLoc TrackLocation { get; set; }
        public Flags SessionFlags { get; set; }

        public MicroSectorModel MicroSectors { get; set; } = new MicroSectorModel();

        public void ApplyTelemetry(in CarTelemetry telemetry)
        {
            Id = telemetry.Id;
            Lap = telemetry.Lap;
            LapDistPct = telemetry.LapDistPct;
            OnPitRoad = telemetry.OnPitRoad;
            Position = telemetry.Position;
            ClassPosition = telemetry.ClassPosition;
            LastLapTime = telemetry.LastLapTime;
            TrackSurface = telemetry.TrackSurface;
            TrackLocation = telemetry.TrackLocation;
            SessionFlags = telemetry.SessionFlags;
        }

        public void ApplyDriver(DriverModel driver)
        {
            Id = driver.CarIdx;
            Name = driver.UserName;
            ClassStr = driver.CarClassShortName;
            CarNumber = driver.CarNumber;
            EstLapTime = driver.CarClassEstLapTime;
            IRating = driver.IRating;
            License = ((float)driver.IRating / 1000).ToString("0.0") + "k";
            IsPaceCar = driver.CarIsPaceCar > 0;
            ClassColor = ToColor(driver.CarClassColor);
            LicenseColor = ToColor(driver.LicColor);
        }

        public void ApplyPosition(PositionModel position)
        {
            Position = position.Position;
            ClassPosition = position.ClassPosition + 1;
            LapsCompleted = position.LapsComplete;
            FastestLapTime = position.FastestTime;
            LastLapTime = position.LastTime;
        }

        // iRacing reports colors as "0xRRGGBB" or "0xundefined".
        private static string ToColor(string sdkColor) =>
            sdkColor == UndefinedSdkColor || sdkColor.Length < 3 ? string.Empty : "#" + sdkColor.Substring(2);
    }
}
