namespace IRDCav.LapFiles
{
    /// <summary>What names iRacing's lap file of the player's car at a track.</summary>
    /// <param name="TrackName">iRacing's internal track name, e.g. "suzuka grandprix"; the lap files' folder.</param>
    /// <param name="CarPath">iRacing's internal car name, e.g. "amvantageevogt3".</param>
    /// <param name="CustomerId">The player's iRacing customer ID, which every lap file name starts with.</param>
    public sealed record LapFileKey(string TrackName, string CarPath, int CustomerId)
    {
        public static readonly LapFileKey Unknown = new(string.Empty, string.Empty, 0);

        public bool IsKnown => TrackName.Length > 0 && CarPath.Length > 0 && CustomerId > 0;
    }
}
