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
            string returnString = string.Empty;

            if (parameter is string targetCase && targetType.IsAssignableTo(typeof(string)))
            {
                if (value is float sourceValueFloat)
                {
                    if (sourceValueFloat <= 0.0f)
                    {
                        returnString = string.Empty;
                    }
                    else
                    {
                        switch (targetCase)
                        {
                            case "Interval":
                                if (sourceValueFloat > 60.0f || sourceValueFloat < -60.0f)
                                {
                                    returnString = TimeSpan.FromSeconds(sourceValueFloat).ToString(@"m\:ss\.f");
                                }
                                else
                                {
                                    returnString = TimeSpan.FromSeconds(sourceValueFloat).ToString(@"s\.f");
                                }
                                break;
                            case "Laptime":
                                returnString = TimeSpan.FromSeconds(sourceValueFloat).ToString(@"m\:ss\.fff");
                                break;
                        }
                    }
                }
                else if (value is double sourceValueDouble)
                {
                    if (sourceValueDouble <= 0.0f)
                    {
                        returnString = string.Empty;
                    }
                    else
                    {
                        switch (targetCase)
                        {
                            case "Session":
                                if (sourceValueDouble > 3600.0f)
                                {
                                    returnString = TimeSpan.FromSeconds(sourceValueDouble).ToString(@"h\:mm\:ss");
                                }
                                else
                                {
                                    returnString = TimeSpan.FromSeconds(sourceValueDouble).ToString(@"m\:ss");
                                }
                                return returnString;
                        }
                    }
                }

                return returnString.PadLeft(7);
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
