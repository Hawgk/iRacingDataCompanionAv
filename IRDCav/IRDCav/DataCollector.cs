using IRDCav.Models;
using IRDCav.ViewModels;
using IRSDKSharper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using static IRSDKSharper.IRacingSdkSessionInfo.DriverInfoModel;
using static IRSDKSharper.IRacingSdkSessionInfo.SessionInfoModel.SessionModel;

namespace IRDCav
{
    public class DataCollector
    {
        private bool _isInitialized = false;
        private int _lastLapCount;

        private RaceDataController _raceDataController = new RaceDataController();

        private FuelDataModel _fuelDataModel;
        private DataViewModel _dataViewModel;
        private RelativesViewModel _relativesViewModel;

        private IRacingSdk _irsdk;
        private IRacingSdkDatum _carIdxClassDatum;
        private IRacingSdkDatum _carIdxLapDistPctDatum;
        private IRacingSdkDatum _carIdxOnPitRoadDatum;
        private IRacingSdkDatum _carIdxPositionDatum;
        private IRacingSdkDatum _carIdxClassPositionDatum;
        private IRacingSdkDatum _carIdxEstTimeDatum;
        private IRacingSdkDatum _carIdxLastLapTimeDatum;
        private IRacingSdkDatum _carIdxBestLapTimeDatum;
        private IRacingSdkDatum _carIdxBestLapNumDatum;
        private IRacingSdkDatum _carIdxLapDatum;
        private IRacingSdkDatum _lapBestLapTimeDatum;
        private IRacingSdkDatum _fuelLevelDatum;

        public DataCollector(DataViewModel dataViewModel, RelativesViewModel relativesViewModel)
        {
            _dataViewModel = dataViewModel;
            _relativesViewModel = relativesViewModel;

            _irsdk = new IRacingSdk();
            _irsdk.OnConnected += OnConnected;
            _irsdk.OnDisconnected += OnDisconnected;
            _irsdk.OnStopped += OnStopped;
            _irsdk.OnSessionInfo += OnSessionInfo;
            _irsdk.OnTelemetryData += OnTelemetryData;

            _irsdk.UpdateInterval = 9;
            _irsdk.Start();
        }

        public void OnConnected()
        {
            _dataViewModel.IsConnected = true;
            _relativesViewModel.IsConnected = true;
        }

        public void OnDisconnected()
        {
            _dataViewModel.IsConnected = false;
            _relativesViewModel.IsConnected = false;
            _raceDataController.Clear();
            _relativesViewModel.FuelDataModel.Clear();
            _relativesViewModel.RaceDataList.Clear();
            _dataViewModel.RaceDataList.Clear();
        }

        public void OnStopped()
        {
            _dataViewModel.IsConnected = false;
            _relativesViewModel.IsConnected = false;
            _raceDataController.Clear();
            _fuelDataModel.Clear();
            _relativesViewModel.FuelDataModel.Clear();
            _relativesViewModel.RaceDataList.Clear();
            _dataViewModel.RaceDataList.Clear();
        }

        public void Terminate()
        {
            _irsdk.Stop();
        }

        private void OnSessionInfo()
        {
            var weekendInfo = _irsdk.Data.SessionInfo.WeekendInfo;

            if (weekendInfo != null)
            {
                SessionInfoModel sessionInfoModel = new SessionInfoModel
                {
                    TrackName = weekendInfo.TrackDisplayName,
                    AirTemp = weekendInfo.TrackAirTemp,
                    SurfaceTemp = weekendInfo.TrackSurfaceTemp,
                    FogLevel = weekendInfo.TrackFogLevel,
                    Humidity = weekendInfo.TrackRelativeHumidity,
                    ClassCount = weekendInfo.NumCarClasses,
                };

                _dataViewModel.SessionInfoModel = sessionInfoModel;
            }

            _fuelDataModel = new FuelDataModel();
        }

