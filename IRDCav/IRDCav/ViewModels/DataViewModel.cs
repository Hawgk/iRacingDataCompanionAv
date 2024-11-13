﻿using IRDCav.Models;
using System.Collections.ObjectModel;

namespace IRDCav.ViewModels
{
    public class DataViewModel : ViewModelBase
    {
        private bool _isConnected = false;
        private SessionInfoModel _sessionInfoModel = new SessionInfoModel();
        private TelemetryModel _telemetryModel = new TelemetryModel();
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
                _telemetryModel = value;
                OnPropertyChanged(nameof(TelemetryModel));
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
