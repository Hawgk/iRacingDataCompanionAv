using System;

namespace IRDCav.Converters
{
    /// <summary>
    /// Formats seconds. Parameter "Laptime" for float lap times, "Session" for the double
    /// remaining session time. Zero, negative and NaN values are blank.
    /// </summary>
    public class SecToTimeConverter : OneWayConverter
    {
        private const double SecondsPerHour = 3600;

        protected override object Convert(object? value, string? parameter) => (value, parameter) switch
        {
            (float seconds, "Laptime") when seconds > 0 => TimeFormat.Format(seconds, 3, @"m\:ss\.fff"),
            (double seconds, "Session") when seconds > 0 => TimeFormat.Format(seconds, 0, seconds > SecondsPerHour ? @"h\:mm\:ss" : @"m\:ss"),
            (float or double, "Laptime" or "Session") => string.Empty,
            _ => UnsupportedValue(value, parameter),
        };
    }
}
