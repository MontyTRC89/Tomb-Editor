using DarkUI.Forms;
using System;
using System.IO;
using System.Windows.Forms;
using TombIDE.Shared;

namespace TombIDE.ProjectMaster
{
	public partial class SettingsSpecialFunctions : UserControl
	{
		private IDE _ide;

		public SettingsSpecialFunctions()
		{
			InitializeComponent();
		}

		public void Initialize(IDE ide)
		{
			_ide = ide;

			textBox_LauncherName.Text = Path.GetFileName(_ide.Project.LaunchFilePath);
		}

		private void button_DeleteLogs_Click(object sender, EventArgs e)
		{
			try
			{
				string[] files = Directory.GetFiles(_ide.Project.EnginePath);

				bool wereFilesDeleted = false;

				foreach (string file in files)
				{
					string fileName = Path.GetFileName(file);

					if (fileName == "db_patches_crash.bin"
						|| fileName == "DETECTED CRASH.txt"
						|| fileName == "LastExtraction.lst"
						|| (fileName.StartsWith("Last_Crash_") && (fileName.EndsWith(".txt") || fileName.EndsWith(".mem")))
						|| fileName.EndsWith("_warm_up_log.txt"))
					{
						File.Delete(file);
						wereFilesDeleted = true;
					}
				}

				string logsDirectory = Path.Combine(_ide.Project.EnginePath, "logs");

				if (Directory.Exists(logsDirectory))
				{
					Directory.Delete(logsDirectory, true);
					wereFilesDeleted = true;
				}

				if (wereFilesDeleted)
					DarkMessageBox.Show(this, "Successfully deleted all log files\n" +
					"and error dumps from the project folder.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
				else
					DarkMessageBox.Show(this, "No log files or error dumps were found.", "Information",
						MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
			catch (Exception ex)
			{
				DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void button_RenameLauncher_Click(object sender, EventArgs e)
		{
			using (FormRenameLauncher form = new FormRenameLauncher(_ide))
				form.ShowDialog(this);

			textBox_LauncherName.Text = Path.GetFileName(_ide.Project.LaunchFilePath);
		}
	}
}
