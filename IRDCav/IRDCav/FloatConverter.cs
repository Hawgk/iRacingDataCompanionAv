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
            if (value is float sourceValue && targetType.IsAssignableTo(typeof(string)))
            {
                return sourceValue.ToString();
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
