using IRDCav.Models;
using IRDCav.ViewModels;
using IRSDKSharper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using static IRSDKSharper.IRacingSdkSessionInfo.DriverInfoModel;
using static IRSDKSharper.IRacingSdkSessionInfo.SessionInfoModel.SessionModel;

namespace IRDCav
{
    public class DataCollector
    {
        private DataViewModel _dataViewModel;
        private IRacingSdk _irsdk;

        public DataCollector(DataViewModel dataViewModel)
        {
            _dataViewModel = dataViewModel;
            _irsdk = new IRacingSdk();

            _irsdk.OnConnected += OnConnected;
            _irsdk.OnDisconnected += OnDisconnected;
            _irsdk.OnSessionInfo += OnSessionInfo;
            _irsdk.OnTelemetryData += OnTelemetryData;

            _irsdk.UpdateInterval = 6;
            _irsdk.Start();
        }

        public void OnConnected()
        {
            _dataViewModel.IsConnected = true;
        }

        public void OnDisconnected()
        {
            _dataViewModel.IsConnected = false;
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
                    Humidity = weekendInfo.TrackRelativeHumidity
                };

                _dataViewModel.SessionInfoModel = sessionInfoModel;
            }
        }

        private void OnTelemetryData()
        {
            var sessionInfo = _irsdk.Data.SessionInfo;

            if (sessionInfo != null)
            {
                int currentSessionIndex = sessionInfo.SessionInfo.Sessions.Count - 1;

                List<DriverModel> drivers = sessionInfo.DriverInfo.Drivers;
                List<PositionModel> positions = sessionInfo.SessionInfo.Sessions[currentSessionIndex].ResultsPositions;
                List<FastestLapModel> fastestLap = sessionInfo.SessionInfo.Sessions[currentSessionIndex].ResultsFastestLap;

                if (drivers != null && positions != null)
                {
                    List<ResultsModel> resultsList = new List<ResultsModel>(
                        positions.Join(
                            drivers,
                            position => position.CarIdx,
                            driver => driver.CarIdx,
                            (position, driver) =>
                            {
                                TimeSpan fastestTime = TimeSpan.FromSeconds(position.FastestTime);
                                TimeSpan lastTime = TimeSpan.FromSeconds(position.LastTime);
                                string classColor = "#40" + driver.CarClassColor.Substring(2);
                                bool isFastestLap = false;

                                // Only get the fastest lap of the race, but not class specific.
                                // This will need a manual implementation for each class.
                                foreach (FastestLapModel fl in fastestLap)
                                {
                                    if (fl.CarIdx == driver.CarIdx)
                                    {
                                        isFastestLap = true;
                                    }
                                }

                                if (position.ReasonOutId != 0)
                                {
                                    classColor = "#40000000";
                                }

                                return new ResultsModel
                                {
                                    IsMe = (driver.CarIdx == sessionInfo.DriverInfo.DriverCarIdx),
                                    IsFastest = isFastestLap,
                                    Name = driver.UserName,
                                    Class = driver.CarClassShortName,
                                    CarPath = driver.CarScreenNameShort,
                                    License = driver.LicString,
                                    Rating = driver.IRating,
                                    ClassPosition = position.ClassPosition + 1,
                                    LapsCompleted = position.LapsComplete,
                                    FastestLapTime = fastestTime.ToString(@"mm\:ss\:fff"),
                                    LastLapTime = lastTime.ToString(@"mm\:ss\:fff"),
                                    ClassColor = "#40" + driver.CarClassColor.Substring(2)
                                };
                            }
                        )
                    );

                    _dataViewModel.ResultsList = new ObservableCollection<ResultsModel>(resultsList);
                }
            }
        }
    }
}
