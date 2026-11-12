using Avalonia;
using Avalonia.Controls;

namespace IRDCav.Views
{
    public partial class RelativesWindow : Window
    {
        public RelativesWindow()
        {
            WindowTransparency.ToTransparentWindow(this);
            InitializeComponent();
            OverlayWindowBehavior.Attach(this, BottomRightCorner);
        }

        private static PixelPoint BottomRightCorner(Window window) => new(
            (int)(OverlayWindowBehavior.ScreenWidth - window.Width),
            (int)(OverlayWindowBehavior.ScreenHeight - window.Height));
    }
}
