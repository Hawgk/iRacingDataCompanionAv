using IRSDKSharper;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using static IRSDKSharper.IRacingSdkEnum;

namespace IRDCav.Telemetry
{
    /// <summary>Reads telemetry through datum handles that are resolved once per connection.</summary>
    internal sealed class IRacingTelemetryFrame : ITelemetryFrame
    {
        private const int CarCount = IRacingSdkConst.MaxNumCars;

        private readonly IRacingSdkData _data;

        private readonly IRacingSdkDatum _throttle;
        private readonly IRacingSdkDatum _brake;

        private readonly IRacingSdkDatum _sessionNum;
        private readonly IRacingSdkDatum _sessionTime;
        private readonly IRacingSdkDatum _sessionTimeRemain;
        private readonly IRacingSdkDatum _fuelLevel;
        private readonly IRacingSdkDatum _isOnTrack;

        private readonly IRacingSdkDatum _carLap;
        private readonly IRacingSdkDatum _carLapDistPct;
        private readonly IRacingSdkDatum _carOnPitRoad;
        private readonly IRacingSdkDatum _carPosition;
        private readonly IRacingSdkDatum _carClassPosition;
        private readonly IRacingSdkDatum _carLastLapTime;
        private readonly IRacingSdkDatum _carTrackLocation;
        private readonly IRacingSdkDatum _carTrackSurface;
        private readonly IRacingSdkDatum _carSessionFlags;

        // Reused on every read to avoid allocations on the telemetry thread.
        private readonly int[] _laps = new int[CarCount];
        private readonly float[] _lapDistPcts = new float[CarCount];
        private readonly bool[] _onPitRoad = new bool[CarCount];
        private readonly int[] _positions = new int[CarCount];
        private readonly int[] _classPositions = new int[CarCount];
        private readonly float[] _lastLapTimes = new float[CarCount];
        private readonly int[] _trackLocations = new int[CarCount];
        private readonly int[] _trackSurfaces = new int[CarCount];
        private readonly uint[] _sessionFlags = new uint[CarCount];
        private readonly CarTelemetry[] _cars = new CarTelemetry[CarCount];

        public IRacingTelemetryFrame(IRacingSdkData data)
        {
            _data = data;

            _throttle = Resolve("ThrottleRaw");
            _brake = Resolve("BrakeRaw");

            _sessionNum = Resolve("SessionNum");
            _sessionTime = Resolve("SessionTime");
            _sessionTimeRemain = Resolve("SessionTimeRemain");
            _fuelLevel = Resolve("FuelLevel");
            _isOnTrack = Resolve("IsOnTrack");

            _carLap = Resolve("CarIdxLap");
            _carLapDistPct = Resolve("CarIdxLapDistPct");
            _carOnPitRoad = Resolve("CarIdxOnPitRoad");
            _carPosition = Resolve("CarIdxPosition");
            _carClassPosition = Resolve("CarIdxClassPosition");
            _carLastLapTime = Resolve("CarIdxLastLapTime");
            _carTrackLocation = Resolve("CarIdxTrackSurface");
            _carTrackSurface = Resolve("CarIdxTrackSurfaceMaterial");
            _carSessionFlags = Resolve("CarIdxSessionFlags");
        }

        public InputSample ReadInputs() => new(_data.GetFloat(_throttle), _data.GetFloat(_brake));

        public SessionTelemetry ReadSession() => new()
        {
            SessionNum = _data.GetInt(_sessionNum),
            SessionTime = _data.GetDouble(_sessionTime),
            TimeRemain = _data.GetDouble(_sessionTimeRemain),
            FuelLevel = _data.GetFloat(_fuelLevel),
            IsOnTrack = _data.GetBool(_isOnTrack),
        };

        public IReadOnlyList<CarTelemetry> ReadCars()
        {
            ReadCarArrays();
            for (int i = 0; i < CarCount; i++)
            {
                _cars[i] = new CarTelemetry
                {
                    Id = i,
                    Lap = _laps[i],
                    LapDistPct = _lapDistPcts[i],
                    OnPitRoad = _onPitRoad[i],
                    Position = _positions[i],
                    ClassPosition = _classPositions[i],
                    LastLapTime = _lastLapTimes[i],
                    TrackLocation = (TrkLoc)_trackLocations[i],
                    TrackSurface = (TrkSurf)_trackSurfaces[i],
                    SessionFlags = (Flags)_sessionFlags[i],
                };
            }
            return _cars;
        }

        public bool TryGetSessionInfo([NotNullWhen(true)] out IRacingSdkSessionInfo? sessionInfo)
        {
            sessionInfo = _data.SessionInfo;
            return sessionInfo != null;
        }

        public int SessionInfoVersion => _data.SessionInfoUpdate;

        private IRacingSdkDatum Resolve(string name)
        {
            if (!_data.TelemetryDataProperties.TryGetValue(name, out IRacingSdkDatum? datum))
            {
                throw new InvalidOperationException($"iRacing telemetry variable '{name}' is not available in this session.");
            }
            return datum;
        }

        private void ReadCarArrays()
        {
            _data.GetIntArray(_carLap, _laps, 0, CarCount);
            _data.GetFloatArray(_carLapDistPct, _lapDistPcts, 0, CarCount);
            _data.GetBoolArray(_carOnPitRoad, _onPitRoad, 0, CarCount);
            _data.GetIntArray(_carPosition, _positions, 0, CarCount);
            _data.GetIntArray(_carClassPosition, _classPositions, 0, CarCount);
            _data.GetFloatArray(_carLastLapTime, _lastLapTimes, 0, CarCount);
            _data.GetIntArray(_carTrackLocation, _trackLocations, 0, CarCount);
            _data.GetIntArray(_carTrackSurface, _trackSurfaces, 0, CarCount);
            _data.GetBitFieldArray(_carSessionFlags, _sessionFlags, 0, CarCount);
        }
    }
}
