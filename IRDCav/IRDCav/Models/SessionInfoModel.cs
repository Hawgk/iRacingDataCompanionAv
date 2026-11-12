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

        /// <summary>First letter of the session name, e.g. "P", "Q" or "R".</summary>
        public string SessionType { get; init; } = string.Empty;

        public int SOF { get; init; }
        public int DriverCount { get; init; }
        public double TimeRemain { get; init; }
    }
}
