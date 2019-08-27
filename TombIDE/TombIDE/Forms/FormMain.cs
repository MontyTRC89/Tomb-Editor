using DarkUI.Controls;
using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using TombIDE.Shared;
using TombLib.Projects;

namespace TombIDE
{
	/* Important notes */
	// All program buttons are instances of the DarkButton class.
	// Other buttons on the left tool strip (such as Project Master, Script Editor etc.) are simple panels.
	// Program button names are just numbers from 0 to *darkbuttons.Count*.
	// The name number depends on the position of the button.
	// Swapping 2 buttons will affect their names.
	// Program button Tags contain .exe paths.

	public partial class FormMain : DarkForm
	{
		private IDE _ide;

		#region Initialization

		public FormMain(IDE ide)
		{
			_ide = ide;
			_ide.IDEEventRaised += OnIDEEventRaised;

			InitializeComponent();

			// Add the current project name to the window title
			Text = "TombIDE - " + _ide.Project.Name;

			panel_CoverLoading.BringToFront(); // Cover the whole form with this panel to hide the graphical glitches happening while loading

			// Check if flep.exe exists in the ProjectPath folder, if so, then create a "Launch FLEP" button for quick access
			string flepExePath = Path.Combine(_ide.Project.ProjectPath, "flep.exe");

			if (File.Exists(flepExePath))
			{
				button_Special.Image = Icon.ExtractAssociatedIcon(flepExePath).ToBitmap();
				toolTip.SetToolTip(button_Special, "Launch FLEP");
				button_Special.Click += Special_LaunchFLEP;
			}
			else
			{
				// Dispose the "Special button" and move all controls, which were underneath the button, 46 units higher
				button_Special.Dispose();

				button_OpenFolder.Location = new Point(button_OpenFolder.Location.X, button_OpenFolder.Location.Y - 46);
				button_LaunchGame.Location = new Point(button_LaunchGame.Location.X, button_LaunchGame.Location.Y - 46);
				label_Separator_03.Location = new Point(label_Separator_03.Location.X, label_Separator_03.Location.Y - 46);
				button_AddProgram.Location = new Point(button_AddProgram.Location.X, button_AddProgram.Location.Y - 46);
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			ApplySavedSettings();
			AddPinnedPrograms();

			base.OnLoad(e);
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);

			using (FormLoading form = new FormLoading(_ide))
			{
				if (form.ShowDialog(this) == DialogResult.OK)
				{
					// Initialize tabs
					projectMaster.Initialize(_ide);
					scriptEditor.Initialize(_ide);

					// Run through all tabs to avoid validation glitches
					_ide.SelectIDETab("Project Master");
					_ide.SelectIDETab("Script Editor");
					_ide.SelectIDETab("Tools");

					// Always start with the Project Master
					_ide.SelectIDETab("Project Master");

					// Drop the panel
					panel_CoverLoading.Dispose();
				}
			}
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			_ide.RaiseEvent(new IDE.ProgramClosingEvent()); // This will ask the Script Editor if everything is saved

			if (_ide.ClosingCancelled)
				e.Cancel = true;

			XmlHandling.SaveTRPROJ(_ide.Project);
			SaveSettings();

			base.OnClosing(e);
		}

		private void SaveSettings()
		{
			_ide.Configuration.IDE_OpenMaximized = WindowState == FormWindowState.Maximized;

			if (WindowState == FormWindowState.Normal)
				_ide.Configuration.IDE_WindowSize = Size;
			else
				_ide.Configuration.IDE_WindowSize = RestoreBounds.Size;

			_ide.Configuration.Save();
		}

		private void ApplySavedSettings()
		{
			Size = _ide.Configuration.IDE_WindowSize;

			if (_ide.Configuration.IDE_OpenMaximized)
				WindowState = FormWindowState.Maximized;
		}

		private void AddPinnedPrograms()
		{
			if (_ide.Configuration.PinnedProgramPaths.Count > 0) // If there are any pinned program paths in the config
			{
				foreach (string programPath in _ide.Configuration.PinnedProgramPaths)
					AddProgramButton(programPath, false);
			}
			else
			{
				// Add the default buttons
				AddProgramButton(Path.Combine(SharedMethods.GetProgramDirectory(), "TombEditor.exe"), false);
				AddProgramButton(Path.Combine(SharedMethods.GetProgramDirectory(), "WadTool.exe"), false);
			}

			// Update the list with only valid programs
			SavePinnedPrograms();
		}

		private void SavePinnedPrograms()
		{
			_ide.Configuration.PinnedProgramPaths.Clear();

			// Use for() instead of foreach() to also save the position of the button based on its name
			for (int i = 0; i < panel_Programs.Controls.OfType<DarkButton>().Count(); i++)
			{
				DarkButton button = (DarkButton)panel_Programs.Controls.Find(i.ToString(), false).First();
				_ide.Configuration.PinnedProgramPaths.Add(button.Tag.ToString());
			}

			_ide.Configuration.Save();
			_ide.RaiseEvent(new IDE.ProgramButtonsChangedEvent());
		}

