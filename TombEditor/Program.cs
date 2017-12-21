using NLog;
using NLog.Config;
using NLog.Targets;
using System;
using System.Windows.Forms;
using DarkUI.Win32;
using System.Text;
using System.Collections.Generic;
using System.Threading;
using TombLib.Wad.Catalog;
using TombLib.NG;
using TombLib.Graphics;

namespace TombEditor
{
    public static class Program
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The main entry point of the application.
        /// </summary>
        [STAThread]
        public static void Main()
        {
            // Load configuration
            var configurationLoadExceptions = new List<LogEventInfo>();
            var configuration = Configuration.LoadOrUseDefault(configurationLoadExceptions);

            // Setup logging
            InitLogging(configuration.Log_MinLevel, configuration.Log_WriteToFile, configuration.Log_ArchiveN);
            logger.Info($"Tomb Editor {Application.ProductVersion} is starting");
            foreach (var configurationLoadException in configurationLoadExceptions)
                logger.Log(configurationLoadException);

            // Setup application
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.AddMessageFilter(new ControlScrollFilter());
            SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());

            //Run
            configuration.SaveTry();
            Editor editor = new Editor(WindowsFormsSynchronizationContext.Current, configuration);
            Editor.Instance = editor;
            TrCatalog.LoadCatalog("Editor\\Misc\\TRCatalog.xml");
            NgCatalog.LoadCatalog("Editor\\Misc\\NgCatalog.xml");
            Application.Run(new FormMain(editor));
            editor.Configuration.SaveTry();
            logger.Info("The editor has exited cleanly. The configuration has been saved!");
        }

        private const string _layout =
            "[${date:format=HH\\:mm\\:ss.fff} ${pad:padding=5:inner=${level:uppercase=true}}] ${logger} | ${message} | " +
            "${exception:innerFormat=Type,Message,StackTrace:maxInnerExceptionLevel=32:" +
            "innerExceptionSeparator=\n\n\n:separator=\n:format=Type,Message,Data,StackTrace}";

        private static void InitLogging(LogLevel minLogLevel, bool writeToFile, int archiveFileCount)
        {
            var config = new LoggingConfiguration();
            config.AddTargetAndRule(minLogLevel, new ColoredConsoleTarget("Console") { Layout = _layout, DetectConsoleAvailable = true });
            config.AddTargetAndRule(minLogLevel, new DebuggerTarget("Debugger") { Layout = _layout });
            if (writeToFile)
                config.AddTargetAndRule(minLogLevel, new FileTarget("File")
                {
                    Layout = _layout,
                    FileName = "TombEditorLog.txt",
                    KeepFileOpen = true,
                    DeleteOldFileOnStartup = true,

                    MaxArchiveFiles = archiveFileCount,
                    ArchiveNumbering = ArchiveNumberingMode.DateAndSequence,
                    ArchiveOldFileOnStartup = archiveFileCount > 0
                });
            LogManager.Configuration = config;

            // Setup application exception handler to use nlog
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
                LogManager.GetCurrentClassLogger().Fatal(e.ExceptionObject as Exception, "Unhandled exception");
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
                {
                    var sharpDXException = e.ExceptionObject as SharpDX.SharpDXException;
                    if (sharpDXException != null)
                        if ((sharpDXException.ResultCode == 0x887A0005) || sharpDXException.Descriptor.ApiCode.Contains("DXGI_ERROR_DEVICE_REMOVED"))
                        {
                            var device = (SharpDX.Direct3D11.Device)(DeviceManager.DefaultDeviceManager.Device);
                            LogManager.GetCurrentClassLogger().Info(sharpDXException, "Device '" + device.DebugName +
                                "' was removed because '" + device.DeviceRemovedReason + "'");
                        }
                };
        }

        private static Target AddTargetAndRule(this LoggingConfiguration loggingConfiguration, LogLevel minLevel, Target target)
        {
            loggingConfiguration.AddTarget(target);
            loggingConfiguration.AddRule(minLevel, LogLevel.Fatal, target);
            return target;
        }
    }
}
