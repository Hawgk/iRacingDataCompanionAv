using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Styling;
using IRDCav.Configuration;
using IRDCav.Converters;
using System;

namespace IRDCav.Views.Theme
{
    /// <summary>
    /// Publishes the user's colors, font sizes and number formats as application resources. The XAML
    /// refers to them through DynamicResource, so applying a new configuration updates open windows.
    /// </summary>
    internal static class OverlayTheme
    {
        private const byte PlayerRowAlpha = 0x24;
        private const byte ChipAlpha = 0x29;
        private const byte TrackAlpha = 0x17;
        private const double AccentStep = 0.15;

        private const double RowHeightPerNormalSize = 2;
        private const double TitleRowHeightPerSmallSize = 2;
        private const double FactLineHeightPerNormalSize = 1.5;
        private const double MixedLineHeightPerLargeSize = 1.2;
        private const double IconSizePerNormalSize = 1.25;
        private const double StripeHeightPerNormalSize = 1.1;
        private const double BadgeHeightPerSmallSize = 1.6;
        private const double LetterBadgeWidthPerSmallSize = 2;

        private const double SectorLabelCharacters = 4;
        private const double SectorTimeDigits = 8;
        private const double SectorDeltaDigits = 7;
        private const double SectorColumnRoom = 10;

        public const string DigitWidthKey = "Overlay.DigitWidth";
        public const string TitleCharacterWidthKey = "Overlay.TitleCharacterWidth";

        // Must match the LetterSpacing of TextBlock.columnTitle in App.axaml.
        private const double ColumnTitleLetterSpacing = 1;

        public static void Apply(Application application, OverlayConfiguration configuration)
        {
            AddBaseBrushes(application.Resources, configuration.ColorScheme);
            AddStateBrushes(application.Resources, configuration.ColorScheme);
            AddFontSizes(application.Resources, configuration.FontSizes);
            AddControlAccent(application.Resources, configuration.ColorScheme.PlayerHighlight);
            AddFont(application.Resources, OverlayFonts.Resolve(configuration.Font), configuration.FontSizes);
            application.Resources["GapConverter"] = new SignedTimeConverter(configuration.Decimals.Gap);
            application.Resources["IntervalConverter"] = new SignedTimeConverter(configuration.Decimals.Interval);
        }

        /// <summary>Digits of equal width keep times and gaps from jittering as they change. Call once.</summary>
        public static void AddTabularDigits(Application application) => application.Styles.Add(TabularDigits());

        private static void AddBaseBrushes(IResourceDictionary resources, ColorScheme colors)
        {
            resources["Overlay.Background"] = Brush(colors.Background);
            resources["Overlay.Surface"] = Brush(colors.Surface);
            resources["Overlay.Text"] = Brush(colors.Text);
            resources["Overlay.TextMuted"] = Brush(colors.TextMuted);
            resources["Overlay.OnText"] = Brush(WithAlpha(colors.Background, 0xFF));
            resources["Overlay.SolidBackground"] = Brush(WithAlpha(colors.Background, 0xFF));
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
            resources["Overlay.IRatingGain"] = Brush(colors.IRatingGain);
            resources["Overlay.IRatingLoss"] = Brush(colors.IRatingLoss);
            resources["Overlay.TimeGain"] = Brush(colors.TimeGain);
            resources["Overlay.TimeLoss"] = Brush(colors.TimeLoss);
        }

        /// <summary>
        /// The Fluent controls of the settings window take the player highlight as their accent color,
        /// so checked boxes, selections and the main button match the overlays.
        /// </summary>
        private static void AddControlAccent(IResourceDictionary resources, Color accent)
        {
            resources["SystemAccentColor"] = accent;
            for (int step = 1; step <= 3; step++)
            {
                resources[$"SystemAccentColorLight{step}"] = Mix(accent, Colors.White, step * AccentStep);
                resources[$"SystemAccentColorDark{step}"] = Mix(accent, Colors.Black, step * AccentStep);
            }
        }

