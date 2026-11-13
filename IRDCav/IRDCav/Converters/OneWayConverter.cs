using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace IRDCav.Converters
{
    /// <summary>Base for display-only converters that produce strings.</summary>
    public abstract class OneWayConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (!targetType.IsAssignableTo(typeof(string)))
            {
                return Error($"{GetType().Name} can only convert to string, not {targetType.Name}.");
            }
            return Convert(value, parameter as string);
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            throw new NotSupportedException($"{GetType().Name} is a one-way converter.");

        protected abstract object Convert(object? value, string? parameter);

        protected BindingNotification Error(string message) =>
            new(new InvalidCastException(message), BindingErrorType.Error);

        protected BindingNotification UnsupportedValue(object? value, string? parameter) =>
            Error($"{GetType().Name} does not support value '{value}' with parameter '{parameter}'.");
    }
}
