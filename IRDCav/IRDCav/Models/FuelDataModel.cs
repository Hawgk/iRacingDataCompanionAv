namespace IRDCav.Models
{
    public class FuelDataModel
    {
        private float _fuelDelta = 0;

        public float Level { get; set; }
        public float MinConsumption { get; set; }
        public float AvgConsumption { get; set; }
        public float MaxConsumption { get; set; }

        public void SetLevel(float level)
        {
            if (Level != 0)
            {
                _fuelDelta += Level - level;
            }
            Level = level;
        }

        public void Update()
        {
            if (MinConsumption > _fuelDelta || MinConsumption == 0)
            {
                MinConsumption = _fuelDelta;
            }

            if (MaxConsumption < _fuelDelta || MaxConsumption == 0)
            {
                MaxConsumption = _fuelDelta;
            }

            if (AvgConsumption == 0)
            {
                AvgConsumption = _fuelDelta;
            }
            else
            {
                AvgConsumption = (AvgConsumption + _fuelDelta) / 2;
            }

            _fuelDelta = 0;
        }

        public void Clear()
        {
            _fuelDelta = 0;
            Level = 0;
            MinConsumption = 0;
            AvgConsumption = 0;
            MaxConsumption = 0;
        }
    }
}
