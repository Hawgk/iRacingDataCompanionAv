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
            int[] laps = ReadInts(_carLap);
            float[] lapDistPcts = ReadFloats(_carLapDistPct);
            bool[] onPitRoad = ReadBools(_carOnPitRoad);
            int[] positions = ReadInts(_carPosition);
            int[] classPositions = ReadInts(_carClassPosition);
            float[] lastLapTimes = ReadFloats(_carLastLapTime);
            int[] trackLocations = ReadInts(_carTrackLocation);
            int[] trackSurfaces = ReadInts(_carTrackSurface);
            uint[] sessionFlags = ReadBitFields(_carSessionFlags);

            var cars = new CarTelemetry[CarCount];
            for (int i = 0; i < CarCount; i++)
            {
                cars[i] = new CarTelemetry
                {
                    Id = i,
                    Lap = laps[i],
                    LapDistPct = lapDistPcts[i],
                    OnPitRoad = onPitRoad[i],
                    Position = positions[i],
                    ClassPosition = classPositions[i],
                    LastLapTime = lastLapTimes[i],
                    TrackLocation = (TrkLoc)trackLocations[i],
                    TrackSurface = (TrkSurf)trackSurfaces[i],
                    SessionFlags = (Flags)sessionFlags[i],
                };
            }
            return cars;
        }

        public bool TryGetSessionInfo([NotNullWhen(true)] out IRacingSdkSessionInfo? sessionInfo)
        {
            sessionInfo = _data.SessionInfo;
            return sessionInfo != null;
        }

        private IRacingSdkDatum Resolve(string name)
        {
            if (!_data.TelemetryDataProperties.TryGetValue(name, out IRacingSdkDatum? datum))
            {
                throw new InvalidOperationException($"iRacing telemetry variable '{name}' is not available in this session.");
            }
            return datum;
        }

        private int[] ReadInts(IRacingSdkDatum datum)
        {
            var values = new int[CarCount];
            _data.GetIntArray(datum, values, 0, CarCount);
            return values;
        }

        private float[] ReadFloats(IRacingSdkDatum datum)
        {
            var values = new float[CarCount];
            _data.GetFloatArray(datum, values, 0, CarCount);
            return values;
        }

        private bool[] ReadBools(IRacingSdkDatum datum)
        {
            var values = new bool[CarCount];
            _data.GetBoolArray(datum, values, 0, CarCount);
            return values;
        }

        private uint[] ReadBitFields(IRacingSdkDatum datum)
        {
            var values = new uint[CarCount];
            _data.GetBitFieldArray(datum, values, 0, CarCount);
            return values;
        }
    }
}
