using DarkUI.Forms;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using TombIDE.Shared;
using TombLib.LevelData;
using TombLib.Projects;
using TombLib.Utils;

namespace TombIDE
{
	public partial class FormImportProject : DarkForm
	{
		private IDE _ide;

		#region Initialization

		public FormImportProject(IDE ide, string exeFilePath)
		{
			_ide = ide;

			InitializeComponent();

			// Setup some information
			textBox_ExePath.BackColor = Color.FromArgb(48, 48, 48); // Mark as uneditable
			textBox_ExePath.Text = exeFilePath;

			textBox_ProjectName.Text = Path.GetFileName(Path.GetDirectoryName(exeFilePath)); // Folder name of the specified .exe file

			FillScriptPathTextBox(exeFilePath);
			FillLevelsPathTextBox(exeFilePath);

			button_Import.Text = "Import " + GetGameVersion(exeFilePath) + " Project";
		}

		private void FillScriptPathTextBox(string exeFilePath)
		{
			DirectoryInfo directoryInfo = new DirectoryInfo(Path.GetDirectoryName(exeFilePath));

			// Find the /Script/ folder
			foreach (DirectoryInfo directory in directoryInfo.GetDirectories())
			{
				if (directory.Name.ToLower() == "script")
				{
					// Check if the /Script/ folder contains a script.txt file
					if (IsScriptFolderValid(directory))
						textBox_ScriptPath.Text = directory.FullName;
				}
			}
		}

		private void FillLevelsPathTextBox(string exeFilePath)
		{
			string suggestedLevelsPath = Path.Combine(Path.GetDirectoryName(exeFilePath), "Levels");

			textBox_LevelsPath.Text = suggestedLevelsPath;

			// Check if the /Levels/ folder already exists for the project, if not, then highlight the textBox and add a toolTip for it
			// to indicate that the pre-set path is just a suggestion
			if (!Directory.Exists(suggestedLevelsPath))
			{
				textBox_LevelsPath.BackColor = Color.FromArgb(48, 96, 64);
				toolTip.SetToolTip(textBox_LevelsPath, "Suggested path");
			}
		}

		#endregion Initialization

		#region Events

		private void textBox_LevelsPath_TextChanged(object sender, EventArgs e)
		{
			// Reset the suggestion stuff
			if (textBox_LevelsPath.BackColor == Color.FromArgb(48, 96, 64))
			{
				textBox_LevelsPath.BackColor = Color.FromArgb(69, 73, 74); // Default DarkTextBox color
				toolTip.RemoveAll();
			}
		}

		private void button_BrowseScript_Click(object sender, EventArgs e)
		{
			BrowseFolderDialog dialog = new BrowseFolderDialog
			{
				Title = "Choose an existing /Script/ folder for the project"
			};

			if (dialog.ShowDialog(this) == DialogResult.OK)
				textBox_ScriptPath.Text = dialog.Folder;
		}

		private void button_BrowseLevels_Click(object sender, EventArgs e)
		{
			BrowseFolderDialog dialog = new BrowseFolderDialog
			{
				Title = "Choose a /Levels/ folder for the project"
			};

			if (dialog.ShowDialog(this) == DialogResult.OK)
				textBox_LevelsPath.Text = dialog.Folder;
		}

		private void button_Import_Click(object sender, EventArgs e)
		{
			button_Import.Enabled = false;

			try
			{
				if (string.IsNullOrWhiteSpace(textBox_ProjectName.Text))
					throw new ArgumentException("You must enter a name for the project.");

				if (string.IsNullOrWhiteSpace(textBox_ScriptPath.Text))
					throw new ArgumentException("You must specify the /Script/ folder path of the project.");

				if (string.IsNullOrWhiteSpace(textBox_LevelsPath.Text))
					throw new ArgumentException("You must specify the /Levels/ folder path for the project.");

				string projectName = textBox_ProjectName.Text.Trim();

				// Check for name duplicates
				foreach (Project project in _ide.AvailableProjects)
				{
					if (project.Name == projectName)
						throw new ArgumentException("A project with the same name already exists on the list.");
				}

				GameVersion gameVersion = GetGameVersion(textBox_ExePath.Text);

				string projectPath = Path.GetDirectoryName(textBox_ExePath.Text);
				string scriptPath = textBox_ScriptPath.Text.Trim();
				string levelsPath = textBox_LevelsPath.Text.Trim();

				// Check if the specified paths are not just random symbols
				if (Uri.IsWellFormedUriString(scriptPath, UriKind.RelativeOrAbsolute)
					|| Uri.IsWellFormedUriString(levelsPath, UriKind.RelativeOrAbsolute))
					throw new ArgumentException("One of the specified paths is invalid or not formatted correclty.");

				// Check if the specified /Script/ folder contains a script.txt file
				if (!IsScriptFolderValid(new DirectoryInfo(scriptPath)))
					throw new ArgumentException("Selected /Script/ folder does not contain a Script.txt file.");

				if (!Directory.Exists(levelsPath))
					Directory.CreateDirectory(levelsPath);

				Project importedProject = new Project
				{
					Name = projectName,
					GameVersion = gameVersion,
					ProjectPath = projectPath,
					ScriptPath = scriptPath,
					LevelsPath = levelsPath
				};

				// Create the .trproj file
				XmlHandling.SaveTRPROJ(importedProject); // .trproj = .xml but .trproj can be opened with TombIDE

				_ide.AddProjectToList(importedProject);
			}
			catch (Exception ex)
			{
				DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				button_Import.Enabled = true;
				DialogResult = DialogResult.None;
			}
		}

		#endregion Events

		#region Methods

		private GameVersion GetGameVersion(string exeFilePath)
		{
			GameVersion gameVersion = 0;

			switch (Path.GetFileName(exeFilePath).ToLower())
			{
				case "tomb4.exe":
					gameVersion = GameVersion.TR4;
					break;

				case "pctomb5.exe":
					gameVersion = GameVersion.TR5;
					break;
			}

			DirectoryInfo directoryInfo = new DirectoryInfo(Path.GetDirectoryName(exeFilePath));

			foreach (FileInfo file in directoryInfo.GetFiles("*.dll", SearchOption.TopDirectoryOnly))
			{
				if (file.Name.ToLower() == "tomb_nextgeneration.dll")
					gameVersion = GameVersion.TRNG;
			}

			// TODO: Add TR5Main detection

			return gameVersion;
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

		#endregion Methods
	}
}
