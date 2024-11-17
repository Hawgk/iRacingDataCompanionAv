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
            string returnString = string.Empty.PadLeft(8);

            if (parameter is string targetCase && targetType.IsAssignableTo(typeof(string)))
            {
                if (value is float sourceValueFloat)
                {
                    if (sourceValueFloat != 0.0f)
                    {
                        switch (targetCase)
                        {
                            case "Gap":
                                if (sourceValueFloat > 60.0f || sourceValueFloat < -60.0f)
                                {
                                    returnString = TimeSpan.FromSeconds(Math.Round(Math.Abs(sourceValueFloat), 3)).ToString(@"m\:ss\.fff");
                                }
                                else
                                {
                                    returnString = TimeSpan.FromSeconds(Math.Round(Math.Abs(sourceValueFloat), 3)).ToString(@"s\.fff");
                                }

                                if (sourceValueFloat < 0.0f)
                                {
                                    returnString = "-" + returnString;
                                }

                                returnString = returnString.PadLeft(7);
                                break;
                            case "Interval":
                                if (sourceValueFloat > 60.0f || sourceValueFloat < -60.0f)
                                {
                                    returnString = TimeSpan.FromSeconds(Math.Round(Math.Abs(sourceValueFloat), 1)).ToString(@"m\:ss\.f");
                                }
                                else
                                {
                                    returnString = TimeSpan.FromSeconds(Math.Round(Math.Abs(sourceValueFloat), 1)).ToString(@"s\.f");
                                }

                                if (sourceValueFloat < 0.0f)
                                {
                                    returnString = "-" + returnString;
                                }

                                returnString = returnString.PadLeft(7);
                                break;
                            case "Laptime":
                                if (sourceValueFloat > 0.0f)
                                {
                                    returnString = TimeSpan.FromSeconds(Math.Round(sourceValueFloat, 3)).ToString(@"m\:ss\.fff").PadLeft(7);
                                }
                                break;
                        }
                    }
                }
                else if (value is double sourceValueDouble)
                {
                    if (sourceValueDouble > 0.0f)
                    {
                        switch (targetCase)
                        {
                            case "Session":
                                if (sourceValueDouble > 3600.0f)
                                {
                                    returnString = TimeSpan.FromSeconds(Math.Round(sourceValueDouble, 0)).ToString(@"h\:mm\:ss");
                                }
                                else
                                {
                                    returnString = TimeSpan.FromSeconds(Math.Round(sourceValueDouble, 0)).ToString(@"m\:ss");
                                }
                                break;
                        }
                    }
                }

                return returnString;
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
