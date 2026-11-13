using Avalonia.Media;

namespace IRDCav.Configuration
{
    /// <summary>
    /// Overlay colors as #AARRGGBB or #RRGGBB. The default is greyscale with pastel tones
    /// for the states a driver needs to notice.
    /// </summary>
    public sealed record ColorScheme
    {
        public Color Background { get; init; } = Color.Parse("#80141518");
        public Color Surface { get; init; } = Color.Parse("#801E1F23");
        public Color Text { get; init; } = Color.Parse("#E6E7EA");
        public Color TextMuted { get; init; } = Color.Parse("#8B8E96");
        public Color PlayerHighlight { get; init; } = Color.Parse("#F2D49B");
        public Color LapAhead { get; init; } = Color.Parse("#F2A7A3");
        public Color LapBehind { get; init; } = Color.Parse("#A7C4F2");
        public Color FastestLap { get; init; } = Color.Parse("#C8B3F0");
        public Color PitRoad { get; init; } = Color.Parse("#A8DCC4");
        public Color Warning { get; init; } = Color.Parse("#F2D49B");
        public Color IRatingGain { get; init; } = Color.Parse("#9ED6A5");
        public Color IRatingLoss { get; init; } = Color.Parse("#F0A3A0");
        public Color TimeGain { get; init; } = Color.Parse("#9ED6A5");
        public Color TimeLoss { get; init; } = Color.Parse("#F0A3A0");
        public Color Throttle { get; init; } = Color.Parse("#9ED6A5");
        public Color Brake { get; init; } = Color.Parse("#F0A3A0");
    }
}
