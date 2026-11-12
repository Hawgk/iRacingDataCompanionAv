using Avalonia;
using Avalonia.Controls;

namespace IRDCav.Views
{
    public partial class InputGraphWindow : Window
    {
        private const int Top = 480;

        public InputGraphWindow()
        {
            WindowTransparency.ToTransparentWindow(this);
            InitializeComponent();
            OverlayWindowBehavior.Attach(this, HorizontallyCentered);
        }

        private static PixelPoint HorizontallyCentered(Window window) =>
            new((int)((OverlayWindowBehavior.ScreenWidth - window.Width) / 2), Top);
    }
}
