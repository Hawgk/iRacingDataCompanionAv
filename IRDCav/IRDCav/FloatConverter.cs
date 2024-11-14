using Avalonia.Data.Converters;
using Avalonia.Data;
using System;
using System.Globalization;

namespace IRDCav
{
    public class FloatConverter : IValueConverter
    {
        public static readonly FloatConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            string returnString = string.Empty;

            if (value is float sourceValue && parameter is string targetCase && targetType.IsAssignableTo(typeof(string)))
            {
                switch (targetCase)
                {
                    case "Fuel":
                        returnString = sourceValue.ToString("0.00") + "L";
                        return returnString.PadLeft(6);
                    case "FuelPerMinute":
                        returnString = sourceValue.ToString("0.00") + "L/min";
                        return returnString.PadLeft(6);
                }
            }
            // converter used for the wrong type
            return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
