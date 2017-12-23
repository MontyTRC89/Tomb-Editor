using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WadTool
{
    static class Program
    {
        /// <summary>
        /// Punto di ingresso principale dell'applicazione.
        /// </summary>
        [STAThread]
        static void Main()
        {
            System.Globalization.CultureInfo.DefaultThreadCurrentCulture =
                System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = new System.Globalization.CultureInfo("en-US");

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FormMain());
        }
    }
}
