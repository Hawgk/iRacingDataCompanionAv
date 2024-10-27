namespace IRDCav.Models
{
    public class SessionInfoModel
    {
        private string _trackName = string.Empty;
        private string _airTemp = string.Empty;
        private string _surfaceTemp = string.Empty;
        private string _fogLevel = string.Empty;
        private string _humidity = string.Empty;

        public string TrackName
        {
            get { return _trackName; }
            set { _trackName = value; }
        }
        public string AirTemp
        {
            get { return _airTemp; }
            set { _airTemp = value; }
        }
        public string SurfaceTemp
        {
            get { return _surfaceTemp; }
            set { _surfaceTemp = value; }
        }
        public string FogLevel
        {
            get { return _fogLevel; }
            set { _fogLevel = value; }
        }
        public string Humidity
        {
            get { return _humidity; }
            set { _humidity = value; }
        }
    }
}
