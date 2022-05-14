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

		public string _musicZipFilePath;

		public FileInfo _cdaudioDatFile;
		public FileInfo _cdaudioMp3File;
		public FileInfo _cdaudioWadFile;

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
			textBox_ProjectName.Focus();
			textBox_ProjectName.SelectAll();
		}

		private void button_Help_Click(object sender, EventArgs e)
		{
			var form = new FormEngineHelp(Cursor.Position);
			form.Show(this);
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
			using (var dialog = new BrowseFolderDialog())
			{
				dialog.Title = "Choose where you want to install your project";

				if (dialog.ShowDialog(this) == DialogResult.OK)
					textBox_ProjectPath.Text = Path.Combine(dialog.Folder, PathHelper.RemoveIllegalPathSymbols(textBox_ProjectName.Text.Trim()));
			}
		}

		private void button_BrowseScript_Click(object sender, EventArgs e)
		{
			using (var dialog = new BrowseFolderDialog())
			{
				dialog.Title = "Choose a custom /Script/ folder for your project";

				if (dialog.ShowDialog(this) == DialogResult.OK)
					textBox_ScriptPath.Text = dialog.Folder;
			}
		}

		private void button_BrowseLevels_Click(object sender, EventArgs e)
		{
			using (var dialog = new BrowseFolderDialog())
			{
				dialog.Title = "Choose a custom /Levels/ folder for your project";

				if (dialog.ShowDialog(this) == DialogResult.OK)
					textBox_LevelsPath.Text = dialog.Folder;
			}
		}

		private void button_Next_Click(object sender, EventArgs e)
		{
			try
			{
				string projectName = PathHelper.RemoveIllegalPathSymbols(textBox_ProjectName.Text.Trim());

				if (string.IsNullOrWhiteSpace(projectName))
					throw new ArgumentException("You must enter a valid name for your project.");

				if (projectName.Equals("Engine", StringComparison.OrdinalIgnoreCase)) // Safety
					throw new ArgumentException("Illegal project name.");

				if (string.IsNullOrWhiteSpace(textBox_ProjectPath.Text))
					throw new ArgumentException("You must select a folder where you want to install your project.");

				if (ProjectChecker.IsProjectNameDuplicate(projectName))
					throw new ArgumentException("A project with the same name already exists on the list.");

				if (comboBox_EngineType.SelectedIndex == 0)
					throw new ArgumentException("You must specify the engine type of the project.");

				string projectPath = textBox_ProjectPath.Text.Trim();

				if (Directory.Exists(projectPath) && Directory.EnumerateFileSystemEntries(projectPath).ToArray().Length > 0)
					throw new ArgumentException("Selected project folder is not empty.");

				tableLayoutPanel_Content02.Controls.Clear();

				if (comboBox_EngineType.SelectedIndex == 1)
				{
					tableLayoutPanel_Content02.Controls.Add(panel_LevelsRadioChoice, 0, 0);
					tableLayoutPanel_Content02.Controls.Add(progressBar, 0, 4);
					tableLayoutPanel_Content02.Controls.Add(button_BrowseLevels, 1, 1);
					tableLayoutPanel_Content02.Controls.Add(textBox_LevelsPath, 0, 1);
				}
				else
				{
					tableLayoutPanel_Content02.Controls.Add(panel_LevelsRadioChoice, 0, 2);
					tableLayoutPanel_Content02.Controls.Add(progressBar, 0, 4);
					tableLayoutPanel_Content02.Controls.Add(panel_ScriptRadioChoice, 0, 0);
					tableLayoutPanel_Content02.Controls.Add(button_BrowseLevels, 1, 3);
					tableLayoutPanel_Content02.Controls.Add(textBox_LevelsPath, 0, 3);
					tableLayoutPanel_Content02.Controls.Add(textBox_ScriptPath, 0, 1);
					tableLayoutPanel_Content02.Controls.Add(button_BrowseScript, 1, 1);
				}

				tablessTabControl.SelectTab(1);
			}
			catch (Exception ex)
			{
				DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void button_Back_Click(object sender, EventArgs e)
			=> tablessTabControl.SelectTab(0);

		private void comboBox_EngineType_SelectedIndexChanged(object sender, EventArgs e)
			=> checkBox_IncludeFLEP.Visible = checkBox_IncludeFLEP.Enabled = comboBox_EngineType.SelectedIndex == 5;

		private void button_Create_Click(object sender, EventArgs e)
		{
			button_Create.Enabled = false;

			try
			{
				string projectName = PathHelper.RemoveIllegalPathSymbols(textBox_ProjectName.Text.Trim());

				if (string.IsNullOrWhiteSpace(projectName))
					throw new ArgumentException("You must enter a valid name for your project.");

				if (projectName.Equals("Engine", StringComparison.OrdinalIgnoreCase)) // Safety
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

				if (comboBox_EngineType.SelectedIndex == 1)
					scriptPath = Path.Combine(enginePath, "cfg");

				string levelsPath = radio_Levels_01.Checked ? Path.Combine(projectPath, "Levels") : textBox_LevelsPath.Text.Trim();

				switch (comboBox_EngineType.SelectedIndex)
				{
					case 1:
						using (var form = new FormFindMusic())
						{
							if (form.ShowDialog(this) == DialogResult.OK)
							{
								_musicZipFilePath = form.MusicArchiveFilePath;
							}
							else
							{
								button_Create.Enabled = true;
								DialogResult = DialogResult.None;
								return;
							}
						}

						break;

					case 2:
					case 3:
						DialogResult result = DarkMessageBox.Show(this,
							"In order to install the game, you will have to select an /audio/ folder from an original\n" +
							"copy of the game (Steam and GOG versions are also valid).\n" +
							"Do you want to continue?",
							"Continue?", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

						if (result == DialogResult.Yes)
						{
							using (var dialog = new BrowseFolderDialog())
							{
								dialog.Title = "Select an original /audio/ folder.";

								if (dialog.ShowDialog(this) == DialogResult.OK)
								{
									var directory = new DirectoryInfo(dialog.Folder);
									FileInfo[] files = directory.GetFiles();

									string audioDir = Path.Combine(enginePath, "audio");

									switch (comboBox_EngineType.SelectedIndex)
									{
										case 2:
											_cdaudioDatFile = Array.Find(files, x => x.Name.Equals("cdaudio.dat", StringComparison.OrdinalIgnoreCase));

											if (_cdaudioDatFile == null)
												throw new ArgumentException("Selected /audio/ folder doesn't have a valid cdaudio.dat file.");

											_cdaudioMp3File = Array.Find(files, x => x.Name.Equals("cdaudio.mp3", StringComparison.OrdinalIgnoreCase));

											if (_cdaudioMp3File == null)
												throw new ArgumentException("Selected /audio/ folder doesn't have a valid cdaudio.mp3 file.");

											break;

										case 3:
											_cdaudioWadFile = Array.Find(files, x => x.Name.Equals("cdaudio.wad", StringComparison.OrdinalIgnoreCase));

											if (_cdaudioWadFile == null)
												throw new ArgumentException("Selected /audio/ folder doesn't have a valid cdaudio.wad file.");

											break;
									}
								}
								else
								{
									button_Create.Enabled = true;
									DialogResult = DialogResult.None;
									return;
								}
							}
						}
						else if (result == DialogResult.No)
						{
							button_Create.Enabled = true;
							DialogResult = DialogResult.None;
							return;
						}

						break;
				}

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

				_musicZipFilePath = null;
				_cdaudioDatFile = _cdaudioMp3File = _cdaudioWadFile = null;

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
					gameVersion = TRVersion.Game.TR1;
					break;

				case 2:
					gameVersion = TRVersion.Game.TR2;
					break;

				case 3:
					gameVersion = TRVersion.Game.TR3;
					break;

				case 4:
					gameVersion = TRVersion.Game.TR4;
					break;

				case 5:
					gameVersion = TRVersion.Game.TRNG;
					break;
			}

			string launchFilePath = Path.Combine(projectPath, "PLAY.exe");

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
			string engineBasePath = DefaultPaths.EngineTemplatesDirectory;

			switch (comboBox_EngineType.SelectedIndex)
			{
				case 1:
					engineBasePath = Path.Combine(engineBasePath, "TR1.zip");
					break;

				case 2:
					engineBasePath = Path.Combine(engineBasePath, "TR2.zip");
					break;

				case 3:
					engineBasePath = Path.Combine(engineBasePath, "TR3.zip");
					break;

				case 4:
					engineBasePath = Path.Combine(engineBasePath, "TR4.zip");
					break;

				case 5:
					engineBasePath = Path.Combine(engineBasePath, "TRNG.zip");
					break;
			}

			string sharedArchivePath = Path.Combine(TemplatePaths.GetSharedTemplatesPath(project.GameVersion), "Shared.zip");
			string flepArchivePath = Path.Combine(DefaultPaths.ProgramDirectory, "TIDE", "Templates", "TOMB4", "FLEP.zip");

			bool includeFLEP = checkBox_IncludeFLEP.Enabled && checkBox_IncludeFLEP.Checked;

			// Extract the engine base into the ProjectPath folder
			using (var engineArchive = new ZipArchive(File.OpenRead(engineBasePath)))
			using (var sharedArchive = new ZipArchive(File.OpenRead(sharedArchivePath)))
			using (var flepArchive = new ZipArchive(File.OpenRead(flepArchivePath)))
			{
				progressBar.Maximum = engineArchive.Entries.Count + sharedArchive.Entries.Count + 1;

				if (includeFLEP)
					progressBar.Maximum += flepArchive.Entries.Count;

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

				if (includeFLEP)
				{
					foreach (ZipArchiveEntry entry in flepArchive.Entries)
					{
						if (entry.FullName.EndsWith("/"))
						{
							string dirPath = Path.Combine(project.EnginePath, entry.FullName);

							if (!Directory.Exists(dirPath))
								Directory.CreateDirectory(dirPath);
						}
						else
							entry.ExtractToFile(Path.Combine(project.EnginePath, entry.FullName), true);

						progressBar.Increment(1);
					}
				}
			}

			if (project.GameVersion == TRVersion.Game.TR1)
			{
				string musicDir = Path.Combine(project.EnginePath, "music");

				if (!Directory.Exists(musicDir))
					Directory.CreateDirectory(musicDir);

				if (_musicZipFilePath != null)
				{
					using (var musicArchive = new ZipArchive(File.OpenRead(_musicZipFilePath)))
					{
						foreach (ZipArchiveEntry entry in musicArchive.Entries)
						{
							if (entry.FullName.EndsWith("/"))
								Directory.CreateDirectory(Path.Combine(project.EnginePath, entry.FullName));
							else
								entry.ExtractToFile(Path.Combine(project.EnginePath, entry.FullName));
						}
					}
				}
			}
			else if (project.GameVersion == TRVersion.Game.TR2 || project.GameVersion == TRVersion.Game.TR3)
			{
				string audioDir = Path.Combine(project.EnginePath, "audio");

				if (project.GameVersion == TRVersion.Game.TR2)
				{
					_cdaudioDatFile.CopyTo(Path.Combine(audioDir, _cdaudioDatFile.Name));
					_cdaudioMp3File.CopyTo(Path.Combine(audioDir, _cdaudioMp3File.Name));
				}
				else if (project.GameVersion == TRVersion.Game.TR3)
					_cdaudioWadFile.CopyTo(Path.Combine(audioDir, _cdaudioWadFile.Name));
			}

			// Save the .trproj file
			project.Save(); // .trproj = .xml but .trproj can be opened with TombIDE

			progressBar.Increment(1); // 100%
		}

		#endregion Methods
	}
}
