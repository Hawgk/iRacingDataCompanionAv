namespace IRDCav.Models
{
    /// <summary>Immutable display data of one row in the relative or standings view.</summary>
    public sealed record RaceRow
    {
        private const string DefaultBackgroundColor = "#C0242423";

        public static readonly RaceRow Empty = new();

        public bool IsMe { get; init; }
        public bool IsFastest { get; init; }
        public bool IsBehind { get; init; }
        public bool IsInfront { get; init; }
        public bool OnPitRoad { get; init; }

        public string Name { get; init; } = string.Empty;
        public string CarNumber { get; init; } = string.Empty;
        public string License { get; init; } = string.Empty;
        public string ClassColor { get; init; } = DefaultBackgroundColor;
        public string LicenseColor { get; init; } = DefaultBackgroundColor;

        public int ClassPosition { get; init; }
        public int LapsCompleted { get; init; }

        public float Interval { get; init; }
        public float Gap { get; init; }
        public float LastLapTime { get; init; }
        public float FastestLapTime { get; init; }

        public static RaceRow From(RaceDataModel car) => new()
        {
            IsMe = car.IsMe,
            IsFastest = car.IsFastest,
            IsBehind = car.IsBehind,
            IsInfront = car.IsInfront,
            OnPitRoad = car.OnPitRoad,
            Name = car.Name,
            CarNumber = car.CarNumber,
            License = car.License,
            ClassColor = car.ClassColor,
            LicenseColor = car.LicenseColor,
            ClassPosition = car.ClassPosition,
            LapsCompleted = car.LapsCompleted,
            Interval = car.Interval,
            LastLapTime = car.LastLapTime,
            FastestLapTime = car.FastestLapTime,
        };
    }
}
