using IRDCav.Configuration;
using IRDCav.Models;

namespace IRDCav.ViewModels
{
    /// <summary>One value of the fuel table, computed for one consumption estimate.</summary>
    public sealed record FuelEstimate
    {
        public string Value { get; init; } = FuelSummary.Unknown;

        /// <summary>"MIN", "AVG" or "MAX", shown after the value.</summary>
        public string Caption { get; init; } = string.Empty;

        public bool NeedsRefuel { get; init; }

        /// <summary>The estimate chosen in the configuration, shown large.</summary>
        public bool IsSelected { get; init; }
    }

    /// <summary>
    /// The fuel figures a driver checks mid-race: what is left in the tank, and for minimum, average
    /// and maximum consumption how much to add at the next stop and how many laps the tank lasts.
    /// </summary>
    public sealed record FuelSummary
    {
        public const string Unknown = "–";

        public string Tank { get; init; } = Unknown;

        /// <summary>Liters to add to reach the finish; negative when that much is spare.</summary>
        public FuelEstimate RefuelMinimum { get; init; } = new();
        public FuelEstimate RefuelAverage { get; init; } = new();
        public FuelEstimate RefuelMaximum { get; init; } = new();

        public FuelEstimate LapsMinimum { get; init; } = new();
        public FuelEstimate LapsAverage { get; init; } = new();
        public FuelEstimate LapsMaximum { get; init; } = new();

        public static FuelSummary From(FuelDataModel fuel, RefuelEstimate selected)
        {
            var estimates = new EstimateFactory(fuel, selected);
            return new FuelSummary
            {
                Tank = fuel.Level.ToString("0.0"),
                RefuelMinimum = estimates.Refuel(RefuelEstimate.Minimum, fuel.RefuelMin),
                RefuelAverage = estimates.Refuel(RefuelEstimate.Average, fuel.RefuelAvg),
                RefuelMaximum = estimates.Refuel(RefuelEstimate.Maximum, fuel.RefuelMax),
                LapsMinimum = estimates.Laps(RefuelEstimate.Minimum, fuel.MinConsumption),
                LapsAverage = estimates.Laps(RefuelEstimate.Average, fuel.AvgConsumption),
                LapsMaximum = estimates.Laps(RefuelEstimate.Maximum, fuel.MaxConsumption),
            };
        }

        private sealed record EstimateFactory(FuelDataModel Fuel, RefuelEstimate Selected)
        {
            private bool HasEstimates => Fuel.AvgConsumption > 0;

            /// <param name="leftAtFinish">Fuel left at the finish; negative is the amount to add.</param>
            public FuelEstimate Refuel(RefuelEstimate estimate, float leftAtFinish) => new()
            {
                Value = HasEstimates ? (-leftAtFinish).ToString("0.0") : Unknown,
                Caption = CaptionOf(estimate),
                NeedsRefuel = HasEstimates && leftAtFinish < 0,
                IsSelected = estimate == Selected,
            };

            public FuelEstimate Laps(RefuelEstimate estimate, float consumptionPerLap) => new()
            {
                Value = HasEstimates && consumptionPerLap > 0 ? (Fuel.Level / consumptionPerLap).ToString("0.0") : Unknown,
                Caption = CaptionOf(estimate),
            };

            private static string CaptionOf(RefuelEstimate estimate) => estimate switch
            {
                RefuelEstimate.Minimum => "MIN",
                RefuelEstimate.Maximum => "MAX",
                _ => "AVG",
            };
        }
    }
}
