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

			// Get the "ProjectPath" folder name
			string currentDirectory = Path.GetDirectoryName(exeFilePath);
			string prevDirectory = Path.GetDirectoryName(currentDirectory);

			if (Path.GetFileName(currentDirectory).ToLower() == "engine")
				textBox_ProjectName.Text = Path.GetFileName(prevDirectory);
			else
				textBox_ProjectName.Text = Path.GetFileName(currentDirectory);

			// Fill the text boxes
			FillScriptPathTextBox(exeFilePath);
			FillLevelsPathTextBox(exeFilePath);

			button_Import.Text = "Import " + GetGameVersion(exeFilePath) + " Project";
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);

			if (textBox_ProjectName.Text.ToLower() == "engine") // This will ONLY happen when the user intentionally tries to break the software
			{
				DarkMessageBox.Show(this, "LOL you did that on purpose, didn't you? Your project directory cannot be named \"Engine\",\n" +
					"because it will cause many conflicts inside the software. Please rename the directory before importing.", "Why?",
					MessageBoxButtons.OK, MessageBoxIcon.Error);

				DialogResult = DialogResult.Cancel;
				return;
			}
		}

		private void FillScriptPathTextBox(string exeFilePath)
		{
			// Find the /Script/ directory

			string currentDirectory = Path.GetDirectoryName(exeFilePath);
			string prevDirectory = Path.GetDirectoryName(currentDirectory);

			foreach (string directory in Directory.GetDirectories(currentDirectory))
			{
				if (Path.GetFileName(directory).ToLower() == "script")
				{
					// Check if a script.txt file exists in the /Script/ directory, if not, then leave the textBox empty
					if (File.Exists(Path.Combine(directory, "script.txt")))
						textBox_ScriptPath.Text = directory;

					break;
				}
			}

			if (string.IsNullOrEmpty(textBox_ScriptPath.Text))
			{
				// Check the previous folder too, because the user might be using the new project format
				foreach (string directory in Directory.GetDirectories(prevDirectory))
				{
					if (Path.GetFileName(directory).ToLower() == "script")
					{
						// Check if a script.txt file exists in the /Script/ directory, if not, then leave the textBox empty
						if (File.Exists(Path.Combine(directory, "script.txt")))
							textBox_ScriptPath.Text = directory;

						break;
					}
				}
			}
		}

		private void FillLevelsPathTextBox(string exeFilePath)
		{
			string levelsPath = string.Empty;
			string mapsPath = string.Empty; // Legacy solution

			string currentDirectory = Path.GetDirectoryName(exeFilePath);
			string prevDirectory = Path.GetDirectoryName(currentDirectory);

			foreach (string directory in Directory.GetDirectories(currentDirectory))
			{
				if (Path.GetFileName(directory).ToLower() == "levels")
					levelsPath = directory;
				else if (Path.GetFileName(directory).ToLower() == "maps")
					mapsPath = directory;
			}

			if (string.IsNullOrEmpty(levelsPath))
			{
				// Check the previous folder too, because the user might be using the new project format
				foreach (string directory in Directory.GetDirectories(prevDirectory))
				{
					if (Path.GetFileName(directory).ToLower() == "levels")
						levelsPath = directory;
				}
			}

			if (Directory.Exists(levelsPath) && !Directory.Exists(mapsPath))
				textBox_LevelsPath.Text = levelsPath;
			else if (!Directory.Exists(levelsPath) && Directory.Exists(mapsPath))
				textBox_LevelsPath.Text = mapsPath;
			else if (Directory.Exists(levelsPath) && Directory.Exists(mapsPath))
				textBox_LevelsPath.Text = levelsPath; // Prefer the new solution
			else // Both directories don't exist
			{
				textBox_LevelsPath.Text = Path.Combine(Path.GetDirectoryName(exeFilePath), "Levels");

				// Highlight the textBox and add a toolTip for it to indicate that the pre-set path is just a suggestion
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
			using (BrowseFolderDialog dialog = new BrowseFolderDialog())
			{
				dialog.Title = "Choose an existing /Script/ folder for the project";

				if (dialog.ShowDialog(this) == DialogResult.OK)
					textBox_ScriptPath.Text = dialog.Folder;
			}
		}

		private void button_BrowseLevels_Click(object sender, EventArgs e)
		{
			using (BrowseFolderDialog dialog = new BrowseFolderDialog())
			{
				dialog.Title = "Choose a /Levels/ folder for the project";

				if (dialog.ShowDialog(this) == DialogResult.OK)
					textBox_LevelsPath.Text = dialog.Folder;
			}
		}

		private void button_Import_Click(object sender, EventArgs e)
		{
			button_Import.Enabled = false;

			try
			{
				string projectName = SharedMethods.RemoveIllegalPathSymbols(textBox_ProjectName.Text.Trim());

				if (string.IsNullOrWhiteSpace(projectName))
					throw new ArgumentException("You must enter a valid name for the project.");

				if (projectName.ToLower() == "engine") // Safety
					throw new ArgumentException("Invalid project name.");

				if (string.IsNullOrWhiteSpace(textBox_ScriptPath.Text))
					throw new ArgumentException("You must specify the /Script/ folder path of the project.");

				if (string.IsNullOrWhiteSpace(textBox_LevelsPath.Text))
					throw new ArgumentException("You must specify the /Levels/ folder path for the project.");

				// Check for name duplicates
				foreach (Project project in _ide.AvailableProjects)
				{
					if (project.Name.ToLower() == projectName.ToLower())
						throw new ArgumentException("A project with the same name already exists on the list.");
				}

				GameVersion gameVersion = GetGameVersion(textBox_ExePath.Text);

				string projectPath = string.Empty;

				string currentDirectory = Path.GetDirectoryName(textBox_ExePath.Text);
				string prevDirectory = Path.GetDirectoryName(currentDirectory);

				if (Path.GetFileName(currentDirectory).ToLower() == "engine")
					projectPath = prevDirectory;
				else
					projectPath = currentDirectory;

				string enginePath = Path.GetDirectoryName(textBox_ExePath.Text);
				string scriptPath = textBox_ScriptPath.Text.Trim();
				string levelsPath = textBox_LevelsPath.Text.Trim();

				// Check if a script.txt file exists in the specified /Script/ folder
				if (!File.Exists(Path.Combine(scriptPath, "script.txt")))
					throw new ArgumentException("Selected /Script/ folder does not contain a Script.txt file.");

				if (!Directory.Exists(levelsPath))
					Directory.CreateDirectory(levelsPath);

				Project importedProject = new Project
				{
					Name = projectName,
					GameVersion = gameVersion,
					ProjectPath = projectPath,
					EnginePath = enginePath,
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

			if (gameVersion == GameVersion.TR4 && File.Exists(Path.Combine(Path.GetDirectoryName(exeFilePath), "tomb_nextgeneration.dll")))
				gameVersion = GameVersion.TRNG;

			// TODO: Add TR5Main detection

			return gameVersion;
		}

		#endregion Methods
	}
}
