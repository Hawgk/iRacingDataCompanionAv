using IRDCav.Models;
using System.Collections.ObjectModel;

namespace IRDCav.ViewModels
{
    public class RelativesViewModel : ViewModelBase
    {
        private bool _isConnected = false;
        public string _incidentCount = string.Empty;
        private SessionInfoModel _sessionInfoModel = new SessionInfoModel();
        private FuelDataModel _fuelDataModel = new FuelDataModel();
        private ObservableCollection<RaceDataModel> _raceDataList = new ObservableCollection<RaceDataModel>();

        public bool IsConnected
        {
            get
            {
                return _isConnected;
            }
            set
            {
                if (_isConnected != value)
                {
                    _isConnected = value;
                    OnPropertyChanged(nameof(IsConnected));
                }
            }
        }

        public string IncidentCount
        {
            get
            {
                return _incidentCount;
            }
            set
            {
                if (_incidentCount != value)
                {
                    _incidentCount = value;
                    OnPropertyChanged(nameof(IncidentCount));
                }
            }
        }

        public SessionInfoModel SessionInfoModel
        {
            get
            {
                return _sessionInfoModel;
            }
            set
            {
                if (_sessionInfoModel != value)
                {
                    _sessionInfoModel = value;
                    OnPropertyChanged(nameof(SessionInfoModel));
                }
            }
        }

        public FuelDataModel FuelDataModel
        {
            get
            {
                return _fuelDataModel;
            }
            set
            {
                _fuelDataModel = value;
                OnPropertyChanged(nameof(FuelDataModel));
            }
        }

        public ObservableCollection<RaceDataModel> RaceDataList
        {
            get
            {
                return _raceDataList;
            }
            set
            {
                _raceDataList = value;
                OnPropertyChanged(nameof(RaceDataList));
            }
        }
    }
}
