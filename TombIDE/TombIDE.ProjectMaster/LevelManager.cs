using System.IO;
using System.Windows.Forms;
using TombIDE.Shared;

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

		private void button_Main_New_Click(object sender, System.EventArgs e)
		{
			_ide.RaiseEvent(new IDE.RequestCreateNewLevelEvent());
		}

		private void button_Main_Import_Click(object sender, System.EventArgs e)
		{
			_ide.RaiseEvent(new IDE.RequestImportLevelEvent());
		}
	}
}
