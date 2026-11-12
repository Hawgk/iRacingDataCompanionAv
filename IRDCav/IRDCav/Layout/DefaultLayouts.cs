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

        public static readonly OverlayDefinition InputGraph = new("inputGraph", new OverlayPlacement
        {
            Horizontal = Anchor.Center,
            Vertical = Anchor.Start,
            OffsetY = 480.0 / 1440,
        });
    }
}
