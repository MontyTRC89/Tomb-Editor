using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using TombIDE.ProjectMaster;
using TombIDE.ScriptingStudio.Bases;
using TombIDE.Shared;
using TombIDE.Shared.SharedForms;
using TombLib.LevelData;

namespace TombIDE
{
	public partial class FormMain : DarkForm
	{
		private IDE _ide;

		private LevelManager levelManager;
		private StudioBase scriptingStudio;
		private PluginManager pluginManager;
		private Miscellaneous miscellaneous;

		private WinEventDelegate eventDelegate;
		private IntPtr eventHook = IntPtr.Zero;

		#region Initialization

		public FormMain(IDE ide, Project project)
		{
			InitializeComponent();
			Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);

			_ide = ide;
			_ide.Project = project;

			_ide.IDEEventRaised += OnIDEEventRaised;

			levelManager = new LevelManager { Dock = DockStyle.Fill };
			tabPage_LevelManager.Controls.Add(levelManager);

			miscellaneous = new Miscellaneous { Dock = DockStyle.Fill };
			tabPage_Misc.Controls.Add(miscellaneous);

			if (_ide.Project.GameVersion == TRVersion.Game.TRNG)
			{
				pluginManager = new PluginManager { Dock = DockStyle.Fill };
				tabPage_Plugins.Controls.Add(pluginManager);
			}

			if (_ide.Project.GameVersion == TRVersion.Game.TR4 || _ide.Project.GameVersion == TRVersion.Game.TRNG)
				scriptingStudio = new ScriptingStudio.ClassicScriptStudio { Parent = this };
			else if (_ide.Project.GameVersion == TRVersion.Game.TR2 || _ide.Project.GameVersion == TRVersion.Game.TR3)
				scriptingStudio = new ScriptingStudio.GameFlowScriptStudio { Parent = this };
			else if (_ide.Project.GameVersion == TRVersion.Game.TR1)
				scriptingStudio = new ScriptingStudio.Tomb1MainStudio { Parent = this };

			scriptingStudio.Dock = DockStyle.Fill;
			tabPage_ScriptingStudio.Controls.Add(scriptingStudio);

			Text = "TombIDE - " + _ide.Project.Name;

			eventDelegate = new WinEventDelegate(WinEventProc);
			eventHook = NativeMethods.SetWinEventHook(
				NativeMethods.EVENT_SYSTEM_FOREGROUND, NativeMethods.EVENT_SYSTEM_FOREGROUND,
				IntPtr.Zero, eventDelegate, 0, 0, NativeMethods.WINEVENT_OUTOFCONTEXT);

			panel_CoverLoading.BringToFront();
		}

		public void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
		{
			if (!IsDisposed && NativeMethods.GetForegroundWindow() == Handle)
			{
				levelManager.IsMainWindowFocued = true;
				scriptingStudio.IsMainWindowFocued = true;

				scriptingStudio.EditorTabControl.TryRunFileReloadQueue();
			}

			if (!IsDisposed && NativeMethods.GetForegroundWindow() != Handle)
			{
				levelManager.IsMainWindowFocued = false;
				scriptingStudio.IsMainWindowFocued = false;
			}

			if (IsDisposed)
				NativeMethods.UnhookWinEvent(eventHook);
		}

		protected override void OnLoad(EventArgs e)
		{
			ApplySavedSettings();

			base.OnLoad(e);
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);

			using (var form = new FormLoading(_ide))
			{
				if (form.ShowDialog(this) == DialogResult.OK)
				{
					// Initialize the IDE interfaces
					sideBar.Initialize(_ide);
					levelManager.Initialize(_ide);
					miscellaneous.Initialize(_ide);

					if (_ide.Project.GameVersion == TRVersion.Game.TRNG)
						pluginManager.Initialize(_ide);

					sideBar.SelectedIDETabChanged += SideBar_SelectedIDETabChanged;
					sideBar.SelectIDETab(IDETab.LevelManager);

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

		#endregion Initialization

		#region IDE events

		private void OnIDEEventRaised(IIDEEvent obj)
		{
			if (obj is IDE.ProjectScriptPathChangedEvent || obj is IDE.ProjectLevelsPathChangedEvent)
				OnProjectPathsChanged(obj);
		}

		#endregion IDE events

		#region IDE event methods

		private void OnProjectPathsChanged(IIDEEvent obj)
		{
			DialogResult result = DarkMessageBox.Show(this,
				"To apply the changes, you must restart TombIDE.\n" +
				"Are you sure you want to do that?",
				"Are you sure?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

			if (result == DialogResult.Yes)
			{
				if (!_ide.CanClose())
				{
					// User pressed "Cancel"

					DarkMessageBox.Show(this,
						"Operation cancelled.\n" +
						"No paths have been affected.",
						"Operation cancelled", MessageBoxButtons.OK, MessageBoxIcon.Information);

					return;
				}

				if (obj is IDE.ProjectScriptPathChangedEvent)
					_ide.Project.ScriptPath = ((IDE.ProjectScriptPathChangedEvent)obj).NewPath;
				else if (obj is IDE.ProjectLevelsPathChangedEvent)
				{
					var projectLevels = new List<ProjectLevel>();
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
			{
				DarkMessageBox.Show(this,
					"Operation cancelled.\n" +
					"No paths have been affected.",
					"Operation cancelled", MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
		}

		#endregion IDE event methods

		#region Other events

		private void SideBar_SelectedIDETabChanged(object sender, IDETab e) => SelectIDETab(e);

		private void SelectIDETab(IDETab tab)
		{
			scriptingStudio.EditorTabControl.EnsureTabFileSynchronization();

			switch (tab)
			{
				case IDETab.LevelManager:
					tablessTabControl.SelectTab(0);
					break;

				case IDETab.ScriptingStudio:
					tablessTabControl.SelectTab(1);
					break;

				case IDETab.PluginManager:
					if (_ide.Project.GameVersion != TRVersion.Game.TRNG)
						break;

					tablessTabControl.SelectTab(2);
					break;

				case IDETab.Miscellaneous:
					tablessTabControl.SelectTab(3);
					break;
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
			var startInfo = new ProcessStartInfo
			{
				FileName = Assembly.GetExecutingAssembly().Location,
				Arguments = "\"" + _ide.Project.GetTrprojFilePath() + "\""
			};

			Process.Start(startInfo);
		}

		#endregion Other events
	}
}
