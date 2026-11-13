using System;

namespace IRDCav.Converters
{
    /// <summary>
    /// Formats seconds. Parameter "Laptime" for float lap times, "Sector" for float sector times
    /// without a minute below 60 seconds, "Session" for the double remaining session time.
    /// Zero, negative and NaN values are blank.
    /// </summary>
    public class SecToTimeConverter : OneWayConverter
    {
        private const double SecondsPerHour = 3600;

        private const double SecondsPerMinute = 60;
        private const int Milliseconds = 3;

        protected override object Convert(object? value, string? parameter) => (value, parameter) switch
        {
            (float seconds, "Laptime") when seconds > 0 => TimeFormat.Format(seconds, Milliseconds, @"m\:ss\.fff"),
            (float seconds, "Sector") when seconds > 0 => FormatSector(seconds),
            (double seconds, "Session") when seconds > 0 => TimeFormat.Format(seconds, 0, seconds > SecondsPerHour ? @"h\:mm\:ss" : @"m\:ss"),
            (float or double, "Laptime" or "Sector" or "Session") => string.Empty,
            _ => UnsupportedValue(value, parameter),
        };

        /// <summary>"22.123", or "1:02.123" from a minute on. Decides on the rounded value, so 59.9996 becomes "1:00.000".</summary>
        private static string FormatSector(float seconds)
        {
            bool isMinuteOrMore = Math.Round(seconds, Milliseconds) >= SecondsPerMinute;
            return TimeFormat.Format(seconds, Milliseconds, isMinuteOrMore ? @"m\:ss\.fff" : @"%s\.fff");
        }
    }
}
