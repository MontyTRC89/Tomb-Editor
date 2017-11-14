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
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public static void Launch(LevelSettings settings, IWin32Window owner)
        {
            string executablePath = settings.MakeAbsolute(settings.GameExecutableFilePath);

            // Try to launch the game
            try
            {
                var info = new ProcessStartInfo
                {
                    WorkingDirectory = Path.GetDirectoryName(executablePath),
                    FileName = executablePath
                };

                // Start the game (i.e. "tomb4.exe")
                Process process = Process.Start(info);
                try
                {
                    // Seperate thread to wait for the options dialog to appear
                    // so it can be suppressed subsequently by sending WM_CLOSE.
                    if (settings.GameExecutableSuppressAskingForOptions && IsWindows)
                    {
                        Process process2 = process;
                        Thread thread = new Thread(() =>
                        {
                            try
                            {
                                process = null;
                                Tomb4ConvinienceImprovements.Do(process);
                            }
                            catch (Exception exc)
                            {
                                logger.Error(exc, "The 'Tomb4ConvinienceImprovements' thread caused issues.");
                            }
                            finally
                            {
                                process2?.Dispose();
                            }
                        });

                        thread.IsBackground = true;
                        thread.Priority = ThreadPriority.BelowNormal;
                        thread.Start();
                        process = null;
                    }
                }
                finally
                {
                    process?.Dispose();
                }
            }
            catch (Exception exc)
            {
                logger.Warn(exc, "Trying to launch the game  '" + executablePath + "'.");

                // Show message
                string message = "Go to tools, level settings, game paths to set a valid executable path.";
                if (Utils.IsFileNotFoundException(exc) || !File.Exists(executablePath))
                    message = "Unable to find '" + executablePath + "'. " + message;
                else
                    message = "Unable to start '" + executablePath + "' because a " + exc.GetType().Name + " occurred (" + exc.Message + "). " + message;
                DarkUI.Forms.DarkMessageBox.Show(owner, message, "Unable to start executable", MessageBoxIcon.Error);
            }
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

        private class Tomb4ConvinienceImprovements
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
            [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
            private static extern int FindWindow(string className, string windowText);
            [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
            private static extern int ShowWindowAsync(int hwnd, int command);
            [StructLayout(LayoutKind.Sequential)]
            private struct RECT
            {
                public int Left;
                public int Top;
                public int Right;
                public int Bottom;
            }
            private const uint WM_CLOSE = 0x0010;
            private const int SW_HIDE = 0;
            private const int SW_SHOW = 1;

            private const int maxWaitMilliseconds = 4000;
            private const int windowIdenficicationMaxHeight = 100;
            private const int windowIdenficicationMaxWidth = 400;
            private const int maxTaskBarShowDelay = 5000;
            private const int maxRetryDelay = 300;
            private bool _closedWindow;
            private Process _process;

            public static void Do(Process process)
            {
                var this_ = new Tomb4ConvinienceImprovements { _process = process };

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

                // Make sure the task bar stays visible
                // (Workaround super annoying TRNG problem!)
                {
                    Stopwatch timer = new Stopwatch();
                    timer.Start();
                    int taskbarHandle = ReportError(FindWindow("Shell_TrayWnd", ""));

                    while (timer.ElapsedMilliseconds < maxTaskBarShowDelay)
                    {
                        ReportError(ShowWindowAsync(taskbarHandle, SW_SHOW));
                        Thread.Sleep(maxRetryDelay);
                    }
                }
            }

            private static int ReportError(int result)
            {
                if (result != 0)
                    return result;
                int errorCode = Marshal.GetLastWin32Error();
                logger.Warn("An WinAPI error occurred in 'TombLauncher': " + errorCode);
                return 0;
            }

            private static int OnEnumWindow(IntPtr hWnd, IntPtr lParam)
            {
                // Check if the window is owned by Tomb4.exe
                var this_ = (Tomb4ConvinienceImprovements)(GCHandle.FromIntPtr(lParam).Target);
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
