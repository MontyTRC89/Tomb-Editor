using System;
using System.Drawing;
using System.Linq;
using System.Reflection;
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
#if NET5_0
			typeof(Control)
				.GetRuntimeFields()
				.FirstOrDefault(x => x.Name == "s_defaultFont")?
				.SetValue(null, new Font("Segoe UI", 8.25F));
#else
            Application.SetDefaultFont(new System.Drawing.Font("Segoe UI", 8.25f));
#endif
			Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
