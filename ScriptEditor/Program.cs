using System;
using System.Windows.Forms;

namespace ScriptEditor
{
	internal static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		private static void Main(string[] args)
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			using (FormMain form = new FormMain())
			{
				form.Show();
				if (args.Length > 1) // Open files on start
					form.OpenFile(args[0]);
				Application.Run(form);
			}
		}
	}
}
