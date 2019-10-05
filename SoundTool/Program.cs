using DarkUI.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using TombLib.Utils;
using TombLib.Wad.Catalog;

namespace SoundTool
{
    static class Program
    {
        static Mutex mutex = new Mutex(true, "{CB5908BE-8C0C-435E-886D-3B3B67308FB0}");

        [STAThread]
        public static void Main(string[] args)
        {
            var extensions = new List<string>() { ".xml", ".txt" };

            string startFile = null;
            if (args.Length > 0) // Open files on start
            {
                foreach(var ext in extensions)
                    if(args[0].EndsWith(ext, StringComparison.InvariantCultureIgnoreCase))
                    {
                        startFile = args[0];
                        break;
                    }
            }

            if (mutex.WaitOne(TimeSpan.Zero, true))
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.AddMessageFilter(new ControlScrollFilter());

                if (!File.Exists(Application.StartupPath + "\\Catalogs\\TrCatalog.xml"))
                {
                    MessageBox.Show("TrCatalog.xml is missing.\nMake sure you have TrCatalog.xml in /Catalogs/ subfolder.");
                    Environment.Exit(1);
                }
                TrCatalog.LoadCatalog(Application.StartupPath + "\\Catalogs\\TRCatalog.xml");

                using (FormMain form = new FormMain(startFile))
                {
                    form.Show();
                    Application.Run(form);
                }
            }
            else if (startFile != null )
                SingleInstanceManagement.Send(Process.GetCurrentProcess(), extensions, startFile);
            else
                SingleInstanceManagement.Bump(Process.GetCurrentProcess());
        }
    }
}
