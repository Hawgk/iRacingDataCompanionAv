using System;
using System.Linq;

namespace IRDCav.Converters
{
    /// <summary>"John Michael Doe" → "J. M. Doe".</summary>
    public class DriverNameConverter : OneWayConverter
    {
        protected override object Convert(object? value, string? parameter)
        {
            if (value is not string name)
            {
                return UnsupportedValue(value, parameter);
            }

            string[] parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return Initials(parts) + (parts.LastOrDefault() ?? string.Empty);
        }

        private static string Initials(string[] parts) =>
            string.Concat(parts.SkipLast(1).Select(part => part[0] + ". "));
    }
}
