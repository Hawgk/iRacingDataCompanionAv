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

        /// <summary>The player's best lap time of the session in seconds, as iRacing counts it; 0 or less when there is none.</summary>
        public float BestLapTime { get; init; }
        public bool IsOnTrack { get; init; }

        /// <summary>Compass direction the wind comes from, in radians.</summary>
        public float WindDirection { get; init; }

        /// <summary>Wind speed in m/s.</summary>
        public float WindSpeed { get; init; }

        /// <summary>Compass heading of the player's car, in radians.</summary>
        public float Heading { get; init; }
    }
}
