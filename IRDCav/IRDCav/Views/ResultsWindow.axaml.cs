using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using System;

namespace IRDCav.Views
{
    public partial class ResultsWindow : Window
    {
        private bool _f6Pressed = false;
        private SolidColorBrush _windowBackgroundMoving = new SolidColorBrush(Color.FromArgb(0x40, 0xD3, 0xD0, 0xCB));
        private SolidColorBrush _windowBackgroundFixed = new SolidColorBrush(Color.FromArgb(0xA0, 0x1E, 0x20, 0x19));

        public PixelSize ScreenSize { get; set; }

        public ResultsWindow()
        {
            WindowTransparency.ToTransparentWindow(this);
            InitializeComponent();
            Background = _windowBackgroundFixed;
        }

        private void OnWindowLoaded(object sender, RoutedEventArgs args)
        {
            PixelSize screenSize = Screens.Primary.WorkingArea.Size;
            PixelSize windowSize = PixelSize.FromSize(ClientSize, Screens.Primary.Scaling);

            Position = new PixelPoint(0, (int)(940 - Height / 2));
        }

        private void OnResize(object sender, EventArgs e)
        {
            int x = Position.X;
            int y = Position.Y;

            if (Position.X < 0)
            {
                x = 0;
            }
            else if (Position.X > (int)(2560 - Width))
            {
                x = (int)(2560 - Width);
            }

            if (Position.X < 0)
            {
                x = 0;
            }
            else if (Position.Y > (int)(1420 - Height))
            {
                y = (int)(1420 - Height);
            }

            Position = new PixelPoint(x, y);
        }

        private void OnWindowDeactivated(object sender, EventArgs e)
        {
            Topmost = true;
        }

        private void OnCloseClicked(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OnMouseDown(object sender, PointerPressedEventArgs args)
        {
            var point = args.GetCurrentPoint(sender as Control);
            if (point.Properties.IsLeftButtonPressed && _f6Pressed)
            {
                BeginMoveDrag(args);
            }
        }

        private void OnKeyPressed(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F6)
            {
                if (_f6Pressed)
                {
                    Background = _windowBackgroundFixed;
                    _f6Pressed = false;
                }
                else
                {
                    Background = _windowBackgroundMoving;
                    _f6Pressed = true;
                }
            }
        }
    }
}