using IRDCav.Models;
using System;

namespace IRDCav.Services
{
    /// <summary>Accumulates fuel usage per lap and derives consumption statistics and refuel amounts.</summary>
    internal sealed class FuelCalculator
    {
        private const float MinimumLapSeconds = 5;
        private const float SecondsPerMinute = 60;

        private float _lapConsumption;
        private double _lapStartTimeRemain;

        public FuelDataModel Current { get; private set; } = new();

        public void Reset()
        {
            Current = new FuelDataModel();
            _lapConsumption = 0;
        }

        public void StartStint(float level)
        {
            Current = Current with { Level = level };
            _lapConsumption = 0;
        }

        public void AddSample(float level, double timeRemain)
        {
            _lapConsumption += Current.Level - level;
            Current = Current with
            {
                Level = level,
                RefuelMin = Refuel(level, Current.FuelPerMinuteMin, timeRemain),
                RefuelAvg = Refuel(level, Current.FuelPerMinuteAvg, timeRemain),
                RefuelMax = Refuel(level, Current.FuelPerMinuteMax, timeRemain),
            };
        }

        public void CompleteLap(float level, double timeRemain)
        {
            float lapSeconds = (float)(_lapStartTimeRemain - timeRemain);
            Current = lapSeconds > MinimumLapSeconds
                ? WithLapStatistics(level, lapSeconds)
                : WithoutLapStatistics(level);

            _lapStartTimeRemain = timeRemain;
            _lapConsumption = 0;
        }

        private static float Refuel(float level, float fuelPerMinute, double timeRemain) =>
            fuelPerMinute > 0
                ? (float)Math.Round(level - fuelPerMinute / SecondsPerMinute * timeRemain, 2)
                : 0;

        private FuelDataModel WithoutLapStatistics(float level) => new()
        {
            Level = level,
            MinConsumption = Current.MinConsumption,
            AvgConsumption = Current.AvgConsumption,
            MaxConsumption = Current.MaxConsumption,
        };

        private FuelDataModel WithLapStatistics(float level, float lapSeconds)
        {
            float min = Current.MinConsumption == 0 ? _lapConsumption : Math.Min(Current.MinConsumption, _lapConsumption);
            float avg = Current.AvgConsumption == 0 ? _lapConsumption : (Current.AvgConsumption + _lapConsumption) / 2;
            float max = Current.MaxConsumption == 0 ? _lapConsumption : Math.Max(Current.MaxConsumption, _lapConsumption);

            return new FuelDataModel
            {
                Level = level,
                MinConsumption = min,
                AvgConsumption = avg,
                MaxConsumption = max,
                LastConsumption = _lapConsumption,
                FuelPerMinuteMin = min / lapSeconds * SecondsPerMinute,
                FuelPerMinuteAvg = avg / lapSeconds * SecondsPerMinute,
                FuelPerMinuteMax = max / lapSeconds * SecondsPerMinute,
            };
        }
    }
}
