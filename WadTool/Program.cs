using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        public static void Main()
        {
            // Load configuration
            var initialEvents = new List<LogEventInfo>();
            var configuration = Configuration.LoadOrUseDefault(initialEvents);

            // Setup logging
            using (var log = new Logging(configuration.Log_MinLevel, configuration.Log_WriteToFile, configuration.Log_ArchiveN, initialEvents))
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                // Run
                TrCatalog.LoadCatalog("Catalogs\\TRCatalog.xml");
                using (WadToolClass tool = new WadToolClass(configuration))
                    Application.Run(new FormMain(tool));
            }
        }
    }
}