        private void OnTelemetryData()
        {
            int[] classArr = new int[IRacingSdkConst.MaxNumCars];
            float[] lapDistPctArr = new float[IRacingSdkConst.MaxNumCars];
            bool[] onPitRoadArr = new bool[IRacingSdkConst.MaxNumCars];
            int[] positionArr = new int[IRacingSdkConst.MaxNumCars];
            int[] classPositionArr = new int[IRacingSdkConst.MaxNumCars];
            float[] estTimeArr = new float[IRacingSdkConst.MaxNumCars];
            float[] lastLapTimeArr = new float[IRacingSdkConst.MaxNumCars];
            float[] bestLapTimeArr = new float[IRacingSdkConst.MaxNumCars];
            int[] bestLapNumArr = new int[IRacingSdkConst.MaxNumCars];
            int[] lapArr = new int[IRacingSdkConst.MaxNumCars];
            float lapBestLapTime = 0;
            float fuelLevel = 0;

            var sessionInfo = _irsdk.Data.SessionInfo;

            if (_isInitialized == false)
            {
                _carIdxClassDatum = _irsdk.Data.TelemetryDataProperties["CarIdxClass"];
                _carIdxLapDistPctDatum = _irsdk.Data.TelemetryDataProperties["CarIdxLapDistPct"];
                _carIdxOnPitRoadDatum = _irsdk.Data.TelemetryDataProperties["CarIdxOnPitRoad"];
                _carIdxPositionDatum = _irsdk.Data.TelemetryDataProperties["CarIdxPosition"];
                _carIdxClassPositionDatum = _irsdk.Data.TelemetryDataProperties["CarIdxClassPosition"];
                _carIdxEstTimeDatum = _irsdk.Data.TelemetryDataProperties["CarIdxEstTime"];
                _carIdxLastLapTimeDatum = _irsdk.Data.TelemetryDataProperties["CarIdxLastLapTime"];
                _carIdxBestLapTimeDatum = _irsdk.Data.TelemetryDataProperties["CarIdxBestLapTime"];
                _carIdxBestLapNumDatum = _irsdk.Data.TelemetryDataProperties["CarIdxBestLapNum"];
                _carIdxLapDatum = _irsdk.Data.TelemetryDataProperties["CarIdxLap"];
                _lapBestLapTimeDatum = _irsdk.Data.TelemetryDataProperties["LapBestLapTime"];
                _fuelLevelDatum = _irsdk.Data.TelemetryDataProperties["FuelLevel"];

                _isInitialized = true;
            }

            // Data contained in the Session Info
            if (sessionInfo != null)
            {
                int currentSessionIndex = sessionInfo.SessionInfo.Sessions.Count - 1;
                string incidentLimit = sessionInfo.WeekendInfo.WeekendOptions.IncidentLimit;

                List<DriverModel> drivers = sessionInfo.DriverInfo.Drivers;
                List<PositionModel> positions = sessionInfo.SessionInfo.Sessions[currentSessionIndex].ResultsPositions;
                List<LiveDataModel> liveDataList = new List<LiveDataModel>();

                // Data from Telemetry
                _irsdk.Data.GetIntArray(_carIdxClassDatum, classArr, 0, IRacingSdkConst.MaxNumCars);
                _irsdk.Data.GetFloatArray(_carIdxLapDistPctDatum, lapDistPctArr, 0, IRacingSdkConst.MaxNumCars);
                _irsdk.Data.GetBoolArray(_carIdxOnPitRoadDatum, onPitRoadArr, 0, IRacingSdkConst.MaxNumCars);
                _irsdk.Data.GetIntArray(_carIdxPositionDatum, positionArr, 0, IRacingSdkConst.MaxNumCars);
                _irsdk.Data.GetIntArray(_carIdxClassPositionDatum, classPositionArr, 0, IRacingSdkConst.MaxNumCars);
                _irsdk.Data.GetFloatArray(_carIdxEstTimeDatum, estTimeArr, 0, IRacingSdkConst.MaxNumCars);
                _irsdk.Data.GetFloatArray(_carIdxLastLapTimeDatum, lastLapTimeArr, 0, IRacingSdkConst.MaxNumCars);
                _irsdk.Data.GetFloatArray(_carIdxBestLapTimeDatum, bestLapTimeArr, 0, IRacingSdkConst.MaxNumCars);
                _irsdk.Data.GetIntArray(_carIdxBestLapNumDatum, bestLapNumArr, 0, IRacingSdkConst.MaxNumCars);
                _irsdk.Data.GetIntArray(_carIdxLapDatum, lapArr, 0, IRacingSdkConst.MaxNumCars);
                lapBestLapTime = _irsdk.Data.GetFloat(_lapBestLapTimeDatum);
                fuelLevel = _irsdk.Data.GetFloat(_fuelLevelDatum);

                if (_fuelDataModel != null)
                {
                    _fuelDataModel.SetLevel(fuelLevel);

                    if (lapArr[sessionInfo.DriverInfo.DriverCarIdx] - _lastLapCount > 0)
                    {
                        _fuelDataModel.Update();
                    }
                    // TODO: Not updating on view
                    _relativesViewModel.FuelDataModel = _fuelDataModel;
                }
                _lastLapCount = lapArr[sessionInfo.DriverInfo.DriverCarIdx];

                _raceDataController.Clear();
                _raceDataController.SetPlayerId(sessionInfo.DriverInfo.DriverCarIdx);

                for (int i = 0; i < IRacingSdkConst.MaxNumCars; i++)
                {
                    int lapcountS = lapArr[sessionInfo.DriverInfo.DriverCarIdx];
                    int lapcountC = lapArr[i];

                    if (lapcountC >= 0)
                    {
                        // If the other car is up to half a lap in front, we consider the delta 'ahead', otherwise 'behind'.
                        float delta = 0;
                        int lapDelta = lapcountC - lapcountS;

                        float L = lapBestLapTime > 0 ? lapBestLapTime : estTimeArr[sessionInfo.DriverInfo.DriverCarIdx];
                        float C = estTimeArr[i];
                        float S = estTimeArr[sessionInfo.DriverInfo.DriverCarIdx];

                        // Does the delta between us and the other car span across the start/finish line?
                        bool wrap = Math.Abs(lapDistPctArr[i] - lapDistPctArr[sessionInfo.DriverInfo.DriverCarIdx]) > 0.5f;

                        if (wrap)
                        {
                            delta = S > C ? (C - S) + L : (C - S) - L;
                            lapDelta += S > C ? -1 : 1;
                        }
                        else
                        {
                            delta = C - S;
                        }

                        _raceDataController.SetFromLiveDataModel(i, new LiveDataModel
                        {
                            Id = i,
                            Class = classArr[i],
                            LapDistPct = lapDistPctArr[i],
                            OnPitRoad = onPitRoadArr[i],
                            Position = positionArr[i],
                            ClassPosition = classPositionArr[i],
                            Interval = (float)Math.Round(delta, 3),
                            LastLapTime = lastLapTimeArr[i],
                            BestLapTime = bestLapTimeArr[i],
                            BestLapNum = bestLapNumArr[i],
                        });
                    }
                }

                if (drivers != null && positions != null)
                {
                    foreach (PositionModel position in positions)
                    {
                        _raceDataController.SetFromPositionModel(position.CarIdx, position);
                    }
                    foreach (DriverModel driver in drivers)
                    {
                        if (_dataViewModel.SessionInfoModel.ClassCount == 1)
                        {
                            driver.CarClassColor = "0x101010";
                        }
                        _raceDataController.SetFromDriverModel(driver.CarIdx, driver);
                    }

                    if (incidentLimit == "unlimited")
                    {
                        incidentLimit = "-";
                    }
                    _relativesViewModel.IncidentCount = drivers[sessionInfo.DriverInfo.DriverCarIdx].CurDriverIncidentCount.ToString() + "/" + incidentLimit;
                }

                _raceDataController.CalculateBestLaps();
                _relativesViewModel.RaceDataList = new ObservableCollection<RaceDataModel>(_raceDataController.GetRelativeViewRaceData(7));
                _dataViewModel.RaceDataList = new ObservableCollection<RaceDataModel>(_raceDataController.GetResultsViewRaceData(15));
            }
        }
    }
}
