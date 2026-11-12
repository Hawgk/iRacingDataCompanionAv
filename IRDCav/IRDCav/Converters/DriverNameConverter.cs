using System;
using System.Linq;

namespace IRDCav.Converters
{
    /// <summary>
    /// Parameter "LastUpper": "John Doe" → "DOE".
    /// Parameter "ShortUpper": "John Doe" → "J. DOE".
    /// </summary>
    public class DriverNameConverter : OneWayConverter
    {
        protected override object Convert(object? value, string? parameter)
        {
            if (value is not string name)
            {
                return UnsupportedValue(value, parameter);
            }

            string[] parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return parameter switch
            {
                "LastUpper" => LastName(parts),
                "ShortUpper" => Initials(parts) + LastName(parts),
                _ => UnsupportedValue(value, parameter),
            };
        }

        private static string LastName(string[] parts) => parts.LastOrDefault()?.ToUpper() ?? string.Empty;

        private static string Initials(string[] parts) =>
            string.Concat(parts.SkipLast(1).Select(part => part[0] + ". "));
    }
}
