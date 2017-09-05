using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TombEditor.Geometry;

namespace TombEditor
{
    public static class TombLauncher
    {
        public static void Launch(LevelSettings settings)
        {
            string executablePath = settings.MakeAbsolute(settings.GameExecutableFilePath);
            var info = new ProcessStartInfo
            {
                WorkingDirectory = Path.GetDirectoryName(executablePath),
                FileName = executablePath
            };

            // Just start Tomb4.exe ?
            if (!settings.GameExecutableSuppressAskingForOptions || !IsWindows)
            {
                Process.Start(info).Dispose();
                return;
            }

            // Start tomb4.exe in seperate thread to wait for the options dialog to appear
            // so it can be suppressed subsequently by sending WM_CLOSE.
            Thread thread = new Thread(() =>
                {
                    using (Process process = Process.Start(info))
                        CloseAskingForOptionsWindowState.Do(process);
                });
            thread.IsBackground = true;
            thread.Priority = ThreadPriority.BelowNormal;
            thread.Start();
        }

        private static bool IsWindows
        {
            get
            {
                switch (Environment.OSVersion.Platform)
                {
                    case PlatformID.Win32NT:
                    case PlatformID.Win32S:
                    case PlatformID.Win32Windows:
                    case PlatformID.WinCE:
                        return true;
                    default:
                        return false;
                }
            }
        }

        private class CloseAskingForOptionsWindowState
        {
            private static readonly Logger logger = LogManager.GetCurrentClassLogger();

            private delegate int EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

            [DllImport("User32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
            private static extern int GetWindowThreadProcessId(IntPtr hWnd, out int processID);
            [DllImport("User32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
            private static extern int EnumWindows(EnumWindowsProc proc, IntPtr lParam);
            [DllImport("User32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
            private static extern int PostMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
            [DllImport("user32.dll")]
            private static extern int GetWindowRect(IntPtr hWnd, out RECT lpRect);
            [StructLayout(LayoutKind.Sequential)]
            private struct RECT
            {
                public int Left;
                public int Top;
                public int Right;
                public int Bottom; 
            }
            private const uint WM_CLOSE = 0x0010;
            
            private const int maxWaitMilliseconds = 4000;
            private const int windowIdenficicationMaxHeight = 100;
            private const int windowIdenficicationMaxWidth = 400;
            private bool _closedWindow;
            private Process _process;

            public static void Do(Process process)
            {
                var this_ = new CloseAskingForOptionsWindowState { _process = process };
                
                // Create GC handles.
                EnumWindowsProc onEnumWindowDelegate = OnEnumWindow;
                GCHandle thisHandle = GCHandle.Alloc(this_);
                try
                {
                    // Wait up to 2 seconds to find a suitable window.
                    Stopwatch timer = new Stopwatch();
                    timer.Start();
                    int currentWaitMilliseconds = 0;
                    while ((timer.ElapsedMilliseconds < maxWaitMilliseconds) && !this_._closedWindow)
                    {
                        // Look for any window of Tomb4.exe
                        ReportError(EnumWindows(onEnumWindowDelegate, GCHandle.ToIntPtr(thisHandle)));

                        // Wait
                        Thread.Sleep(currentWaitMilliseconds);
                        currentWaitMilliseconds = ((currentWaitMilliseconds + 1) * 4) / 3;
                    }
                }
                finally
                {
                    thisHandle.Free();
                }
            }

            private static void ReportError(int result)
            {
                if (result != 0)
                    return;
                int errorCode = Marshal.GetLastWin32Error();
                logger.Warn("An WinAPI error occurred in 'TR4Controller': " + errorCode);
            }

            private static int OnEnumWindow(IntPtr hWnd, IntPtr lParam)
            {
                // Check if the window is owned by Tomb4.exe
                var this_ = (CloseAskingForOptionsWindowState)(GCHandle.FromIntPtr(lParam).Target);
                int processID;
                ReportError(GetWindowThreadProcessId(hWnd, out processID));
                if (this_._process.Id != processID)
                    return 1;
               
                // Check if this is the CTRL window by comparing its size
                RECT area;
                ReportError(GetWindowRect(hWnd, out area));
                int width = area.Right - area.Left;
                int height = area.Bottom - area.Top;
                if ((width > windowIdenficicationMaxWidth) ||
                    (height > windowIdenficicationMaxHeight))
                    return 1;

                // Close window
                logger.Info("Window with handle " + hWnd + " has been identified as TRNG settings window. It is being closed.");
                ReportError(PostMessage(hWnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero));
                this_._closedWindow = true;
                return 0;
            }
            
            //[DllImport("ntdll.dll", CharSet = CharSet.Unicode, SetLastError = true)]
            //private static extern int NtSuspendProcess(IntPtr processHandle);

            //[DllImport("User32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
            //private static extern int GetWindowTextW(IntPtr hWnd, StringBuilder outString, int maxCount);
            //[DllImport("User32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
            //private static extern int GetWindowTextLength(IntPtr hWnd);
            //private static string GetWindowsText(IntPtr hWnd)
            //{
            //    int strLength = GetWindowTextLength(hWnd);
            //    if (strLength == 0)
            //        return "";
            //    var str = new StringBuilder(strLength + 2);
            //    ReportWindowsError(GetWindowTextW(hWnd, str, str.Capacity));
            //    return str.ToString();
            //}

        }
    }
}
