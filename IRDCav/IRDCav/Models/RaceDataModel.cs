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
        private const string DefaultBackgroundColor = "#C0242423";
        private const string UndefinedLicenseColor = "#60FF7247";
        private const string TranslucentAlpha = "#60";

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
        public string ClassColor { get; set; } = DefaultBackgroundColor;
        public string LicenseColor { get; set; } = DefaultBackgroundColor;

        public int Position { get; set; }
        public int ClassPosition { get; set; }
        public int Lap { get; set; }
        public int LapsCompleted { get; set; }

        /// <summary>Laps this car is ahead of (positive) or behind (negative) the player.</summary>
        public int LapDelta { get; set; }

        public bool IsBehind => LapDelta > 0;
        public bool IsInfront => LapDelta < 0;

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

        /// <summary>Shallow copy for handing to the UI thread while the original keeps being updated.</summary>
        public RaceDataModel Snapshot() => (RaceDataModel)MemberwiseClone();

        public void ApplyTelemetry(CarTelemetry telemetry)
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
            License = ((float)driver.IRating / 1000).ToString("0.0") + "k";
            IsPaceCar = driver.CarIsPaceCar > 0;
            ClassColor = ToTranslucentColor(driver.CarClassColor);
            LicenseColor = ToLicenseColor(driver.LicColor);
        }

        public void ApplyPosition(PositionModel position)
        {
            Position = position.Position;
            ClassPosition = position.ClassPosition + 1;
            LapsCompleted = position.LapsComplete;
            FastestLapTime = position.FastestTime;
            LastLapTime = position.LastTime;
        }

        private static string ToLicenseColor(string sdkColor) =>
            sdkColor == "0xundefined" ? UndefinedLicenseColor : ToTranslucentColor(sdkColor);

        // iRacing reports colors as "0xRRGGBB".
        private static string ToTranslucentColor(string sdkColor) => TranslucentAlpha + sdkColor.Substring(2);
    }
}
