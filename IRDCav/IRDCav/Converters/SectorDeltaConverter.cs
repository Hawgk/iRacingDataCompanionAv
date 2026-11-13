using System;
using System.Globalization;

namespace IRDCav.Converters
{
    /// <summary>
    /// Formats a float time difference in milliseconds: "+0.123", "-0.123", or "0.000" when equal.
    /// NaN, which stands for "nothing to compare", is blank.
    /// </summary>
    public sealed class SectorDeltaConverter : OneWayConverter
    {
        protected override object Convert(object? value, string? parameter)
        {
            if (value is not float delta)
            {
                return UnsupportedValue(value, parameter);
            }
            return float.IsNaN(delta) ? string.Empty : FormatSigned(Math.Round(delta, 3));
        }

        private static string FormatSigned(double delta)
        {
            string magnitude = Math.Abs(delta).ToString("0.000", CultureInfo.InvariantCulture);
            return delta switch
            {
                > 0 => "+" + magnitude,
                < 0 => "-" + magnitude,
                _ => magnitude,
            };
        }
    }
}
