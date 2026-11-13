using Avalonia;
using System;

namespace IRDCav.Layout
{
    /// <summary>
    /// Pure conversions between <see cref="OverlayPlacement"/> and physical window positions.
    /// All rectangles are in physical pixels.
    /// </summary>
    public static class PlacementCalculator
    {
        /// <summary>Screen height (in device independent pixels) the overlay layouts were designed for.</summary>
        public const double ReferenceScreenHeight = 1440;

        public const double MinScale = 0.5;
        public const double MaxScale = 2.0;

        /// <summary>
        /// Layout scale that makes an overlay cover the same fraction of the screen height on every
        /// monitor, independent of its resolution and DPI setting.
        /// </summary>
        public static double ContentScale(PixelRect screen, double screenScaling, double userScale) =>
            screen.Height / (ReferenceScreenHeight * screenScaling) * userScale;

        public static double ClampScale(double scale) => Math.Clamp(scale, MinScale, MaxScale);

        public static PixelPoint ToPosition(OverlayPlacement placement, PixelRect screen, PixelSize window)
        {
            double fx = Fraction(placement.Horizontal);
            double fy = Fraction(placement.Vertical);
            double x = screen.X + screen.Width * (fx + placement.OffsetX) - window.Width * fx;
            double y = screen.Y + screen.Height * (fy + placement.OffsetY) - window.Height * fy;
            return KeepInside(new PixelPoint((int)Math.Round(x), (int)Math.Round(y)), screen, window);
        }

        /// <summary>
        /// Placement for a window the user dropped at <paramref name="window"/>. The anchors are
        /// chosen by the screen third the window center lies in, so e.g. a window dropped near the
        /// bottom keeps its distance to the bottom edge when it grows.
        /// </summary>
        public static OverlayPlacement FromPosition(PixelRect window, PixelRect screen, double userScale)
        {
            Anchor horizontal = AnchorFor(window.Center.X - screen.X, screen.Width);
            Anchor vertical = AnchorFor(window.Center.Y - screen.Y, screen.Height);
            double fx = Fraction(horizontal);
            double fy = Fraction(vertical);

            return new OverlayPlacement
            {
                Horizontal = horizontal,
                Vertical = vertical,
                OffsetX = (window.X + window.Width * fx - screen.X) / (double)screen.Width - fx,
                OffsetY = (window.Y + window.Height * fy - screen.Y) / (double)screen.Height - fy,
                Scale = userScale,
                Screen = new ScreenArea(screen.X, screen.Y, screen.Width, screen.Height),
            };
        }

        public static PixelPoint KeepInside(PixelPoint position, PixelRect screen, PixelSize window) => new(
            ClampAxis(position.X, screen.X, screen.Right - window.Width),
            ClampAxis(position.Y, screen.Y, screen.Bottom - window.Height));

        private static double Fraction(Anchor anchor) => (int)anchor / 2.0;

        private static Anchor AnchorFor(double center, int length) =>
            center < length / 3.0 ? Anchor.Start
            : center < length * 2 / 3.0 ? Anchor.Center
            : Anchor.End;

        // A window larger than the screen is aligned to the screen start.
        private static int ClampAxis(int value, int min, int max) => Math.Max(min, Math.Min(value, max));
    }
}
