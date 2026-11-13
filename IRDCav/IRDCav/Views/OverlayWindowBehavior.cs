using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Platform;
using IRDCav.Layout;
using System;
using System.Linq;

namespace IRDCav.Views
{
    /// <summary>Services shared by all overlay windows for placement.</summary>
    internal sealed record OverlayLayoutContext(IOverlayLayoutStore Store, ScreenWatcher Screens);

    /// <summary>
    /// Shared overlay behavior: keeps the window topmost, scales its content to the screen size and
    /// places it according to its <see cref="OverlayPlacement"/>. F6 toggles move mode, in which the
    /// window can be dragged (also to other monitors) and scaled with the mouse wheel; leaving move
    /// mode saves the new placement.
    /// </summary>
    internal sealed class OverlayWindowBehavior
    {
        private const Key MoveModeKey = Key.F6;
        private const double ScaleStep = 0.05;

        private static readonly IBrush MovingBackground = new SolidColorBrush(Color.FromArgb(0x40, 0xD3, 0xD0, 0xCB));
        // The panel inside draws the configured background; a second layer here would make it more opaque.
        private static readonly IBrush FixedBackground = Brushes.Transparent;

        private readonly Window _window;
        private readonly LayoutTransformControl _scaleHost;
        private readonly OverlayDefinition _overlay;
        private readonly IOverlayLayoutStore _store;
        private OverlayPlacement _placement;
        private double _appliedContentScale;
        private bool _isMoveMode;

        private OverlayWindowBehavior(Window window, OverlayDefinition overlay, IOverlayLayoutStore store)
        {
            _window = window;
            _overlay = overlay;
            _store = store;
            _placement = store.Load(overlay);
            _scaleHost = window.Content as LayoutTransformControl
                ?? throw new InvalidOperationException(
                    $"{window.GetType().Name} must wrap its content in a LayoutTransformControl to support overlay scaling.");
        }

        /// <summary>Call after InitializeComponent so the XAML content and background exist.</summary>
        public static void Attach(Window window, OverlayDefinition overlay, OverlayLayoutContext context)
        {
            var behavior = new OverlayWindowBehavior(window, overlay, context.Store);
            window.Background = FixedBackground;
            // Overlays reappear whenever the player gets into the car; they must not take the focus from the sim.
            window.ShowActivated = false;
            window.Opened += (_, _) => behavior.ApplyPlacement();
            window.ScalingChanged += (_, _) => behavior.ApplyPlacement();
            context.Screens.Changed += behavior.ApplyPlacement;
            window.PropertyChanged += (_, args) => behavior.OnPropertyChanged(args);
            window.PositionChanged += (_, _) => behavior.OnPositionChanged();
            window.Deactivated += (_, _) => window.Topmost = true;
            window.KeyDown += (_, args) => behavior.OnKeyDown(args);
            window.PointerPressed += (_, args) => behavior.OnPointerPressed(args);
            window.PointerWheelChanged += (_, args) => behavior.OnPointerWheelChanged(args);
        }

        private void ApplyPlacement()
        {
            if (TryResolveScreen(out Screen? screen))
            {
                ApplyContentScale(screen);
                MoveTo(screen);
            }
        }

        private void ApplyContentScale(Screen screen)
        {
            double scale = PlacementCalculator.ContentScale(screen.Bounds, screen.Scaling, _placement.Scale);
            if (scale != _appliedContentScale)
            {
                _appliedContentScale = scale;
                _scaleHost.LayoutTransform = new ScaleTransform(scale, scale);
            }
        }

        private void MoveTo(Screen screen)
        {
            PixelPoint position = PlacementCalculator.ToPosition(_placement, screen.Bounds, WindowSize());
            if (position != _window.Position)
            {
                _window.Position = position;
            }
        }

        /// <summary>Keeps the anchored edge in place when the content changes the window size.</summary>
        private void OnPropertyChanged(AvaloniaPropertyChangedEventArgs args)
        {
            if (args.Property == TopLevel.ClientSizeProperty && TryResolveScreen(out Screen? screen))
            {
                MoveTo(screen);
            }
        }

        private void OnPositionChanged()
        {
            if (_isMoveMode)
            {
                CapturePlacement();
            }
        }

        private void OnKeyDown(KeyEventArgs args)
        {
            if (args.Key != MoveModeKey)
            {
                return;
            }

            _isMoveMode = !_isMoveMode;
            _window.Background = _isMoveMode ? MovingBackground : FixedBackground;
            if (!_isMoveMode)
            {
                CapturePlacement();
                _store.Save(_overlay.Id, _placement);
            }
        }

        private void OnPointerPressed(PointerPressedEventArgs args)
        {
            if (_isMoveMode && args.GetCurrentPoint(_window).Properties.IsLeftButtonPressed)
            {
                _window.BeginMoveDrag(args);
            }
        }

        private void OnPointerWheelChanged(PointerWheelEventArgs args)
        {
            if (!_isMoveMode || args.Delta.Y == 0)
            {
                return;
            }

            double scale = PlacementCalculator.ClampScale(_placement.Scale + Math.Sign(args.Delta.Y) * ScaleStep);
            _placement = _placement with { Scale = scale };
            ApplyPlacement();
        }

        private void CapturePlacement()
        {
            Screen? screen = _window.Screens.ScreenFromWindow(_window);
            if (screen != null || TryResolveScreen(out screen))
            {
                var bounds = new PixelRect(_window.Position, WindowSize());
                _placement = PlacementCalculator.FromPosition(bounds, screen.Bounds, _placement.Scale);
            }
        }

        /// <summary>The monitor the placement refers to, falling back to the primary monitor.</summary>
        private bool TryResolveScreen([System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out Screen? screen)
        {
            ScreenArea? saved = _placement.Screen;
            screen = _window.Screens.All.FirstOrDefault(candidate => saved == AreaOf(candidate))
                ?? _window.Screens.Primary
                ?? _window.Screens.All.FirstOrDefault();
            return screen != null;
        }

        private PixelSize WindowSize() => PixelSize.FromSize(_window.ClientSize, _window.DesktopScaling);

        private static ScreenArea AreaOf(Screen screen) =>
            new(screen.Bounds.X, screen.Bounds.Y, screen.Bounds.Width, screen.Bounds.Height);
    }
}
