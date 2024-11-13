using Avalonia.Data.Converters;
using Avalonia.Data;
using System;
using System.Globalization;
using System.Linq;

namespace IRDCav
{
    public class DriverNameConverter : IValueConverter
    {
        public static readonly DriverNameConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string sourceValue && parameter is string targetCase && targetType.IsAssignableTo(typeof(string)))
            {
                switch (targetCase)
                {
                    case "LastUpper":
                        return sourceValue.Split(" ").LastOrDefault().ToUpper();
                    case "ShortUpper":
                        string returnString = string.Empty;
                        string[] strs = sourceValue.Split(" ");

                        for (int i = 0; i < strs.Length - 1; i++)
                        {
                            returnString += strs[i].Substring(0, 1) + ". ";
                        }
                        returnString += strs[strs.Length - 1].ToUpper();

                        return returnString;
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
