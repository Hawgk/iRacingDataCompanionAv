using IRDCav.Telemetry;

namespace IRDCav.Presentation
{
    /// <summary>Accepts pedal input samples. Must never block for long; callable from any thread.</summary>
    public interface IInputSink
    {
        void Push(InputSample sample);
        void Clear();
    }
}
