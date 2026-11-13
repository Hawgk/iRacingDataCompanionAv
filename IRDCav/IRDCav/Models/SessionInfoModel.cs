namespace IRDCav.Models
{
    /// <summary>Session summary shown in the overlay headers.</summary>
    public sealed record SessionInfoModel
    {
        public string AirTemp { get; init; } = string.Empty;
        public string SurfaceTemp { get; init; } = string.Empty;
        public string Precipitation { get; init; } = string.Empty;
        public string Humidity { get; init; } = string.Empty;

        // TODO: Laps remaining is not reported reliably by iRacing and must be calculated internally.
        public string LapsString { get; init; } = string.Empty;

        public string DriverClassName { get; init; } = string.Empty;
        public string IncidentCount { get; init; } = string.Empty;

        /// <summary>"P" for practice, "Q" for qualifying, "R" for race.</summary>
        public string SessionType { get; init; } = string.Empty;

        public bool IsRace => SessionType == "R";

        /// <summary>Where the wind comes from relative to the player's car, clockwise in degrees. 0 is a headwind.</summary>
        public double WindAngle { get; init; }

        /// <summary>Wind speed in km/h.</summary>
        public double WindSpeed { get; init; }

        public int SOF { get; init; }
        public int DriverCount { get; init; }
        public double TimeRemain { get; init; }
    }
}
