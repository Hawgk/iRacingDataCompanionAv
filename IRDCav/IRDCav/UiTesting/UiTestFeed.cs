using IRDCav.Presentation;
using IRDCav.Services;
using System.Diagnostics;
using System.Threading;

namespace IRDCav.UiTesting
{
    /// <summary>
    /// Stands in for iRacing: drives the test scenario on its own background thread at the
    /// sim's frame rate and publishes through the same store and buffer as live telemetry.
    /// </summary>
    internal sealed class UiTestFeed : IOverlayDataFeed
    {
        private const int FramesPerSecond = 30;
        private const int StateFrameInterval = 4;

        private readonly UiTestScenario _scenario;
        private readonly IOverlayStatePublisher _publisher;
        private readonly IInputSink _inputs;
        private readonly CancellationTokenSource _cancellation = new();
        private readonly Thread _thread;
        private int _frameCounter;

        public UiTestFeed(UiTestScenario scenario, IOverlayStatePublisher publisher, IInputSink inputs)
        {
            _scenario = scenario;
            _publisher = publisher;
            _inputs = inputs;
            _thread = new Thread(Run) { IsBackground = true, Name = "UI testing feed" };
        }

        public void Start() => _thread.Start();

        public void Stop()
        {
            _cancellation.Cancel();
            if (_thread.IsAlive)
            {
                _thread.Join();
            }
        }

        private void Run()
        {
            var clock = Stopwatch.StartNew();
            double lastSeconds = 0;
            while (!_cancellation.Token.WaitHandle.WaitOne(1000 / FramesPerSecond))
            {
                double seconds = clock.Elapsed.TotalSeconds;
                ProcessFrame(seconds - lastSeconds);
                lastSeconds = seconds;
            }
        }

        private void ProcessFrame(double elapsedSeconds)
        {
            _scenario.Advance(elapsedSeconds);
            _inputs.Push(_scenario.CurrentInput());

            if (_frameCounter == 0)
            {
                _publisher.Publish(_scenario.CurrentState());
            }
            _frameCounter = (_frameCounter + 1) % StateFrameInterval;
        }
    }
}
