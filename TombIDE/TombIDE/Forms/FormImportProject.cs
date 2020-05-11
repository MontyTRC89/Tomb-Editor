using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using TombIDE.Shared;
using TombIDE.Shared.SharedClasses;
using TombLib.LevelData;
using TombLib.Utils;

namespace TombIDE
{
	public partial class FormImportProject : DarkForm
	{
		public Project ImportedProject { get; private set; }

		#region Initialization

		public FormImportProject(string gameExeFilePath, string launcherFilePath)
		{
			InitializeComponent();

			/* Setup some information */

			textBox_ExePath.Text = gameExeFilePath;
			textBox_LauncherPath.Text = launcherFilePath;

			textBox_ProjectName.Text = Path.GetFileName(GetProjectDirectory(gameExeFilePath));

			FillScriptPathTextBoxIfPossible(gameExeFilePath);
			FillLevelsPathTextBoxIfPossible(gameExeFilePath);

			button_Import.Text = "Import " + GetGameVersion(gameExeFilePath) + " Project";
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

			textBox_ProjectName.Focus();
		}

		private void FillScriptPathTextBoxIfPossible(string exeFilePath)
		{
			/* Find the /Script/ directory */

			string gameExeDirectory = Path.GetDirectoryName(exeFilePath);

			// Check if the project is using the new format
			if (Path.GetFileName(gameExeDirectory).ToLower() == "engine")
			{
				string parentDirectory = Path.GetDirectoryName(gameExeDirectory);

				foreach (string directory in Directory.GetDirectories(parentDirectory))
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

			if (string.IsNullOrEmpty(textBox_ScriptPath.Text))
			{
				// Method reaches this point if the project is using the old format or no valid /Script/ folder was found in the parent directory

				foreach (string directory in Directory.GetDirectories(gameExeDirectory))
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

		private void FillLevelsPathTextBoxIfPossible(string exeFilePath)
		{
			/* Find the /Levels/ (or /Maps/) directory */

			string gameExeDirectory = Path.GetDirectoryName(exeFilePath);

			// Check if the project is using the new format
			if (Path.GetFileName(gameExeDirectory).ToLower() == "engine")
			{
				string parentDirectory = Path.GetDirectoryName(gameExeDirectory);

				foreach (string directory in Directory.GetDirectories(parentDirectory))
				{
					if (Path.GetFileName(directory).ToLower() == "levels")
					{
						textBox_LevelsPath.Text = directory;
						break;
					}
				}
			}

			if (string.IsNullOrEmpty(textBox_LevelsPath.Text))
			{
				// Method reaches this point if the project is using the old format or no /Levels/ folder was found in the parent directory

				string levelsPath = string.Empty;
				string mapsPath = string.Empty; // Legacy solution

				foreach (string directory in Directory.GetDirectories(gameExeDirectory))
				{
					if (Path.GetFileName(directory).ToLower() == "levels")
						levelsPath = directory;
					else if (Path.GetFileName(directory).ToLower() == "maps")
						mapsPath = directory;
				}

				if (Directory.Exists(levelsPath) && !Directory.Exists(mapsPath))
					textBox_LevelsPath.Text = levelsPath;
				else if (!Directory.Exists(levelsPath) && Directory.Exists(mapsPath))
					textBox_LevelsPath.Text = mapsPath;
				else if (Directory.Exists(levelsPath) && Directory.Exists(mapsPath))
					textBox_LevelsPath.Text = levelsPath; // Prefer the new solution
			}

			if (string.IsNullOrEmpty(textBox_LevelsPath.Text)) // If still no /Levels/ or /Maps/ folder was found
			{
				// Suggest a path
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
			// Reset the suggestion indication
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
				dialog.Title = "Select an existing /Script/ folder for the project";

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
				if (!File.Exists(textBox_ExePath.Text))
					throw new FileNotFoundException("The game's .exe file doesn't exist anymore.");

				if (!File.Exists(textBox_LauncherPath.Text))
					throw new FileNotFoundException("The project's launcher file doesn't exist anymore.");

				string projectName = PathHelper.RemoveIllegalPathSymbols(textBox_ProjectName.Text.Trim());

				if (string.IsNullOrWhiteSpace(projectName))
					throw new ArgumentException("You must enter a valid name for the project.");

				if (projectName.ToLower() == "engine") // Safety
					throw new ArgumentException("Illegal project name.");

				if (string.IsNullOrWhiteSpace(textBox_ScriptPath.Text))
					throw new ArgumentException("You must specify the /Script/ folder path of the project.");

				if (string.IsNullOrWhiteSpace(textBox_LevelsPath.Text))
					throw new ArgumentException("You must specify the /Levels/ folder path for the project.");

				if (ProjectChecker.IsProjectNameDuplicate(projectName))
					throw new ArgumentException("A project with the same name already exists on the list.");

				TRVersion.Game gameVersion = GetGameVersion(textBox_ExePath.Text);

				string launcherFilePath = textBox_LauncherPath.Text;

				string projectPath = GetProjectDirectory(textBox_ExePath.Text);
				string enginePath = Path.GetDirectoryName(textBox_ExePath.Text);
				string scriptPath = textBox_ScriptPath.Text.Trim();
				string levelsPath = textBox_LevelsPath.Text.Trim();

				// Check if a script.txt file exists in the specified /Script/ folder
				if (!File.Exists(Path.Combine(scriptPath, "script.txt")))
					throw new FileNotFoundException("Selected /Script/ folder does not contain a Script.txt file.");

				// Check if the levelsPath directory exists, if so, check if it contains any valid .prj2 files
				if (Directory.Exists(levelsPath))
				{
					// Check if the directory contains non-backup .prj2 files
					List<string> prj2Files = LevelHandling.GetValidPrj2FilesFromDirectory(levelsPath);

					if (prj2Files.Count > 0)
					{
						DialogResult result = DarkMessageBox.Show(this,
							"TombIDE will change the \"Game\" settings of all the .prj2 files\n" +
							"in the specified /Levels/ folder to match the imported project settings.\n" +
							"Would you like to to create a backup of the folder first?", "Create backup?",
							MessageBoxButtons.YesNo, MessageBoxIcon.Question);

						if (result == DialogResult.Yes)
							CreateLevelsBackup(levelsPath);
					}
				}
				else
					Directory.CreateDirectory(levelsPath);

				Project importedProject = new Project
				{
					Name = projectName,
					GameVersion = gameVersion,
					DefaultLanguage = GameLanguage.English,
					LaunchFilePath = launcherFilePath,
					ProjectPath = projectPath,
					EnginePath = enginePath,
					ScriptPath = scriptPath,
					LevelsPath = levelsPath
				};

				// Create the .trproj file
				importedProject.Save(); // .trproj = .xml but .trproj can be opened with TombIDE

				// // // // // // // //
				ImportedProject = importedProject;
				// // // // // // // //
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

		private void CreateLevelsBackup(string levelsPath)
		{
			try
			{
				// Check if there are any existing backup folders of the /Levels/ folder in the parent directory of the levelsPath
				string[] existingBackupFolders = Directory.GetDirectories(
					Path.GetDirectoryName(levelsPath), Path.GetFileName(levelsPath) + "_BACKUP*", SearchOption.TopDirectoryOnly);

				string newBackupFolderPath;

				// Generate a name for the new backup folder

				if (existingBackupFolders.Length == 0)
					newBackupFolderPath = levelsPath + "_BACKUP";
				else if (existingBackupFolders.Length == 1)
					newBackupFolderPath = levelsPath + "_BACKUP_2";
				else
				{
					List<int> existingNumbers = new List<int>();

					foreach (string existingBackupFolder in existingBackupFolders)
					{
						int result;

						if (int.TryParse(Path.GetFileNameWithoutExtension(existingBackupFolder).Split('_')[2], out result))
							existingNumbers.Add(result);
					}

					int nextFolderNumber = existingNumbers.Max() + 1;

					newBackupFolderPath = levelsPath + "_BACKUP_" + nextFolderNumber;
				}

				Directory.CreateDirectory(newBackupFolderPath);

				// Create all of the subdirectories
				foreach (string directory in Directory.GetDirectories(levelsPath, "*", SearchOption.AllDirectories))
					Directory.CreateDirectory(directory.Replace(levelsPath, newBackupFolderPath));

				// Copy all the .prj2 files
				foreach (string file in Directory.GetFiles(levelsPath, "*.prj2", SearchOption.AllDirectories))
				{
					if (!ProjectLevel.IsBackupFile(Path.GetFileName(file)))
						File.Copy(file, file.Replace(levelsPath, newBackupFolderPath));
				}

				DarkMessageBox.Show(this, "Backup successfully created in:\n" + newBackupFolderPath, "Information",
					MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
			catch (Exception ex)
			{
				DarkMessageBox.Show(this, "Failed to create backup.\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private string GetProjectDirectory(string gameExeFilePath)
		{
			string gameExeDirectory = Path.GetDirectoryName(gameExeFilePath);

			// Check if the project is using the new format
			if (Path.GetFileName(gameExeDirectory).ToLower() == "engine")
			{
				string parentDirectory = Path.GetDirectoryName(gameExeDirectory);
				return parentDirectory;
			}
			else
				return gameExeDirectory;
		}

		private TRVersion.Game GetGameVersion(string exeFilePath)
		{
			TRVersion.Game gameVersion = 0;

			switch (Path.GetFileName(exeFilePath).ToLower())
			{
				case "tomb4.exe":
					gameVersion = TRVersion.Game.TR4;
					break;

				case "pctomb5.exe":
					gameVersion = TRVersion.Game.TR5Main;
					break;
			}

			if (gameVersion == TRVersion.Game.TR4 && File.Exists(Path.Combine(Path.GetDirectoryName(exeFilePath), "tomb_nextgeneration.dll")))
				gameVersion = TRVersion.Game.TRNG;

			return gameVersion;
		}

		#endregion Methods
	}
}
