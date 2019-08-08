using DarkUI.Forms;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows.Forms;
using TombIDE.Shared;
using TombLib.LevelData;
using TombLib.Projects;
using TombLib.Utils;

namespace TombIDE
{
	public partial class FormProjectSetup : DarkForm
	{
		private IDE _ide;

		public FormProjectSetup(IDE ide)
		{
			_ide = ide;

			InitializeComponent();

			comboBox_EngineType.SelectedItem = "TRNG"; // TRNG should be the default selection
		}

		private void radio_Script_01_CheckedChanged(object sender, EventArgs e)
		{
			textBox_ScriptPath.Enabled = false;
			button_BrowseScript.Enabled = false;
		}

		private void radio_Script_02_CheckedChanged(object sender, EventArgs e)
		{
			textBox_ScriptPath.Enabled = true;
			button_BrowseScript.Enabled = true;
		}

		private void radio_Levels_01_CheckedChanged(object sender, EventArgs e)
		{
			textBox_LevelsPath.Enabled = false;
			button_BrowseLevels.Enabled = false;
		}

		private void radio_Level_02_CheckedChanged(object sender, EventArgs e)
		{
			textBox_LevelsPath.Enabled = true;
			button_BrowseLevels.Enabled = true;
		}

		private void button_BrowseProject_Click(object sender, EventArgs e)
		{
			BrowseFolderDialog dialog = new BrowseFolderDialog
			{
				Title = "Choose where you want to install your project"
			};

			if (dialog.ShowDialog(this) == DialogResult.OK)
				textBox_ProjectPath.Text = Path.Combine(dialog.Folder, textBox_ProjectName.Text);
		}

		private void button_BrowseScript_Click(object sender, EventArgs e)
		{
			BrowseFolderDialog dialog = new BrowseFolderDialog
			{
				Title = "Choose a custom /Script/ folder for your project"
			};

			if (dialog.ShowDialog(this) == DialogResult.OK)
				textBox_ScriptPath.Text = dialog.Folder;
		}

		private void button_BrowseLevels_Click(object sender, EventArgs e)
		{
			BrowseFolderDialog dialog = new BrowseFolderDialog
			{
				Title = "Choose a custom /Levels/ folder for your project"
			};

			if (dialog.ShowDialog(this) == DialogResult.OK)
				textBox_LevelsPath.Text = dialog.Folder;
		}

