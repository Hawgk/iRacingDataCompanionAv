namespace IRDCav.Services
{
    /// <summary>Produces overlay data on its own thread until stopped.</summary>
    internal interface IOverlayDataFeed
    {
        void Start();
        void Stop();
    }
}
