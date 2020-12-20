using DarkUI.Config;
using DarkUI.Win32;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using TombEditor.Forms;
using TombLib.NG;
using TombLib.LevelData;
using TombLib.Utils;
using TombLib.Wad.Catalog;

namespace TombEditor
{
    public static class Program
    {
        static Mutex mutex = new Mutex(true, "{84867F76-232B-442B-9B10-DC72C8288839}");

        [STAThread]
        public static void Main(string[] args)
        { 
            string startFile = null;
            string batchFile = null;
            bool doBatchCompile = false;
            BatchCompileList batchList = null;

            if (args.Length >= 1)
            {
                // Open files on start
                if (args[0].EndsWith(".prj2", StringComparison.InvariantCultureIgnoreCase))
                    startFile = args[0];

                // Batch-compile levels
                if (args[0].EndsWith(".xml", StringComparison.InvariantCultureIgnoreCase))
                {
                    batchFile = args[0];
                    batchList = BatchCompileList.ReadFromXml(batchFile);
                    doBatchCompile = batchList?.Files.Count > 0;
                }
            }

            // Load configuration
            var initialEvents = new List<LogEventInfo>();
            var configuration = new Configuration().LoadOrUseDefault<Configuration>(initialEvents);

            // Update DarkUI configuration
            Colors.Brightness = configuration.UI_FormColor_Brightness / 100.0f;

            if (configuration.Editor_AllowMultipleInstances || doBatchCompile ||
                mutex.WaitOne(TimeSpan.Zero, true))
            {
                // Setup logging
                using (var log = new Logging(configuration.Log_MinLevel, configuration.Log_WriteToFile, configuration.Log_ArchiveN, initialEvents))
                {
                    // Create configuration file
                    configuration.SaveTry();

                    // Setup application
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
                    Application.AddMessageFilter(new ControlScrollFilter());
                    SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());

                    if (!File.Exists(Application.StartupPath + "\\Catalogs\\TrCatalog.xml") ||
                        !File.Exists(Application.StartupPath + "\\Catalogs\\NgCatalog.xml"))
                    {
                        MessageBox.Show("One of the catalog files is missing.\nMake sure you have TrCatalog.xml and NgCatalog.xml in /Catalogs/ subfolder.");
                        Environment.Exit(1);
                    }

                    // Load catalogs
                    TrCatalog.LoadCatalog(Application.StartupPath + "\\Catalogs\\TrCatalog.xml");
                    NgCatalog.LoadCatalog(Application.StartupPath + "\\Catalogs\\NgCatalog.xml");

                    // Run
                    Editor editor = new Editor(SynchronizationContext.Current, configuration);

                    // Run editor normally if no batch compile is pending.
                    // Otherwise, don't load main form and jump straight to batch-compiling levels.

                    if (!doBatchCompile)
                    {
                        using (FormMain form = new FormMain(editor))
                        {
                            form.Show();

                            if (!string.IsNullOrEmpty(startFile)) // Open files on start
                            {
                                if (startFile.EndsWith(".prj", StringComparison.InvariantCultureIgnoreCase))
                                    EditorActions.OpenLevelPrj(form, startFile);
                                else
                                    EditorActions.OpenLevel(form, startFile);
                            }
                            else if (editor.Configuration.Editor_OpenLastProjectOnStartup)
                            {
                                if (Properties.Settings.Default.RecentProjects != null && Properties.Settings.Default.RecentProjects.Count > 0 &&
                                    File.Exists(Properties.Settings.Default.RecentProjects[0]))
                                    EditorActions.OpenLevel(form, Properties.Settings.Default.RecentProjects[0]);
                            }
                            Application.Run(form);
                        }
                    }
                    else
                        EditorActions.BuildInBatch(editor, batchList, batchFile);
                }
            }
            else if (startFile != null) // Send opening file to existing editor instance
                SingleInstanceManagement.Send(Process.GetCurrentProcess(), new List<string>() { ".prj2" }, startFile);
            else // Just bring editor to top, if user tries to launch another copy
                SingleInstanceManagement.Bump(Process.GetCurrentProcess());
        }
    }
}
