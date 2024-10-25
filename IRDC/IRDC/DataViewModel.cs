using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

namespace IRDC
{
    public class DataViewModel : INotifyPropertyChanged
    {
        private bool _isConnected = false;
        private Visibility _mainWindowVisibility = Visibility.Hidden;
        private SessionInfoModel _sessionInfoModel = new SessionInfoModel();
        private TelemetryModel _telemetryModel = new TelemetryModel();
        private ObservableCollection<ResultsModel> _resultsList = new ObservableCollection<ResultsModel>();

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

        public Visibility MainWindowVisibility
        {
            get
            {
                return _mainWindowVisibility;
            }
            set
            {
                if (_mainWindowVisibility != value)
                {
                    _mainWindowVisibility = value;
                    OnPropertyChanged(nameof(MainWindowVisibility));
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

        public TelemetryModel TelemetryModel
        {
            get
            {
                return _telemetryModel;
            }
            set
            {
                if (_telemetryModel != value)
                {
                    _telemetryModel = value;
                    OnPropertyChanged(nameof(TelemetryModel));
                }
            }
        }

        public ObservableCollection<ResultsModel> ResultsList
        {
            get
            {
                return _resultsList;
            }
            set
            {
                if (_resultsList != value)
                {
                    _resultsList = value;
                    OnPropertyChanged(nameof(ResultsList));
                }
            }
        }

        #region INotifyPropertyChanged Members
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    }
}
