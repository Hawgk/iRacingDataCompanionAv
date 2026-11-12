using CommunityToolkit.Mvvm.ComponentModel;
using IRDCav.Models;

namespace IRDCav.ViewModels
{
    public partial class RelativesViewModel : ViewModelBase
    {
        [ObservableProperty]
        private bool _isVisible;

        [ObservableProperty]
        private SessionInfoModel _sessionInfo = new();

        [ObservableProperty]
        private FuelDataModel _fuelData = new();

        public RaceRowCollection RaceDataList { get; } = new();
    }
}
