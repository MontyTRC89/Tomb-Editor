using DarkUI.Forms;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
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

			Version engineVersion = _ide.Project.GetCurrentEngineVersion();
			string engineVersionString = engineVersion is null ? "Unknown" : engineVersion.ToString();
			label_EngineVersion.Text = $"Engine Version: {engineVersionString}";

			Version latestVersion = _ide.Project.GetLatestEngineVersion();

			if (engineVersion is null || latestVersion is null)
				label_OutdatedState.Visible = false;
			else
			{
				if (engineVersion < latestVersion)
				{
					label_OutdatedState.Text = $"(Outdated, Latest version is: {latestVersion})";
					label_OutdatedState.ForeColor = Color.LightPink;
				}
				else
				{
					label_OutdatedState.Text = "(Latest)";
					label_OutdatedState.ForeColor = Color.LightGreen;
				}
			}

			section_LevelList.Initialize(ide);
			section_LevelProperties.Initialize(ide);

			// Initialize the watchers
			prj2FileWatcher.Path = _ide.Project.LevelsDirectoryPath;
			levelFolderWatcher.Path = _ide.Project.LevelsDirectoryPath;
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
	}
}
