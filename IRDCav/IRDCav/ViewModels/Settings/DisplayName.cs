using System.Linq;
using System.Text.RegularExpressions;

namespace IRDCav.ViewModels.Settings
{
    /// <summary>Turns setting names into labels: "LapAhead" → "Lap ahead", "IRatingGain" → "iRating gain".</summary>
    internal static class DisplayName
    {
        private static readonly Regex WordStart = new("(?<=[a-z])(?=[A-Z])", RegexOptions.Compiled);

        public static string Of(string name)
        {
            string[] words = WordStart.Split(name);
            string label = string.Join(" ", words.Select((word, index) => index == 0 ? word : word.ToLowerInvariant()));
            return label.Replace("IRating", "iRating");
        }
    }
}
