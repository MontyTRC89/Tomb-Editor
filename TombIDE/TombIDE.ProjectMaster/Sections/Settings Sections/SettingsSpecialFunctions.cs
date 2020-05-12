using DarkUI.Forms;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using TombIDE.Shared;
using TombIDE.Shared.SharedClasses;
using TombLib.LevelData;

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

			if (_ide.Project.ProjectPath.ToLower() == _ide.Project.EnginePath.ToLower())
			{
				button_RenameLauncher.Enabled = false;

				textBox_LauncherName.ForeColor = Color.Gray;
				textBox_LauncherName.Text = "Unavailable for legacy projects";
			}
			else
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
			if (!File.Exists(_ide.Project.LaunchFilePath))
			{
				DarkMessageBox.Show(this, "Couldn't find the launcher executable of the project.\n" +
					"Please restart TombIDE to resolve any issues.", "Error",
					MessageBoxButtons.OK, MessageBoxIcon.Error);

				return;
			}

			using (FormRenameLauncher form = new FormRenameLauncher(_ide))
				form.ShowDialog(this);

			textBox_LauncherName.Text = Path.GetFileName(_ide.Project.LaunchFilePath);
		}

		private void button_BatchBuild_Click(object sender, EventArgs e)
		{
			BatchCompileList batchList = new BatchCompileList();

			foreach (ProjectLevel level in _ide.Project.Levels)
			{
				string prj2Path;

				if (level.SpecificFile == "$(LatestFile)")
					prj2Path = Path.Combine(level.FolderPath, level.GetLatestPrj2File());
				else
					prj2Path = Path.Combine(level.FolderPath, level.SpecificFile);

				batchList.Files.Add(prj2Path);
			}

			string batchListFilePath = Path.Combine(Path.GetTempPath(), "tide_batch.xml");
			BatchCompileList.SaveToXml(batchListFilePath, batchList);

			ProcessStartInfo startInfo = new ProcessStartInfo
			{
				FileName = Path.Combine(PathHelper.GetProgramDirectory(), "TombEditor.exe"),
				Arguments = "\"" + batchListFilePath + "\""
			};

			Process.Start(startInfo);
		}
	}
}
