namespace IRDCav.Models
{
    public class LiveDataModel
    {
        public int Id { get; set; }
        public int Class { get; set; }
        public float LapDistPct { get; set; }
        public bool OnPitRoad { get; set; }
        public int Position { get; set; }
        public int ClassPosition { get; set; }
        public float Interval { get; set; }
        public bool ConsiderForRelative {  get; set; }
        public float LastLapTime { get; set; }
        public float BestLapTime { get; set; }
        public int BestLapNum { get; set; }
    }
}