		private void RestartApplication()
		{
			XmlHandling.SaveTRPROJ(_ide.Project);
			SaveSettings();

			Application.Exit();

			// Restart with the current project selected
			ProcessStartInfo startInfo = new ProcessStartInfo
			{
				FileName = Assembly.GetExecutingAssembly().Location,
				Arguments = "\"" + _ide.Project.GetTRPROJFilePath() + "\""
			};

			Process.Start(startInfo);
		}

		#endregion Initialization

		#region Program buttons

		private void button_AddProgram_Click(object sender, EventArgs e)
		{
			using (OpenFileDialog dialog = new OpenFileDialog())
			{
				dialog.Title = "Choose the .exe file of the program you want to add";
				dialog.Filter = "Executable Files|*.exe|Batch Files|*.bat";

				if (dialog.ShowDialog(this) == DialogResult.OK)
				{
					try
					{
						if (panel_Programs.Controls.OfType<DarkButton>().ToList().Exists(x => x.Tag.ToString().ToLower() == dialog.FileName.ToLower()))
							throw new ArgumentException("Program shortcut already exists.");

						AddProgramButton(dialog.FileName);
					}
					catch (Exception ex)
					{
						DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					}
				}
			}
		}

		private void AddProgramButton(string filePath, bool saveList = true)
		{
			// Check if the program is valid
			if (!File.Exists(filePath))
				return;

			int buttonIndex = panel_Programs.Controls.OfType<DarkButton>().Count();
			Image image = Icon.ExtractAssociatedIcon(filePath).ToBitmap();

			// Create the button
			DarkButton button = new DarkButton
			{
				Name = buttonIndex.ToString(),
				Tag = filePath,
				Image = image,
				Size = new Size(40, 40),
				Location = button_AddProgram.Location
			};

			// Bind event methods to the button
			button.Click += ProgramButton_Click;
			button.MouseDown += ProgramButton_MouseDown;
			button.MouseMove += ProgramButton_MouseMove;

			button.ContextMenuStrip = contextMenu_ProgramButton;

			string programName = FileVersionInfo.GetVersionInfo(filePath).ProductName;

			// Handle batch files and programs without ProductNames
			if (string.IsNullOrEmpty(programName) && Path.GetExtension(filePath).ToLower() == ".bat")
				programName = Path.GetFileNameWithoutExtension(filePath) + " (Batch File)";
			else if (string.IsNullOrEmpty(programName))
				programName = Path.GetFileNameWithoutExtension(filePath);

			toolTip.SetToolTip(button, programName);

			// Add the button exactly where button_AddProgram is and move button_AddProgram lower
			panel_Programs.Controls.Add(button);
			button_AddProgram.Location = new Point(button_AddProgram.Location.X, button_AddProgram.Location.Y + 46); // 40 + 6 (Margins)

			if (saveList)
				SavePinnedPrograms();
		}

		private void ProgramButton_Click(object sender, EventArgs e)
		{
			// Check if a button is being dragged, if not, then launch the clicked program
			if (_draggedButton != null)
			{
				string programFilePath = ((Button)sender).Tag.ToString();

				ProcessStartInfo startInfo = new ProcessStartInfo
				{
					FileName = programFilePath,
					WorkingDirectory = Path.GetDirectoryName(programFilePath)
				};

				Process.Start(startInfo);
			}
		}

		private void menuItem_DeleteButton_Click(object sender, EventArgs e)
		{
			ToolStripMenuItem source = (ToolStripMenuItem)sender;
			DarkContextMenu owner = (DarkContextMenu)source.GetCurrentParent();
			DarkButton disposedButton = (DarkButton)owner.SourceControl;

			disposedButton.Dispose();

			// Rename every button beneath the disposedButton to reset the numbers in the names
			for (int i = int.Parse(disposedButton.Name) + 1; i <= panel_Programs.Controls.OfType<DarkButton>().Count(); i++)
			{
				DarkButton button = (DarkButton)panel_Programs.Controls.Find(i.ToString(), false).First();
				button.Location = new Point(button.Location.X, button.Location.Y - 46);
				button.Name = (int.Parse(button.Name) - 1).ToString();
			}

			button_AddProgram.Location = new Point(button_AddProgram.Location.X, button_AddProgram.Location.Y - 46); // 40 + 6 (Margins)

			SavePinnedPrograms();
		}

		#endregion Program buttons

		#region Program button Drag 'n' Drop

		private DarkButton _draggedButton = null;
		private Point _clickPosition;

