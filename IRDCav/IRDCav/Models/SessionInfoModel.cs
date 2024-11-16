namespace IRDCav.Models
{
    public class SessionInfoModel
    {
        public string TrackName { get; set; } = string.Empty;
        public string AirTemp { get; set; } = string.Empty;
        public string SurfaceTemp { get; set; } = string.Empty;
        public string Precipitation { get; set; } = string.Empty;
        public string Humidity { get; set; } = string.Empty;
        public string LapsString { get; set; } = string.Empty;
        public string TimeString { get; set; } = string.Empty;
        public string DriverClassName { get; set; } = string.Empty;
        public string IncidentCount { get; set; } = string.Empty;
        public string SessionType { get; set; } = string.Empty;

        public int ClassCount { get; set; }
        public int LapsRemain { get; set; }
        public int LapsTotal { get; set; }
        public int SOF { get; set; }
        public int DriverCount { get; set; }
        public int NumCarClasses { get; set; }

        public double TimeRemain { get; set; }
        public double TimeTotal { get; set; }
    }
}
