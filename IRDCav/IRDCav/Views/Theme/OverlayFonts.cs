using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;

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

        private static readonly string[] BundledNames = { "RobotoMono", "Lekton", "Inter" };

        /// <summary>The bundled fonts first, then the installed ones.</summary>
        public static IEnumerable<string> AvailableNames() =>
            BundledNames.Concat(FontManager.Current.SystemFonts.Select(family => family.Name).Distinct().Order());

        public static FontFamily Resolve(string name) =>
            FontFamily.Parse(Bundled.TryGetValue(name, out string? source) ? source : name);
    }
}
