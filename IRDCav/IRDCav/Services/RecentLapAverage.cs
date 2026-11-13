using System.Collections.Generic;
using System.Linq;

namespace IRDCav.Services
{
    /// <summary>
    /// The average of the player's last <see cref="LapCount"/> lap times as iRacing reports them, out
    /// laps and slow laps included: a plain pace figure for the race. Until there are enough laps, the
    /// ones there are count. Runs on the telemetry thread.
    /// </summary>
    internal sealed class RecentLapAverage
    {
        public const int LapCount = 5;

        private readonly Queue<float> _lapTimes = new();
        private float _lastReported = float.NaN;

        /// <summary>Seconds; 0 before the first lap.</summary>
        public float Average => _lapTimes.Count > 0 ? _lapTimes.Average() : 0;

        public void Reset()
        {
            _lapTimes.Clear();
            _lastReported = float.NaN;
        }

        /// <summary>
        /// iRacing's last lap time; every new value is a finished lap. The first value after a reset may
        /// be from before it, so it is only noted. Two laps of exactly the same time count as one.
        /// </summary>
        public void Observe(float lastLapTime)
        {
            bool isNewLap = !float.IsNaN(_lastReported) && lastLapTime > 0 && lastLapTime != _lastReported;
            _lastReported = lastLapTime;
            if (!isNewLap)
            {
                return;
            }
            _lapTimes.Enqueue(lastLapTime);
            if (_lapTimes.Count > LapCount)
            {
                _lapTimes.Dequeue();
            }
        }
    }
}
