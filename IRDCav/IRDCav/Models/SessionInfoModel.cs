namespace IRDCav.Models
{
    public class SessionInfoModel
    {
        private string _trackName = string.Empty;
        private string _airTemp = string.Empty;
        private string _surfaceTemp = string.Empty;
        private string _fogLevel = string.Empty;
        private string _humidity = string.Empty;
        private string _incidentCount = string.Empty;
        private string _lapsString = string.Empty;
        private string _timeString = string.Empty;
        private int _classCount;
        private int _lapsRemain;
        private int _lapsTotal;
        private double _timeRemain;
        private double _timeTotal;

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
        public string LapsString
        {
            get { return _lapsString; }
            set { _lapsString = value; }
        }
        public string TimeString
        {
            get { return _timeString; }
            set { _timeString = value; }
        }
        public int ClassCount
        {
            get { return _classCount; }
            set { _classCount = value; }
        }
        public int LapsRemain
        {
            get { return _lapsRemain; }
            set { _lapsRemain = value; }
        }
        public int LapsTotal
        {
            get { return _lapsTotal; }
            set { _lapsTotal = value; }
        }
        public double TimeRemain
        {
            get { return _timeRemain; }
            set { _timeRemain = value; }
        }
        public double TimeTotal
        {
            get { return _timeTotal; }
            set { _timeTotal = value; }
        }
    }
}
