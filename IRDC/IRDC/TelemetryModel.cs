using System.ComponentModel;

namespace IRDC
{
    public class TelemetryModel : INotifyPropertyChanged
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
                    OnPropertyChanged(nameof(ThrottlePerc));
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
                    OnPropertyChanged(nameof(ClutchPerc));
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
                    OnPropertyChanged(nameof(BrakePerc));
                }
            }
        }

        #region INotifyPropertyChanged Members
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    }
}
