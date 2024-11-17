using IRDCav.Models;
using System.Collections.ObjectModel;

namespace IRDCav.ViewModels
{
    public class RelativesViewModel : ViewModelBase
    {
        private bool _isVisible = false;
        private bool _isConnected = false;
        private SessionInfoModel _sessionInfo = new SessionInfoModel();
        private FuelDataModel _fuelData = new FuelDataModel();
        private ObservableCollection<RaceDataModel> _raceDataList = new ObservableCollection<RaceDataModel>();

        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                _isVisible = value;
                OnPropertyChanged(nameof(IsVisible));
            }
        }

        public bool IsConnected
        {
            get => _isConnected;
            set
            {
                if (_isConnected != value)
                {
                    _isConnected = value;
                    OnPropertyChanged(nameof(IsConnected));
                }
            }
        }

        public SessionInfoModel SessionInfo
        {
            get => _sessionInfo;
            set
            {
                if (_sessionInfo != value)
                {
                    _sessionInfo = value;
                    OnPropertyChanged(nameof(SessionInfo));
                }
            }
        }

        public FuelDataModel FuelData
        {
            get => _fuelData;
            set
            {
                _fuelData = value;
                OnPropertyChanged(nameof(FuelData));
            }
        }

        public ObservableCollection<RaceDataModel> RaceDataList
        {
            get => _raceDataList;
            set
            {
                _raceDataList = value;
                OnPropertyChanged(nameof(RaceDataList));
            }
        }
    }
}
