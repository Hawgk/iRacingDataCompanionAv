using System;

namespace IRDCav.Converters
{
    /// <summary>
    /// Formats seconds. Parameter "Gap", "Interval" or "Laptime" for float values,
    /// "Session" for double values. Zero, negative (session/lap time) and NaN values are shown blank.
    /// </summary>
    public class SecToTimeConverter : OneWayConverter
    {
        private sealed record SignedFormat(int Decimals, string Long, string Short);

        private static readonly SignedFormat GapFormat = new(3, @"m\:ss\.fff", @"s\.fff");
        private static readonly SignedFormat IntervalFormat = new(1, @"m\:ss\.f", @"s\.f");

        private const int ColumnWidth = 7;
        private const float SecondsPerMinute = 60;
        private const double SecondsPerHour = 3600;
        private static readonly string Blank = string.Empty.PadLeft(8);

        protected override object Convert(object? value, string? parameter)
        {
            if (parameter == null)
            {
                return UnsupportedValue(value, parameter);
            }

            return value switch
            {
                float seconds when seconds != 0 && !float.IsNaN(seconds) => FormatFloat(seconds, parameter),
                double seconds when seconds > 0 && !double.IsNaN(seconds) => FormatDouble(seconds, parameter),
                _ => Blank,
            };
        }

        private static string FormatFloat(float seconds, string parameter) => parameter switch
        {
            "Gap" => FormatSigned(seconds, GapFormat),
            "Interval" => FormatSigned(seconds, IntervalFormat),
            "Laptime" when seconds > 0 => FormatTime(seconds, 3, @"m\:ss\.fff").PadLeft(ColumnWidth),
            _ => Blank,
        };

        private static string FormatDouble(double seconds, string parameter) => parameter switch
        {
            "Session" => FormatTime(seconds, 0, seconds > SecondsPerHour ? @"h\:mm\:ss" : @"m\:ss"),
            _ => Blank,
        };

        private static string FormatSigned(float seconds, SignedFormat format)
        {
            string pattern = Math.Abs(seconds) > SecondsPerMinute ? format.Long : format.Short;
            string sign = seconds < 0 ? "-" : string.Empty;
            return (sign + FormatTime(Math.Abs(seconds), format.Decimals, pattern)).PadLeft(ColumnWidth);
        }

        private static string FormatTime(double seconds, int decimals, string pattern) =>
            TimeSpan.FromSeconds(Math.Round(seconds, decimals)).ToString(pattern);
    }
}
