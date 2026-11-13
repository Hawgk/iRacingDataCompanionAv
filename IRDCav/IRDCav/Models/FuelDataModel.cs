namespace IRDCav.Models
{
    public sealed record FuelDataModel
    {
        public float Level { get; init; }

        public float MinConsumption { get; init; }
        public float AvgConsumption { get; init; }
        public float MaxConsumption { get; init; }
        public float LastConsumption { get; init; }

        public float FuelPerMinuteMin { get; init; }
        public float FuelPerMinuteAvg { get; init; }
        public float FuelPerMinuteMax { get; init; }

        /// <summary>Fuel left at session end; negative values are the amount to refuel.</summary>
        public float RefuelMin { get; init; }
        public float RefuelAvg { get; init; }
        public float RefuelMax { get; init; }
    }
}
