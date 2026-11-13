namespace IRDCav.Models
{
    public enum RaceRowKind
    {
        Car,
        Empty,
        ClassHeader,
    }

    /// <summary>Immutable display data of one row in the relative or standings view.</summary>
    public sealed record RaceRow
    {
        public static readonly RaceRow Empty = new() { Kind = RaceRowKind.Empty };

        public RaceRowKind Kind { get; init; } = RaceRowKind.Car;

        public bool IsMe { get; init; }
        public bool IsFastest { get; init; }
        public bool IsLapAhead { get; init; }
        public bool IsLapBehind { get; init; }
        public bool OnPitRoad { get; init; }

        public string Name { get; init; } = string.Empty;
        public string CarNumber { get; init; } = string.Empty;
        public string ClassName { get; init; } = string.Empty;
        public string License { get; init; } = string.Empty;

        /// <summary>"#RRGGBB", or empty when unknown.</summary>
        public string ClassColor { get; init; } = string.Empty;

        /// <summary>"#RRGGBB", or empty when unknown.</summary>
        public string LicenseColor { get; init; } = string.Empty;

        public int IRatingChange { get; init; }
        public bool HasIRatingChange { get; init; }

        public int ClassPosition { get; init; }
        public int LapsCompleted { get; init; }

        public float Interval { get; init; }
        public float Gap { get; init; }
        public float LastLapTime { get; init; }
        public float FastestLapTime { get; init; }

        public static RaceRow ClassHeader(string className, string classColor) => new()
        {
            Kind = RaceRowKind.ClassHeader,
            ClassName = className,
            ClassColor = classColor,
        };

        public static RaceRow From(RaceDataModel car) => new()
        {
            IsMe = car.IsMe,
            IsFastest = car.IsFastest,
            IsLapAhead = car.IsLapAhead,
            IsLapBehind = car.IsLapBehind,
            OnPitRoad = car.OnPitRoad,
            Name = car.Name,
            CarNumber = car.CarNumber,
            ClassName = car.ClassStr,
            License = car.License,
            ClassColor = car.ClassColor,
            LicenseColor = car.LicenseColor,
            IRatingChange = car.IRatingChange,
            HasIRatingChange = car.HasIRatingChange,
            ClassPosition = car.ClassPosition,
            LapsCompleted = car.LapsCompleted,
            Interval = car.Interval,
            LastLapTime = car.LastLapTime,
            FastestLapTime = car.FastestLapTime,
        };
    }
}
