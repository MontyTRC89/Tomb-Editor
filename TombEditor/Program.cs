using DarkUI.Win32;
using NLog;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using TombEditor.Forms;
using TombLib.NG;
using TombLib.Utils;
using TombLib.Wad.Catalog;

namespace TombEditor
{
    public static class Program
    {
        /// <summary>
        /// The main entry point of the application.
        /// </summary>
        [STAThread]
        public static void Main(string[] args)
        {
            // Load configuration
            var initialEvents = new List<LogEventInfo>();
            var configuration = Configuration.LoadOrUseDefault(initialEvents);

            // Setup logging
            using (var log = new Logging(configuration.Log_MinLevel, configuration.Log_WriteToFile, configuration.Log_ArchiveN, initialEvents))
            {
                // Create configuration file, but only if there is a need to.
                if (initialEvents.Count != 0)
                    configuration.SaveTry();

                // Setup application
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.AddMessageFilter(new ControlScrollFilter());
                SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());

                TrCatalog.LoadCatalog(Application.StartupPath + "\\Catalogs\\TrCatalog.xml");
                NgCatalog.LoadCatalog(Application.StartupPath + "\\Catalogs\\NgCatalog.xml");

                //Run
                Editor editor = new Editor(SynchronizationContext.Current, configuration);
                Editor.Instance = editor;
                using (FormMain form = new FormMain(editor))
                {
                    form.Show();

                    if (args.Length > 0) // Open files on start
                        if (args[0].EndsWith(".prj", StringComparison.InvariantCultureIgnoreCase))
                            EditorActions.OpenLevelPrj(form, args[0]);
                        else
                            EditorActions.OpenLevel(form, args[0]);
                    Application.Run(form);
                }
            }
        }
    }
}
