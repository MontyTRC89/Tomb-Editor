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
using TombIDE.Shared.SharedClasses;
using TombIDE.Shared.SharedForms;

namespace TombIDE
{
	/* Important notes */
	// All program buttons are instances of the DarkButton class.
	// Tab buttons on the left tool strip (such as "Project Master", "Script Editor" etc.) are simple panels.
	// Other buttons (such as "Launch Game") are System.Windows.Forms buttons.
	// Program button names are just numbers from 0 to {darkbuttons.Count}.
	// The name number depends on the position of the button.
	// Swapping 2 buttons will affect their names.
	// Program button Tags contain .exe paths.

	public partial class FormMain : DarkForm
	{
		private IDE _ide;

		private ScriptingStudio.ClassicScriptStudio classicScriptStudio;
		private ProjectMaster.ProjectMaster projectMaster;

		private WinEventDelegate eventDelegate = null;
		private IntPtr eventHook = IntPtr.Zero;

		#region Initialization

		public FormMain(IDE ide, Project project)
		{
			InitializeComponent();
			Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);

			_ide = ide;
			_ide.Project = project;

			_ide.IDEEventRaised += OnIDEEventRaised;

			projectMaster = new ProjectMaster.ProjectMaster();
			projectMaster.Dock = DockStyle.Fill;
			tabPage_ProjectMaster.Controls.Add(projectMaster);

			classicScriptStudio = new ScriptingStudio.ClassicScriptStudio { Parent = this };
			classicScriptStudio.Dock = DockStyle.Fill;
			tabPage_ScriptEditor.Controls.Add(classicScriptStudio);

			// Add the current project name to the window title
			Text = "TombIDE - " + _ide.Project.Name;

			panel_CoverLoading.BringToFront(); // Cover the whole form with this panel to hide the graphical glitches happening while loading

			eventDelegate = new WinEventDelegate(WinEventProc);
			eventHook = NativeMethods.SetWinEventHook(
				NativeMethods.EVENT_SYSTEM_FOREGROUND, NativeMethods.EVENT_SYSTEM_FOREGROUND,
				IntPtr.Zero, eventDelegate, 0, 0, NativeMethods.WINEVENT_OUTOFCONTEXT);
		}

		public void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
		{
			if (!IsDisposed && NativeMethods.GetForegroundWindow() == Handle)
			{
				classicScriptStudio.IsMainWindowFocued = true;
				classicScriptStudio.EditorTabControl.TryRunFileReloadQueue();
			}

			if (!IsDisposed && NativeMethods.GetForegroundWindow() != Handle)
				classicScriptStudio.IsMainWindowFocued = false;

			if (IsDisposed)
				NativeMethods.UnhookWinEvent(eventHook);
		}

