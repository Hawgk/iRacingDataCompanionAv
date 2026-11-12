using System;

namespace IRDCav.Converters
{
    /// <summary>Formats fuel values. Parameter: "Fuel", "FuelPerLap" or "FuelPerMinute".</summary>
    public class FloatConverter : OneWayConverter
    {
        private const int Width = 11;

        protected override object Convert(object? value, string? parameter)
        {
            if (value is not float amount)
            {
                return UnsupportedValue(value, parameter);
            }

            return parameter switch
            {
                "Fuel" => WithUnit(amount, " L    "),
                "FuelPerLap" => WithUnit(amount, " L/Lap"),
                "FuelPerMinute" => WithUnit(amount, " L/Min"),
                _ => UnsupportedValue(value, parameter),
            };
        }

        private static string WithUnit(float amount, string unit) =>
            (Math.Round(amount, 2).ToString("0.00") + unit).PadLeft(Width);
    }
}
