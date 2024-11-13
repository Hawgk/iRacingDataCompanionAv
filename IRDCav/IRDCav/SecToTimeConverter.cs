using Avalonia.Data.Converters;
using Avalonia.Data;
using System;
using System.Globalization;

namespace IRDCav
{
    public class SecToTimeConverter : IValueConverter
    {
        public static readonly SecToTimeConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is float sourceValue && parameter is string targetCase && targetType.IsAssignableTo(typeof(string)))
            {
                if (sourceValue <= 0.0f)
                {
                    return string.Empty;
                }

                switch (targetCase)
                {
                    case "interval":
                    case "Interval":
                        if (sourceValue > 60.0f)
                        {
                            return TimeSpan.FromSeconds(sourceValue).ToString(@"m\:ss\.f");
                        }
                        return TimeSpan.FromSeconds(sourceValue).ToString(@"s\.f");
                    case "laptime":
                    case "Laptime":
                    case "LapTime":
                        return TimeSpan.FromSeconds(sourceValue).ToString(@"m\:ss\.fff");
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