		private void button_Create_Click(object sender, EventArgs e)
		{
			if (comboBox_EngineType.SelectedItem.ToString().Contains("TR5")) // TEMPORARY
			{
				DialogResult = DialogResult.None;
				return;
			}

			button_Create.Text = "Installing...";
			button_Create.Enabled = false;

			try
			{
				if (string.IsNullOrWhiteSpace(textBox_ProjectName.Text))
					throw new ArgumentException("You must enter a name for your project.");

				if (string.IsNullOrWhiteSpace(textBox_ProjectPath.Text))
					throw new ArgumentException("You must select a folder where you want to install your project.");

				if (radio_Script_02.Checked && string.IsNullOrWhiteSpace(textBox_ScriptPath.Text))
					throw new ArgumentException("You must specify the custom /Script/ folder path.");

				if (radio_Levels_02.Checked && string.IsNullOrWhiteSpace(textBox_LevelsPath.Text))
					throw new ArgumentException("You must specify the custom /Levels/ folder path.");

				string projectName = textBox_ProjectName.Text.Trim();

				// Check for name duplicates
				foreach (Project project in _ide.AvailableProjects)
				{
					if (project.Name == projectName)
						throw new ArgumentException("A project with the same name already exists on the list.");
				}

				string projectPath = textBox_ProjectPath.Text.Trim();

				string scriptPath = radio_Script_01.Checked ? Path.Combine(projectPath, "Script") : textBox_ScriptPath.Text.Trim();
				string levelsPath = radio_Levels_01.Checked ? Path.Combine(projectPath, "Levels") : textBox_LevelsPath.Text.Trim();

				// Check if the specified paths are not just random symbols
				if (Uri.IsWellFormedUriString(projectPath, UriKind.RelativeOrAbsolute)
					|| Uri.IsWellFormedUriString(scriptPath, UriKind.RelativeOrAbsolute)
					|| Uri.IsWellFormedUriString(levelsPath, UriKind.RelativeOrAbsolute))
					throw new ArgumentException("One of the specified paths is invalid or not formatted correclty.");

				if (!Directory.Exists(projectPath))
					Directory.CreateDirectory(projectPath);

				if (Directory.EnumerateFileSystemEntries(projectPath).ToArray().Length > 0)
					throw new ArgumentException("Selected project folder is not empty.");

				if (!Directory.Exists(scriptPath))
					Directory.CreateDirectory(scriptPath);

				if (Directory.EnumerateFileSystemEntries(scriptPath).ToArray().Length > 0)
					throw new ArgumentException("Selected /Script/ folder is not empty.");

				if (!Directory.Exists(levelsPath))
					Directory.CreateDirectory(levelsPath);

				if (Directory.EnumerateFileSystemEntries(levelsPath).ToArray().Length > 0)
					throw new ArgumentException("Selected /Levels/ folder is not empty.");

				// Create the Project instance
				Project createdProject = CreateNewProject(projectName, projectPath, scriptPath, levelsPath);

				// Install the game files into the specified projectPath folder
				InstallEngine(createdProject);

				DarkMessageBox.Show(this, "Project installation finished successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

				// Trigger the IDE.ProjectAddedEvent
				_ide.AddProjectToList(createdProject);
			}
			catch (Exception ex)
			{
				DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

				button_Create.Text = "Create Project";
				button_Create.Enabled = true;

				DialogResult = DialogResult.None;
			}
		}

		private Project CreateNewProject(string projectName, string projectPath, string scriptPath, string levelsPath)
		{
			GameVersion gameVersion = 0;

			switch (comboBox_EngineType.SelectedItem)
			{
				case "TR4":
					gameVersion = GameVersion.TR4;
					break;

				case "TRNG":
					gameVersion = GameVersion.TRNG;
					break;

				case "TR5":
					gameVersion = GameVersion.TR5;
					break;

				case "TR5Main":
					gameVersion = GameVersion.TR5Main;
					break;
			}

			return new Project
			{
				Name = projectName,
				GameVersion = gameVersion,
				ProjectPath = projectPath,
				ScriptPath = scriptPath,
				LevelsPath = levelsPath
			};
		}

		private void InstallEngine(Project project)
		{
			string engineBasePath = @"Templates\Engines"; // The folder where all engine bases are located

			switch (project.GameVersion)
			{
				case GameVersion.TR4:
					engineBasePath = Path.Combine(engineBasePath, "TR4.zip");
					break;

				case GameVersion.TRNG:
					engineBasePath = Path.Combine(engineBasePath, "TRNG.zip");
					break;

				case GameVersion.TR5:
					engineBasePath = Path.Combine(engineBasePath, "TR5.zip");
					break;

				case GameVersion.TR5Main:
					engineBasePath = Path.Combine(engineBasePath, "TR5Main.zip");
					break;
			}

			// Un-Zip the engine base into the ProjectPath folder
			using (ZipArchive archive = new ZipArchive(File.OpenRead(engineBasePath)))
			{
				progressBar.Maximum = archive.Entries.Count + 2; // +2 because there are 2 more events after this

				foreach (ZipArchiveEntry entry in archive.Entries)
				{
					if (entry.FullName.EndsWith("/"))
						Directory.CreateDirectory(Path.Combine(project.ProjectPath, entry.FullName));
					else
						entry.ExtractToFile(Path.Combine(project.ProjectPath, entry.FullName));

					progressBar.Increment(1);
				}
			}

			// Copy Script templates into the ScriptPath folder
			File.Copy(@"Templates\Script.txt", Path.Combine(project.ScriptPath, "Script.txt"));
			File.Copy(@"Templates\English.txt", Path.Combine(project.ScriptPath, "English.txt"));

			progressBar.Increment(1);

			// Create the .trproj file
			XmlHandling.SaveTRPROJ(project); // .trproj = .xml but .trproj can be opened with TombIDE

			progressBar.Increment(1);
		}
	}
}
