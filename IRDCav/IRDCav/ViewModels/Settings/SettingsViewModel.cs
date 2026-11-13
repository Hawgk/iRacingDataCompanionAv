using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IRDCav.Configuration;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace IRDCav.ViewModels.Settings
{
    /// <summary>Edits every setting of config.json; saving writes the file and applies it right away.</summary>
    public partial class SettingsViewModel : ViewModelBase
    {
        private readonly IOverlayConfigurationStore _store;

        [ObservableProperty] private string _font = string.Empty;
        [ObservableProperty] private decimal? _smallFontSize;
        [ObservableProperty] private decimal? _normalFontSize;
        [ObservableProperty] private decimal? _largeFontSize;
        [ObservableProperty] private RefuelEstimate _refuelEstimate;
        [ObservableProperty] private decimal? _gapDecimals;
        [ObservableProperty] private decimal? _intervalDecimals;
        [ObservableProperty] private string _statusMessage = string.Empty;
        [ObservableProperty] private bool _hasError;

        public SettingsViewModel(IOverlayConfigurationStore store, IEnumerable<string> fontNames)
        {
            _store = store;
            FontNames = new ObservableCollection<string>(fontNames);
            Load(store.Current);
        }

        public ObservableCollection<string> FontNames { get; }
        public IReadOnlyList<RefuelEstimate> RefuelEstimates { get; } = Enum.GetValues<RefuelEstimate>();
        public ObservableCollection<ColorSettingViewModel> Colors { get; } = new();
        public ColumnListViewModel Standings { get; } = new();
        public ColumnListViewModel Relatives { get; } = new();

        [RelayCommand]
        private void Save()
        {
            try
            {
                _store.Save(ToConfiguration());
                ShowStatus("Saved and applied.", isError: false);
            }
            catch (Exception exception) when (exception is InvalidDataException or IOException or UnauthorizedAccessException)
            {
                ShowStatus(exception.Message, isError: true);
            }
        }

        [RelayCommand]
        private void ResetToDefaults()
        {
            Load(new OverlayConfiguration());
            ShowStatus("Defaults loaded. Save to apply them.", isError: false);
        }

        private void Load(OverlayConfiguration configuration)
        {
            LoadGeneral(configuration);
            Colors.Clear();
            foreach (ColorSettingViewModel color in ColorSettingViewModel.From(configuration.ColorScheme))
            {
                Colors.Add(color);
            }
            Standings.Load(configuration.Standings);
            Relatives.Load(configuration.Relatives);
        }

        private void LoadGeneral(OverlayConfiguration configuration)
        {
            if (!FontNames.Contains(configuration.Font))
            {
                FontNames.Insert(0, configuration.Font);
            }
            Font = configuration.Font;
            SmallFontSize = (decimal)configuration.FontSizes.Small;
            NormalFontSize = (decimal)configuration.FontSizes.Normal;
            LargeFontSize = (decimal)configuration.FontSizes.Large;
            RefuelEstimate = configuration.Fuel.RefuelEstimate;
            GapDecimals = configuration.Decimals.Gap;
            IntervalDecimals = configuration.Decimals.Interval;
        }

        /// <exception cref="InvalidDataException">A value is missing or can't be parsed.</exception>
        private OverlayConfiguration ToConfiguration() => new()
        {
            Font = Font,
            ColorScheme = ColorSettingViewModel.ToColorScheme(Colors),
            FontSizes = new FontSizesConfiguration
            {
                Small = (double)Required(SmallFontSize, "small font size"),
                Normal = (double)Required(NormalFontSize, "normal font size"),
                Large = (double)Required(LargeFontSize, "large font size"),
            },
            Fuel = new FuelConfiguration { RefuelEstimate = RefuelEstimate },
            Decimals = new DecimalsConfiguration
            {
                Gap = (int)Required(GapDecimals, "gap decimals"),
                Interval = (int)Required(IntervalDecimals, "interval decimals"),
            },
            Standings = Standings.ToConfiguration(),
            Relatives = Relatives.ToConfiguration(),
        };

        private static decimal Required(decimal? value, string name) =>
            value ?? throw new InvalidDataException($"Enter a value for the {name}.");

        private void ShowStatus(string message, bool isError)
        {
            StatusMessage = message;
            HasError = isError;
        }
    }
}
