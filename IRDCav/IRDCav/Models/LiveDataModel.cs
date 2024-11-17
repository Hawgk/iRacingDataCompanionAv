using static IRSDKSharper.IRacingSdkEnum;

namespace IRDCav.Models
{
    public class LiveDataModel
    {
        public bool OnPitRoad { get; set; }
        public bool ConsiderForRelative { get; set; }

        public int Id { get; set; }
        public int Class { get; set; }
        public int Position { get; set; }
        public int ClassPosition { get; set; }
        public int BestLapNum { get; set; }
        public int LapDelta { get; set; }
        public int TrackLocation { get; set; }
        public int TrackSurface { get; set; }

        public float LapDistPct { get; set; }
        public float Interval { get; set; }
        public float LastLapTime { get; set; }
        public float BestLapTime { get; set; }
        public float EstTime { get; set; }

        public uint SessionFlags { get; set; }
    }
}
