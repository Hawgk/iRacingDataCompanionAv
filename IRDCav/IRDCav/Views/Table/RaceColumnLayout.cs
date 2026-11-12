using Avalonia.Layout;
using IRDCav.Configuration;
using System;

namespace IRDCav.Views.Table
{
    /// <summary>Title, fixed width (design pixels at 1440p) and alignment of each column.</summary>
    internal sealed record RaceColumnLayout(string Title, double Width, HorizontalAlignment Alignment)
    {
        public RaceColumnLayout Scaled(double factor) => this with { Width = Math.Round(Width * factor) };

        public static RaceColumnLayout Of(RaceColumn column) => column switch
        {
            RaceColumn.Position => new("POS", 32, HorizontalAlignment.Right),
            RaceColumn.CarNumber => new("#", 50, HorizontalAlignment.Left),
            RaceColumn.CarClass => new("CLASS", 50, HorizontalAlignment.Left),
            RaceColumn.Driver => new("DRIVER", 150, HorizontalAlignment.Left),
            RaceColumn.Pit => new("PIT", 40, HorizontalAlignment.Left),
            RaceColumn.IRating => new("IR", 56, HorizontalAlignment.Left),
            RaceColumn.Laps => new("LAPS", 42, HorizontalAlignment.Right),
            RaceColumn.FastestLap => new("BEST", 72, HorizontalAlignment.Right),
            RaceColumn.LastLap => new("LAST", 72, HorizontalAlignment.Right),
            RaceColumn.Gap => new("GAP", 72, HorizontalAlignment.Right),
            RaceColumn.Interval => new("INT", 64, HorizontalAlignment.Right),
            _ => throw new ArgumentOutOfRangeException(nameof(column), column, "Column has no layout."),
        };
    }
}
