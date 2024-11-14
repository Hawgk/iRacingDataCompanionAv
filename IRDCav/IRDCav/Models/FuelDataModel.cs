namespace IRDCav.Models
{
    public class FuelDataModel
    {
        public float Level { get; set; }

        public float MinConsumption { get; set; }
        public float AvgConsumption { get; set; }
        public float MaxConsumption { get; set; }
        public float LastConsumption { get; set; }

        public float FuelPerMinuteMin { get; set; }
        public float FuelPerMinuteAvg { get; set; }
        public float FuelPerMinuteMax { get; set; }

        public float RefuelMin { get; set; }
        public float RefuelAvg { get; set; }
        public float RefuelMax { get; set; }
    }
}
