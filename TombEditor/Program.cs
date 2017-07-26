using System;
using System.Windows.Forms;

namespace TombEditor
{
    static class Program
    {
        /// <summary>
        /// The main entry point of the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Debug.InitLogging();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FormMain());
        }
    }
}
