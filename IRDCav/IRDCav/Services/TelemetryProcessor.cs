using IRDCav.Presentation;
using IRDCav.Telemetry;
using IRSDKSharper;
using System.Collections.Generic;
using static IRSDKSharper.IRacingSdkSessionInfo.SessionInfoModel.SessionModel;

namespace IRDCav.Services
{
    /// <summary>
    /// Routes each telemetry frame through the calculation services and publishes the results.
    /// Runs entirely on the telemetry source's thread and never touches the UI.
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
        private readonly IOverlayStatePublisher _publisher;
        private readonly IInputSink _inputs;

        private OverlayState _state = OverlayState.Disconnected;
        private int _frameCounter;

        public TelemetryProcessor(TelemetryServices services, IOverlayStatePublisher publisher, IInputSink inputs)
        {
            _source = services.Source;
            _raceData = services.RaceData;
            _fuel = services.Fuel;
            _sessionInfoBuilder = services.SessionInfoBuilder;
            _relativeBuilder = services.RelativeBuilder;
            _standingsBuilder = services.StandingsBuilder;
            _publisher = publisher;
            _inputs = inputs;

            _source.Connected += OnConnected;
            _source.Disconnected += OnDisconnected;
            _source.Stopped += OnStopped;
            _source.FrameReceived += OnFrameReceived;
        }

        public void Start() => _source.Start();

        public void Stop() => _source.Stop();

        private void OnConnected() => Publish(OverlayState.Disconnected with { IsConnected = true });

        private void OnDisconnected()
        {
            _raceData.Clear();
            _fuel.Reset();
            _inputs.Clear();
            Publish(OverlayState.Disconnected);
        }

        private void OnStopped()
        {
            _fuel.Reset();
            Publish(_state with { Fuel = _fuel.Current });
        }

        private void OnFrameReceived(ITelemetryFrame frame)
        {
            _inputs.Push(frame.ReadInputs());

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

            OverlayState state = _state with { IsConnected = true };
            state = UpdateSessionInfo(state, sessionInfo, session);
            MergeRaceData(sessionInfo, session, cars);
            state = WithRaceRows(state);
            state = UpdateFuel(state, session, cars[sessionInfo.DriverInfo.DriverCarIdx].Lap);
            Publish(state);
        }

        private OverlayState UpdateSessionInfo(OverlayState state, IRacingSdkSessionInfo sessionInfo, SessionTelemetry session) =>
            sessionInfo.WeekendInfo != null
                ? state with { Session = _sessionInfoBuilder.Build(sessionInfo, session) }
                : state;

        private void MergeRaceData(IRacingSdkSessionInfo sessionInfo, SessionTelemetry session, IReadOnlyList<CarTelemetry> cars)
        {
            _raceData.SetPlayer(sessionInfo.DriverInfo.DriverCarIdx);
            _raceData.ApplyTelemetry(cars);
            _raceData.ApplyPositions(CurrentResults(sessionInfo, session.SessionNum));
            _raceData.ApplyDrivers(sessionInfo.DriverInfo.Drivers ?? new());
            _raceData.Advance(session.SessionTime);
        }

        private OverlayState WithRaceRows(OverlayState state) =>
            state with
            {
                Relatives = _relativeBuilder.Build(_raceData.Cars),
                Standings = _standingsBuilder.Build(_raceData.Cars, state.Session.SessionType),
            };

        private OverlayState UpdateFuel(OverlayState state, SessionTelemetry session, int playerLap)
        {
            _fuel.Update(session, playerLap);
            return _fuel.IsDriving ? state with { Fuel = _fuel.Current } : state;
        }

        private void Publish(OverlayState state)
        {
            _state = state;
            _publisher.Publish(state);
        }

        private static IEnumerable<PositionModel> CurrentResults(IRacingSdkSessionInfo sessionInfo, int sessionNum)
        {
            var sessions = sessionInfo.SessionInfo.Sessions;
            bool isKnownSession = sessionNum >= 0 && sessionNum < sessions.Count;
            return (isKnownSession ? sessions[sessionNum].ResultsPositions : null) ?? new List<PositionModel>();
        }
    }
}
