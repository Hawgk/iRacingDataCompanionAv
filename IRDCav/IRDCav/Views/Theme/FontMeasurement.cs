using Avalonia.Media;
using System;

namespace IRDCav.Views.Theme
{
    /// <summary>Measurements of the configured font that the layout adapts to, in em.</summary>
    internal static class FontMeasurement
    {
        // Fallback when the font can't be measured: a typical monospace digit.
        private const double FallbackDigitAdvance = 0.6;

        /// <summary>
        /// How far the capitals' center lies below the line box center; 0 when unknown. Fonts differ
        /// here (Inter centers exactly, Lekton sits noticeably high), so every text is shifted by this
        /// offset times its font size to put texts of different sizes onto one center line.
        /// </summary>
        public static double CapitalOffset(FontFamily family)
        {
            if (!TryGetTypeface(family, out IGlyphTypeface typeface)
                || !typeface.TryGetGlyphMetrics(typeface.GetGlyph('H'), out GlyphMetrics capital))
            {
                return 0;
            }
            FontMetrics metrics = typeface.Metrics;
            double ascent = Math.Abs(metrics.Ascent);
            double descent = Math.Abs(metrics.Descent);
            double capitalHeight = Math.Abs(capital.Height);
            return (ascent - descent - capitalHeight) / (2.0 * metrics.DesignEmHeight);
        }

        /// <summary>Width of a digit; column widths are measured in digits so they fit any font.</summary>
        public static double DigitAdvance(FontFamily family)
        {
            if (!TryGetTypeface(family, out IGlyphTypeface typeface))
            {
                return FallbackDigitAdvance;
            }
            return typeface.GetGlyphAdvance(typeface.GetGlyph('0')) / (double)typeface.Metrics.DesignEmHeight;
        }

        private static bool TryGetTypeface(FontFamily family, out IGlyphTypeface typeface)
        {
            bool found = FontManager.Current.TryGetGlyphTypeface(new Typeface(family), out IGlyphTypeface? glyphTypeface);
            typeface = glyphTypeface!;
            return found;
        }
    }
}
