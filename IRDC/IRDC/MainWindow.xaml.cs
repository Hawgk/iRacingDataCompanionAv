using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace IRDC
{
    public partial class MainWindow : Window
    {
        private bool _f6Pressed = false;
        private SolidColorBrush _windowBackgroundMoving = new SolidColorBrush(Color.FromArgb(0x80, 0xFF, 0xFF, 0xFF));
        private SolidColorBrush _windowBackgroundFixed = new SolidColorBrush(Color.FromArgb(0x70, 0x00, 0x00, 0x00));

        public MainWindow()
        {
            InitializeComponent();
            Background = _windowBackgroundFixed;

        }

        private void OnWindowDeactivated(object sender, EventArgs e)
        {
            Window window = (Window)sender;
            window.Topmost = true;
        }

        private void OnCloseClicked(object sender, EventArgs e)
        {
            Close();
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && _f6Pressed)
            {
                DragMove();
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