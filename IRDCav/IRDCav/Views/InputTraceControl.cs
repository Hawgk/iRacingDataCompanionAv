using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using IRDCav.Telemetry;
using System;
using System.Collections.Generic;

namespace IRDCav.Views
{
    /// <summary>
    /// Lightweight throttle/brake trace. Draws two polylines directly instead of
    /// building a chart model, which keeps the per-frame UI cost minimal.
    /// </summary>
    public sealed class InputTraceControl : Control
    {
        public static readonly StyledProperty<IReadOnlyList<InputSample>> SamplesProperty =
            AvaloniaProperty.Register<InputTraceControl, IReadOnlyList<InputSample>>(nameof(Samples), Array.Empty<InputSample>());

        public static readonly StyledProperty<int> CapacityProperty =
            AvaloniaProperty.Register<InputTraceControl, int>(nameof(Capacity), 100);

        private const double StrokeThickness = 2;
        // Small headroom so full and zero pedal lines are not clipped by the edges.
        private const double ValueMin = -0.01;
        private const double ValueMax = 1.01;

        private static readonly IPen ThrottlePen = new ImmutablePen(new ImmutableSolidColorBrush(Colors.Green), StrokeThickness);
        private static readonly IPen BrakePen = new ImmutablePen(new ImmutableSolidColorBrush(Colors.Red), StrokeThickness);

        static InputTraceControl()
        {
            AffectsRender<InputTraceControl>(SamplesProperty, CapacityProperty);
        }

        public IReadOnlyList<InputSample> Samples
        {
            get => GetValue(SamplesProperty);
            set => SetValue(SamplesProperty, value);
        }

        /// <summary>Number of samples that span the full width.</summary>
        public int Capacity
        {
            get => GetValue(CapacityProperty);
            set => SetValue(CapacityProperty, value);
        }

        public override void Render(DrawingContext context)
        {
            if (Samples.Count < 2 || Capacity < 2)
            {
                return;
            }
            DrawTrace(context, sample => sample.Throttle, ThrottlePen);
            DrawTrace(context, sample => sample.Brake, BrakePen);
        }

        private void DrawTrace(DrawingContext context, Func<InputSample, float> value, IPen pen)
        {
            var geometry = new StreamGeometry();
            using (StreamGeometryContext figure = geometry.Open())
            {
                figure.BeginFigure(PointAt(0, value(Samples[0])), isFilled: false);
                for (int i = 1; i < Samples.Count; i++)
                {
                    figure.LineTo(PointAt(i, value(Samples[i])));
                }
                figure.EndFigure(isClosed: false);
            }
            context.DrawGeometry(null, pen, geometry);
        }

        private Point PointAt(int index, float value)
        {
            double x = index * Bounds.Width / (Capacity - 1);
            double y = Bounds.Height * (ValueMax - value) / (ValueMax - ValueMin);
            return new Point(x, y);
        }
    }
}
