namespace IRDCav.Layout
{
    /// <summary>Persists overlay placements between runs.</summary>
    public interface IOverlayLayoutStore
    {
        OverlayPlacement Load(OverlayDefinition overlay);
        void Save(string overlayId, OverlayPlacement placement);
    }
}
