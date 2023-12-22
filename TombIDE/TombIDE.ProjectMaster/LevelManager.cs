using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows.Forms;
using TombIDE.Shared;
using TombIDE.Shared.NewStructure;
using TombIDE.Shared.NewStructure.Implementations;
using TombLib.LevelData;

namespace TombIDE.ProjectMaster
{
	public partial class LevelManager : UserControl
	{
		private IDE _ide;

		private bool _isPendingLevelListReload = false;

		private bool _isMainWindowFocused;

		public bool IsMainWindowFocued
		{
			get => _isMainWindowFocused;
			set
			{
				_isMainWindowFocused = value;

				if (_isMainWindowFocused && _isPendingLevelListReload)
				{
					_ide.RaiseEvent(new IDE.PRJ2FileDeletedEvent());
					_isPendingLevelListReload = false;
				}
			}
		}

		public LevelManager()
		{
			InitializeComponent();
		}

		public void Initialize(IDE ide)
		{
			_ide = ide;

			switch (_ide.Project.GameVersion)
			{
				case TRVersion.Game.TombEngine: panel_GameLabel.BackgroundImage = Properties.Resources.TEN_LVL; break;
				case TRVersion.Game.TRNG: panel_GameLabel.BackgroundImage = Properties.Resources.TRNG_LVL; break;
				case TRVersion.Game.TR4: panel_GameLabel.BackgroundImage = Properties.Resources.TR4_LVL; break;
				case TRVersion.Game.TR3: panel_GameLabel.BackgroundImage = Properties.Resources.TR3_LVL; break;
				case TRVersion.Game.TR2: panel_GameLabel.BackgroundImage = Properties.Resources.TR2_LVL; break;
				case TRVersion.Game.TR1: panel_GameLabel.BackgroundImage = Properties.Resources.TR1_LVL; break;
			}

			UpdateVersionLabel();

			section_LevelList.Initialize(ide);
			section_LevelProperties.Initialize(ide);

			// Initialize the watchers
			prj2FileWatcher.Path = _ide.Project.LevelsDirectoryPath;
			levelFolderWatcher.Path = _ide.Project.LevelsDirectoryPath;
		}

		private void UpdateVersionLabel()
		{
			Version engineVersion = _ide.Project.GetCurrentEngineVersion();
			string engineVersionString = engineVersion is null ? "Unknown" : engineVersion.ToString();
			label_EngineVersion.Text = $"Engine Version: {engineVersionString}";

			Version latestVersion = _ide.Project.GetLatestEngineVersion();

			if (engineVersion is null || latestVersion is null)
				label_OutdatedState.Visible = false;
			else
			{
				label_OutdatedState.Visible = true;

				if (engineVersion < latestVersion)
				{
					label_OutdatedState.Text = $"(Outdated, Latest version is: {latestVersion})";
					label_OutdatedState.ForeColor = Color.LightPink;

					if (_ide.Project.GameVersion is TRVersion.Game.TombEngine)
					{
						button_Update.Visible = true;

						// 1.0.9 is the first version that supports auto-updating
						if (engineVersion.Major <= 1 && engineVersion.Minor <= 0 && engineVersion.Build <= 8)
						{
							button_Update.Enabled = false;
							button_Update.Text = "Can't Auto-Update engine. Current version is too old.";
							button_Update.Width = 300;
						}
					}
					else
						button_Update.Visible = false;

					if (_ide.Project.GameVersion is TRVersion.Game.TR1)
					{
						button_Update.Visible = true;

						// 3.0 is the first version that supports auto-updating
						if (engineVersion.Major <= 2)
						{
							button_Update.Enabled = false;
							button_Update.Text = "Can't Auto-Update engine. Current version is too old.";
							button_Update.Width = 300;
						}
					}
					else
						button_Update.Visible = false;
				}
				else
				{
					label_OutdatedState.Text = "(Latest)";
					label_OutdatedState.ForeColor = Color.LightGreen;

					button_Update.Visible = false;
				}
			}
		}

		// Deleting .prj2 files is critical, so watch out
		private void prj2FileWatcher_Deleted(object sender, FileSystemEventArgs e)
		{
			if (IsMainWindowFocued)
				_ide.RaiseEvent(new IDE.PRJ2FileDeletedEvent());
			else
				_isPendingLevelListReload = true;
		}

		private void levelFolderWatcher_Deleted(object sender, FileSystemEventArgs e)
		{
			if (IsMainWindowFocued)
				_ide.RaiseEvent(new IDE.PRJ2FileDeletedEvent());
			else
				_isPendingLevelListReload = true;
		}

