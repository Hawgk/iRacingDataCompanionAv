using CommunityToolkit.Mvvm.ComponentModel;
using IRDCav.Telemetry;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IRDCav.ViewModels
{
    public partial class InputGraphViewModel : ViewModelBase
    {
        [ObservableProperty]
        private bool _isVisible;

        [ObservableProperty]
        private IReadOnlyList<InputSample> _samples = Array.Empty<InputSample>();

        public InputGraphViewModel(int sampleCount)
        {
            SampleCount = sampleCount;
        }

        /// <summary>Number of samples that fill the whole trace width.</summary>
        public int SampleCount { get; }

        /// <summary>Ignores samples that would draw the same trace, e.g. while the car stands with no pedal pressed.</summary>
        public void SetSamples(IReadOnlyList<InputSample> samples)
        {
            if (!samples.SequenceEqual(Samples))
            {
                Samples = samples;
            }
        }
    }
}
