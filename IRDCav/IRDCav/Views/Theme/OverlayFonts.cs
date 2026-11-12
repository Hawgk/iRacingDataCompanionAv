using Avalonia.Media;
using System;
using System.Collections.Generic;

namespace IRDCav.Views.Theme
{
    /// <summary>
    /// Resolves the configured font name. Lekton and Inter ship with the application;
    /// any other name is looked up among the fonts installed on the system.
    /// </summary>
    internal static class OverlayFonts
    {
        private static readonly Dictionary<string, string> Bundled = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Lekton"] = "avares://IRDCav/Assets/Fonts#Lekton",
            ["Inter"] = "fonts:Inter#Inter",
        };

        public static FontFamily Resolve(string name) =>
            FontFamily.Parse(Bundled.TryGetValue(name, out string? source) ? source : name);
    }
}
