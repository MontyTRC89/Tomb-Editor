using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TombLib.Utils
{
    public static class ClipboardEvents
    {
        private const int WM_ClipboardChanged = 0x031D;
        private static IntPtr HWND_MESSAGE = new IntPtr(-3);
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool AddClipboardFormatListener(IntPtr hwnd);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        private static EventHandler ClipboardChangedEvents = null;
        private static object @lock = new object();
        public static event EventHandler ClipboardChanged
        {
            add
            {
                lock (@lock)
                {
                    ClipboardChangedEvents = (EventHandler)Delegate.Combine(ClipboardChangedEvents, value);
                    if (_form == null)
                        _form = new NotificationForm();
                }
            }
            remove
            {
                lock (@lock)
                    ClipboardChangedEvents = (EventHandler)Delegate.Remove(ClipboardChangedEvents, value);
            }
        }

        // Hidden form to recieve the WM_ClipboardChanged message.
        private static NotificationForm _form = null;
        private class NotificationForm : Form
        {
            public NotificationForm()
            {
                switch (Environment.OSVersion.Platform)
                {
                    case PlatformID.Win32NT:
                    case PlatformID.Win32S:
                    case PlatformID.Win32Windows:
                    case PlatformID.WinCE:
                        break;
                    default:
                        return;
                }

                SetParent(Handle, HWND_MESSAGE);
                AddClipboardFormatListener(Handle);
            }

            protected override void WndProc(ref Message m)
            {
                if (m.Msg == WM_ClipboardChanged)
                    ClipboardChangedEvents?.Invoke(null, EventArgs.Empty);
                base.WndProc(ref m);
            }
        }
    }
}
