using CommunityToolkit.Mvvm.ComponentModel;
using IRDCav.Telemetry;
using OxyPlot;
using System.Collections.Generic;
using System.Linq;

namespace IRDCav.ViewModels
{
    public partial class InputGraphViewModel : ViewModelBase
    {
        private readonly int _sampleCount;

        [ObservableProperty]
        private bool _isVisible;

        [ObservableProperty]
        private PlotModel _model = new();

        public InputGraphViewModel(int sampleCount)
        {
            _sampleCount = sampleCount;
        }

        public void SetSamples(IReadOnlyList<InputSample> samples)
        {
            Model = InputPlotFactory.Create(
                samples.Select(sample => ToPercent(sample.Throttle)),
                samples.Select(sample => ToPercent(sample.Brake)),
                _sampleCount);
        }

        private static float ToPercent(float value) => value * 100;
    }
}
