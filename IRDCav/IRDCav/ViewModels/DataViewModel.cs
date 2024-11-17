using IRDCav.Models;
using System.Collections.ObjectModel;

namespace IRDCav.ViewModels
{
    public class DataViewModel : ViewModelBase
    {
        private bool _isVisible = false;
        private bool _isConnected = false;
        private string _incidentCount = string.Empty;
        private SessionInfoModel _sessionInfo = new SessionInfoModel();
        private TelemetryModel _telemetry = new TelemetryModel();
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

        public SessionInfoModel SessionInfo
        {
            get
            {
                return _sessionInfo;
            }
            set
            {
                if (_sessionInfo != value)
                {
                    _sessionInfo = value;
                    OnPropertyChanged(nameof(SessionInfo));
                }
            }
        }

        public TelemetryModel Telemetry
        {
            get
            {
                return _telemetry;
            }
            set
            {
                _telemetry = value;
                OnPropertyChanged(nameof(Telemetry));
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
