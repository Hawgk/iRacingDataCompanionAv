using System;

namespace IRDCav.Converters
{
    internal static class TimeFormat
    {
        /// <summary>Rounds before formatting so 59.9996 shows as 1:00.000 and not 0:59.999.</summary>
        public static string Format(double seconds, int decimals, string pattern) =>
            TimeSpan.FromSeconds(Math.Round(seconds, decimals)).ToString(pattern);
    }
}
