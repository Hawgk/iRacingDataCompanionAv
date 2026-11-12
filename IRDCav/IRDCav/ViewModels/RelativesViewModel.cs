using CommunityToolkit.Mvvm.ComponentModel;
using IRDCav.Configuration;
using IRDCav.Models;
using System.Collections.Generic;

namespace IRDCav.ViewModels
{
    public partial class RelativesViewModel : ViewModelBase
    {
        private readonly RefuelEstimate _refuelEstimate;

        [ObservableProperty]
        private bool _isVisible;

        [ObservableProperty]
        private SessionInfoModel _sessionInfo = new();

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Fuel))]
        private FuelDataModel _fuelData = new();

        public RelativesViewModel(ListOverlayConfiguration table, RefuelEstimate refuelEstimate)
        {
            Columns = table.Columns;
            ShowColumnTitles = table.ShowColumnTitles;
            _refuelEstimate = refuelEstimate;
        }

        public IReadOnlyList<RaceColumn> Columns { get; }

        public bool ShowColumnTitles { get; }

        public FuelSummary Fuel => FuelSummary.From(FuelData, _refuelEstimate);

        public RaceRowCollection RaceDataList { get; } = new();
    }
}
