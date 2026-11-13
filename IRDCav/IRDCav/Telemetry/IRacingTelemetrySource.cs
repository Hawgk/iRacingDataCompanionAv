using IRSDKSharper;
using System;

namespace IRDCav.Telemetry
{
    internal sealed class IRacingTelemetrySource : ITelemetrySource
    {
        // The SDK fires at 60 Hz; only every second tick is forwarded.
        private const int SdkUpdateInterval = 2;

        private readonly IRacingSdk _sdk = new();
        private IRacingTelemetryFrame? _frame;

        public event Action? Connected;
        public event Action? Disconnected;
        public event Action? Stopped;
        public event Action<ITelemetryFrame>? FrameReceived;

        public IRacingTelemetrySource()
        {
            _sdk.UpdateInterval = SdkUpdateInterval;
            _sdk.OnConnected += OnConnected;
            _sdk.OnDisconnected += OnDisconnected;
            _sdk.OnStopped += () => Stopped?.Invoke();
            _sdk.OnTelemetryData += OnTelemetryData;
        }

        public void Start() => _sdk.Start();

        public void Stop() => _sdk.Stop();

        private void OnConnected()
        {
            // Variable offsets can differ between sim instances, so they are resolved per connection.
            _frame = null;
            Connected?.Invoke();
        }

        private void OnDisconnected()
        {
            _frame = null;
            Disconnected?.Invoke();
        }

        private void OnTelemetryData()
        {
            _frame ??= new IRacingTelemetryFrame(_sdk.Data);
            FrameReceived?.Invoke(_frame);
        }
    }
}
