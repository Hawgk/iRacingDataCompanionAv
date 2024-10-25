using IRSDKSharper;
using System.Collections.ObjectModel;
using static IRSDKSharper.IRacingSdkSessionInfo.DriverInfoModel;
using static IRSDKSharper.IRacingSdkSessionInfo.SessionInfoModel.SessionModel;

namespace IRDC
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
             
            _irsdk.Start();
        }

        public void OnConnected()
        {
            _dataViewModel.IsConnected = true;
            _dataViewModel.MainWindowVisibility = VisibilityConverter.BoolToVisibilityConverter(_dataViewModel.IsConnected);
        }

        public void OnDisconnected()
        {
            _dataViewModel.IsConnected = false;
            _dataViewModel.MainWindowVisibility = VisibilityConverter.BoolToVisibilityConverter(_dataViewModel.IsConnected);
        }

        public void Terminate()
        {
            _irsdk.Stop();
        }

        private void OnSessionInfo()
        {
            string trackName = _irsdk.Data.SessionInfo.WeekendInfo.TrackName;

            SessionInfoModel sessionInfoModel = new SessionInfoModel
            {
                TrackName = trackName
            };

            _dataViewModel.SessionInfoModel = sessionInfoModel;
        }

        private void OnTelemetryData()
        {
            var sessionInfo = _irsdk.Data.SessionInfo;

            if (sessionInfo != null)
            {
                int currentSessionIndex = sessionInfo.SessionInfo.Sessions.Count - 1;

                List<DriverModel> drivers = sessionInfo.DriverInfo.Drivers;
                List<PositionModel> positions = sessionInfo.SessionInfo.Sessions[currentSessionIndex].ResultsPositions;

                if (drivers != null && positions != null)
                {
                    List<ResultsModel> resultsList = new List<ResultsModel>(
                        positions.Join(
                            drivers,
                            position => position.CarIdx,
                            driver => driver.CarIdx,
                            (position, driver) => new ResultsModel
                            {
                                Name = driver.UserName,
                                Class = driver.CarClassShortName,
                                CarPath = driver.CarPath,
                                License = driver.LicString,
                                Rating = driver.IRating,
                                ClassPosition = position.ClassPosition + 1,
                                LapsCompleted = position.LapsComplete,
                                FastestLapTime = position.FastestTime,
                                LastLapTime = position.LastTime
                            }
                        )
                    );
                    _dataViewModel.ResultsList = new ObservableCollection<ResultsModel>(resultsList);
                }
            }

            //-------------------------------------------------------//

            decimal throttlePerc = Math.Round((decimal)_irsdk.Data.GetFloat("Throttle") * 100, 2);
            decimal brakePerc = Math.Round((decimal)_irsdk.Data.GetFloat("Brake") * 100, 2);
            decimal clutchPerc = Math.Round((decimal)_irsdk.Data.GetFloat("Clutch") * 100, 2);

            TelemetryModel telemetryModel = new TelemetryModel
            {
                ThrottlePerc = throttlePerc,
                BrakePerc = brakePerc,
                ClutchPerc = clutchPerc
            };

            _dataViewModel.TelemetryModel = telemetryModel;
        }
    }
}
