using CommunityToolkit.Mvvm.ComponentModel;
using IRDCav.Telemetry;
using System;
using System.Collections.Generic;

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

        public void SetSamples(IReadOnlyList<InputSample> samples) => Samples = samples;
    }
}
