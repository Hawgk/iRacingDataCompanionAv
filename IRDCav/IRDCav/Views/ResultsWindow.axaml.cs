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
        private SolidColorBrush _windowBackgroundMoving = new SolidColorBrush(Color.FromArgb(0x40, 0xFF, 0xFF, 0xFF));
        private SolidColorBrush _windowBackgroundFixed = new SolidColorBrush(Color.FromArgb(0x90, 0x00, 0x00, 0x00));

        public ResultsWindow()
        {
            InitializeComponent();
            Background = _windowBackgroundFixed;

        }

        private void OnWindowLoaded(object sender, RoutedEventArgs args)
        {
            PixelSize screenSize = Screens.Primary.WorkingArea.Size;
            PixelSize windowSize = PixelSize.FromSize(ClientSize, Screens.Primary.Scaling);

            Position = new PixelPoint(0, 0);
        }

        private void OnWindowDeactivated(object sender, EventArgs e)
        {
            Window window = (Window)sender;
            window.Topmost = true;
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