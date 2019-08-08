using DarkUI.Forms;
using System;
using System.IO;
using System.Windows.Forms;
using TombLib.Projects;

namespace TombIDE.Shared
{
	public partial class FormLoading : DarkForm
	{
		private IDE _ide;

		public FormLoading(IDE ide)
		{
			_ide = ide;

			InitializeComponent();
		}

		private const int CP_NOCLOSE_BUTTON = 0x200;

		protected override CreateParams CreateParams
		{
			get
			{
				CreateParams cp = base.CreateParams;
				cp.ClassStyle = cp.ClassStyle | CP_NOCLOSE_BUTTON;
				return cp;
			}
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);

			// We must delay the ScanLevelsDirectory() method to prevent graphical glitches
			timer.Start();
		}

		private void timer_Tick(object sender, EventArgs e)
		{
			timer.Stop();
			ScanLevelsDirectory();
		}

		private void ScanLevelsDirectory()
		{
			DirectoryInfo directoryInfo = new DirectoryInfo(_ide.Project.LevelsPath);

			// Get all subdirectories of the project's /Levels/ folder
			DirectoryInfo[] levelDirectories = directoryInfo.GetDirectories();

			// Setup the progress bar
			progressBar.Maximum = levelDirectories.Length;

			foreach (DirectoryInfo directory in levelDirectories)
			{
				// Check if the folder we're currently scanning is already known for the project
				if (IsFolderKnownForProject(directory))
				{
					progressBar.Increment(1);
					continue;
				}

				// Scan all .prj2 files in the current directory (if there are any)
				if (directory.GetFiles("*.prj2", SearchOption.TopDirectoryOnly).Length > 0)
				{
					// Check if the folder isn't just made up of backup files
					if (FolderOnlyContainsBackupFiles(directory))
					{
						progressBar.Increment(1);
						continue;
					}

					// Create a new ProjectLevel instance from the folder
					ProjectLevel detectedLevel = new ProjectLevel
					{
						Name = directory.Name,
						FolderPath = directory.FullName
					};

					// Update the settings of all the .prj2 files in the folder to match the project settings
					UpdateAllPrj2GameSettings(directory, detectedLevel);

					_ide.Project.Levels.Add(detectedLevel);
				}

				progressBar.Increment(1);
			}

			CheckForFloatingPrj2Files(directoryInfo);

			DialogResult = DialogResult.OK;
		}

		private void CheckForFloatingPrj2Files(DirectoryInfo directoryInfo)
		{
			if (directoryInfo.GetFiles("*.prj2", SearchOption.TopDirectoryOnly).Length > 0)
			{
				DarkMessageBox.Show(this, "Some .prj2 files were found directly placed in the project's /Levels/ folder.\n" +
					"Please make sure to create seperate subfolders for them as having floating .prj2 files is not allowed.",
					"Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
		}

		private bool IsFolderKnownForProject(DirectoryInfo directory)
		{
			foreach (ProjectLevel level in _ide.Project.Levels)
			{
				string projectLevelFolderName = Path.GetFileName(level.FolderPath);

				if (projectLevelFolderName == directory.Name)
					return true;
			}

			return false;
		}

		private bool FolderOnlyContainsBackupFiles(DirectoryInfo directory)
		{
			foreach (FileInfo file in directory.GetFiles("*.prj2", SearchOption.TopDirectoryOnly))
			{
				if (!ProjectLevel.IsBackupFile(file.Name))
					return false; // We got a non-backup file
			}

			return true;
		}

		private void UpdateAllPrj2GameSettings(DirectoryInfo directory, ProjectLevel detectedLevel)
		{
			foreach (FileInfo file in directory.GetFiles("*.prj2", SearchOption.TopDirectoryOnly))
			{
				if (!ProjectLevel.IsBackupFile(file.Name))
					SharedMethods.UpdatePrj2GameSettings(file.FullName, detectedLevel, _ide.Project);
			}
		}
	}
}
