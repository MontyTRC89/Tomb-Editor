using NLog;
using NLog.Config;
using NLog.Targets;
using System;
using System.Windows.Forms;
using DarkUI.Win32;

namespace TombEditor
{
    static class Program
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public static void InitLogging()
        {
            var config = new LoggingConfiguration();

            //if (System.Diagnostics.Debugger.IsAttached)
            {
                var consoleTarget = new ColoredConsoleTarget();
                config.AddTarget("console", consoleTarget);
                consoleTarget.Layout = @"[${date:format=HH\:mm\:ss.fff} ${pad:padding=5:inner=${level:uppercase=true}}] ${logger} | ${message}";

                var rule1 = new LoggingRule("*", LogLevel.Debug, consoleTarget);
                config.LoggingRules.Add(rule1);
            }


            LogManager.Configuration = config;
        }

        /// <summary>
        /// The main entry point of the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            InitLogging();

            logger.Info($"Tomb Editor {Application.ProductVersion} is starting");

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.AddMessageFilter(new ControlScrollFilter());

            //Run
            Editor editor = Editor.Instance;
            Application.Run(new FormMain(editor));
            editor.Configuration.SaveTry();
        }
    }
}
