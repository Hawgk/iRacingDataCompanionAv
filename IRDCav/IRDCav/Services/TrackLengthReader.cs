using System;
using System.Globalization;

namespace IRDCav.Services
{
    /// <summary>Reads the track length from the session info, where iRacing writes it with its unit, e.g. "5.81 km".</summary>
    internal static class TrackLengthReader
    {
        private const double MetersPerKilometer = 1000;
        private const double MetersPerMile = 1609.344;

        /// <summary>The length in meters; 0 when it is missing or can't be read.</summary>
        public static double Meters(string? trackLength)
        {
            string[] parts = (trackLength ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0 || !double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
            {
                return 0;
            }
            string unit = parts.Length > 1 ? parts[1].ToLowerInvariant() : "km";
            return unit.StartsWith("mi") ? value * MetersPerMile : value * MetersPerKilometer;
        }
    }
}
