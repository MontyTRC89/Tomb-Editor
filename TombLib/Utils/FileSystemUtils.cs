using NLog;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace TombLib.Utils
{
    public static class FileSystemUtils
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public static bool IsFileNotFoundException(Exception exc)
        {
            return (exc is FileNotFoundException) || (exc is DirectoryNotFoundException) || (exc is DriveNotFoundException);
        }

        public static string GetFileNameWithoutExtensionTry(string path)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            try
            {
                return Path.GetFileNameWithoutExtension(path);
            }
            catch
            {
                return path;
            }
        }

        public static string GetDirectoryNameTry(string path)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            try
            {
                return Path.GetDirectoryName(path);
            }
            catch
            {
                return path;
            }
        }

        public static bool RetryFor(int waitTimeInMilliseconds, Action actionToTry)
        {
            // File is maybe still open so let's retry until it becomes available
            var watch = new Stopwatch();
            watch.Start();
            int waitTime = 0;
            do
            {
                try
                {
                    actionToTry();
                    return true;
                }
                catch (Exception)
                { }
                Thread.Sleep(waitTime);
                waitTime = ((waitTime + 1) * 4) / 3;
            } while (watch.ElapsedMilliseconds < waitTimeInMilliseconds); // Wait up to 300 milliseconds until the configuration is readable
            return false;
        }
    }
}
