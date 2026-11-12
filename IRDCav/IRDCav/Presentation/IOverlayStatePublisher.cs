namespace IRDCav.Presentation
{
    /// <summary>Accepts new overlay states. Must never block; callable from any thread.</summary>
    public interface IOverlayStatePublisher
    {
        void Publish(OverlayState state);
    }
}
