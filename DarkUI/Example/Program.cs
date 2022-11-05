using System;
using System.Windows.Forms;
using Example.Forms;

namespace Example
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetDefaultFont(new System.Drawing.Font("Segoe UI", 8.25f));
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
