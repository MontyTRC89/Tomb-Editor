using DarkUI.Win32;
using NLog;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using TombLib.Graphics;
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
        public static void Main()
        {
            // Load configuration
            var initialEvents = new List<LogEventInfo>();
            var configuration = Configuration.LoadOrUseDefault(initialEvents);

            // Setup logging
            using (var log = new Logging(configuration.Log_MinLevel, configuration.Log_WriteToFile, configuration.Log_ArchiveN, initialEvents))
            {
                configuration.SaveTry();

                // Setup application
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.AddMessageFilter(new ControlScrollFilter());
                SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());

                TrCatalog.LoadCatalog("Catalogs\\TrCatalog.xml");
                NgCatalog.LoadCatalog("Catalogs\\NgCatalog.xml");

                //Run
                Editor editor = new Editor(WindowsFormsSynchronizationContext.Current, configuration);
                Editor.Instance = editor;
                Application.Run(new FormMain(editor));

                configuration.SaveTry();
            }
        }
    }
}
