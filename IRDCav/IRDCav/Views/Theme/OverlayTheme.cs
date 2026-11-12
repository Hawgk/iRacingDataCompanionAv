using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Styling;
using IRDCav.Configuration;
using IRDCav.Converters;

namespace IRDCav.Views.Theme
{
    /// <summary>
    /// Publishes the user's colors and number formats as application resources. Must run before
    /// the windows are created; the XAML refers to the brushes through DynamicResource.
    /// </summary>
    internal static class OverlayTheme
    {
        private const byte PlayerRowAlpha = 0x24;
        private const byte ChipAlpha = 0x29;
        private const byte TrackAlpha = 0x17;

        public static void Apply(Application application, OverlayConfiguration configuration)
        {
            AddBaseBrushes(application.Resources, configuration.ColorScheme);
            AddStateBrushes(application.Resources, configuration.ColorScheme);
            application.Resources["GapConverter"] = new SignedTimeConverter(configuration.Decimals.Gap);
            application.Resources["IntervalConverter"] = new SignedTimeConverter(configuration.Decimals.Interval);
            application.Styles.Add(TabularDigits());
        }

        private static void AddBaseBrushes(IResourceDictionary resources, ColorScheme colors)
        {
            resources["Overlay.Background"] = Brush(colors.Background);
            resources["Overlay.Surface"] = Brush(colors.Surface);
            resources["Overlay.Text"] = Brush(colors.Text);
            resources["Overlay.TextMuted"] = Brush(colors.TextMuted);
            resources["Overlay.OnText"] = Brush(WithAlpha(colors.Background, 0xFF));
            resources["Overlay.Track"] = Brush(WithAlpha(colors.Text, TrackAlpha));
            resources["Overlay.Throttle"] = Brush(colors.Throttle);
            resources["Overlay.Brake"] = Brush(colors.Brake);
        }

        private static void AddStateBrushes(IResourceDictionary resources, ColorScheme colors)
        {
            resources["Overlay.PlayerHighlight"] = Brush(colors.PlayerHighlight);
            resources["Overlay.PlayerRow"] = Brush(WithAlpha(colors.PlayerHighlight, PlayerRowAlpha));
            resources["Overlay.LapAhead"] = Brush(colors.LapAhead);
            resources["Overlay.LapBehind"] = Brush(colors.LapBehind);
            resources["Overlay.FastestLap"] = Brush(colors.FastestLap);
            resources["Overlay.PitRoad"] = Brush(colors.PitRoad);
            resources["Overlay.PitRoadChip"] = Brush(WithAlpha(colors.PitRoad, ChipAlpha));
            resources["Overlay.Warning"] = Brush(colors.Warning);
        }

        // Digits of equal width keep times and gaps from jittering as they change.
        private static Style TabularDigits() => new(selector => selector.OfType<Window>())
        {
            Setters = { new Setter(TextElement.FontFeaturesProperty, new FontFeatureCollection { FontFeature.Parse("tnum") }) },
        };

        private static IBrush Brush(Color color) => new ImmutableSolidColorBrush(color);

        private static Color WithAlpha(Color color, byte alpha) => Color.FromArgb(alpha, color.R, color.G, color.B);
    }
}