		private void ProgramButton_MouseDown(object sender, MouseEventArgs e)
		{
			if (_draggedButton == null)
				_clickPosition = e.Location; // Get the cursor position for the dragging threshold

			_draggedButton = (DarkButton)sender;
		}

		private void ProgramButton_MouseMove(object sender, MouseEventArgs e)
		{
			if ((e.Button != MouseButtons.Left) || (_draggedButton == null))
				return;

			// Dragging threshold. 5 seems like a good number here.
			if (Math.Abs(e.Y - _clickPosition.Y) < 5)
				return;

			panel_Programs.Capture = true; // While dragging the button, always capture the panel, even if the cursor is out of bounds
			Cursor.Current = Cursors.SizeAll; // Indicate that the user is dragging a button
		}

		private void panel_Programs_MouseUp(object sender, MouseEventArgs e)
		{
			_draggedButton = null;

			panel_Programs.Capture = false;
			Cursor.Current = Cursors.Default;
		}

		// We move from ProgramButton_MouseMove to this method if panel_Programs.Capture is true
		private void panel_Programs_MouseMove(object sender, MouseEventArgs e)
		{
			if ((e.Button != MouseButtons.Left) || (_draggedButton == null))
				return;

			// Check if we can swap the button

			DarkButton hoveredButton = GetHoveredButton();

			if (hoveredButton == null)
				return;

			if (hoveredButton == _draggedButton)
				return;

			SwapButtons(_draggedButton, hoveredButton);

			SavePinnedPrograms();
		}

		private DarkButton GetHoveredButton()
		{
			foreach (DarkButton button in panel_Programs.Controls.OfType<DarkButton>())
			{
				int allScreenWidth = GetAllScreenWidth();
				Rectangle rect = new Rectangle(-allScreenWidth, button.Location.Y, allScreenWidth * 4, button.Size.Height);
				// "allScreenWidth * 4" because someone would eventually break it

				if (rect.Contains(PointToClient(Cursor.Position)))
					return button;
			}

			return null;
		}

		private int GetAllScreenWidth()
		{
			int allScreenWidth = 0;

			foreach (Screen scr in Screen.AllScreens)
				allScreenWidth += scr.Bounds.Width;

			return allScreenWidth;
		}

		private void SwapButtons(DarkButton src, DarkButton dst)
		{
			string srcName = src.Name;
			Point srcLoc = src.Location;

			string dstName = dst.Name;
			Point dstLoc = dst.Location;

			src.Name = dstName;
			src.Location = dstLoc;

			dst.Name = srcName;
			dst.Location = srcLoc;
		}

		#endregion Program button Drag 'n' Drop

		#region Other events

