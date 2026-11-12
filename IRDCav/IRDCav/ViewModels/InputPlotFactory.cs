using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System.Collections.Generic;

namespace IRDCav.ViewModels
{
    /// <summary>Creates the borderless, axis-free throttle/brake trace plot.</summary>
    internal static class InputPlotFactory
    {
        private const double PercentMin = -1;
        private const double PercentMax = 101;
        private const double EdgeMargin = -8;

        public static PlotModel Create(IEnumerable<float> throttle, IEnumerable<float> brake, int sampleCount)
        {
            var model = new PlotModel
            {
                PlotAreaBorderColor = OxyColors.Transparent,
                PlotMargins = new OxyThickness(EdgeMargin),
            };

            model.Series.Add(CreateSeries(throttle, OxyColors.Green));
            model.Series.Add(CreateSeries(brake, OxyColors.Red));
            model.Axes.Add(CreateHiddenAxis(AxisPosition.Bottom, 0, sampleCount - 1));
            model.Axes.Add(CreateHiddenAxis(AxisPosition.Left, PercentMin, PercentMax));
            return model;
        }

        private static LineSeries CreateSeries(IEnumerable<float> values, OxyColor color)
        {
            var series = new LineSeries { Color = color };
            int x = 0;
            foreach (float y in values)
            {
                series.Points.Add(new DataPoint(x++, y));
            }
            return series;
        }

        private static LinearAxis CreateHiddenAxis(AxisPosition position, double minimum, double maximum) => new()
        {
            Position = position,
            IsAxisVisible = false,
            Minimum = minimum,
            Maximum = maximum,
        };
    }
}
