namespace IRDCav.Layout
{
    public enum Anchor
    {
        Start = 0,
        Center = 1,
        End = 2,
    }

    /// <summary>A monitor's bounds in physical pixels, used to find the same monitor again.</summary>
    public readonly record struct ScreenArea(int X, int Y, int Width, int Height);

    /// <summary>
    /// Resolution independent position and size of an overlay. The window point given by the
    /// anchors (e.g. bottom right corner) is kept at the same relative screen position, so the
    /// overlay grows away from its anchored edge and keeps its place on any screen size.
    /// </summary>
    public sealed record OverlayPlacement
    {
        public Anchor Horizontal { get; init; }
        public Anchor Vertical { get; init; }

        /// <summary>Offset from the anchor point as a fraction of the screen width.</summary>
        public double OffsetX { get; init; }

        /// <summary>Offset from the anchor point as a fraction of the screen height.</summary>
        public double OffsetY { get; init; }

        /// <summary>User scale on top of the automatic screen size scaling.</summary>
        public double Scale { get; init; } = 1.0;

        /// <summary>Monitor the overlay was placed on; the primary monitor when unknown.</summary>
        public ScreenArea? Screen { get; init; }
    }

    /// <summary>Identifies an overlay and its placement before the user moves it.</summary>
    public sealed record OverlayDefinition(string Id, OverlayPlacement DefaultPlacement);
}
