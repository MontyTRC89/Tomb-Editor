using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace TombLib.Utils
{
    [StructLayout(LayoutKind.Sequential)]
    public struct COPYDATASTRUCT
    {
        public IntPtr dwData;
        public Int32  cbData;
        public IntPtr lpData;
    }

    public static class SingleInstanceManagement
    {
        public const int WM_COPYDATA   = 0x004A;
        public const int WM_SHOWWINDOW = 0x7515;

        private const uint SW_RESTORE = 0x09;

        [DllImport("user32", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int SendMessage(IntPtr hwnd, int wMsg, IntPtr wParam, ref COPYDATASTRUCT lParam);
        [DllImport("user32", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int SendMessage(IntPtr hwnd, int wMsg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int ShowWindow(IntPtr hWnd, uint Msg);

        public static bool Send(Process process, List<string> supportedExtensions, string filename)
        {
            Process[] existingProcesses = Process.GetProcessesByName(process.ProcessName);

            if (existingProcesses.Length <= 1 || !File.Exists(filename))
                return false;

            var extension = Path.GetExtension(filename);
            if (extension == null)
                return false;

            foreach (var ext in supportedExtensions)
            {
                if (extension.EndsWith(ext, StringComparison.InvariantCultureIgnoreCase))
                    break;
                return false;
            }

            foreach (Process p in existingProcesses)
            {
                if (p.Id != process.Id)
                {
                    COPYDATASTRUCT cds;
                    cds.dwData = IntPtr.Zero;
                    cds.lpData = Marshal.StringToHGlobalAnsi(filename);
                    cds.cbData = filename.Length;

                    SendMessage(p.MainWindowHandle, WM_COPYDATA, IntPtr.Zero, ref cds);
                    return true; // Send only to first opened one
                }
            }
            return false; // Only current one is running
        }

        public static void Bump(Process process)
        {
            Process[] existingProcesses = Process.GetProcessesByName(process.ProcessName);

            if (existingProcesses.Length <= 1)
                return;

            foreach (Process p in existingProcesses)
                if (p.Id != process.Id) // Send only to first opened one
                    SendMessage(p.MainWindowHandle, WM_SHOWWINDOW, IntPtr.Zero, IntPtr.Zero);
        }

        public static string Catch(ref Message message)
        {
            if (message.Msg != WM_COPYDATA)
                return null;

            COPYDATASTRUCT CD = (COPYDATASTRUCT)message.GetLParam(typeof(COPYDATASTRUCT));
            byte[] B = new byte[CD.cbData];
            IntPtr lpData = CD.lpData;
            Marshal.Copy(lpData, B, 0, CD.cbData);
            string strData = Encoding.ASCII.GetString(B);

            if (strData != null && File.Exists(strData))
                return strData;
            else
                return null;
        }

        public static void RestoreWindowState(Form form)
        {
            if (form.WindowState == FormWindowState.Minimized)
                ShowWindow(form.Handle, SW_RESTORE);
            else
            {
                // Ordinary form.Activate() won't work in case there's message box in the way.
                var backupState = form.WindowState;
                form.WindowState = FormWindowState.Minimized;
                form.Show();
                form.WindowState = backupState;
            }
        }
    }
}
