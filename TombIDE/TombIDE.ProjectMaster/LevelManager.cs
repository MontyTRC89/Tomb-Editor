using DarkUI.Forms;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using TombIDE.Shared;
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

			section_LevelList.Initialize(ide);
			section_LevelProperties.Initialize(ide);

			// Initialize the watchers
			prj2FileWatcher.Path = _ide.Project.LevelsPath;
			levelFolderWatcher.Path = _ide.Project.LevelsPath;
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

		private void button_RebuildAll_Click(object sender, System.EventArgs e)
		{
			if (_ide.Project.Levels.Count == 0)
			{
				DarkMessageBox.Show(this,
					"There are no levels in the current project.",
					"Information", MessageBoxButtons.OK, MessageBoxIcon.Information);

				return;
			}

			BatchCompileList batchList = new BatchCompileList();

			foreach (ProjectLevel level in _ide.Project.Levels)
			{
				string prj2Path;

				if (level.SpecificFile == "$(LatestFile)")
					prj2Path = Path.Combine(level.FolderPath, level.GetLatestPrj2File());
				else
					prj2Path = Path.Combine(level.FolderPath, level.SpecificFile);

				batchList.Files.Add(prj2Path);
			}

			string batchListFilePath = Path.Combine(Path.GetTempPath(), "tide_batch.xml");
			BatchCompileList.SaveToXml(batchListFilePath, batchList);

			ProcessStartInfo startInfo = new ProcessStartInfo
			{
				FileName = Path.Combine(DefaultPaths.ProgramDirectory, "TombEditor.exe"),
				Arguments = "\"" + batchListFilePath + "\""
			};

			Process.Start(startInfo);
		}
	}
}
