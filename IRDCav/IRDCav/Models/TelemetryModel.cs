namespace IRDCav.Models
{
    public class TelemetryModel
    {
        private decimal _throttlePerc;
        private decimal _clutchPerc;
        private decimal _brakePerc;

        public decimal ThrottlePerc
        {
            get
            {
                return _throttlePerc;
            }
            set
            {
                if (_throttlePerc != value)
                {
                    _throttlePerc = value;
                }
            }
        }

        public decimal ClutchPerc
        {
            get
            {
                return _clutchPerc;
            }
            set
            {
                if (_clutchPerc != value)
                {
                    _clutchPerc = value;
                }
            }
        }

        public decimal BrakePerc
        {
            get
            {
                return _brakePerc;
            }
            set
            {
                if (_brakePerc != value)
                {
                    _brakePerc = value;
                }
            }
        }
    }
}
