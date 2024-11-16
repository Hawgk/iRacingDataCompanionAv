namespace IRDCav.Models
{
    public class TelemetryModel
    {
        private decimal _throttle;
        private decimal _clutch;
        private decimal _brake;

        public decimal Throttle
        {
            get
            {
                return _throttle;
            }
            set
            {
                if (_throttle != value)
                {
                    _throttle = value;
                }
            }
        }

        public decimal Clutch
        {
            get
            {
                return _clutch;
            }
            set
            {
                if (_clutch != value)
                {
                    _clutch = value;
                }
            }
        }

        public decimal Brake
        {
            get
            {
                return _brake;
            }
            set
            {
                if (_brake != value)
                {
                    _brake = value;
                }
            }
        }
    }
}
