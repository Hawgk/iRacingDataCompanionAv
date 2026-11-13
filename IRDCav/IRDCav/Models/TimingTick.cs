namespace IRDCav.Models
{
    /// <summary>One race data tick: the session clock now and the time since the previous tick, in seconds.</summary>
    public readonly record struct TimingTick(double SessionTime, double ElapsedTime)
    {
        public double PreviousTime => SessionTime - ElapsedTime;
    }
}