		private void OnIDEEventRaised(IIDEEvent obj)
		{
			if (obj is IDE.SelectedIDETabChangedEvent)
			{
				string tabPageName = ((IDE.SelectedIDETabChangedEvent)obj).Current;

				switch (tabPageName)
				{
					case "Project Master":
					{
						panelButton_ProjectMaster.BackColor = Color.FromArgb(135, 135, 135);
						panelButton_ScriptEditor.BackColor = Color.FromArgb(48, 48, 48);
						panelButton_Tools.BackColor = Color.FromArgb(48, 48, 48);

						tablessTabControl.SelectTab(0);
						break;
					}
					case "Script Editor":
					{
						if (timer_ScriptButtonBlinking.Enabled)
							timer_ScriptButtonBlinking.Stop(); // Stop the blinking

						panelButton_ProjectMaster.BackColor = Color.FromArgb(48, 48, 48);
						panelButton_ScriptEditor.BackColor = Color.FromArgb(135, 135, 135);
						panelButton_Tools.BackColor = Color.FromArgb(48, 48, 48);

						tablessTabControl.SelectTab(1);
						break;
					}
					case "Tools":
					{
						panelButton_ProjectMaster.BackColor = Color.FromArgb(48, 48, 48);
						panelButton_ScriptEditor.BackColor = Color.FromArgb(48, 48, 48);
						panelButton_Tools.BackColor = Color.FromArgb(135, 135, 135);

						tablessTabControl.SelectTab(2);
						break;
					}
				}
			}
			else if (obj is IDE.LevelAddedEvent)
			{
				// Check if any messages were sent for the Script Editor
				if (((IDE.LevelAddedEvent)obj).ScriptMessages.Count > 0)
				{
					// Indicate changes inside the Script Editor
					timer_ScriptButtonBlinking.Interval = 1;
					timer_ScriptButtonBlinking.Start();
				}
			}
			else if (obj is IDE.ScriptEditorContentChangedEvent)
			{
				// Indicate changes inside the Script Editor
				timer_ScriptButtonBlinking.Interval = 1;
				timer_ScriptButtonBlinking.Start();
			}
			else if (obj is IDE.ProjectScriptPathChangedEvent || obj is IDE.ProjectLevelsPathChangedEvent)
			{
				DialogResult result = DarkMessageBox.Show(this, "To apply the changes, you must restart TombIDE.\n" +
					"Are you sure you want to do that?", "Are you sure?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

				if (result == DialogResult.Yes)
				{
					_ide.RaiseEvent(new IDE.ProgramClosingEvent()); // This will ask the Script Editor if everything is saved

					if (_ide.ClosingCancelled)
					{
						// User pressed "Cancel"

						DarkMessageBox.Show(this, "Operation cancelled.\nNo paths have been affected.", "Operation cancelled",
						   MessageBoxButtons.OK, MessageBoxIcon.Information);

						return;
					}

					if (obj is IDE.ProjectScriptPathChangedEvent)
						_ide.Project.ScriptPath = ((IDE.ProjectScriptPathChangedEvent)obj).NewPath;
					else if (obj is IDE.ProjectLevelsPathChangedEvent)
					{
						List<ProjectLevel> projectLevels = new List<ProjectLevel>();
						projectLevels.AddRange(_ide.Project.Levels);

						// Remove all internal level entries from the project's Levels list (for safety)
						foreach (ProjectLevel projectLevel in projectLevels)
						{
							if (projectLevel.FolderPath.StartsWith(_ide.Project.LevelsPath))
								_ide.Project.Levels.Remove(projectLevel);
						}

						_ide.Project.LevelsPath = ((IDE.ProjectLevelsPathChangedEvent)obj).NewPath;
					}

					RestartApplication();
				}
				else if (result == DialogResult.No)
					DarkMessageBox.Show(this, "Operation cancelled.\nNo paths have been affected.", "Operation cancelled",
						   MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
			else if (obj is IDE.NewPluginsAddedEvent)
			{
				DialogResult result = DarkMessageBox.Show(this,
					"It is highly recommended to restart TombIDE after adding new plugins,\n" +
					"otherwise some script elements won't be available.\n" +
					"Would you like to restart TombIDE now?", "Restart required", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

				if (result == DialogResult.Yes)
				{
					_ide.RaiseEvent(new IDE.ProgramClosingEvent()); // This will ask the Script Editor if everything is saved

					if (_ide.ClosingCancelled)
						return; // User pressed "Cancel"

					RestartApplication();
				}
			}
			else if (obj is IDE.RequestedApplicationRestartEvent)
				RestartApplication();
		}

		private void Special_LaunchFLEP(object sender, EventArgs e)
		{
			ProcessStartInfo startInfo = new ProcessStartInfo
			{
				FileName = Path.Combine(_ide.Project.ProjectPath, "flep.exe"),
				WorkingDirectory = _ide.Project.ProjectPath
			};

			Process.Start(startInfo);
		}

		// All 3 methods below trigger IDE.SelectedIDETabChangedEvent
		private void panelButton_ProjectMaster_Click(object sender, EventArgs e) => _ide.SelectIDETab("Project Master");
		private void panelButton_ScriptEditor_Click(object sender, EventArgs e) => _ide.SelectIDETab("Script Editor");
		private void panelButton_Tools_Click(object sender, EventArgs e) => _ide.SelectIDETab("Tools");

		private void button_OpenFolder_Click(object sender, EventArgs e) =>
			SharedMethods.OpenFolderInExplorer(_ide.Project.ProjectPath);

		private void button_LaunchGame_Click(object sender, EventArgs e)
		{
			try
			{
				string scriptDatFilePath = Path.Combine(_ide.Project.ProjectPath, "script.dat");

				if (!File.Exists(scriptDatFilePath))
				{
					// A friendly reminder
					DarkMessageBox.Show(this,
						"Before launching the game, you must compile\n" +
						"your first script using the Script Editor.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);

					_ide.SelectIDETab("Script Editor");
					return;
				}

				string launchFile = Path.Combine(_ide.Project.ProjectPath, "launch.exe");
				string gameFile = Path.Combine(_ide.Project.ProjectPath, _ide.Project.GetExeFileName());

				string exeFilePath = File.Exists(launchFile) ? launchFile : gameFile;

				ProcessStartInfo startInfo = new ProcessStartInfo
				{
					FileName = exeFilePath,
					WorkingDirectory = _ide.Project.ProjectPath
				};

				Process.Start(startInfo);
			}
			catch (Exception ex)
			{
				DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void timer_ScriptButtonBlinking_Tick(object sender, EventArgs e)
		{
			if (panelButton_ScriptEditor.BackColor == Color.FromArgb(48, 48, 48))
				panelButton_ScriptEditor.BackColor = Color.FromArgb(180, 100, 0);
			else
				panelButton_ScriptEditor.BackColor = Color.FromArgb(48, 48, 48);

			timer_ScriptButtonBlinking.Interval = 500;
		}

		#endregion Other events
	}
}
