using NLog;
using NLog.Config;
using NLog.Targets;
using System;
using System.Windows.Forms;

namespace TombEditor
{
    static class Program
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private static void InitLogging()
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

            //new TombEngine.TombRaider4Level(@"D:\Eigenes\Spiele\TR\Levelbau\Levels\Junglelevel\data\Level0000.tr4").Load("");
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FormMain());
        }
    }
}
