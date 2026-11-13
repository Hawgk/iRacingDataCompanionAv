using IRDCav.Telemetry;
using System;
using System.Threading;

namespace IRDCav.Presentation
{
    /// <summary>
    /// Fixed-size history of input samples, written by the telemetry thread and read by the UI.
    /// The lock only guards copying a few hundred bytes.
    /// </summary>
    public sealed class InputTraceBuffer : IInputSink
    {
        private readonly object _lock = new();
        private readonly InputSample[] _samples;
        private int _next;
        private int _count;
        private long _version;

        public InputTraceBuffer(int capacity)
        {
            _samples = new InputSample[capacity];
        }

        public int Capacity => _samples.Length;

        /// <summary>Changes whenever the content changes; lets readers skip unchanged frames.</summary>
        public long Version => Interlocked.Read(ref _version);

        public void Push(InputSample sample)
        {
            lock (_lock)
            {
                _samples[_next] = sample;
                _next = (_next + 1) % _samples.Length;
                _count = Math.Min(_count + 1, _samples.Length);
                Interlocked.Increment(ref _version);
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                _next = 0;
                _count = 0;
                Interlocked.Increment(ref _version);
            }
        }

        /// <summary>Copies the samples oldest first into a new array.</summary>
        public InputSample[] Snapshot()
        {
            lock (_lock)
            {
                var snapshot = new InputSample[_count];
                int oldest = (_next - _count + _samples.Length) % _samples.Length;
                for (int i = 0; i < _count; i++)
                {
                    snapshot[i] = _samples[(oldest + i) % _samples.Length];
                }
                return snapshot;
            }
        }
    }
}
