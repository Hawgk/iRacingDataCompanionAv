namespace IRDCav.BestLaps
{
    /// <summary>The player's car at a track, as iRacing names them. It decides which best lap applies.</summary>
    /// <param name="CarClass">The class the car runs in this session; the same car may run in different classes.</param>
    public sealed record CarAtTrack(string Track, string TrackConfig, string Car, string CarClass)
    {
        public static readonly CarAtTrack Unknown = new(string.Empty, string.Empty, string.Empty, string.Empty);

        public bool IsKnown => Track.Length > 0 && Car.Length > 0;
    }
}