		private void button_RebuildAll_Click(object sender, EventArgs e)
		{
			LevelProject[] levels = _ide.Project.GetAllValidLevelProjects();

			if (levels.Length == 0)
			{
				DarkMessageBox.Show(this,
					"There are no levels in the current project.",
					"Information", MessageBoxButtons.OK, MessageBoxIcon.Information);

				return;
			}

			var batchList = new BatchCompileList();

			foreach (ILevelProject level in levels)
			{
				string prj2Path;

				if (level.TargetPrj2FileName is null)
					prj2Path = Path.Combine(level.DirectoryPath, level.GetMostRecentlyModifiedPrj2FileName());
				else
					prj2Path = Path.Combine(level.DirectoryPath, level.TargetPrj2FileName);

				batchList.Files.Add(prj2Path);
			}

			string batchListFilePath = Path.Combine(Path.GetTempPath(), "tide_batch.xml");
			BatchCompileList.SaveToXml(batchListFilePath, batchList);

			var startInfo = new ProcessStartInfo
			{
				FileName = Path.Combine(DefaultPaths.ProgramDirectory, "TombEditor.exe"),
				Arguments = "\"" + batchListFilePath + "\"",
				WorkingDirectory = DefaultPaths.ProgramDirectory,
				UseShellExecute = true
			};

			Process.Start(startInfo);
		}

		private void button_Update_Click(object sender, EventArgs e)
		{
			switch (_ide.Project.GameVersion)
			{
				case TRVersion.Game.TombEngine:
					UpdateTEN();
					break;

				case TRVersion.Game.TR1:
					UpdateTR1X();
					break;
			}
		}

		private void UpdateTEN()
		{
			try
			{
				string enginePresetPath = Path.Combine(DefaultPaths.PresetsDirectory, "TEN.zip");
				using var engineArchive = new ZipArchive(File.OpenRead(enginePresetPath));

				var bin = engineArchive.Entries.Where(entry => entry.FullName.StartsWith("Engine/Bin")).ToList();
				ExtractEntries(bin, _ide.Project);

				var shaders = engineArchive.Entries.Where(entry => entry.FullName.StartsWith("Engine/Shaders")).ToList();
				ExtractEntries(shaders, _ide.Project);

				var scriptsEngine = engineArchive.Entries.Where(entry => entry.FullName.StartsWith("Engine/Scripts/Engine")).ToList();
				ExtractEntries(scriptsEngine, _ide.Project);

				ZipArchiveEntry systemStrings = engineArchive.Entries.FirstOrDefault(entry => entry.Name.Equals("SystemStrings.lua"));
				systemStrings?.ExtractToFile(Path.Combine(_ide.Project.DirectoryPath, systemStrings.FullName), true);

				UpdateVersionLabel();

				DarkMessageBox.Show(this, "Engine has been updated successfully!", "Done.", MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
			catch (Exception ex)
			{
				DarkMessageBox.Show(this, "An error occurred while updating the engine:\n\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void UpdateTR1X()
		{
			try
			{
				string enginePresetPath = Path.Combine(DefaultPaths.PresetsDirectory, "TR1.zip");
				using var engineArchive = new ZipArchive(File.OpenRead(enginePresetPath));

				var data = engineArchive.Entries.Where(entry => entry.FullName.StartsWith("Engine/data")).ToList();
				ExtractEntries(data, _ide.Project, false);

				var shaders = engineArchive.Entries.Where(entry => entry.FullName.StartsWith("Engine/shaders")).ToList();
				ExtractEntries(shaders, _ide.Project);

				var executables = engineArchive.Entries.Where(entry => entry.FullName.EndsWith(".exe")).ToList();
				ExtractEntries(executables, _ide.Project);

				UpdateVersionLabel();

				DarkMessageBox.Show(this, "Engine has been updated successfully!", "Done.", MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
			catch (Exception ex)
			{
				DarkMessageBox.Show(this, "An error occurred while updating the engine:\n\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private static void ExtractEntries(List<ZipArchiveEntry> entries, IGameProject targetProject, bool overwrite = true)
		{
			foreach (ZipArchiveEntry entry in entries)
			{
				if (entry.FullName.EndsWith("/"))
				{
					string dirPath = Path.Combine(targetProject.DirectoryPath, entry.FullName);

					if (!Directory.Exists(dirPath))
						Directory.CreateDirectory(dirPath);
				}
				else
				{
					try
					{
						entry.ExtractToFile(Path.Combine(targetProject.DirectoryPath, entry.FullName), overwrite);
					}
					catch { }
				}
			}
		}
	}
}
