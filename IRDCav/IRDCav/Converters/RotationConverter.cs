using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace IRDCav.Converters
{
    /// <summary>Turns an angle in degrees (clockwise) into a rotation, e.g. for the wind arrow.</summary>
    public sealed class RotationConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            value is double degrees ? new RotateTransform(degrees) : null;

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            throw new NotSupportedException($"{nameof(RotationConverter)} is a one-way converter.");
    }
}
