using Avalonia.Data.Converters;
using Avalonia.Data;
using System;
using System.Globalization;

namespace IRDCav
{
    public class IntToLapConverter : IValueConverter
    {
        public static readonly IntToLapConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            string returnString = string.Empty;

            if (value is int sourceValue && targetType.IsAssignableTo(typeof(string)))
            {
                if (sourceValue > 0)
                {
                    returnString = sourceValue + "L";
                }

                return returnString.PadLeft(4);
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
