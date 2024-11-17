using CircularBuffer;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;

namespace IRDCav.ViewModels
{
    public class InputGraphViewModel : ViewModelBase
    {
        private bool _isVisible = false;
        private PlotModel _model = new PlotModel();
        private CircularBuffer<float> _throttlePoints = new CircularBuffer<float>(100);
        private CircularBuffer<float> _brakePoints = new CircularBuffer<float>(100);
        private CircularBuffer<float> _clutchPoints = new CircularBuffer<float>(100);

        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                _isVisible = value;
                OnPropertyChanged(nameof(IsVisible));
            }
        }

        public PlotModel Model
        {
            get => _model;
            set
            {
                _model = value;
                OnPropertyChanged(nameof(Model));
            }
        }

        public void Clear()
        {
            _throttlePoints.Clear();
            _brakePoints.Clear();
            _clutchPoints.Clear();
            Model.Series.Clear();
        }

        public void AddPoints(float throttle, float brake, float clutch)
        {
            int x = 0;

            PlotModel plotModel = new PlotModel();
            LineSeries lsThrottle = new LineSeries();
            LineSeries lsBrake = new LineSeries();
            LineSeries lsClutch = new LineSeries();

            lsThrottle.Color = OxyColors.Green;
            lsBrake.Color = OxyColors.Red;
            lsClutch.Color = OxyColors.Blue;

            _throttlePoints.PushBack((float)Math.Round(throttle * 100, 0));
            _brakePoints.PushBack((float)Math.Round(brake * 100, 0));
            _clutchPoints.PushBack((float)Math.Round(clutch * 100, 0));

            foreach (float y in _throttlePoints)
            {
                lsThrottle.Points.Add(new DataPoint(x, y));
                x++;
            }
            plotModel.Series.Add(lsThrottle);

            x = 0;
            foreach (float y in _brakePoints)
            {
                lsBrake.Points.Add(new DataPoint(x, y));
                x++;
            }
            plotModel.Series.Add(lsBrake);

            /*
            x = 0;
            foreach (float y in _clutchPoints)
            {
                lsClutch.Points.Add(new DataPoint(x, y));
                x++;
            }
            plotModel.Series.Add(lsClutch);
            */

            plotModel.Axes.Add(new LinearAxis()
            {
                Position = AxisPosition.Bottom,
                IsAxisVisible = false,
                Minimum = 0,
                Maximum = 99,
            });

            plotModel.Axes.Add(new LinearAxis()
            {
                Position = AxisPosition.Left,
                IsAxisVisible = false,
                Minimum = -1,
                Maximum = 101,
            });

            plotModel.PlotAreaBorderColor = OxyColors.Transparent;
            plotModel.PlotMargins = new OxyThickness(-8, -8, -8, -8);


            Model = plotModel;
        }
    }
}
