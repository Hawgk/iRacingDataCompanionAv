namespace IRDCav.Models
{
    public class SessionInfoModel
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
                }
            }
        }
    }
}
