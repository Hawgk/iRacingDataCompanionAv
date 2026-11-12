using Avalonia.Media;
using System;
using System.Collections.Generic;

namespace IRDCav.Views.Theme
{
    /// <summary>
    /// Resolves the configured font name. Roboto Mono, Lekton and Inter ship with the application;
    /// any other name is looked up among the fonts installed on the system.
    /// </summary>
    internal static class OverlayFonts
    {
        private static readonly Dictionary<string, string> Bundled = new(StringComparer.OrdinalIgnoreCase)
        {
            ["RobotoMono"] = "avares://IRDCav/Assets/Fonts#Roboto Mono",
            ["Roboto Mono"] = "avares://IRDCav/Assets/Fonts#Roboto Mono",
            ["Lekton"] = "avares://IRDCav/Assets/Fonts#Lekton",
            ["Inter"] = "fonts:Inter#Inter",
        };

        public static FontFamily Resolve(string name) =>
            FontFamily.Parse(Bundled.TryGetValue(name, out string? source) ? source : name);
    }
}
