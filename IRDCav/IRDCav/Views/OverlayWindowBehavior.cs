using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using System;

namespace IRDCav.Views
{
    /// <summary>
    /// Shared overlay behavior: stays topmost, starts at a given position, stays on screen,
    /// and can be dragged while move mode is toggled with F6.
    /// </summary>
    internal sealed class OverlayWindowBehavior
    {
        // TODO: Read the actual screen size instead of assuming a 2560x1440 primary monitor.
        public const int ScreenWidth = 2560;
        public const int ScreenHeight = 1440;

        private const Key MoveModeKey = Key.F6;

        private static readonly IBrush MovingBackground = new SolidColorBrush(Color.FromArgb(0x40, 0xD3, 0xD0, 0xCB));
        private static readonly IBrush FixedBackground = new SolidColorBrush(Color.FromArgb(0xA0, 0x1E, 0x20, 0x19));

        private readonly Window _window;
        private readonly Func<Window, PixelPoint> _initialPosition;
        private bool _isMoveMode;

        private OverlayWindowBehavior(Window window, Func<Window, PixelPoint> initialPosition)
        {
            _window = window;
            _initialPosition = initialPosition;
        }

        /// <summary>Call after InitializeComponent so the XAML background is overridden.</summary>
        public static void Attach(Window window, Func<Window, PixelPoint> initialPosition)
        {
            var behavior = new OverlayWindowBehavior(window, initialPosition);
            window.Background = FixedBackground;
            window.Loaded += (_, _) => behavior.MoveToInitialPosition();
            window.LayoutUpdated += (_, _) => behavior.KeepOnScreen();
            window.Deactivated += (_, _) => window.Topmost = true;
            window.KeyDown += (_, args) => behavior.OnKeyDown(args);
            window.PointerPressed += (_, args) => behavior.OnPointerPressed(args);
        }

        private void MoveToInitialPosition() => _window.Position = _initialPosition(_window);

        private void KeepOnScreen()
        {
            int maxX = (int)(ScreenWidth - _window.Width);
            int maxY = (int)(ScreenHeight - _window.Height);
            var position = new PixelPoint(
                ClampToScreen(_window.Position.X, maxX),
                ClampToScreen(_window.Position.Y, maxY));
            if (position != _window.Position)
            {
                _window.Position = position;
            }
        }

        private static int ClampToScreen(int coordinate, int max) =>
            coordinate < 0 ? 0 : Math.Min(coordinate, max);

        private void OnKeyDown(KeyEventArgs args)
        {
            if (args.Key == MoveModeKey)
            {
                _isMoveMode = !_isMoveMode;
                _window.Background = _isMoveMode ? MovingBackground : FixedBackground;
            }
        }

        private void OnPointerPressed(PointerPressedEventArgs args)
        {
            if (_isMoveMode && args.GetCurrentPoint(_window).Properties.IsLeftButtonPressed)
            {
                _window.BeginMoveDrag(args);
            }
        }
    }
}
