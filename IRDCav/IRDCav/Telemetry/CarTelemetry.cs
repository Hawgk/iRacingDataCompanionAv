using static IRSDKSharper.IRacingSdkEnum;

namespace IRDCav.Telemetry
{
    /// <summary>Per-car values from the iRacing CarIdx* telemetry arrays.</summary>
    public readonly record struct CarTelemetry
    {
        public int Id { get; init; }
        public int Lap { get; init; }
        public float LapDistPct { get; init; }
        public bool OnPitRoad { get; init; }
        public int Position { get; init; }
        public int ClassPosition { get; init; }
        public float LastLapTime { get; init; }
        public TrkLoc TrackLocation { get; init; }
        public TrkSurf TrackSurface { get; init; }
        public Flags SessionFlags { get; init; }

        /// <summary>Index of the tire compound the car runs; -1 when unknown.</summary>
        public int TireCompound { get; init; }
    }
}
