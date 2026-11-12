using IRDCav.Telemetry;

namespace IRDCav.UiTesting
{
    /// <summary>
    /// Repeating corner: full throttle on the straight, a hard stop with trail braking,
    /// a short coast and a throttle ramp out of the corner.
    /// </summary>
    internal static class InputPattern
    {
        private const double CornerSeconds = 6;
        private const double BrakingStart = 0.55;
        private const double BrakePeak = 0.60;
        private const double BrakingEnd = 0.75;
        private const double ThrottleStart = 0.80;

        public static InputSample At(double seconds)
        {
            double phase = seconds % CornerSeconds / CornerSeconds;
            return new InputSample(Throttle(phase), Brake(phase));
        }

        private static float Throttle(double phase) => phase switch
        {
            < BrakingStart => 1f,
            < ThrottleStart => 0f,
            _ => Ramp(phase, ThrottleStart, 1),
        };

        private static float Brake(double phase) => phase switch
        {
            < BrakingStart => 0f,
            < BrakePeak => Ramp(phase, BrakingStart, BrakePeak),
            < BrakingEnd => 1 - Ramp(phase, BrakePeak, BrakingEnd),
            _ => 0f,
        };

        private static float Ramp(double phase, double start, double end) => (float)((phase - start) / (end - start));
    }
}
