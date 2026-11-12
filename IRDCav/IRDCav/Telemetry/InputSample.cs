namespace IRDCav.Telemetry
{
    /// <summary>Pedal positions in the range 0..1.</summary>
    public readonly record struct InputSample(float Throttle, float Brake);
}
