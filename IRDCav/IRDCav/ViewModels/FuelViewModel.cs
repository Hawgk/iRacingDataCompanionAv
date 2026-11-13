using CommunityToolkit.Mvvm.ComponentModel;
using IRDCav.Configuration;
using IRDCav.Models;

namespace IRDCav.ViewModels
{
    /// <summary>The fuel window: weather and the fuel table.</summary>
    public partial class FuelViewModel : ViewModelBase
    {
        [ObservableProperty]
        private bool _isVisible;

        [ObservableProperty]
        private SessionInfoModel _sessionInfo = new();

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Fuel))]
        private FuelDataModel _fuelData = new();

        /// <summary>The estimate that is enlarged in the fuel table.</summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Fuel))]
        private RefuelEstimate _refuelEstimate = RefuelEstimate.Average;

        public FuelSummary Fuel => FuelSummary.From(FuelData, RefuelEstimate);
    }
}
