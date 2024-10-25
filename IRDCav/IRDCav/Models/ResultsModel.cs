namespace IRDCav.Models
{
    public class ResultsModel
    {
        private bool _isMe = false;
        private bool _isFastest = false;
        private string _name = string.Empty;
        private string _class = string.Empty;
        private string _carPath = string.Empty;
        private string _license = string.Empty;
        private string _fastestLapTime = string.Empty;
        private string _lastLapTime = string.Empty;

        private int _rating;
        private int _classPosition;
        private int _lapsCompleted;

        private string _classColor = string.Empty;


        public bool IsMe
        {
            get { return _isMe; }
            set { _isMe = value; }
        }
        public bool IsFastest
        {
            get { return _isFastest; }
            set { _isFastest = value; }
        }
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
        public string Class
        {
            get { return _class; }
            set { _class = value; }
        }
        public string CarPath
        {
            get { return _carPath; }
            set { _carPath = value; }
        }
        public string License
        {
            get { return _license; }
            set { _license = value; }
        }
        public string ClassColor
        {
            get { return _classColor; }
            set { _classColor = value; }
        }
        public string FastestLapTime
        {
            get { return _fastestLapTime; }
            set { _fastestLapTime = value; }
        }
        public string LastLapTime
        {
            get { return _lastLapTime; }
            set { _lastLapTime = value; }
        }

        public int Rating
        {
            get { return _rating; }
            set { _rating = value; }
        }
        public int ClassPosition
        {
            get { return _classPosition; }
            set { _classPosition = value; }
        }
        public int LapsCompleted
        {
            get { return _lapsCompleted; }
            set { _lapsCompleted = value; }
        }
    }
}
