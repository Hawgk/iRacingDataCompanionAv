using Avalonia.Layout;
using IRDCav.Configuration;
using System;

namespace IRDCav.Views.Table
{
    /// <summary>Character widths of the configured font: values at the normal size, column titles at the small size.</summary>
    internal sealed record ColumnMetrics(double DigitWidth, double TitleCharacterWidth);

    /// <summary>
    /// Title, width and alignment of each column. The width fits the longest expected content in
    /// digits of the normal font size plus fixed room for stripes and badges, and the title, so the
    /// columns fit whichever font is configured.
    /// </summary>
    internal sealed record RaceColumnLayout(string Title, double Digits, double FixedWidth, HorizontalAlignment Alignment)
    {
        // Cell padding (5 on each side) plus a little air between the texts of neighbouring columns.
        private const double CellRoom = 12;
        private const double StripeRoom = 8;
        private const double BadgeRoom = 10;

        public double WidthFor(ColumnMetrics metrics)
        {
            double content = Digits * metrics.DigitWidth + FixedWidth;
            double title = Title.Length * metrics.TitleCharacterWidth;
            return Math.Ceiling(Math.Max(content, title) + CellRoom);
        }

        public static RaceColumnLayout Of(RaceColumn column) => column switch
        {
            RaceColumn.Position => new("POS", 2, 0, HorizontalAlignment.Right),
            RaceColumn.CarNumber => new("#", 3, StripeRoom, HorizontalAlignment.Left),
            RaceColumn.CarClass => new("CLASS", 4, 0, HorizontalAlignment.Left),
            RaceColumn.Driver => new("DRIVER", 16, 0, HorizontalAlignment.Left),
            RaceColumn.Pit => new("PIT", 3, BadgeRoom, HorizontalAlignment.Left),
            RaceColumn.IRating => new("IR", 4, BadgeRoom, HorizontalAlignment.Left),
            RaceColumn.Laps => new("LAPS", 3, 0, HorizontalAlignment.Right),
            RaceColumn.FastestLap => new("BEST", 8, 0, HorizontalAlignment.Right),
            RaceColumn.LastLap => new("LAST", 8, 0, HorizontalAlignment.Right),
            RaceColumn.Gap => new("GAP", 9, 0, HorizontalAlignment.Right),
            RaceColumn.Interval => new("INT", 7, 0, HorizontalAlignment.Right),
            _ => throw new ArgumentOutOfRangeException(nameof(column), column, "Column has no layout."),
        };
    }
}
