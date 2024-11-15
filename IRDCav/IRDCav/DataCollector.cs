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
        private bool _firstLap = true;
        private bool _driving = false;
        private int _lastLapCount = 1;
        private int _sessionIndex = 0;

        private RaceDataController _raceDataController = new RaceDataController();
        private FuelDataController _fuelDataController = new FuelDataController();

        private DataViewModel _dataViewModel;
        private RelativesViewModel _relativesViewModel;

        private IRacingSdk _irsdk;
        private IRacingSdkDatum _carIdxClassDatum;
        private IRacingSdkDatum _carIdxLapDistPctDatum;
        private IRacingSdkDatum _carIdxOnPitRoadDatum;
        private IRacingSdkDatum _carIdxPositionDatum;
        private IRacingSdkDatum _carIdxClassPositionDatum;
        private IRacingSdkDatum _carIdxEstTimeDatum;
        private IRacingSdkDatum _carIdxF2TimeDatum;
        private IRacingSdkDatum _carIdxLastLapTimeDatum;
        private IRacingSdkDatum _carIdxBestLapTimeDatum;
        private IRacingSdkDatum _carIdxBestLapNumDatum;
        private IRacingSdkDatum _carIdxLapDatum;
        private IRacingSdkDatum _lapBestLapTimeDatum;
        private IRacingSdkDatum _fuelLevelDatum;
        private IRacingSdkDatum _isOnTrackDatum;
        private IRacingSdkDatum _sessionTimeTotalDatum;
        private IRacingSdkDatum _sessionTimeRemainDatum;
        private IRacingSdkDatum _sessionLapsRemainExDatum;
        private IRacingSdkDatum _sessionLapsTotalDatum;
        private IRacingSdkDatum _sessionNumDatum;

        public DataCollector(DataViewModel dataViewModel, RelativesViewModel relativesViewModel)
        {
            _dataViewModel = dataViewModel;
            _relativesViewModel = relativesViewModel;

            _irsdk = new IRacingSdk();
            _irsdk.OnConnected += OnConnected;
            _irsdk.OnDisconnected += OnDisconnected;
            _irsdk.OnStopped += OnStopped;
            _irsdk.OnTelemetryData += OnTelemetryData;
            _irsdk.OnSessionInfo += OnSessionInfo;

            _irsdk.UpdateInterval = 6;
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
            _fuelDataController.Clear();

            _relativesViewModel.FuelData = new FuelDataModel();

            _relativesViewModel.RaceDataList.Clear();
            _dataViewModel.RaceDataList.Clear();

            _fuelDataController.SessionInfo = new Models.SessionInfoModel();
            _relativesViewModel.SessionInfo = new Models.SessionInfoModel();
            _dataViewModel.SessionInfo = new Models.SessionInfoModel();
        }

        public void OnStopped()
        {
            _fuelDataController.Clear();
            _relativesViewModel.FuelData = new FuelDataModel();
        }

        public void Terminate()
        {
            _irsdk.Stop();
        }

        public void OnSessionInfo()
        {
            //_sessionIndex++;
        }

        private void OnTelemetryData()
        {
            int[] classArr = new int[IRacingSdkConst.MaxNumCars];
            float[] lapDistPctArr = new float[IRacingSdkConst.MaxNumCars];
            bool[] onPitRoadArr = new bool[IRacingSdkConst.MaxNumCars];
            int[] positionArr = new int[IRacingSdkConst.MaxNumCars];
            int[] classPositionArr = new int[IRacingSdkConst.MaxNumCars];
            float[] estTimeArr = new float[IRacingSdkConst.MaxNumCars];
            float[] f2TimeArr = new float[IRacingSdkConst.MaxNumCars];
            float[] lastLapTimeArr = new float[IRacingSdkConst.MaxNumCars];
            float[] bestLapTimeArr = new float[IRacingSdkConst.MaxNumCars];
            int[] bestLapNumArr = new int[IRacingSdkConst.MaxNumCars];
            int[] lapArr = new int[IRacingSdkConst.MaxNumCars];
            float lapBestLapTime = 0;
            float fuelLevel = 0;
            bool isOnTrack = false;
            int sessionLapsRemain = 0;
            int sessionLapsTotal = 0;
            double sessionTimeRemain = 0;
            double sessionTimeTotal = 0;
            int sof = 0;
            int classDriverCount = 0;
            string driverClassName = string.Empty;
            string incidentCount = string.Empty;
            int sessionNum = 0;

            var sessionInfo = _irsdk.Data.SessionInfo;

            if (_isInitialized == false)
            {
                _carIdxClassDatum = _irsdk.Data.TelemetryDataProperties["CarIdxClass"];
                _carIdxLapDistPctDatum = _irsdk.Data.TelemetryDataProperties["CarIdxLapDistPct"];
                _carIdxOnPitRoadDatum = _irsdk.Data.TelemetryDataProperties["CarIdxOnPitRoad"];
                _carIdxPositionDatum = _irsdk.Data.TelemetryDataProperties["CarIdxPosition"];
                _carIdxClassPositionDatum = _irsdk.Data.TelemetryDataProperties["CarIdxClassPosition"];
                _carIdxEstTimeDatum = _irsdk.Data.TelemetryDataProperties["CarIdxEstTime"];
                _carIdxF2TimeDatum = _irsdk.Data.TelemetryDataProperties["CarIdxF2Time"];
                _carIdxLastLapTimeDatum = _irsdk.Data.TelemetryDataProperties["CarIdxLastLapTime"];
                _carIdxBestLapTimeDatum = _irsdk.Data.TelemetryDataProperties["CarIdxBestLapTime"];
                _carIdxBestLapNumDatum = _irsdk.Data.TelemetryDataProperties["CarIdxBestLapNum"];
                _carIdxLapDatum = _irsdk.Data.TelemetryDataProperties["CarIdxLap"];
                _lapBestLapTimeDatum = _irsdk.Data.TelemetryDataProperties["LapBestLapTime"];
                _fuelLevelDatum = _irsdk.Data.TelemetryDataProperties["FuelLevel"];
                _isOnTrackDatum = _irsdk.Data.TelemetryDataProperties["IsOnTrack"];
                _sessionTimeRemainDatum = _irsdk.Data.TelemetryDataProperties["SessionTimeRemain"];
                _sessionTimeTotalDatum = _irsdk.Data.TelemetryDataProperties["SessionTimeTotal"];
                _sessionLapsRemainExDatum = _irsdk.Data.TelemetryDataProperties["SessionLapsRemainEx"];
                _sessionLapsTotalDatum = _irsdk.Data.TelemetryDataProperties["SessionLapsTotal"];
                _sessionNumDatum = _irsdk.Data.TelemetryDataProperties["SessionNum"];

                _isInitialized = true;
            }

            // Data contained in the Session Info
            if (sessionInfo != null)
            {
                var weekendInfo = _irsdk.Data.SessionInfo.WeekendInfo;
                string incidentLimit = sessionInfo.WeekendInfo.WeekendOptions.IncidentLimit;

                List<DriverModel> drivers = sessionInfo.DriverInfo.Drivers;
                List<LiveDataModel> liveDataList = new List<LiveDataModel>();

                // Data from Telemetry
                _irsdk.Data.GetIntArray(_carIdxClassDatum, classArr, 0, IRacingSdkConst.MaxNumCars);
                _irsdk.Data.GetFloatArray(_carIdxLapDistPctDatum, lapDistPctArr, 0, IRacingSdkConst.MaxNumCars);
                _irsdk.Data.GetBoolArray(_carIdxOnPitRoadDatum, onPitRoadArr, 0, IRacingSdkConst.MaxNumCars);
                _irsdk.Data.GetIntArray(_carIdxPositionDatum, positionArr, 0, IRacingSdkConst.MaxNumCars);
                _irsdk.Data.GetIntArray(_carIdxClassPositionDatum, classPositionArr, 0, IRacingSdkConst.MaxNumCars);
                _irsdk.Data.GetFloatArray(_carIdxEstTimeDatum, estTimeArr, 0, IRacingSdkConst.MaxNumCars);
                _irsdk.Data.GetFloatArray(_carIdxF2TimeDatum, f2TimeArr, 0, IRacingSdkConst.MaxNumCars);
                _irsdk.Data.GetFloatArray(_carIdxLastLapTimeDatum, lastLapTimeArr, 0, IRacingSdkConst.MaxNumCars);
                _irsdk.Data.GetFloatArray(_carIdxBestLapTimeDatum, bestLapTimeArr, 0, IRacingSdkConst.MaxNumCars);
                _irsdk.Data.GetIntArray(_carIdxBestLapNumDatum, bestLapNumArr, 0, IRacingSdkConst.MaxNumCars);
                _irsdk.Data.GetIntArray(_carIdxLapDatum, lapArr, 0, IRacingSdkConst.MaxNumCars);
                lapBestLapTime = _irsdk.Data.GetFloat(_lapBestLapTimeDatum);
                fuelLevel = _irsdk.Data.GetFloat(_fuelLevelDatum);
                isOnTrack = _irsdk.Data.GetBool(_isOnTrackDatum);
                sessionNum = _irsdk.Data.GetInt(_sessionNumDatum);

                string sessionType = sessionInfo.SessionInfo.Sessions[sessionNum].SessionName;
                List<PositionModel> positions = sessionInfo.SessionInfo.Sessions[sessionNum].ResultsPositions;

                // Fuel calculation shenanigans.
                // We need to make sure that one full lap was completed before a calculation is done.
                // Not the prettiest but this blocks the FuelDataController calculation.
                // TODO: Currently doesnt work with instant resets as the car stays registered as on track.
                if (isOnTrack && _firstLap)
                {
                    _fuelDataController.StartTimer(fuelLevel);
                    _lastLapCount = lapArr[sessionInfo.DriverInfo.DriverCarIdx];
                    _firstLap = false;
                    _driving = true;
                }
                else if (!isOnTrack && _driving)
                {
                    _fuelDataController.StopTimer();
                    _firstLap = true;
                    _driving = false;
                }

                // If the car is driving on track either only update fuel level or calculate lap fuel data.
                if (_driving)
                {
                    if (lapArr[sessionInfo.DriverInfo.DriverCarIdx] - _lastLapCount > 0)
                    {
                        _relativesViewModel.FuelData = _fuelDataController.GetLapFuelDataModel(fuelLevel);
                    }
                    else
                    {
                        _relativesViewModel.FuelData = _fuelDataController.GetFuelDataModel(fuelLevel);
                    }
                }

                _lastLapCount = lapArr[sessionInfo.DriverInfo.DriverCarIdx];

                // Get current race data from remaining structures and merge into existing data
                if (drivers != null)
                {
                    // Get current race data from array and calculate deltas
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
                            float L = 0;

                            if (bestLapTimeArr[sessionInfo.DriverInfo.DriverCarIdx] < drivers[sessionInfo.DriverInfo.DriverCarIdx].CarClassEstLapTime &&
                                bestLapTimeArr[sessionInfo.DriverInfo.DriverCarIdx] > 0)
                            {
                                L = bestLapTimeArr[sessionInfo.DriverInfo.DriverCarIdx];
                            }
                            else
                            {
                                L = drivers[sessionInfo.DriverInfo.DriverCarIdx].CarClassEstLapTime;
                            }

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
                                ConsiderForRelative = true,
                                LastLapTime = lastLapTimeArr[i],
                                BestLapTime = bestLapTimeArr[i],
                                BestLapNum = bestLapNumArr[i],
                            });
                        }
                    }

                    if (positions != null)
                    {
                        foreach (PositionModel position in positions)
                        {
                            _raceDataController.SetFromPositionModel(position.CarIdx, position);
                        }
                    }
                    foreach (DriverModel driver in drivers)
                    {
                        _raceDataController.SetFromDriverModel(driver.CarIdx, driver);

                        if (driver.CarClassID == drivers[sessionInfo.DriverInfo.DriverCarIdx].CarClassID)
                        {
                            sof += driver.IRating;
                            classDriverCount++;
                        }
                    }
                    sof /= classDriverCount;
                    driverClassName = drivers[sessionInfo.DriverInfo.DriverCarIdx].CarClassShortName;
                    incidentCount = drivers[sessionInfo.DriverInfo.DriverCarIdx].CurDriverIncidentCount.ToString();

                    if (incidentLimit == "unlimited")
                    {
                        incidentLimit = "-";
                    }
                }

                if (weekendInfo != null)
                {
                    sessionLapsRemain = _irsdk.Data.GetInt(_sessionLapsRemainExDatum);
                    sessionLapsTotal = _irsdk.Data.GetInt(_sessionLapsTotalDatum);
                    sessionTimeRemain = _irsdk.Data.GetDouble(_sessionTimeRemainDatum);
                    sessionTimeTotal = _irsdk.Data.GetDouble(_sessionTimeTotalDatum);

                    Models.SessionInfoModel sessionInfoModel = new Models.SessionInfoModel
                    {
                        TrackName = weekendInfo.TrackDisplayName,
                        AirTemp = weekendInfo.TrackAirTemp.Split(" ")[0] + "°C",
                        SurfaceTemp = weekendInfo.TrackSurfaceTemp.Split(" ")[0] + "°C",
                        Precipitation = weekendInfo.TrackPrecipitation.Split(" ")[0] + "%",
                        Humidity = weekendInfo.TrackRelativeHumidity.Split(" ")[0] + "%",
                        ClassCount = weekendInfo.NumCarClasses,
                        LapsRemain = sessionLapsRemain,
                        LapsTotal = sessionLapsTotal,
                        LapsString = (sessionLapsTotal - sessionLapsRemain).ToString() + "/~" + sessionLapsTotal.ToString(),
                        TimeRemain = sessionTimeRemain,
                        TimeTotal = sessionTimeTotal,
                        SOF = sof,
                        DriverCount = classDriverCount,
                        DriverClassName = driverClassName,
                        IncidentCount = incidentCount + "/" + incidentLimit,
                        SessionType = sessionType[0].ToString(),
                    };

                    _fuelDataController.SessionInfo = sessionInfoModel;
                    _relativesViewModel.SessionInfo = sessionInfoModel;
                    _dataViewModel.SessionInfo = sessionInfoModel;
                }

                _raceDataController.CalculateBestLaps();
                _relativesViewModel.RaceDataList = new ObservableCollection<RaceDataModel>(_raceDataController.GetRelativeViewRaceData(7));
                _dataViewModel.RaceDataList = new ObservableCollection<RaceDataModel>(_raceDataController.GetResultsViewRaceData(15));
            }
        }
    }
}
