namespace IRDCav.ViewModels
{
    /// <summary>How a tire compound is shown: its first letter in the color racing conventionally uses for it.</summary>
    public static class TireCompoundStyle
    {
        public static string LetterOf(string compound) =>
            compound.Length == 0 ? string.Empty : compound.Substring(0, 1).ToUpperInvariant();

        /// <summary>"#RRGGBB", or empty for compounds without a conventional color.</summary>
        public static string ColorOf(string compound) => compound.ToLowerInvariant() switch
        {
            var name when name.Contains("soft") => "#E8463A",
            var name when name.Contains("medium") => "#F2C230",
            var name when name.Contains("hard") || name.Contains("dry") => "#EDEDED",
            var name when name.Contains("inter") => "#3CC66B",
            var name when name.Contains("wet") || name.Contains("rain") => "#3A8EE8",
            _ => string.Empty,
        };
    }
}
