using Avalonia.Controls;
using Avalonia.Platform;
using System;
using System.Runtime.InteropServices;

namespace IRDCav.Views
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

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hwnd, IntPtr insertAfter, int x, int y, int width, int height, uint flags);

        private static readonly IntPtr HWND_TOPMOST = new(-1);
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOACTIVATE = 0x0010;

        /// <summary>
        /// Puts the window on top of all other topmost windows without taking the focus. The
        /// WS_EX_TOPMOST style alone has no effect; only SetWindowPos changes the z-order.
        /// Does nothing outside of Windows.
        /// </summary>
        public static void BringToTop(Window window)
        {
            IntPtr? hwnd = window.TryGetPlatformHandle()?.Handle;
            if (OperatingSystem.IsWindows() && hwnd is IntPtr handle)
            {
                SetWindowPos(handle, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
            }
        }

        /// <summary>Makes the window click-through and topmost. Does nothing outside of Windows.</summary>
        public static void ToTransparentWindow(this Window window)
        {
            if (!OperatingSystem.IsWindows())
            {
                return;
            }

            window.Initialized += delegate {
                IPlatformHandle? platformHandle = window.TryGetPlatformHandle();
                if (platformHandle != null)
                {
                    IntPtr hwnd = platformHandle.Handle;
                    // Click-through, always on top.
                    int extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
                    SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_COMPOSITED | WS_EX_LAYERED | WS_EX_TRANSPARENT | WS_EX_TOPMOST);
                    SetLayeredWindowAttributes(hwnd, 0, 255, 0x2);
                }
            };
        }
    }
}
