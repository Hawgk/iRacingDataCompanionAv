using IRDCav.Configuration;
using IRDCav.Models;
using System;

namespace IRDCav.ViewModels
{
    /// <summary>
    /// The fuel figures a driver checks mid-race: how much to add at the next stop
    /// (or how much is spare), what is in the tank and how long it lasts.
    /// </summary>
    public sealed record FuelSummary
    {
        private const string Unknown = "–";

        public bool NeedsRefuel { get; init; }
        public string Label { get; init; } = "REFUEL";
        public string Amount { get; init; } = Unknown;
        public string Tank { get; init; } = Unknown;
        public string PerLap { get; init; } = Unknown;
        public string Laps { get; init; } = Unknown;

        public static FuelSummary From(FuelDataModel fuel, RefuelEstimate estimate)
        {
            string tank = fuel.Level.ToString("0.0");
            if (fuel.AvgConsumption <= 0)
            {
                return new FuelSummary { Tank = tank };
            }

            float leftAtFinish = LeftAtFinish(fuel, estimate);
            return new FuelSummary
            {
                NeedsRefuel = leftAtFinish < 0,
                Label = leftAtFinish < 0 ? "REFUEL" : "SPARE",
                Amount = Math.Abs(leftAtFinish).ToString("0.0"),
                Tank = tank,
                PerLap = fuel.AvgConsumption.ToString("0.00"),
                Laps = (fuel.Level / fuel.AvgConsumption).ToString("0.0"),
            };
        }

        private static float LeftAtFinish(FuelDataModel fuel, RefuelEstimate estimate) => estimate switch
        {
            RefuelEstimate.Minimum => fuel.RefuelMin,
            RefuelEstimate.Maximum => fuel.RefuelMax,
            _ => fuel.RefuelAvg,
        };
    }
}
