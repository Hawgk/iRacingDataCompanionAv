namespace IRDCav.Layout
{
    /// <summary>Initial overlay placements, matching the original layout on a 2560x1440 screen.</summary>
    public static class DefaultLayouts
    {
        public static readonly OverlayDefinition Results = new("results", new OverlayPlacement
        {
            Horizontal = Anchor.Start,
            Vertical = Anchor.Center,
            OffsetY = 220.0 / 1440,
        });

        public static readonly OverlayDefinition Relatives = new("relatives", new OverlayPlacement
        {
            Horizontal = Anchor.End,
            Vertical = Anchor.End,
        });

        /// <summary>Right above the relatives: nine 26 pixel rows plus margins, and a 12 pixel gap.</summary>
        public static readonly OverlayDefinition Fuel = new("fuel", new OverlayPlacement
        {
            Horizontal = Anchor.End,
            Vertical = Anchor.End,
            OffsetY = -(9 * 26 + 8 + 12) / 1440.0,
        });

        /// <summary>Right above the fuel window, which is about 134 pixels high, with another 12 pixel gap.</summary>
        public static readonly OverlayDefinition LapSectors = new("lapSectors", new OverlayPlacement
        {
            Horizontal = Anchor.End,
            Vertical = Anchor.End,
            OffsetY = -(9 * 26 + 8 + 12 + 134 + 12) / 1440.0,
        });

        public static readonly OverlayDefinition InputGraph = new("inputGraph", new OverlayPlacement
        {
            Horizontal = Anchor.Center,
            Vertical = Anchor.Start,
            OffsetY = 480.0 / 1440,
        });
    }
}
