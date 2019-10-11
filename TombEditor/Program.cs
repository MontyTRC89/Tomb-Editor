﻿using DarkUI.Win32;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using TombEditor.Forms;
using TombLib.NG;
using TombLib;
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
            // Open files on start
            string startFile = null;
            if (args.Length > 0 && args[0].EndsWith(".prj2", StringComparison.InvariantCultureIgnoreCase))
                startFile = args[0];

            // Load configuration
            var initialEvents = new List<LogEventInfo>();
            var configuration = new Configuration().LoadOrUseDefault<Configuration>(initialEvents);

            if (configuration.Editor_AllowMultipleInstances ||
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

                    TrCatalog.LoadCatalog(Application.StartupPath + "\\Catalogs\\TrCatalog.xml");
                    NgCatalog.LoadCatalog(Application.StartupPath + "\\Catalogs\\NgCatalog.xml");

                    //Run
                    Editor editor = new Editor(SynchronizationContext.Current, configuration);
                    Editor.Instance = editor;
                    using (FormMain form = new FormMain(editor))
                    {
                        form.Show();

                        if (args.Length > 0) // Open files on start
                        {
                            if (args[0].EndsWith(".prj", StringComparison.InvariantCultureIgnoreCase))
                                EditorActions.OpenLevelPrj(form, args[0]);
                            else
                                EditorActions.OpenLevel(form, args[0]);
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
            }
            else if (startFile != null)
                SingleInstanceManagement.Send(Process.GetCurrentProcess(), new List<string>() { ".prj2" }, startFile);
            else
                SingleInstanceManagement.Bump(Process.GetCurrentProcess());
        }
    }
}
