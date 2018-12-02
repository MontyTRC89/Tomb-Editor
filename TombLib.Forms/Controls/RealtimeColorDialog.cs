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

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool EnumChildWindows(IntPtr hWndParent, EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true)]
        static extern int GetClassName(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        private Color _previousColor;
        private Action<Color> _onColorChange;

        private ColorScheme _colorScheme;

        public RealtimeColorDialog(Action<Color> onColorChange, ColorScheme colorScheme = null)
        {
            FullOpen = true;
            _onColorChange = onColorChange;
            _colorScheme = colorScheme;
        }

        protected override bool RunDialog(IntPtr hwndOwner)
        {
            if(_colorScheme != null)
            {
                // ColorDialog doesn't allow rewriting every entry in its CustomColors dialog, only
                // reference to new int[] array will work.

                var newCustomColors = new int[16];
                for (int i = 0; i < CustomColors.Length && i < _colorScheme.CustomColors.Length; i++)
                    newCustomColors[i] = (int)Math.Max(0, Math.Min(255, Math.Round(_colorScheme.CustomColors[i].X * 255.0f)))      |
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
                    int[] colours = new int[3];

                    for(int i = 0; i < 3; i++)
                    {
                        GetWindowText(childWindows[i+3], str, str.Capacity);
                        if (!Int32.TryParse(str.ToString(), out colours[i]))
                            colours[i] = 0; // In case user accidentally types in random char
                    }

                    if (colours[0] < 0 || colours[0] > 255 ||
                        colours[1] < 0 || colours[1] > 255 ||
                        colours[2] < 0 || colours[2] > 255)
                        return base.HookProc(hWnd, msg, wparam, lparam);

                    // Prevent endless updates of color unless real RGB color is changed
                    var currentColor = Color.FromArgb(colours[0], colours[1], colours[2]);
                    if(currentColor != _previousColor)
                    {
                        _previousColor = currentColor;
                        _onColorChange?.Invoke(currentColor);
                    }
                }
            }

            return base.HookProc(hWnd, msg, wparam, lparam);
        }
    }
}
