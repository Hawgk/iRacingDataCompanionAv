namespace IRDCav.Telemetry
{
    /// <summary>The player's lap as iRacing times it, including the delta to the all-time best lap.</summary>
    public readonly record struct PlayerLapTelemetry
    {
        /// <summary>Seconds since session start; see <see cref="SessionTelemetry.SessionTime"/>.</summary>
        public double SessionTime { get; init; }

        /// <summary>0 at the start/finish line up to 1; negative while the car is not on track.</summary>
        public float LapDistPct { get; init; }

        /// <summary>Seconds since the start of the lap in progress.</summary>
        public float CurrentLapTime { get; init; }

        /// <summary>Seconds of the last finished lap; iRacing updates it a moment after the line.</summary>
        public float LastLapTime { get; init; }

        /// <summary>
        /// Seconds the lap in progress is behind (positive) the all-time best lap at the same point
        /// of the track; the reference is the lap iRacing keeps in its lap files.
        /// </summary>
        public float DeltaToBestLap { get; init; }

        /// <summary>False while iRacing has no best lap to compare with, e.g. before the first lap.</summary>
        public bool IsDeltaToBestLapValid { get; init; }
    }
}
