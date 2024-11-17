using Avalonia.Controls;
using Avalonia.Platform;
using Avalonia.VisualTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace IRDCav
{
    public static class WindowTransparency
    {
        internal const int WS_EX_TRANSPARENT = 0x00000020;
        internal const int WS_EX_COMPOSITED = 0x02000000;
        internal const int WS_EX_LAYERED = 0x00080000;
        internal const int WS_EX_TOPMOST = 0x00000008;
        internal const int GWL_EXSTYLE = (-20);

        [DllImport("user32.dll")]
        public static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint key, byte alpha, uint flags);

        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

        public static void ToTransparentWindow(this Window window)
        {
            window.Initialized += delegate {
                // Get this window's handle
                IPlatformHandle? platformHandle = window.TryGetPlatformHandle();
                if (platformHandle != null)
                {
                    IntPtr hwnd = platformHandle.Handle;
                    // Change the extended window style to include WS_EX_TRANSPARENT
                    int extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
                    SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_COMPOSITED | WS_EX_LAYERED | WS_EX_TRANSPARENT | WS_EX_TOPMOST);
                    SetLayeredWindowAttributes(hwnd, 0, 255, 0x2);
                }
            };
        }
    }
}
