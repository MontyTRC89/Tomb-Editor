using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.Drawing;
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

				if (comboBox_EngineType.SelectedIndex == 0)
					throw new ArgumentException("You must specify the engine type of the project.");

				string projectPath = textBox_ProjectPath.Text.Trim();

				if (Directory.Exists(projectPath) && Directory.EnumerateFileSystemEntries(projectPath).ToArray().Length > 0)
					throw new ArgumentException("Selected project folder is not empty.");

				tableLayoutPanel_Content02.Controls.Clear();

				if (comboBox_EngineType.SelectedIndex == 1 || comboBox_EngineType.SelectedIndex == 6)
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

				if (comboBox_EngineType.SelectedIndex == 0)
					throw new ArgumentException("You must specify the engine type of the project.");

				string projectPath = textBox_ProjectPath.Text.Trim();
				string enginePath = Path.Combine(projectPath, "Engine");
				string scriptPath = radio_Script_01.Checked ? Path.Combine(projectPath, "Script") : textBox_ScriptPath.Text.Trim();

				if (comboBox_EngineType.SelectedIndex == 1)
					scriptPath = Path.Combine(enginePath, "cfg");
				else if (comboBox_EngineType.SelectedIndex == 6)
					scriptPath = Path.Combine(enginePath, "Scripts");

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
							"In order to correctly install the game, you will have to select an /audio/ folder\n" +
							"from an original copy of the game (Steam and GOG versions are also valid).\n" +
							"Do you want to do it now?",
							"Select /audio/ folder?", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

						if (result == DialogResult.Yes)
						{
							using (var dialog = new BrowseFolderDialog())
							{
								string gameName = comboBox_EngineType.SelectedIndex == 2 ? "Tomb Raider 2" : "Tomb Raider 3";

								dialog.Title = $"Select an original {gameName} /audio/ folder.";

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
						else if (result == DialogResult.Cancel)
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

				switch (createdProject.GameVersion)
				{
					case TRVersion.Game.TR1: InstallTR1Engine(createdProject); break;
					case TRVersion.Game.TR2: InstallTR2Engine(createdProject); break;
					case TRVersion.Game.TR3: InstallTR3Engine(createdProject); break;
					case TRVersion.Game.TR4: InstallTR4Engine(createdProject); break;
					case TRVersion.Game.TRNG: InstallTRNGEngine(createdProject, checkBox_IncludeFLEP.Checked); break;
					case TRVersion.Game.TombEngine: InstallTENEngine(createdProject); break;
				}

				AddLauncherToProject(createdProject);

				DarkMessageBox.Show(this, "Project has been successfully installed.", "Success",
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
				case 1: gameVersion = TRVersion.Game.TR1; break;
				case 2: gameVersion = TRVersion.Game.TR2; break;
				case 3: gameVersion = TRVersion.Game.TR3; break;
				case 4: gameVersion = TRVersion.Game.TR4; break;
				case 5: gameVersion = TRVersion.Game.TRNG; break;
				case 6: gameVersion = TRVersion.Game.TombEngine; break;
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

		private void InstallTR1Engine(Project targetProject)
		{
			progressBar.Maximum = 1;

			string enginePresetPath = Path.Combine(DefaultPaths.PresetsDirectory, "TR1.zip");

			using (var engineArchive = new ZipArchive(File.OpenRead(enginePresetPath)))
				ExtractEntries(engineArchive.Entries.ToList(), targetProject);

			string musicDir = Path.Combine(targetProject.EnginePath, "music");

			if (!Directory.Exists(musicDir))
				Directory.CreateDirectory(musicDir);

			if (_musicZipFilePath != null)
			{
				using (var musicArchive = new ZipArchive(File.OpenRead(_musicZipFilePath)))
				{
					foreach (ZipArchiveEntry entry in musicArchive.Entries)
					{
						if (entry.FullName.EndsWith("/"))
							Directory.CreateDirectory(Path.Combine(targetProject.EnginePath, entry.FullName));
						else
							entry.ExtractToFile(Path.Combine(targetProject.EnginePath, entry.FullName));
					}
				}
			}

			targetProject.Save();
			progressBar.Increment(1);
		}

		private void InstallTR2Engine(Project targetProject)
		{
			progressBar.Maximum = 1;

			string enginePresetPath = Path.Combine(DefaultPaths.PresetsDirectory, "TR2.zip");

			using (var engineArchive = new ZipArchive(File.OpenRead(enginePresetPath)))
				ExtractEntries(engineArchive.Entries.ToList(), targetProject);

			if (_cdaudioDatFile != null && _cdaudioMp3File != null)
			{
				string audioDir = Path.Combine(targetProject.EnginePath, "audio");

				_cdaudioDatFile.CopyTo(Path.Combine(audioDir, _cdaudioDatFile.Name));
				_cdaudioMp3File.CopyTo(Path.Combine(audioDir, _cdaudioMp3File.Name));
			}

			targetProject.Save();
			progressBar.Increment(1);
		}

		private void InstallTR3Engine(Project targetProject)
		{
			progressBar.Maximum = 1;

			string enginePresetPath = Path.Combine(DefaultPaths.PresetsDirectory, "TR3.zip");

			using (var engineArchive = new ZipArchive(File.OpenRead(enginePresetPath)))
				ExtractEntries(engineArchive.Entries.ToList(), targetProject);

			if (_cdaudioWadFile != null)
			{
				string audioDir = Path.Combine(targetProject.EnginePath, "audio");
				_cdaudioWadFile.CopyTo(Path.Combine(audioDir, _cdaudioWadFile.Name));
			}

			targetProject.Save();
			progressBar.Increment(1);
		}

		private void InstallTR4Engine(Project targetProject)
		{
			progressBar.Maximum = 1;

			string enginePresetPath = Path.Combine(DefaultPaths.PresetsDirectory, "TR4.zip");
			string sharedFilesArchivePath = Path.Combine(DefaultPaths.TemplatesDirectory, "Shared", "TR4-TRNG Shared Files.zip");
			string sharedAudioArchivePath = Path.Combine(DefaultPaths.TemplatesDirectory, "Shared", "TR4-TEN Shared Audio.zip");

			using (var engineArchive = new ZipArchive(File.OpenRead(enginePresetPath)))
			using (var sharedFilesArchive = new ZipArchive(File.OpenRead(sharedFilesArchivePath)))
			using (var sharedAudioArchive = new ZipArchive(File.OpenRead(sharedAudioArchivePath)))
			{
				var allFiles = new List<ZipArchiveEntry>();
				allFiles.AddRange(engineArchive.Entries);
				allFiles.AddRange(sharedFilesArchive.Entries);
				allFiles.AddRange(sharedAudioArchive.Entries);

				ExtractEntries(allFiles, targetProject);
			}

			targetProject.Save();
			progressBar.Increment(1);
		}

		private void InstallTRNGEngine(Project targetProject, bool includeFLEP)
		{
			progressBar.Maximum = 1;

			string enginePresetPath = Path.Combine(DefaultPaths.PresetsDirectory, "TRNG.zip");
			string sharedFilesArchivePath = Path.Combine(DefaultPaths.TemplatesDirectory, "Shared", "TR4-TRNG Shared Files.zip");
			string sharedAudioArchivePath = Path.Combine(DefaultPaths.TemplatesDirectory, "Shared", "TR4-TEN Shared Audio.zip");
			string flepArchivePath = Path.Combine(DefaultPaths.TemplatesDirectory, "Extras", "FLEP.zip");

			using (var engineArchive = new ZipArchive(File.OpenRead(enginePresetPath)))
			using (var sharedFilesArchive = new ZipArchive(File.OpenRead(sharedFilesArchivePath)))
			using (var sharedAudioArchive = new ZipArchive(File.OpenRead(sharedAudioArchivePath)))
			using (var flepArchive = new ZipArchive(File.OpenRead(flepArchivePath)))
			{
				var allFiles = new List<ZipArchiveEntry>();
				allFiles.AddRange(engineArchive.Entries);
				allFiles.AddRange(sharedFilesArchive.Entries);
				allFiles.AddRange(sharedAudioArchive.Entries);

				if (includeFLEP)
					allFiles.AddRange(flepArchive.Entries);

				ExtractEntries(allFiles, targetProject);
			}

			targetProject.Save();
			progressBar.Increment(1);
		}

		private void InstallTENEngine(Project targetProject)
		{
			const int extraSteps = 2;
			progressBar.Maximum = 1 + extraSteps;

			string enginePresetPath = Path.Combine(DefaultPaths.PresetsDirectory, "TEN.zip");
			string sharedAudioArchivePath = Path.Combine(DefaultPaths.TemplatesDirectory, "Shared", "TR4-TEN Shared Audio.zip");

			using (var engineArchive = new ZipArchive(File.OpenRead(enginePresetPath)))
			using (var sharedAudioArchive = new ZipArchive(File.OpenRead(sharedAudioArchivePath)))
			{
				var allFiles = new List<ZipArchiveEntry>();
				allFiles.AddRange(engineArchive.Entries);
				allFiles.AddRange(sharedAudioArchive.Entries);

				ExtractEntries(allFiles, targetProject);
			}

			Directory.Move(Path.Combine(targetProject.EnginePath, "audio"), Path.Combine(targetProject.EnginePath, "audio_temp"));
			progressBar.Increment(1);

			Directory.Move(Path.Combine(targetProject.EnginePath, "audio_temp"), Path.Combine(targetProject.EnginePath, "Audio"));
			progressBar.Increment(1);

			targetProject.Save();
			progressBar.Increment(1);
		}

		private void AddLauncherToProject(Project targetProject)
		{
			string sharedLauncherFilePath = Path.Combine(DefaultPaths.TemplatesDirectory, "Shared", "PLAY.exe");
			string sharedSplashPropertiesFilePath = Path.Combine(DefaultPaths.TemplatesDirectory, "Shared", "splash.xml");

			// Copy launcher to project
			File.Copy(sharedLauncherFilePath, targetProject.LaunchFilePath, true);
			File.Copy(sharedSplashPropertiesFilePath, Path.Combine(targetProject.EnginePath, "splash.xml"), true);

			// Apply icon to launcher
			string icoFilePath = Path.Combine(DefaultPaths.TemplatesDirectory, "Defaults", "Game Icons", targetProject.GameVersion + ".ico");
			IconUtilities.InjectIcon(targetProject.LaunchFilePath, icoFilePath);
		}

		private void ExtractEntries(List<ZipArchiveEntry> entries, Project targetProject)
		{
			progressBar.Maximum += entries.Count;

			foreach (ZipArchiveEntry entry in entries)
			{
				if (entry.FullName.EndsWith("/"))
				{
					string dirPath = Path.Combine(targetProject.ProjectPath, entry.FullName);

					if (!Directory.Exists(dirPath))
						Directory.CreateDirectory(dirPath);
				}
				else
					entry.ExtractToFile(Path.Combine(targetProject.ProjectPath, entry.FullName), true);

				progressBar.Increment(1);
			}
		}

		#endregion Methods
	}
}
