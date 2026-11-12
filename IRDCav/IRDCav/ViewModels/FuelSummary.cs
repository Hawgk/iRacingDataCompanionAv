using IRDCav.Configuration;
using IRDCav.Models;

namespace IRDCav.ViewModels
{
    /// <summary>Fuel to add at the next stop to reach the finish, for one consumption estimate.</summary>
    public sealed record FuelEstimate
    {
        public string Label { get; init; } = string.Empty;

        /// <summary>Liters to add; negative when that much is spare.</summary>
        public string Amount { get; init; } = FuelSummary.Unknown;

        public bool NeedsRefuel { get; init; }

        /// <summary>The estimate chosen in the configuration, shown large.</summary>
        public bool IsSelected { get; init; }
    }

    /// <summary>
    /// The fuel figures a driver checks mid-race: how much to add at the next stop for
    /// minimum, average and maximum consumption, what is in the tank and how long it lasts.
    /// </summary>
    public sealed record FuelSummary
    {
        public const string Unknown = "–";

        public FuelEstimate Minimum { get; init; } = new();
        public FuelEstimate Average { get; init; } = new();
        public FuelEstimate Maximum { get; init; } = new();
        public string Tank { get; init; } = Unknown;
        public string PerLap { get; init; } = Unknown;
        public string Laps { get; init; } = Unknown;

        public static FuelSummary From(FuelDataModel fuel, RefuelEstimate selected)
        {
            bool hasEstimates = fuel.AvgConsumption > 0;
            var estimates = new EstimateFactory(hasEstimates, selected);
            return new FuelSummary
            {
                Minimum = estimates.Create(RefuelEstimate.Minimum, fuel.RefuelMin),
                Average = estimates.Create(RefuelEstimate.Average, fuel.RefuelAvg),
                Maximum = estimates.Create(RefuelEstimate.Maximum, fuel.RefuelMax),
                Tank = fuel.Level.ToString("0.0"),
                PerLap = hasEstimates ? fuel.AvgConsumption.ToString("0.00") : Unknown,
                Laps = hasEstimates ? (fuel.Level / fuel.AvgConsumption).ToString("0.0") : Unknown,
            };
        }

        private sealed record EstimateFactory(bool HasEstimates, RefuelEstimate Selected)
        {
            /// <param name="leftAtFinish">Fuel left at the finish; negative is the amount to add.</param>
            public FuelEstimate Create(RefuelEstimate estimate, float leftAtFinish) => new()
            {
                Label = LabelOf(estimate),
                Amount = HasEstimates ? (-leftAtFinish).ToString("0.0") + " L" : Unknown,
                NeedsRefuel = HasEstimates && leftAtFinish < 0,
                IsSelected = estimate == Selected,
            };

            private static string LabelOf(RefuelEstimate estimate) => estimate switch
            {
                RefuelEstimate.Minimum => "MIN",
                RefuelEstimate.Maximum => "MAX",
                _ => "AVG",
            };
        }
    }
}