        private static Color Mix(Color color, Color other, double share) => Color.FromRgb(
            (byte)Math.Round(color.R + (other.R - color.R) * share),
            (byte)Math.Round(color.G + (other.G - color.G) * share),
            (byte)Math.Round(color.B + (other.B - color.B) * share));

        private static void AddFontSizes(IResourceDictionary resources, FontSizesConfiguration sizes)
        {
            resources["Overlay.FontSize.Small"] = sizes.Small;
            resources["Overlay.FontSize.Normal"] = sizes.Normal;
            resources["Overlay.FontSize.Large"] = sizes.Large;
            resources["Overlay.RowHeight"] = Math.Ceiling(sizes.Normal * RowHeightPerNormalSize);
            resources["Overlay.TitleRowHeight"] = Math.Ceiling(sizes.Small * TitleRowHeightPerSmallSize);
            resources["Overlay.FactLineHeight"] = Math.Ceiling(sizes.Normal * FactLineHeightPerNormalSize);
            resources["Overlay.MixedLineHeight"] = Math.Ceiling(sizes.Large * MixedLineHeightPerLargeSize);
            resources["Overlay.IconSize"] = Math.Round(sizes.Normal * IconSizePerNormalSize);
            resources["Overlay.StripeHeight"] = Math.Round(sizes.Normal * StripeHeightPerNormalSize);
            resources["Overlay.BadgeHeight"] = Math.Ceiling(sizes.Small * BadgeHeightPerSmallSize);
            resources["Overlay.LetterBadgeWidth"] = Math.Ceiling(sizes.Small * LetterBadgeWidthPerSmallSize);
        }

        private static void AddFont(IResourceDictionary resources, FontFamily family, FontSizesConfiguration sizes)
        {
            resources["Overlay.FontFamily"] = family;
            double digitAdvance = FontMeasurement.DigitAdvance(family);
            resources[DigitWidthKey] = digitAdvance * sizes.Normal;
            resources[TitleCharacterWidthKey] = digitAdvance * sizes.Small + ColumnTitleLetterSpacing;
            AddSectorColumnWidths(resources, digitAdvance * sizes.Normal, digitAdvance * sizes.Small + ColumnTitleLetterSpacing);
            double offset = FontMeasurement.CapitalOffset(family);
            resources["Overlay.TextShift.Small"] = new TranslateTransform(0, -offset * sizes.Small);
            resources["Overlay.TextShift.Normal"] = new TranslateTransform(0, -offset * sizes.Normal);
            resources["Overlay.TextShift.Large"] = new TranslateTransform(0, -offset * sizes.Large);
        }

        /// <summary>
        /// Fixed column widths of the sector view, so its rows line up and the panel keeps its width
        /// whether deltas are shown or not: a short label ("BEST"), a lap time ("1:44.212") and a delta ("+12.345").
        /// </summary>
        private static void AddSectorColumnWidths(IResourceDictionary resources, double digitWidth, double labelCharacterWidth)
        {
            resources["Overlay.SectorLabelWidth"] = Math.Ceiling(SectorLabelCharacters * labelCharacterWidth + SectorColumnRoom);
            resources["Overlay.SectorTimeWidth"] = Math.Ceiling(SectorTimeDigits * digitWidth + SectorColumnRoom);
            resources["Overlay.SectorDeltaWidth"] = Math.Ceiling(SectorDeltaDigits * digitWidth + SectorColumnRoom);
        }

        private static Style TabularDigits() => new(selector => selector.OfType<Window>())
        {
            Setters = { new Setter(TextElement.FontFeaturesProperty, new FontFeatureCollection { FontFeature.Parse("tnum") }) },
        };

        private static IBrush Brush(Color color) => new ImmutableSolidColorBrush(color);

        private static Color WithAlpha(Color color, byte alpha) => Color.FromArgb(alpha, color.R, color.G, color.B);
    }
}
