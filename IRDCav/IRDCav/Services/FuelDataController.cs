using IRDCav.Models;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace IRDCav.Services
{
    public delegate void FuelDataReadyHandler(FuelDataModel fuelData);

    internal class FuelDataController
    {
        public event FuelDataReadyHandler OnUpdated;

        private FuelDataModel _lastFuelData = new FuelDataModel();
        private Stopwatch _timer = new Stopwatch();

        private double _lastSessionTime;
        private float _fuelDelta;

        public void Update(bool isLapComplete, float level, double timeRemain)
        {
            Task t = new Task(() =>
            {
                FuelDataModel fuelData;

                if (isLapComplete)
                {
                    float elapsedTime = (float)(_lastSessionTime - timeRemain);

                    fuelData = GetLapFuelDataModel(level, elapsedTime);
                    _lastSessionTime = timeRemain;
                }
                else
                {
                    fuelData = GetFuelDataModel(level, timeRemain);
                }

                _lastFuelData = fuelData;

                OnUpdated?.Invoke(fuelData);
            });
            t.Start();
        }

        public void Start(float level)
        {
            _lastFuelData.Level = level;
            _fuelDelta = 0;
        }

        public void Clear()
        {
            _lastFuelData = new FuelDataModel();
            _fuelDelta = 0;
        }

        private FuelDataModel GetFuelDataModel(float level, double timeRemain)
        {
            FuelDataModel fuelDataModel = new FuelDataModel();

            _fuelDelta += _lastFuelData.Level - level;
            fuelDataModel.Level = level;
            fuelDataModel.MinConsumption = _lastFuelData.MinConsumption;
            fuelDataModel.AvgConsumption = _lastFuelData.AvgConsumption;
            fuelDataModel.MaxConsumption = _lastFuelData.MaxConsumption;
            fuelDataModel.LastConsumption = _lastFuelData.LastConsumption;
            fuelDataModel.FuelPerMinuteMin = _lastFuelData.FuelPerMinuteMin;
            fuelDataModel.FuelPerMinuteAvg = _lastFuelData.FuelPerMinuteAvg;
            fuelDataModel.FuelPerMinuteMax = _lastFuelData.FuelPerMinuteMax;

            if (_lastFuelData.FuelPerMinuteMin > 0)
            {
                fuelDataModel.RefuelMin = (float)Math.Round(level - (_lastFuelData.FuelPerMinuteMin / 60) * timeRemain, 2);
            }
            if (_lastFuelData.FuelPerMinuteAvg > 0)
            {
                fuelDataModel.RefuelAvg = (float)Math.Round(level - (_lastFuelData.FuelPerMinuteAvg / 60) * timeRemain, 2);
            }
            if (_lastFuelData.FuelPerMinuteMax > 0)
            {
                fuelDataModel.RefuelMax = (float)Math.Round(level - (_lastFuelData.FuelPerMinuteMax / 60) * timeRemain, 2);
            }

            return fuelDataModel;
        }

        private FuelDataModel GetLapFuelDataModel(float level, float elapsedTime)
        {
            FuelDataModel fuelDataModel = new FuelDataModel();

            fuelDataModel.Level = level;
            fuelDataModel.MinConsumption = _lastFuelData.MinConsumption;
            fuelDataModel.AvgConsumption = _lastFuelData.AvgConsumption;
            fuelDataModel.MaxConsumption = _lastFuelData.MaxConsumption;

            if (elapsedTime > 5)
            {
                if (fuelDataModel.MinConsumption > _fuelDelta || fuelDataModel.MinConsumption == 0)
                {
                    fuelDataModel.MinConsumption = _fuelDelta;
                }

                if (fuelDataModel.AvgConsumption == 0)
                {
                    fuelDataModel.AvgConsumption = _fuelDelta;
                }
                else
                {
                    fuelDataModel.AvgConsumption = (fuelDataModel.AvgConsumption + _fuelDelta) / 2;
                }

                if (fuelDataModel.MaxConsumption < _fuelDelta || fuelDataModel.MaxConsumption == 0)
                {
                    fuelDataModel.MaxConsumption = _fuelDelta;
                }

                fuelDataModel.LastConsumption = _fuelDelta;

                fuelDataModel.FuelPerMinuteMin = fuelDataModel.MinConsumption / elapsedTime * 60;
                fuelDataModel.FuelPerMinuteAvg = fuelDataModel.AvgConsumption / elapsedTime * 60;
                fuelDataModel.FuelPerMinuteMax = fuelDataModel.MaxConsumption / elapsedTime * 60;
            }

            _fuelDelta = 0;
            return fuelDataModel;
        }
    }
}
