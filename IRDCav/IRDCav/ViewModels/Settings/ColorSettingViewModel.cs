using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using IRDCav.Configuration;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace IRDCav.ViewModels.Settings
{
    /// <summary>One color of the color scheme, edited as #RRGGBB or #AARRGGBB.</summary>
    public partial class ColorSettingViewModel : ViewModelBase
    {
        // Every color of the scheme, so new colors show up in the settings without further changes.
        private static readonly IReadOnlyList<PropertyInfo> ColorProperties =
            typeof(ColorScheme).GetProperties().Where(property => property.PropertyType == typeof(Color)).ToList();

        private readonly PropertyInfo _property;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Preview), nameof(IsValid))]
        private string _hex;

        private ColorSettingViewModel(PropertyInfo property, Color color)
        {
            _property = property;
            _hex = $"#{color.ToUInt32():X8}";
            Label = DisplayName.Of(property.Name).ToUpperInvariant();
        }

        public string Label { get; }

        public bool IsValid => TryParse(out _);

        public IBrush Preview => TryParse(out Color color) ? new SolidColorBrush(color) : Brushes.Transparent;

        public static IEnumerable<ColorSettingViewModel> From(ColorScheme scheme) =>
            ColorProperties.Select(property => new ColorSettingViewModel(property, (Color)property.GetValue(scheme)!));

        /// <exception cref="InvalidDataException">A color can't be parsed.</exception>
        public static ColorScheme ToColorScheme(IEnumerable<ColorSettingViewModel> settings)
        {
            var scheme = new ColorScheme();
            foreach (ColorSettingViewModel setting in settings)
            {
                setting._property.SetValue(scheme, setting.Parse());
            }
            return scheme;
        }

        private Color Parse() => TryParse(out Color color)
            ? color
            : throw new InvalidDataException($"{Label}: '{Hex}' is not a color. Use #RRGGBB or #AARRGGBB.");

        private bool TryParse(out Color color)
        {
            color = default;
            return Hex.StartsWith('#') && Color.TryParse(Hex, out color);
        }
    }
}
