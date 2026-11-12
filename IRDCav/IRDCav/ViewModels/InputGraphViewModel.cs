using CircularBuffer;
using CommunityToolkit.Mvvm.ComponentModel;
using IRDCav.Telemetry;
using OxyPlot;
using System;

namespace IRDCav.ViewModels
{
    public partial class InputGraphViewModel : ViewModelBase
    {
        private const int SampleCount = 100;

        private readonly CircularBuffer<float> _throttle = new(SampleCount);
        private readonly CircularBuffer<float> _brake = new(SampleCount);

        [ObservableProperty]
        private bool _isVisible;

        [ObservableProperty]
        private PlotModel _model = new();

        public void AddSample(InputSample sample)
        {
            _throttle.PushBack(ToPercent(sample.Throttle));
            _brake.PushBack(ToPercent(sample.Brake));
            Model = InputPlotFactory.Create(_throttle, _brake, SampleCount);
        }

        private static float ToPercent(float value) => (float)Math.Round(value * 100, 0);
    }
}
