using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace IRDCav.Converters
{
    /// <summary>
    /// Softens a data color from iRacing ("#RRGGBB", e.g. a class or license color) into a pastel
    /// brush. Parameter "Text" gives the pastel color, "Chip" a faint version for badge backgrounds.
    /// Unknown colors give a neutral grey. Brushes are cached because rows change cars all the time.
    /// </summary>
    public sealed class TintConverter : IValueConverter
    {
        private const double WhiteShare = 0.45;
        private const byte ChipAlpha = 0x29;
        private static readonly Color Neutral = Color.Parse("#A0A3AA");

        private readonly Dictionary<(string, string), IBrush> _cache = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            string color = value as string ?? string.Empty;
            string usage = parameter as string ?? "Text";
            if (!_cache.TryGetValue((color, usage), out IBrush? brush))
            {
                brush = new ImmutableSolidColorBrush(Tint(Pastel(color), usage));
                _cache[(color, usage)] = brush;
            }
            return brush;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            throw new NotSupportedException($"{nameof(TintConverter)} is a one-way converter.");

        private static Color Pastel(string color)
        {
            if (!Color.TryParse(color, out Color parsed))
            {
                return Neutral;
            }
            return Color.FromRgb(TowardsWhite(parsed.R), TowardsWhite(parsed.G), TowardsWhite(parsed.B));
        }

        private static byte TowardsWhite(byte channel) => (byte)Math.Round(channel + (255 - channel) * WhiteShare);

        private static Color Tint(Color pastel, string usage) =>
            usage == "Chip" ? Color.FromArgb(ChipAlpha, pastel.R, pastel.G, pastel.B) : pastel;
    }
}