		protected override void OnLoad(EventArgs e)
		{
			ApplySavedSettings();

			button_LaunchGame.Image = Icon.ExtractAssociatedIcon(_ide.Project.LaunchFilePath).ToBitmap();

			InitializeFLEP();

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
					// Initialize the IDE interfaces
					projectMaster.Initialize(_ide);
					//scriptEditor.Initialize(_ide);

					SelectIDETab(IDETab.ProjectMaster);

					// Drop the panel
					panel_CoverLoading.Dispose();
				}
			}
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			if (!_ide.CanClose())
				e.Cancel = true;

			_ide.Project.Save();
			SaveSettings();

			base.OnClosing(e);
		}

		private void ApplySavedSettings()
		{
			Size = _ide.IDEConfiguration.IDE_WindowSize;

			if (_ide.IDEConfiguration.IDE_OpenMaximized)
				WindowState = FormWindowState.Maximized;
		}

		private void SaveSettings()
		{
			_ide.IDEConfiguration.IDE_OpenMaximized = WindowState == FormWindowState.Maximized;

			if (WindowState == FormWindowState.Normal)
				_ide.IDEConfiguration.IDE_WindowSize = Size;
			else
				_ide.IDEConfiguration.IDE_WindowSize = RestoreBounds.Size;

			_ide.IDEConfiguration.Save();
		}

		private void AddPinnedPrograms()
		{
			if (_ide.IDEConfiguration.PinnedProgramPaths.Count > 0) // If there are any pinned program paths in the config
			{
				foreach (string programPath in _ide.IDEConfiguration.PinnedProgramPaths)
					AddProgramButton(programPath, false);
			}
			else
			{
				// Add the default buttons
				AddProgramButton(Path.Combine(DefaultPaths.ProgramDirectory, "TombEditor.exe"), false);
				AddProgramButton(Path.Combine(DefaultPaths.ProgramDirectory, "WadTool.exe"), false);
				AddProgramButton(Path.Combine(DefaultPaths.ProgramDirectory, "SoundTool.exe"), false);
			}

			// Update the list with only valid programs
			SavePinnedPrograms();
		}

		private void InitializeFLEP()
		{
			// Check if flep.exe exists in the EnginePath folder, if so, then create a "Launch FLEP" button for quick access
			string flepExePath = Path.Combine(_ide.Project.EnginePath, "flep.exe");

			if (File.Exists(flepExePath))
			{
				button_Special.Image = Icon.ExtractAssociatedIcon(flepExePath).ToBitmap();
				toolTip.SetToolTip(button_Special, "Launch FLEP (F2)");
				button_Special.Click += Special_LaunchFLEP;
			}
			else
			{
				// Dispose the "Special button" and move all controls, which were underneath the button, 46 units higher
				button_Special.Dispose();
				button_AddProgram.Location = new Point(button_AddProgram.Location.X, button_AddProgram.Location.Y - 46);
			}
		}

		#endregion Initialization

		#region IDE events

		private void OnIDEEventRaised(IIDEEvent obj)
		{
			if (obj is IDE.SelectedIDETabChangedEvent)
				SelectIDETab(((IDE.SelectedIDETabChangedEvent)obj).Current);
			else if (obj is IDE.ProjectScriptPathChangedEvent || obj is IDE.ProjectLevelsPathChangedEvent)
				OnProjectPathsChanged(obj);
			else if (obj is IDE.ScriptEditor_ContentChangedEvent)
			{
				// Indicate changes inside the Script Editor
				panelButton_ScriptEditor.BackColor = Color.FromArgb(180, 100, 0);
				timer_ScriptButtonBlinking.Interval = 1;
				timer_ScriptButtonBlinking.Start();
			}
			else if (obj is IDE.ApplicationRestartingEvent)
				RestartApplication();
		}

		#endregion IDE events

		#region IDE event methods

		private void OnProjectPathsChanged(IIDEEvent obj)
		{
			DialogResult result = DarkMessageBox.Show(this, "To apply the changes, you must restart TombIDE.\n" +
				"Are you sure you want to do that?", "Are you sure?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

			if (result == DialogResult.Yes)
			{
				if (!_ide.CanClose())
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
						if (projectLevel.FolderPath.StartsWith(_ide.Project.LevelsPath, StringComparison.OrdinalIgnoreCase))
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

		#endregion IDE event methods

		#region Program buttons

		private void button_AddProgram_Click(object sender, EventArgs e)
		{
			using (OpenFileDialog dialog = new OpenFileDialog())
			{
				dialog.Title = "Select the .exe file of the program you want to add";
				dialog.Filter = "Executable Files|*.exe|Batch Files|*.bat";

				if (dialog.ShowDialog(this) == DialogResult.OK)
				{
					try
					{
						// Check for duplicates
						foreach (DarkButton button in panel_Programs.Controls.OfType<DarkButton>())
						{
							if (button.Tag.ToString().ToLower() == dialog.FileName.ToLower())
								throw new ArgumentException("Program shortcut already exists.");
						}

						AddProgramButton(dialog.FileName, true);
					}
					catch (Exception ex)
					{
						DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					}
				}
			}
		}

		private void menuItem_DeleteButton_Click(object sender, EventArgs e)
		{
			ToolStripMenuItem source = (ToolStripMenuItem)sender;
			DarkContextMenu owner = (DarkContextMenu)source.GetCurrentParent();
			DarkButton disposedButton = (DarkButton)owner.SourceControl;

			disposedButton.Dispose();

			// Rename every button beneath the disposedButton to reset the numbers in their names
			for (int i = int.Parse(disposedButton.Name) + 1; i <= panel_Programs.Controls.OfType<DarkButton>().Count(); i++)
			{
				DarkButton button = (DarkButton)panel_Programs.Controls.Find(i.ToString(), false).First();
				button.Location = new Point(button.Location.X, button.Location.Y - 46);
				button.Name = (int.Parse(button.Name) - 1).ToString();
			}

			button_AddProgram.Location = new Point(button_AddProgram.Location.X, button_AddProgram.Location.Y - 46); // 40 + 6 (Margins)

			SavePinnedPrograms();
		}

		private void AddProgramButton(string filePath, bool saveList)
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
			if (Path.GetExtension(filePath).ToLower() == ".bat")
				programName = Path.GetFileNameWithoutExtension(filePath) + " (Batch File)";
			else if (string.IsNullOrWhiteSpace(programName))
				programName = Path.GetFileNameWithoutExtension(filePath);

			toolTip.SetToolTip(button, programName);

			// Add the button exactly where button_AddProgram is and move button_AddProgram lower
			panel_Programs.Controls.Add(button);
			button_AddProgram.Location = new Point(button_AddProgram.Location.X, button_AddProgram.Location.Y + 46); // 40 + 6 (Margins)

			if (saveList)
				SavePinnedPrograms();
		}

		private void SavePinnedPrograms()
		{
			_ide.IDEConfiguration.PinnedProgramPaths.Clear();

			// Use for() instead of foreach() to also save the positions of the buttons based on their name numbers
			for (int i = 0; i < panel_Programs.Controls.OfType<DarkButton>().Count(); i++)
			{
				DarkButton button = (DarkButton)panel_Programs.Controls.Find(i.ToString(), false).First();
				_ide.IDEConfiguration.PinnedProgramPaths.Add(button.Tag.ToString());
			}

			_ide.IDEConfiguration.Save();
			_ide.ProgramButtonsModified();
		}

		private void ProgramButton_Click(object sender, EventArgs e)
		{
			// Check if a button is being dragged, if not, then launch the clicked program
			if (_draggedButton != null)
			{
				try
				{
					string programFilePath = ((Button)sender).Tag.ToString();

					ProcessStartInfo startInfo = new ProcessStartInfo
					{
						FileName = programFilePath,
						WorkingDirectory = Path.GetDirectoryName(programFilePath)
					};

					Process.Start(startInfo);
				}
				catch { }
			}
		}

		#endregion Program buttons

		#region Program button Drag 'n' Drop

		private DarkButton _draggedButton;
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
			Cursor.Current = Cursors.SizeAll; // Visually indicate that the user is dragging a button
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

			foreach (Screen screen in Screen.AllScreens)
				allScreenWidth += screen.Bounds.Width;

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

		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);

			if (ModifierKeys == Keys.None)
			{
				if (e.KeyCode == Keys.F2)
					LaunchFLEP();

				if (e.KeyCode == Keys.F3)
					SharedMethods.OpenInExplorer(_ide.Project.ProjectPath);

				if (e.KeyCode == Keys.F4)
					LaunchGame();
			}
		}

		private void panelButton_ProjectMaster_Click(object sender, EventArgs e) => SelectIDETab(IDETab.ProjectMaster);
		private void panelButton_ScriptEditor_Click(object sender, EventArgs e) => SelectIDETab(IDETab.ScriptEditor);

		private void Special_LaunchFLEP(object sender, EventArgs e) => LaunchFLEP();
		private void button_OpenFolder_Click(object sender, EventArgs e) => SharedMethods.OpenInExplorer(_ide.Project.ProjectPath);
		private void button_LaunchGame_Click(object sender, EventArgs e) => LaunchGame();

		private void timer_ScriptButtonBlinking_Tick(object sender, EventArgs e)
		{
			int red = panelButton_ScriptEditor.BackColor.R;
			int green = panelButton_ScriptEditor.BackColor.G;
			int blue = panelButton_ScriptEditor.BackColor.B;

			if (red > 48)
				red--;

			if (green > 48)
				green--;

			if (blue < 48)
				blue++;

			panelButton_ScriptEditor.BackColor = Color.FromArgb(red, green, blue);

			if (red == 48 && green == 48 && blue == 48)
				timer_ScriptButtonBlinking.Stop();
		}

		private void LaunchFLEP()
		{
			string flepExePath = Path.Combine(_ide.Project.EnginePath, "flep.exe");

			if (!File.Exists(flepExePath))
				return;

			try
			{
				ProcessStartInfo startInfo = new ProcessStartInfo
				{
					FileName = flepExePath,
					WorkingDirectory = _ide.Project.EnginePath
				};

				Process.Start(startInfo);
			}
			catch { }
		}

		private void LaunchGame()
		{
			if (!File.Exists(_ide.Project.LaunchFilePath))
			{
				DarkMessageBox.Show(this, "Couldn't find the launcher executable of the project.\n" +
					"Please restart TombIDE to resolve any issues.", "Error",
					MessageBoxButtons.OK, MessageBoxIcon.Error);

				return;
			}

			string scriptDatFilePath = Path.Combine(_ide.Project.EnginePath, "script.dat");

			if (File.Exists(scriptDatFilePath))
			{
				try
				{
					ProcessStartInfo startInfo = new ProcessStartInfo
					{
						FileName = _ide.Project.LaunchFilePath,
						WorkingDirectory = _ide.Project.EnginePath
					};

					Process.Start(startInfo);
				}
				catch { }
			}
			else
			{
				// A friendly reminder
				DarkMessageBox.Show(this, "Before launching the game, you must compile\n" +
					"your first script using the Script Editor.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);

				SelectIDETab(IDETab.ScriptEditor);
			}
		}

		private void SelectIDETab(IDETab tab)
		{
			classicScriptStudio.EditorTabControl.EnsureTabFileSynchronization();

			switch (tab)
			{
				case IDETab.ProjectMaster:
				{
					panelButton_ProjectMaster.BackColor = Color.FromArgb(135, 135, 135);
					panelButton_ScriptEditor.BackColor = Color.FromArgb(48, 48, 48);

					tablessTabControl.SelectTab(0);
					break;
				}
				case IDETab.ScriptEditor:
				{
					if (timer_ScriptButtonBlinking.Enabled)
						timer_ScriptButtonBlinking.Stop();

					panelButton_ProjectMaster.BackColor = Color.FromArgb(48, 48, 48);
					panelButton_ScriptEditor.BackColor = Color.FromArgb(135, 135, 135);

					tablessTabControl.SelectTab(1);
					break;
				}
			}
		}

		/// <summary>
		/// WARNING: This method doesn't ask IDE.CanClose() for closing permissions !!!
		/// </summary>
		private void RestartApplication()
		{
			_ide.Project.Save();
			SaveSettings();

			Application.Exit();

			// Restart with the current project selected
			ProcessStartInfo startInfo = new ProcessStartInfo
			{
				FileName = Assembly.GetExecutingAssembly().Location,
				Arguments = "\"" + _ide.Project.GetTrprojFilePath() + "\""
			};

			Process.Start(startInfo);
		}

		#endregion Other events
	}
}
