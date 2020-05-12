using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace DarkUI.Win32
{
    internal static class Native
    {
        [DllImport("user32.dll")]
        internal static extern IntPtr WindowFromPoint(Point point);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        internal static extern IntPtr SendMessage(IntPtr hWnd, UInt32 msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32")]
        internal static extern IntPtr GetWindow(IntPtr hWnd, int wCmd);

        internal const int GW_CHILD = 5;

        [DllImport("user32")] [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool ShowWindow(IntPtr hWnd, int wCmd);

        internal const int SW_HIDE = 0;

        internal const int SW_SHOWNOACTIVATE = 4;
        internal const int HWND_TOPMOST = -1;
        internal const uint SWP_NOACTIVATE = 0x0010;

        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        static extern bool SetWindowPos(
            int hWnd,             // Window handle
            int hWndInsertAfter,  // Placement-order handle
            int X,                // Horizontal position
            int Y,                // Vertical position
            int cx,               // Width
            int cy,               // Height
            uint uFlags);         // Window positioning flags

        internal static void ShowInactiveTopmost(Form frm)
        {
            ShowWindow(frm.Handle, SW_SHOWNOACTIVATE);
            SetWindowPos(frm.Handle.ToInt32(), HWND_TOPMOST,
            frm.Left, frm.Top, frm.Width, frm.Height,
            SWP_NOACTIVATE);
        }
    }
}
