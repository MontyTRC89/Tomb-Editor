using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Numerics;
using TombLib.Rendering;

namespace TombLib.Controls
{
    public class RealtimeColorDialog : ColorDialog
    {
        delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        private const int WM_CTLCOLOREDIT = 0x0133;
        private const int WM_INITDIALOG = 0x0110;
        private const int WM_MOVE = 0x0003;
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_SHOWWINDOW = 0x0040;
        private const uint SWP_NOZORDER = 0x0004;
        private const uint UFLAGS = SWP_NOSIZE | SWP_NOZORDER | SWP_SHOWWINDOW;
        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
        private static readonly IntPtr HWND_TOP = new IntPtr(0);
        private static readonly IntPtr HWND_BOTTOM = new IntPtr(1);

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool EnumChildWindows(IntPtr hWndParent, EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true)]
        static extern int GetClassName(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetWindowRect(IntPtr hWnd, ref RECT lpRect);

        private bool _shown;
        private IntPtr _handle;
        private Color _previousColor;
        private ColorScheme _colorScheme;
        private Action<Color> _onColorChange;

        public Point Position
        {
            get { return _position; }

            set
            {
                if (value.X == -1 && value.Y == -1)
                    return;

                RECT dimensions = new RECT();
                GetWindowRect(_handle, ref dimensions);
                _position = value;

                var width  = (dimensions.Right - dimensions.Left);
                var height = (dimensions.Bottom - dimensions.Top);

                if (_position.X + width > Screen.PrimaryScreen.Bounds.Width)
                    _position.X = Screen.PrimaryScreen.Bounds.Width - width;
                if (_position.Y + height > Screen.PrimaryScreen.Bounds.Height)
                    _position.Y = Screen.PrimaryScreen.Bounds.Height - height;
                if (_position.X < 0) _position.X = 0;
                if (_position.Y < 0) _position.Y = 0;

                if (_shown)
                    SetWindowPos(_handle, HWND_TOP, _position.X, _position.Y, 0, 0, UFLAGS);
            }
        }
        private Point _position = new Point(-1, -1);


        public RealtimeColorDialog(int x = -1, int y = -1, Action<Color> onColorChange = null, ColorScheme colorScheme = null)
        {
            FullOpen = true;

            _onColorChange = onColorChange;
            _colorScheme = colorScheme;
            _position = new Point(x, y);
        }
        public RealtimeColorDialog(ColorScheme colorScheme = null) : this(-1, -1, null, colorScheme) { }
        public RealtimeColorDialog(Action<Color> onColorChange, ColorScheme colorScheme) : this(-1, -1, onColorChange, colorScheme) { }

        protected override bool RunDialog(IntPtr hwndOwner)
        {
            if (_colorScheme != null)
            {
                // ColorDialog doesn't allow rewriting every entry in its CustomColors dialog, only
                // reference to new int[] array will work.

                var newCustomColors = new int[16];
                for (int i = 0; i < CustomColors.Length && i < _colorScheme.CustomColors.Length; i++)
                    newCustomColors[i] = (int)Math.Max(0, Math.Min(255, Math.Round(_colorScheme.CustomColors[i].X * 255.0f))) |
                                         (int)Math.Max(0, Math.Min(255, Math.Round(_colorScheme.CustomColors[i].Y * 255.0f))) << 8 |
                                         (int)Math.Max(0, Math.Min(255, Math.Round(_colorScheme.CustomColors[i].Z * 255.0f))) << 16;

                CustomColors = newCustomColors;
            }

            var result = base.RunDialog(hwndOwner);

            if (result == true && _colorScheme != null)
                for (int i = 0; i < CustomColors.Length && i < _colorScheme.CustomColors.Length; i++)
                {
                    var B = (CustomColors[i] & 0xFF0000) >> 16;
                    var G = (CustomColors[i] & 0xFF00) >> 8;
                    var R = (CustomColors[i] & 0xFF);

                    _colorScheme.CustomColors[i] = new Vector4(R, G, B, 255) / 255.0f;
                }

            return result;
        }

        protected override IntPtr HookProc(IntPtr hWnd, int msg, IntPtr wparam, IntPtr lparam)
        {
            IntPtr result = base.HookProc(hWnd, msg, wparam, lparam);

            switch (msg)
            {
                case WM_INITDIALOG:
                    _handle = hWnd;
                    _shown = true;
                    Position = _position;
                    break;

                case WM_MOVE:
                    if (_shown)
                    {
                        RECT dimensions = new RECT();
                        GetWindowRect(_handle, ref dimensions);

                        _position.X = dimensions.Left;
                        _position.Y = dimensions.Top;
                    }
                    break;

                case WM_CTLCOLOREDIT:
                    {
                        List<IntPtr> childWindows = new List<IntPtr>();
                        StringBuilder str = new StringBuilder(8);

                        EnumChildWindows(hWnd, new EnumWindowsProc((hWndChild, lParam) =>
                        {
                            GetClassName(hWndChild, str, str.Capacity);
                            if (str.ToString().Equals("edit", StringComparison.InvariantCultureIgnoreCase))
                                childWindows.Add(hWndChild);
                            return true;
                        }
                        ), IntPtr.Zero);

                        if (childWindows.Count == 6) // In case MS changes color dialog in further Windows versions
                        {
                            int[] colours = new int[3];

                            for (int i = 0; i < 3; i++)
                            {
                                GetWindowText(childWindows[i + 3], str, str.Capacity);
                                if (!Int32.TryParse(str.ToString(), out colours[i]))
                                    colours[i] = 0; // In case user accidentally types in random char
                            }

                            if (colours[0] < 0 || colours[0] > 255 ||
                                colours[1] < 0 || colours[1] > 255 ||
                                colours[2] < 0 || colours[2] > 255)
                                return base.HookProc(hWnd, msg, wparam, lparam);

                            // Prevent endless updates of color unless real RGB color is changed
                            var currentColor = Color.FromArgb(colours[0], colours[1], colours[2]);
                            if (currentColor != _previousColor)
                            {
                                _previousColor = currentColor;
                                _onColorChange?.Invoke(currentColor);
                            }
                        }
                    }
                    break;
            }

            return result;
        }
    }
}
