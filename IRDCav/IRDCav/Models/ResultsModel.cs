namespace IRDCav.Models
{
    public class ResultsModel
    {
        public bool IsFastest { get; set; }

        public string Name { get; set; } = string.Empty;
        public string Class { get; set; } = string.Empty;
        public string CarPath { get; set; } = string.Empty;
        public string License { get; set; } = string.Empty;
        public string ClassColor { get; set; } = string.Empty;

        public float FastestLapTime { get; set; }
        public float LastLapTime { get; set; }
        public float Gap { get; set; }

        public int Rating { get; set; }
        public int ClassPosition { get; set; }
        public int LapsCompleted { get; set; }
        public int Id { get; set; }
    }
}
