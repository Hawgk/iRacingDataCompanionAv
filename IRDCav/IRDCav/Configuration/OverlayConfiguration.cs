using System.Collections.Generic;

namespace IRDCav.Configuration
{
    /// <summary>Columns that can be shown in the standings and relative lists.</summary>
    public enum RaceColumn
    {
        Position,
        CarNumber,
        CarClass,
        Driver,
        Pit,
        IRating,
        Laps,
        FastestLap,
        LastLap,
        Gap,
        Interval,
    }

    /// <summary>Which consumption the big refuel number is based on.</summary>
    public enum RefuelEstimate
    {
        Minimum,
        Average,
        Maximum,
    }

    public sealed record ListOverlayConfiguration
    {
        public bool ShowColumnTitles { get; init; }

        /// <summary>The columns to show, in display order. Columns not listed are hidden.</summary>
        public IReadOnlyList<RaceColumn> Columns { get; init; } = new List<RaceColumn>();
    }

    public sealed record FuelConfiguration
    {
        public RefuelEstimate RefuelEstimate { get; init; } = RefuelEstimate.Average;
    }

    /// <summary>
    /// The only three font sizes the overlays use, in design pixels at 1440p. Row heights,
    /// icon sizes and column widths follow from them.
    /// </summary>
    public sealed record FontSizesConfiguration
    {
        public const double Min = 6;
        public const double Max = 60;

        /// <summary>Labels, column titles, badges and class headers.</summary>
        public double Small { get; init; } = 10;

        /// <summary>All values and list rows.</summary>
        public double Normal { get; init; } = 13;

        /// <summary>Time remaining and the selected refuel amount.</summary>
        public double Large { get; init; } = 22;
    }

    /// <summary>Digits after the decimal point.</summary>
    public sealed record DecimalsConfiguration
    {
        public const int Max = 3;

        public int Gap { get; init; } = 1;
        public int Interval { get; init; } = 1;
    }

    /// <summary>User settings from config.json. Every value has a default, so the file may leave any of them out.</summary>
    public sealed record OverlayConfiguration
    {
        public ColorScheme ColorScheme { get; init; } = new();
        public FontSizesConfiguration FontSizes { get; init; } = new();
        public FuelConfiguration Fuel { get; init; } = new();
        public DecimalsConfiguration Decimals { get; init; } = new();

        public ListOverlayConfiguration Standings { get; init; } = new()
        {
            ShowColumnTitles = true,
            Columns = new List<RaceColumn>
            {
                RaceColumn.Position, RaceColumn.CarNumber, RaceColumn.Driver, RaceColumn.IRating,
                RaceColumn.Laps, RaceColumn.FastestLap, RaceColumn.LastLap, RaceColumn.Gap,
            },
        };

        public ListOverlayConfiguration Relatives { get; init; } = new()
        {
            ShowColumnTitles = false,
            Columns = new List<RaceColumn>
            {
                RaceColumn.Position, RaceColumn.CarNumber, RaceColumn.Driver,
                RaceColumn.Pit, RaceColumn.Interval, RaceColumn.LastLap,
            },
        };
    }
}
