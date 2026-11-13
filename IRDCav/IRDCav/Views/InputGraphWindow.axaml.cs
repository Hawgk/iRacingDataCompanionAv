using Avalonia.Controls;

namespace IRDCav.Views
{
    /// <summary>Placement and scaling are attached by the composition root (see OverlayWindowBehavior).</summary>
    public partial class InputGraphWindow : Window
    {
        public InputGraphWindow()
        {
            WindowTransparency.ToTransparentWindow(this);
            InitializeComponent();
        }
    }
}
