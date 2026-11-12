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
    /// Lightweight throttle/brake trace with the current pedal positions as bars on the right.
    /// Draws directly instead of building a chart model, which keeps the per-frame UI cost minimal.
    /// </summary>
    public sealed class InputTraceControl : Control
    {
        public static readonly StyledProperty<IReadOnlyList<InputSample>> SamplesProperty =
            AvaloniaProperty.Register<InputTraceControl, IReadOnlyList<InputSample>>(nameof(Samples), Array.Empty<InputSample>());

        public static readonly StyledProperty<int> CapacityProperty =
            AvaloniaProperty.Register<InputTraceControl, int>(nameof(Capacity), 100);

        public static readonly StyledProperty<IBrush> ThrottleBrushProperty =
            AvaloniaProperty.Register<InputTraceControl, IBrush>(nameof(ThrottleBrush), Brushes.LightGreen);

        public static readonly StyledProperty<IBrush> BrakeBrushProperty =
            AvaloniaProperty.Register<InputTraceControl, IBrush>(nameof(BrakeBrush), Brushes.LightCoral);

        public static readonly StyledProperty<IBrush> TrackBrushProperty =
            AvaloniaProperty.Register<InputTraceControl, IBrush>(nameof(TrackBrush), Brushes.DimGray);

        private const double StrokeThickness = 2;
        private const double BarWidth = 8;
        private const double BarSpacing = 6;
        private const double TraceToBarsSpacing = 10;
        private const double BarsWidth = 2 * BarWidth + BarSpacing + TraceToBarsSpacing;
        private static readonly double[] GridLines = { 0.25, 0.5, 0.75 };

        private IPen _throttlePen = new ImmutablePen(Brushes.LightGreen, StrokeThickness);
        private IPen _brakePen = new ImmutablePen(Brushes.LightCoral, StrokeThickness);
        private IPen _gridPen = new ImmutablePen(Brushes.DimGray, 1);

        static InputTraceControl()
        {
            AffectsRender<InputTraceControl>(SamplesProperty, CapacityProperty, ThrottleBrushProperty, BrakeBrushProperty, TrackBrushProperty);
        }

        public IReadOnlyList<InputSample> Samples
        {
            get => GetValue(SamplesProperty);
            set => SetValue(SamplesProperty, value);
        }

        /// <summary>Number of samples that span the full trace width.</summary>
        public int Capacity
        {
            get => GetValue(CapacityProperty);
            set => SetValue(CapacityProperty, value);
        }

        public IBrush ThrottleBrush
        {
            get => GetValue(ThrottleBrushProperty);
            set => SetValue(ThrottleBrushProperty, value);
        }

        public IBrush BrakeBrush
        {
            get => GetValue(BrakeBrushProperty);
            set => SetValue(BrakeBrushProperty, value);
        }

        public IBrush TrackBrush
        {
            get => GetValue(TrackBrushProperty);
            set => SetValue(TrackBrushProperty, value);
        }

        private Rect TraceArea => new(0, 0, Math.Max(0, Bounds.Width - BarsWidth), Bounds.Height);

        public override void Render(DrawingContext context)
        {
            DrawGrid(context);
            if (Samples.Count >= 2 && Capacity >= 2)
            {
                DrawTrace(context, sample => sample.Brake, _brakePen);
                DrawTrace(context, sample => sample.Throttle, _throttlePen);
            }
            InputSample current = Samples.Count > 0 ? Samples[^1] : default;
            DrawBar(context, 0, current.Brake, BrakeBrush);
            DrawBar(context, 1, current.Throttle, ThrottleBrush);
        }

        // Pens are created once per brush change instead of on every frame.
        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            if (change.Property == ThrottleBrushProperty)
            {
                _throttlePen = new ImmutablePen(ThrottleBrush.ToImmutable(), StrokeThickness);
            }
            else if (change.Property == BrakeBrushProperty)
            {
                _brakePen = new ImmutablePen(BrakeBrush.ToImmutable(), StrokeThickness);
            }
            else if (change.Property == TrackBrushProperty)
            {
                _gridPen = new ImmutablePen(TrackBrush.ToImmutable(), 1);
            }
        }

        private void DrawGrid(DrawingContext context)
        {
            foreach (double value in GridLines)
            {
                double y = Math.Round(TraceArea.Height * (1 - value)) + 0.5;
                context.DrawLine(_gridPen, new Point(0, y), new Point(TraceArea.Width, y));
            }
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

        private void DrawBar(DrawingContext context, int index, float value, IBrush brush)
        {
            double x = TraceArea.Width + TraceToBarsSpacing + index * (BarWidth + BarSpacing);
            var track = new Rect(x, 0, BarWidth, Bounds.Height);
            double fillHeight = Bounds.Height * Math.Clamp(value, 0, 1);
            var fill = new Rect(x, Bounds.Height - fillHeight, BarWidth, fillHeight);

            context.DrawRectangle(TrackBrush, null, track, BarWidth / 2, BarWidth / 2);
            context.DrawRectangle(brush, null, fill, BarWidth / 2, BarWidth / 2);
        }

        // Half a stroke of headroom keeps full and zero pedal lines from being clipped.
        private Point PointAt(int index, float value)
        {
            double inset = StrokeThickness / 2;
            double x = index * TraceArea.Width / (Capacity - 1);
            double y = inset + (TraceArea.Height - 2 * inset) * (1 - value);
            return new Point(x, y);
        }
    }
}
