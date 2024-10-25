using System.ComponentModel;

namespace IRDC
{
    public class SessionInfoModel : INotifyPropertyChanged
    {
        private string _trackName = string.Empty;

        public string TrackName
        {
            get
            {
                return _trackName;
            }
            set
            {
                if (_trackName != value)
                {
                    _trackName = value;
                    OnPropertyChanged(nameof(TrackName));
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
