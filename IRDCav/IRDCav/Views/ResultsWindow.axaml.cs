using Avalonia;
using Avalonia.Controls;

namespace IRDCav.Views
{
    public partial class ResultsWindow : Window
    {
        private const int VerticalCenter = 940;

        public ResultsWindow()
        {
            WindowTransparency.ToTransparentWindow(this);
            InitializeComponent();
            OverlayWindowBehavior.Attach(this, LeftEdge);
        }

        private static PixelPoint LeftEdge(Window window) =>
            new(0, (int)(VerticalCenter - window.Height / 2));
    }
}
