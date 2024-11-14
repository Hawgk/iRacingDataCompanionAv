using Avalonia.Data.Converters;
using Avalonia.Data;
using System;
using System.Globalization;

namespace IRDCav
{
    public class CarNumberConverter : IValueConverter
    {
        public static readonly CarNumberConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            string returnString = string.Empty;

            if (value is string sourceValue && targetType.IsAssignableTo(typeof(string)))
            {
                returnString = "#" + sourceValue;

                return returnString.PadRight(4);
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
