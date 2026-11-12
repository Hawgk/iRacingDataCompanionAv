using Avalonia.Media;
using System;

namespace IRDCav.Views.Theme
{
    /// <summary>
    /// Fonts draw capitals and digits at different heights within their line box (Inter exactly in
    /// its center, Lekton noticeably above). Shifting each text by this offset times its font size
    /// puts texts of different sizes in one row onto a common center line.
    /// </summary>
    internal static class CapitalCentering
    {
        /// <summary>How far the capitals' center lies below the line box center, in em; 0 when unknown.</summary>
        public static double OffsetOf(FontFamily family)
        {
            if (!FontManager.Current.TryGetGlyphTypeface(new Typeface(family), out IGlyphTypeface? typeface)
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
    }
}
