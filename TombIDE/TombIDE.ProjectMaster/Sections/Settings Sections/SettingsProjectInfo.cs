using DarkUI.Forms;
using System;
using System.IO;
using System.Windows.Forms;
using TombIDE.Shared;
using TombLib.Utils;

namespace TombIDE.ProjectMaster
{
	public partial class SettingsProjectInfo : UserControl
	{
		private IDE _ide;

		public SettingsProjectInfo()
		{
			InitializeComponent();
		}

		public void Initialize(IDE ide)
		{
			_ide = ide;

			checkBox_FullPaths.Checked = _ide.Configuration.ViewFullFolderPaths;

			UpdateProjectInfo();
		}

		private void UpdateProjectInfo()
		{
			textBox_ProjectName.Text = _ide.Project.Name;
			textBox_ProjectPath.Text = _ide.Project.ProjectPath;

			if (checkBox_FullPaths.Checked)
			{
				textBox_ScriptPath.Text = _ide.Project.ScriptPath;
				textBox_LevelsPath.Text = _ide.Project.LevelsPath;
			}
			else
			{
				textBox_ScriptPath.Text = _ide.Project.ScriptPath.Replace(_ide.Project.ProjectPath, "$(ProjectDirectory)");
				textBox_LevelsPath.Text = _ide.Project.LevelsPath.Replace(_ide.Project.ProjectPath, "$(ProjectDirectory)");
			}
		}

		private void checkBox_FullPaths_CheckedChanged(object sender, EventArgs e)
		{
			_ide.Configuration.ViewFullFolderPaths = checkBox_FullPaths.Checked;
			_ide.Configuration.Save();

			UpdateProjectInfo();
		}

		private void button_OpenProjectFolder_Click(object sender, EventArgs e) =>
			SharedMethods.OpenFolderInExplorer(_ide.Project.ProjectPath);

		private void button_OpenScriptFolder_Click(object sender, EventArgs e) =>
			SharedMethods.OpenFolderInExplorer(_ide.Project.ScriptPath);

		private void button_OpenLevelsFolder_Click(object sender, EventArgs e) =>
			SharedMethods.OpenFolderInExplorer(_ide.Project.LevelsPath);

		private void button_ChangeScriptPath_Click(object sender, EventArgs e)
		{
			BrowseFolderDialog dialog = new BrowseFolderDialog
			{
				Title = "Choose a new /Script/ folder for the project"
			};

			if (dialog.ShowDialog(this) == DialogResult.OK)
			{
				try
				{
					if (!IsScriptFolderValid(new DirectoryInfo(dialog.Folder)))
						throw new ArgumentException("Selected /Script/ folder does not contain a Script.txt file.");

					_ide.ChangeScriptFolder(dialog.Folder);
				}
				catch (Exception ex)
				{
					DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
		}

		private void button_ChangeLevelsPath_Click(object sender, EventArgs e)
		{
			BrowseFolderDialog dialog = new BrowseFolderDialog
			{
				Title = "Choose a new /Levels/ folder for the project"
			};

			if (dialog.ShowDialog(this) == DialogResult.OK)
				_ide.ChangeLevelsFolder(dialog.Folder);
		}

		private bool IsScriptFolderValid(DirectoryInfo directory)
		{
			foreach (FileInfo file in directory.GetFiles("*.txt", SearchOption.TopDirectoryOnly))
			{
				if (file.Name.ToLower() == "script.txt")
					return true;
			}

			return false;
		}
	}
}
