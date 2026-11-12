using IRDCav.Models;
using IRDCav.Presentation;
using IRDCav.Telemetry;
using IRSDKSharper;
using System.Collections.Generic;
using static IRSDKSharper.IRacingSdkSessionInfo.SessionInfoModel.SessionModel;

namespace IRDCav.Services
{
    /// <summary>
    /// Routes each telemetry frame through the calculation services and hands the results
    /// to the presenter. Runs entirely on the telemetry source's thread.
    /// </summary>
    internal sealed class TelemetryProcessor
    {
        // Race data is processed on every n-th frame only; inputs on every frame.
        private const int RaceDataFrameInterval = 4;

        private readonly ITelemetrySource _source;
        private readonly RaceDataController _raceData;
        private readonly FuelMonitor _fuel;
        private readonly SessionInfoBuilder _sessionInfoBuilder;
        private readonly RelativeListBuilder _relativeBuilder;
        private readonly StandingsListBuilder _standingsBuilder;
        private readonly IOverlayPresenter _presenter;

        private SessionInfoModel _sessionInfo = new();
        private int _frameCounter;

        public TelemetryProcessor(TelemetryServices services, IOverlayPresenter presenter)
        {
            _source = services.Source;
            _raceData = services.RaceData;
            _fuel = services.Fuel;
            _sessionInfoBuilder = services.SessionInfoBuilder;
            _relativeBuilder = services.RelativeBuilder;
            _standingsBuilder = services.StandingsBuilder;
            _presenter = presenter;

            _source.Connected += OnConnected;
            _source.Disconnected += OnDisconnected;
            _source.Stopped += OnStopped;
            _source.FrameReceived += OnFrameReceived;
        }

        public void Start() => _source.Start();

        public void Stop() => _source.Stop();

        private void OnConnected() => _presenter.ShowOverlays();

        private void OnDisconnected()
        {
            _raceData.Clear();
            _fuel.Reset();
            _sessionInfo = new SessionInfoModel();
            _presenter.HideOverlays();
        }

        private void OnStopped()
        {
            _fuel.Reset();
            _presenter.ShowFuel(_fuel.Current);
        }

        private void OnFrameReceived(ITelemetryFrame frame)
        {
            _presenter.ShowInputs(frame.ReadInputs());

            if (IsRaceDataFrame() && frame.TryGetSessionInfo(out IRacingSdkSessionInfo? sessionInfo))
            {
                ProcessRaceData(frame, sessionInfo);
            }
        }

        private bool IsRaceDataFrame()
        {
            bool isRaceDataFrame = _frameCounter == 0;
            _frameCounter = (_frameCounter + 1) % RaceDataFrameInterval;
            return isRaceDataFrame;
        }

        private void ProcessRaceData(ITelemetryFrame frame, IRacingSdkSessionInfo sessionInfo)
        {
            SessionTelemetry session = frame.ReadSession();
            IReadOnlyList<CarTelemetry> cars = frame.ReadCars();
            int playerIdx = sessionInfo.DriverInfo.DriverCarIdx;

            UpdateSessionInfo(sessionInfo, session);
            UpdateRaceData(sessionInfo, session, cars);
            UpdateFuel(session, cars[playerIdx].Lap);
        }

        private void UpdateSessionInfo(IRacingSdkSessionInfo sessionInfo, SessionTelemetry session)
        {
            if (sessionInfo.WeekendInfo != null)
            {
                _sessionInfo = _sessionInfoBuilder.Build(sessionInfo, session);
                _presenter.ShowSession(_sessionInfo);
            }
        }

        private void UpdateRaceData(IRacingSdkSessionInfo sessionInfo, SessionTelemetry session, IReadOnlyList<CarTelemetry> cars)
        {
            _raceData.SetPlayer(sessionInfo.DriverInfo.DriverCarIdx);
            _raceData.ApplyTelemetry(cars);
            _raceData.ApplyPositions(CurrentResults(sessionInfo, session.SessionNum));
            _raceData.ApplyDrivers(sessionInfo.DriverInfo.Drivers ?? new());
            _raceData.Advance(session.SessionTime);

            IReadOnlyList<RaceDataModel> snapshot = _raceData.Snapshot();
            _presenter.ShowRelatives(_relativeBuilder.Build(snapshot));
            _presenter.ShowStandings(_standingsBuilder.Build(snapshot, _sessionInfo.SessionType));
        }

        private void UpdateFuel(SessionTelemetry session, int playerLap)
        {
            _fuel.Update(session, playerLap);
            if (_fuel.IsDriving)
            {
                _presenter.ShowFuel(_fuel.Current);
            }
        }

        private static IEnumerable<PositionModel> CurrentResults(IRacingSdkSessionInfo sessionInfo, int sessionNum)
        {
            var sessions = sessionInfo.SessionInfo.Sessions;
            bool isKnownSession = sessionNum >= 0 && sessionNum < sessions.Count;
            return (isKnownSession ? sessions[sessionNum].ResultsPositions : null) ?? new List<PositionModel>();
        }
    }
}
