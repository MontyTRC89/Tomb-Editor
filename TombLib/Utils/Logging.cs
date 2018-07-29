using NLog;
using NLog.Config;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace TombLib.Utils
{
    public class Logging : IDisposable
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private const string _layout =
            "[${date:format=HH\\:mm\\:ss.fff} ${pad:padding=5:inner=${level:uppercase=true}}] ${logger} | ${message} | " +
            "${exception:innerFormat=Type,Message,StackTrace:maxInnerExceptionLevel=32:" +
            "innerExceptionSeparator=\r\n\r\n\r\n:separator=\r\n:format=Type,Message,Data,StackTrace}";

        public void HandleException(Exception exception)
        {
            LogManager.GetCurrentClassLogger().Fatal(exception, "Unhandled exception");
            LogManager.Flush();
        }

        public Logging(LogLevel minLogLevel, bool writeToFile, int archiveFileCount, IEnumerable<LogEventInfo> initialEvents)
        {
            // Set culture
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-US");

            // Get assembly information
            var entryAssembly = Assembly.GetEntryAssembly();
            var assemblyProductAttribute = entryAssembly.GetCustomAttribute(typeof(AssemblyProductAttribute)) as AssemblyProductAttribute;
            var assemblyVersionAttribute = entryAssembly.GetCustomAttribute(typeof(AssemblyFileVersionAttribute)) as AssemblyFileVersionAttribute;

            // Setup logging targets
            var config = new LoggingConfiguration();
            AddTargetAndRule(config, minLogLevel, new ColoredConsoleTarget("Console") { Layout = _layout, DetectConsoleAvailable = true });
            AddTargetAndRule(config, minLogLevel, new DebuggerTarget("Debugger") { Layout = _layout });
            if (writeToFile)
            {
                string fileName = assemblyProductAttribute?.Product ?? "TombLib";
                fileName = fileName.Replace(" ", "") + "Log.txt";

                AddTargetAndRule(config, minLogLevel, new FileTarget("File")
                {
                    Layout = _layout,
                    FileName = Path.Combine(PathC.GetDirectoryNameTry(Assembly.GetExecutingAssembly().FullName), fileName),
                    KeepFileOpen = true,
                    DeleteOldFileOnStartup = true,

                    MaxArchiveFiles = archiveFileCount,
                    ArchiveNumbering = ArchiveNumberingMode.DateAndSequence,
                    ArchiveOldFileOnStartup = archiveFileCount > 0
                });
            }
            LogManager.Configuration = config;

            // Setup application exception handler to use nlog
            AppDomain.CurrentDomain.UnhandledException += (sender, e) => HandleException(e.ExceptionObject as Exception);

            // Give startup information about the application in the log
            logger.Info((assemblyProductAttribute?.Product ?? "TombLib") + " version " + (assemblyVersionAttribute?.Version ?? "?") + " is starting");
            logger.Info("TombLib Git Revision: " + GitVersion);

            // Raise initial exceptions
            if (initialEvents != null)
                foreach (var initialEvent in initialEvents)
                    logger.Log(initialEvent);
        }

        public void Dispose()
        {
            var assemblyProductAttribute = Assembly.GetEntryAssembly().GetCustomAttribute(typeof(AssemblyProductAttribute)) as AssemblyProductAttribute;
            logger.Info((assemblyProductAttribute?.Product ?? "TombLib") + " has exited cleanly.");
        }

        private static Target AddTargetAndRule(LoggingConfiguration loggingConfiguration, LogLevel minLevel, Target target)
        {
            loggingConfiguration.AddTarget(target);
            loggingConfiguration.AddRule(minLevel, LogLevel.Fatal, target);
            return target;
        }

        public static string GitVersion
        {
            get
            {
                using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("version"))
                {
                    if (stream == null)
                        return "<Information missing>";
                    using (StreamReader reader = new StreamReader(stream))
                        return reader.ReadToEnd();
                }
            }
        }
    }
}
