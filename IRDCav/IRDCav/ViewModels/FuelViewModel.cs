using CommunityToolkit.Mvvm.ComponentModel;
using IRDCav.Configuration;
using IRDCav.Models;

namespace IRDCav.ViewModels
{
    /// <summary>The fuel window: weather and the fuel table.</summary>
    public partial class FuelViewModel : ViewModelBase
    {
        private readonly RefuelEstimate _refuelEstimate;

        [ObservableProperty]
        private bool _isVisible;

        [ObservableProperty]
        private SessionInfoModel _sessionInfo = new();

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Fuel))]
        private FuelDataModel _fuelData = new();

        public FuelViewModel(RefuelEstimate refuelEstimate)
        {
            _refuelEstimate = refuelEstimate;
        }

        public FuelSummary Fuel => FuelSummary.From(FuelData, _refuelEstimate);
    }
}
