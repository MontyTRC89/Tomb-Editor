using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace TombLib.Controls
{
    public class RealtimeColorDialog : ColorDialog
    {
        delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool EnumChildWindows(IntPtr hWndParent, EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true)]
        static extern int GetClassName(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        private Color _previousColor;
        private Action<Color> _onColorChange;

        public RealtimeColorDialog(Action<Color> onColorChange)
        {
            FullOpen = true;
            _onColorChange = onColorChange;
        }

        protected override IntPtr HookProc(IntPtr hWnd, int msg, IntPtr wparam, IntPtr lparam)
        {
            if(msg == 0x0133) // WM_CTLCOLOREDIT
            {
                List<IntPtr> childWindows = new List<IntPtr>();
                StringBuilder str = new StringBuilder(8);

                EnumChildWindows(hWnd, new EnumWindowsProc((hWndChild, lParam) =>
                {
                    GetClassName(hWndChild, str, str.Capacity);
                    if(str.ToString().Equals("edit", StringComparison.InvariantCultureIgnoreCase))
                        childWindows.Add(hWndChild);
                    return true;
                }
                ), IntPtr.Zero);

                if (childWindows.Count == 6) // In case MS changes color dialog in further Windows versions
                {
                    GetWindowText(childWindows[3], str, str.Capacity);
                    var red = Int32.Parse(str.ToString());
                    GetWindowText(childWindows[4], str, str.Capacity);
                    var green = Int32.Parse(str.ToString());
                    GetWindowText(childWindows[5], str, str.Capacity);
                    var blue = Int32.Parse(str.ToString());

                    // Prevent endless updates of color unless real RGB color is changed
                    var currentColor = Color.FromArgb(red, green, blue);
                    if(currentColor != _previousColor)
                    {
                        _previousColor = currentColor;
                        _onColorChange.Invoke(currentColor);
                    }
                }
            }

            return base.HookProc(hWnd, msg, wparam, lparam);
        }
    }
}
