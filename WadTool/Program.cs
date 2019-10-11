using DarkUI.Win32;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using TombLib.Utils;
using TombLib.Wad.Catalog;

namespace WadTool
{
    static class Program
    {
        /// <summary>
        /// The main entry point of the application.
        /// </summary>
        [STAThread]
        public static void Main(string[] args)
        {
            // Load configuration
            var initialEvents = new List<LogEventInfo>();
            var configuration = new Configuration().LoadOrUseDefault<Configuration>(initialEvents);

            // Setup logging
            using (var log = new Logging(configuration.Log_MinLevel, configuration.Log_WriteToFile, configuration.Log_ArchiveN, initialEvents))
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
                Application.ThreadException += (sender, e) =>
                {
                    log.HandleException(e.Exception);
                    using (var dialog = new ThreadExceptionDialog(e.Exception))
                        if (dialog.ShowDialog() == DialogResult.Abort)
                            Environment.Exit(1);
                };

                // Show startup help
                if (configuration.StartUpHelp_Show)
                {
                    var help = new FormStartupHelp();
                    Application.Run(help);
                    switch (help.DialogResult)
                    {
                        case DialogResult.Cancel:
                            return;
                        case DialogResult.OK:
                            configuration.StartUpHelp_Show = false;
                            break;
                    }
                }
                configuration.SaveTry();

                // Run
                if (!File.Exists(Application.StartupPath + "\\Catalogs\\TrCatalog.xml"))
                {
                    MessageBox.Show("TrCatalog.xml is missing.\nMake sure you have TrCatalog.xml in /Catalogs/ subfolder.");
                    Environment.Exit(1);
                }
                TrCatalog.LoadCatalog(Application.StartupPath + "\\Catalogs\\TRCatalog.xml");

                Application.AddMessageFilter(new ControlScrollFilter());
                SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());

                using (WadToolClass tool = new WadToolClass(configuration))
                using (FormMain form = new FormMain(tool))
                {
                    form.Show();
                    if (args.Length > 0)
                        WadActions.LoadWad(tool, null, true, args[0]);
                    Application.Run(form);
                }
            }
        }
    }
}
