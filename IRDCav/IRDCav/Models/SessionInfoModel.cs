namespace IRDCav.Models
{
    public class SessionInfoModel
    {
        private string _trackName = string.Empty;
        private string _airTemp = string.Empty;
        private string _surfaceTemp = string.Empty;
        private string _precipitation = string.Empty;
        private string _humidity = string.Empty;
        private string _incidentCount = string.Empty;
        private string _lapsString = string.Empty;
        private string _timeString = string.Empty;
        private string _driverClassName = string.Empty;
        private string _sessionType = string.Empty;
        
        private int _classCount;
        private int _lapsRemain;
        private int _lapsTotal;
        private int _sof;
        private int _driverCount;

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
        public string Precipitation
        {
            get { return _precipitation; }
            set { _precipitation = value; }
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
        public string DriverClassName
        {
            get { return _driverClassName; }
            set { _driverClassName = value; }
        }
        public string IncidentCount
        {
            get { return _incidentCount; }
            set { _incidentCount = value; }
        }
        public string SessionType
        {
            get { return _sessionType; }
            set { _sessionType = value; }
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
        public int SOF
        {
            get { return _sof; }
            set { _sof = value; }
        }
        public int DriverCount
        {
            get { return _driverCount; }
            set { _driverCount = value; }
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
