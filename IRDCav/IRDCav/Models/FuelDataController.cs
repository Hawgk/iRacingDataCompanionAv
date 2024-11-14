using System;
using System.Diagnostics;

namespace IRDCav.Models
{
    public class FuelDataController
    {
        private FuelDataModel _lastFuelData = new FuelDataModel();
        private Stopwatch _timer = new Stopwatch();

        private float _fuelDelta;
        private float _minConsumption;
        private float _avgConsumption;
        private float _maxConsumption;
        private float _lastConsumption;
        private float _fpmMin = 0;
        private float _fpmAvg = 0;
        private float _fpmMax = 0;

        public SessionInfoModel SessionInfo { get; set; } = new SessionInfoModel();

        private void CalculateConsumption()
        {
            float elapsedTime = (float)_timer.Elapsed.TotalSeconds;

            if (elapsedTime > 5)
            {
                if (_lastFuelData.MinConsumption > _fuelDelta || _lastFuelData.MinConsumption == 0)
                {
                    _minConsumption = _fuelDelta;
                }

                if (_lastFuelData.MaxConsumption < _fuelDelta || _lastFuelData.MaxConsumption == 0)
                {
                    _maxConsumption = _fuelDelta;
                }

                if (_lastFuelData.AvgConsumption == 0)
                {
                    _avgConsumption = _fuelDelta;
                }
                else
                {
                    _avgConsumption = (_lastFuelData.AvgConsumption + _fuelDelta) / 2;
                }

                _lastConsumption = _fuelDelta;

                _fpmMin = _minConsumption / elapsedTime * 60;
                _fpmAvg = _avgConsumption / elapsedTime * 60;
                _fpmMax = _maxConsumption / elapsedTime * 60;
            }

            _fuelDelta = 0;
        }

        public void StartTimer(float level)
        {
            _lastFuelData.Level = level;
            _fuelDelta = 0;
            _timer.Reset();
        }

        public void StopTimer()
        {
            _timer.Stop();
            _timer.Reset();
        }

        public FuelDataModel GetFuelDataModel(float level)
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

            _lastFuelData = fuelDataModel;

            return fuelDataModel;
        }

        public FuelDataModel GetLapFuelDataModel(float level)
        {
            FuelDataModel fuelDataModel = new FuelDataModel();

            _timer.Stop();

            CalculateConsumption();

            fuelDataModel.Level = level;
            fuelDataModel.MinConsumption = _minConsumption;
            fuelDataModel.AvgConsumption = _avgConsumption;
            fuelDataModel.MaxConsumption = _maxConsumption;
            fuelDataModel.LastConsumption = _lastConsumption;
            fuelDataModel.FuelPerMinuteMin = _fpmMin;
            fuelDataModel.FuelPerMinuteAvg = _fpmAvg;
            fuelDataModel.FuelPerMinuteMax = _fpmMax;

            if (_lastFuelData.FuelPerMinuteMin > 0)
            {
                fuelDataModel.RefuelMin = (float)Math.Round(level - (_lastFuelData.FuelPerMinuteMin / 60) * SessionInfo.TimeRemain, 2);
            }
            if (_lastFuelData.FuelPerMinuteAvg > 0)
            {
                fuelDataModel.RefuelAvg = (float)Math.Round(level - (_lastFuelData.FuelPerMinuteAvg / 60) * SessionInfo.TimeRemain, 2);
            }
            if (_lastFuelData.FuelPerMinuteMax > 0)
            {
                fuelDataModel.RefuelMax = (float)Math.Round(level - (_lastFuelData.FuelPerMinuteMax / 60) * SessionInfo.TimeRemain, 2);
            }

            _timer.Reset();
            _timer.Start();

            _lastFuelData = fuelDataModel;

            return fuelDataModel;
        }

        public void Clear()
        {
            _lastFuelData = new FuelDataModel();
            _fuelDelta = 0;
        }
    }
}
