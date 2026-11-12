namespace IRDCav.Telemetry
{
    /// <summary>Session-wide and player scalar telemetry values.</summary>
    public readonly record struct SessionTelemetry
    {
        public int SessionNum { get; init; }

        /// <summary>Seconds since session start. Always counts up, unlike <see cref="TimeRemain"/>.</summary>
        public double SessionTime { get; init; }

        public double TimeRemain { get; init; }
        public float FuelLevel { get; init; }
        public bool IsOnTrack { get; init; }
    }
}
