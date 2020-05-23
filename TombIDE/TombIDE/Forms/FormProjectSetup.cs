using DarkUI.Forms;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows.Forms;
using TombIDE.Shared;
using TombIDE.Shared.SharedClasses;
using TombLib.LevelData;
using TombLib.Utils;

namespace TombIDE
{
	public partial class FormProjectSetup : DarkForm
	{
		public Project CreatedProject { get; private set; }

		#region Initialization

		public FormProjectSetup()
		{
			InitializeComponent();

			comboBox_EngineType.SelectedIndex = 0;
		}

		#endregion Initialization

		#region Events

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);

			textBox_ProjectName.Text = "New Project";
			textBox_ProjectName.SelectAll();
		}

		private void button_Help_Click(object sender, EventArgs e)
		{
			using (FormEngineHelp form = new FormEngineHelp(Cursor.Position))
				form.ShowDialog(this);
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
			using (BrowseFolderDialog dialog = new BrowseFolderDialog())
			{
				dialog.Title = "Choose where you want to install your project";

				if (dialog.ShowDialog(this) == DialogResult.OK)
					textBox_ProjectPath.Text = Path.Combine(dialog.Folder, PathHelper.RemoveIllegalPathSymbols(textBox_ProjectName.Text.Trim()));
			}
		}

		private void button_BrowseScript_Click(object sender, EventArgs e)
		{
			using (BrowseFolderDialog dialog = new BrowseFolderDialog())
			{
				dialog.Title = "Choose a custom /Script/ folder for your project";

				if (dialog.ShowDialog(this) == DialogResult.OK)
					textBox_ScriptPath.Text = dialog.Folder;
			}
		}

		private void button_BrowseLevels_Click(object sender, EventArgs e)
		{
			using (BrowseFolderDialog dialog = new BrowseFolderDialog())
			{
				dialog.Title = "Choose a custom /Levels/ folder for your project";

				if (dialog.ShowDialog(this) == DialogResult.OK)
					textBox_LevelsPath.Text = dialog.Folder;
			}
		}

		private void button_Create_Click(object sender, EventArgs e)
		{
			button_Create.Text = "Installing...";
			button_Create.Enabled = false;

			try
			{
				string projectName = PathHelper.RemoveIllegalPathSymbols(textBox_ProjectName.Text.Trim());

				if (string.IsNullOrWhiteSpace(projectName))
					throw new ArgumentException("You must enter a valid name for your project.");

				if (projectName.ToLower() == "engine") // Safety
					throw new ArgumentException("Illegal project name.");

				if (string.IsNullOrWhiteSpace(textBox_ProjectPath.Text))
					throw new ArgumentException("You must select a folder where you want to install your project.");

				if (radio_Script_02.Checked && string.IsNullOrWhiteSpace(textBox_ScriptPath.Text))
					throw new ArgumentException("You must specify the custom /Script/ folder path.");

				if (radio_Levels_02.Checked && string.IsNullOrWhiteSpace(textBox_LevelsPath.Text))
					throw new ArgumentException("You must specify the custom /Levels/ folder path.");

				if (ProjectChecker.IsProjectNameDuplicate(projectName))
					throw new ArgumentException("A project with the same name already exists on the list.");

				if (comboBox_EngineType.SelectedIndex == 0)
					throw new ArgumentException("You must specify the engine type of the project.");

				string projectPath = textBox_ProjectPath.Text.Trim();
				string enginePath = Path.Combine(projectPath, "Engine");
				string scriptPath = radio_Script_01.Checked ? Path.Combine(projectPath, "Script") : textBox_ScriptPath.Text.Trim();
				string levelsPath = radio_Levels_01.Checked ? Path.Combine(projectPath, "Levels") : textBox_LevelsPath.Text.Trim();

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
				Project createdProject = CreateNewProject(projectName, projectPath, enginePath, scriptPath, levelsPath);

				// Install the game files into the specified projectPath folder
				InstallEngine(createdProject);

				DarkMessageBox.Show(this, "Project installation finished successfully.", "Success",
					MessageBoxButtons.OK, MessageBoxIcon.Information);

				// // // // // // // //
				CreatedProject = createdProject;
				// // // // // // // //
			}
			catch (Exception ex)
			{
				DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

				button_Create.Text = "Create Project";
				button_Create.Enabled = true;

				DialogResult = DialogResult.None;
			}
		}

		#endregion Events

		#region Methods

		private Project CreateNewProject(string projectName, string projectPath, string enginePath, string scriptPath, string levelsPath)
		{
			TRVersion.Game gameVersion = 0;

			switch (comboBox_EngineType.SelectedIndex)
			{
				case 1:
					gameVersion = TRVersion.Game.TRNG;
					break;

				case 2:
					gameVersion = TRVersion.Game.TRNG;
					break;
			}

			string launchFilePath = Path.Combine(projectPath, "launch.exe");

			return new Project
			{
				Name = projectName,
				GameVersion = gameVersion,
				DefaultLanguage = GameLanguage.English,
				LaunchFilePath = launchFilePath,
				ProjectPath = projectPath,
				EnginePath = enginePath,
				ScriptPath = scriptPath,
				LevelsPath = levelsPath
			};
		}

		private void InstallEngine(Project project)
		{
			string engineBasePath = DefaultPaths.GetEngineTemplatesPath();

			switch (comboBox_EngineType.SelectedIndex)
			{
				case 1:
					engineBasePath = Path.Combine(engineBasePath, "TRNG.zip");
					break;

				case 2:
					engineBasePath = Path.Combine(engineBasePath, "TRNG + FLEP.zip");
					break;
			}

			string sharedArchivePath = Path.Combine(DefaultPaths.GetSharedTemplatesPath(project.GameVersion), "Shared.zip");

			// Extract the engine base into the ProjectPath folder
			using (ZipArchive engineArchive = new ZipArchive(File.OpenRead(engineBasePath)))
			{
				using (ZipArchive sharedArchive = new ZipArchive(File.OpenRead(sharedArchivePath)))
				{
					progressBar.Maximum = engineArchive.Entries.Count + sharedArchive.Entries.Count + 1;

					foreach (ZipArchiveEntry entry in engineArchive.Entries)
					{
						if (entry.FullName.EndsWith("/"))
							Directory.CreateDirectory(Path.Combine(project.ProjectPath, entry.FullName));
						else
							entry.ExtractToFile(Path.Combine(project.ProjectPath, entry.FullName));

						progressBar.Increment(1);
					}

					foreach (ZipArchiveEntry entry in sharedArchive.Entries)
					{
						if (entry.FullName.EndsWith("/"))
							Directory.CreateDirectory(Path.Combine(project.ProjectPath, entry.FullName));
						else
							entry.ExtractToFile(Path.Combine(project.ProjectPath, entry.FullName));

						progressBar.Increment(1);
					}
				}
			}

			// Save the .trproj file
			project.Save(); // .trproj = .xml but .trproj can be opened with TombIDE

			progressBar.Increment(1); // 100%
		}

		#endregion Methods
	}
}
