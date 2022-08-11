using DarkUI.Config;
using DarkUI.Controls;
using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using TombIDE.Shared;
using TombIDE.Shared.SharedClasses;
using TombLib.LevelData;

namespace TombIDE
{
	public partial class FormStart : DarkForm
	{
		private IDE _ide;
		private Project _selectedProject;

		#region Initialization

		public FormStart(IDE ide)
		{
			_ide = ide;

			InitializeComponent();
			Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);

			ApplySavedSettings();

			FillProjectList();
		}

		public void OpenTrprojWithTombIDE(string trprojFilePath)
		{
			try
			{
				var openedProject = Project.FromFile(trprojFilePath);

				if (!ProjectChecker.IsValidProject(openedProject, out string errorMessage))
					throw new ArgumentException(errorMessage);

				if (!IsProjectOnList(openedProject))
					AddProjectToList(openedProject, true);

				_ide.IDEConfiguration.RememberedProject = openedProject.ProjectPath;

				// Continue code in Program.cs
			}
			catch (Exception ex)
			{
				DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			if (!string.IsNullOrWhiteSpace(_ide.IDEConfiguration.RememberedProject))
			{
				SelectProjectOnList(_ide.IDEConfiguration.RememberedProject);

				if (_selectedProject != null)
				{
					OpenSelectedProject();
					return; // Don't go to base.OnLoad(e) to avoid window duplicates
				}
			}

			base.OnLoad(e);
		}

		protected override void OnClosed(EventArgs e)
		{
			SaveSettings();

			base.OnClosed(e);
		}

		private void ApplySavedSettings()
		{
			Size = _ide.IDEConfiguration.Start_WindowSize;

			if (_ide.IDEConfiguration.Start_OpenMaximized)
				WindowState = FormWindowState.Maximized;
		}

		private void SaveSettings()
		{
			_ide.IDEConfiguration.Start_OpenMaximized = WindowState == FormWindowState.Maximized;

			if (WindowState == FormWindowState.Normal)
				_ide.IDEConfiguration.Start_WindowSize = Size;
			else
				_ide.IDEConfiguration.Start_WindowSize = RestoreBounds.Size;

			_ide.IDEConfiguration.Save();
		}

		private void FillProjectList()
		{
			treeView.Nodes.Clear();

			IEnumerable<Project> validProjects = _ide.AvailableProjects
				.Where(project => ProjectChecker.IsValidProject(project));

			foreach (Project project in validProjects)
				AddProjectToList(project, false);

			RefreshAndReserializeProjects(); // Update TombIDEProjects.xml with only valid projects
		}

		#endregion Initialization

		#region Events

		private void button_New_Child_MouseEnter(object sender, EventArgs e) => panelButton_New.Capture = true;
		private void button_Open_Child_MouseEnter(object sender, EventArgs e) => panelButton_Open.Capture = true;
		private void button_Import_Child_MouseEnter(object sender, EventArgs e) => panelButton_Import.Capture = true;

		private void panelButton_MouseEnter(object sender, EventArgs e) => (sender as Control).BackColor = Colors.LighterBackground;
		private void panelButton_MouseDown(object sender, MouseEventArgs e) => (sender as Control).BackColor = Colors.DarkBackground;
		private void panelButton_MouseLeave(object sender, EventArgs e) => (sender as Control).BackColor = Colors.LightBackground;

		private void panelButton_New_MouseUp(object sender, MouseEventArgs e) => CreateNewProject();
		private void panelButton_Open_MouseUp(object sender, MouseEventArgs e) => OpenTrproj();
		private void panelButton_Import_MouseUp(object sender, MouseEventArgs e) => ImportExe();

		private void panelButton_New_MouseMove(object sender, MouseEventArgs e)
		{
			if (!panelButton_New.ClientRectangle.Contains(e.Location))
				panelButton_New.Capture = false;
		}

		private void panelButton_Open_MouseMove(object sender, MouseEventArgs e)
		{
			if (!panelButton_Open.ClientRectangle.Contains(e.Location))
				panelButton_Open.Capture = false;
		}

		private void panelButton_Import_MouseMove(object sender, MouseEventArgs e)
		{
			if (!panelButton_Import.ClientRectangle.Contains(e.Location))
				panelButton_Import.Capture = false;
		}

		private void contextMenuItem_Rename_Click(object sender, EventArgs e) => RenameProject();
		private void contextMenuItem_Delete_Click(object sender, EventArgs e) => DeleteProject();

		private void contextMenuItem_MoveUp_Click(object sender, EventArgs e)
		{
			treeView.MoveSelectedNodeUp();
			RefreshAndReserializeProjects();
		}

		private void contextMenuItem_MoveDown_Click(object sender, EventArgs e)
		{
			treeView.MoveSelectedNodeDown();
			RefreshAndReserializeProjects();
		}

		private void contextMenuItem_OpenDirectory_Click(object sender, EventArgs e) =>
			SharedMethods.OpenInExplorer(((Project)treeView.SelectedNodes[0].Tag).ProjectPath);

		private void button_OpenProject_Click(object sender, EventArgs e) => OpenSelectedProject();

		private void treeView_SelectedNodesChanged(object sender, EventArgs e) => CheckItemSelection();

		private void treeView_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
				OpenSelectedProject();
		}

		private void treeView_KeyDown(object sender, KeyEventArgs e)
		{
			if (treeView.SelectedNodes.Count == 0)
				return;

			if (e.KeyCode == Keys.F2)
				RenameProject();

			if (e.KeyCode == Keys.Delete)
				DeleteProject();
		}

		private void treeView_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down)
				CheckItemSelection();
		}

		#endregion Events

		#region Event methods

		private void CreateNewProject()
		{
			using (var form = new FormProjectSetup())
			{
				if (form.ShowDialog(this) == DialogResult.OK)
				{
					AddProjectToList(form.CreatedProject, true);
					SelectProjectOnList(form.CreatedProject.ProjectPath);
				}
			}
		}

		private void OpenTrproj()
		{
			using (var dialog = new OpenFileDialog())
			{
				dialog.Title = "Select the .trproj file you want to open.";
				dialog.Filter = "TombIDE Project Files|*.trproj";

				if (dialog.ShowDialog(this) == DialogResult.OK)
				{
					try
					{
						var openedProject = Project.FromFile(dialog.FileName);

						if (!ProjectChecker.IsValidProject(openedProject, out string errorMessage))
							throw new ArgumentException(errorMessage);

						if (!IsProjectOnList(openedProject))
							AddProjectToList(openedProject, true);

						SelectProjectOnList(openedProject.ProjectPath);
					}
					catch (Exception ex)
					{
						DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					}
				}
			}
		}

		private void ImportExe()
		{
			using (var dialog = new OpenFileDialog())
			{
				dialog.Title = "Select the .exe file of the game project you want to import.";
				dialog.Filter = "Executable Files|*.exe";

				if (dialog.ShowDialog(this) == DialogResult.OK)
				{
					try
					{
						string gameExeFilePath = FileFinder.GetGameExePathFromSelectedFilePath(dialog.FileName);
						string launcherFilePath = FileFinder.GetLauncherPathFromSelectedFilePath(dialog.FileName);

						// Check if a project that's using the same EnginePath exists on the list
						foreach (Project project in _ide.AvailableProjects)
						{
							if (project.EnginePath.Equals(Path.GetDirectoryName(gameExeFilePath), StringComparison.OrdinalIgnoreCase))
							{
								SelectProjectOnList(project.ProjectPath);
								return;
							}
						}

						using (var form = new FormImportProject(gameExeFilePath, launcherFilePath))
						{
							if (form.ShowDialog(this) == DialogResult.OK)
							{
								if (!IsProjectOnList(form.ImportedProject))
									AddProjectToList(form.ImportedProject, true);

								SelectProjectOnList(form.ImportedProject.ProjectPath);
							}
						}
					}
					catch (Exception ex)
					{
						DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					}
				}
			}
		}

		private void RenameProject()
		{
			using (var form = new FormRenameProject(_selectedProject))
			{
				if (form.ShowDialog(this) == DialogResult.OK)
				{
					var node = treeView.SelectedNodes[0] as DarkTreeNodeEx;

					node.Text = _selectedProject.Name;
					node.SubText = _selectedProject.ProjectPath;
					node.Tag = _selectedProject;

					RefreshAndReserializeProjects();
				}
			}
		}

		private void DeleteProject()
		{
			DialogResult result = DarkMessageBox.Show(this,
				"Are you sure you want to delete the \"" + _selectedProject.Name + "\" project from the list?\n" +
				"This process will NOT affect the project folder nor its files.", "Are you sure?",
				MessageBoxButtons.YesNo, MessageBoxIcon.Question);

			if (result == DialogResult.Yes)
			{
				// Remove the node and clear selection
				treeView.SelectedNodes[0].Remove();
				treeView.SelectedNodes.Clear();

				CheckItemSelection();

				// Update TombIDEProjects.xml with the new list (without the removed node)
				RefreshAndReserializeProjects();
			}
		}

		private void CheckItemSelection()
		{
			// Enable / Disable node specific buttons
			contextMenuItem_Rename.Enabled = treeView.SelectedNodes.Count > 0;
			contextMenuItem_Delete.Enabled = treeView.SelectedNodes.Count > 0;
			contextMenuItem_MoveUp.Enabled = treeView.SelectedNodes.Count > 0;
			contextMenuItem_MoveDown.Enabled = treeView.SelectedNodes.Count > 0;
			contextMenuItem_OpenDirectory.Enabled = treeView.SelectedNodes.Count > 0;
			contextMenuItem_OpenProject.Enabled = treeView.SelectedNodes.Count > 0;

			checkBox_Remember.Enabled = treeView.SelectedNodes.Count > 0;
			button_OpenProject.Enabled = treeView.SelectedNodes.Count > 0;

			// // // // // // // //
			_selectedProject = treeView.SelectedNodes.Count > 0 ? (Project)treeView.SelectedNodes[0].Tag : null;
			// // // // // // // //
		}

		private void OpenSelectedProject()
		{
			if (_selectedProject == null)
				return;

			if (!ProjectChecker.IsValidProject(_selectedProject, out string errorMessage))
			{
				DarkMessageBox.Show(this, "Failed to load project. " + errorMessage, "Error",
					MessageBoxButtons.OK, MessageBoxIcon.Error);

				return;
			}

			if (!IsValidGameLauncher())
				return;

			if (checkBox_Remember.Checked)
				_ide.IDEConfiguration.RememberedProject = _selectedProject.ProjectPath;

			SaveSettings();
			Hide();

			using (var form = new FormMain(_ide, _selectedProject))
			{
				DialogResult result = form.ShowDialog(this);

				if (result == DialogResult.OK) // OK means the user wants to switch projects
				{
					// Reset the RememberedProject setting
					_ide.IDEConfiguration.RememberedProject = string.Empty;
					_ide.IDEConfiguration.Save();

					// Restart the application (without any arguments)
					Application.Restart();
				}
				else if (result == DialogResult.Cancel) // Cancel means the user closed the program
					Application.Exit();
			}
		}

		#endregion Event methods

		#region Other methods

		private void AddProjectToList(Project project, bool reserialize)
		{
			var node = new DarkTreeNodeEx(project.Name, project.ProjectPath) { Tag = project };

			switch (project.GameVersion)
			{
				case TRVersion.Game.TombEngine: node.ExtraIcon = Properties.Resources.TEN_LVL; break;
				case TRVersion.Game.TRNG: node.ExtraIcon = Properties.Resources.TRNG_LVL; break;
				case TRVersion.Game.TR4: node.ExtraIcon = Properties.Resources.TR4_LVL; break;
				case TRVersion.Game.TR3: node.ExtraIcon = Properties.Resources.TR3_LVL; break;
				case TRVersion.Game.TR2: node.ExtraIcon = Properties.Resources.TR2_LVL; break;
				case TRVersion.Game.TR1: node.ExtraIcon = Properties.Resources.TR1_LVL; break;
			}

			if (!string.IsNullOrEmpty(project.LaunchFilePath) && File.Exists(project.LaunchFilePath))
				node.Icon = Icon.ExtractAssociatedIcon(project.LaunchFilePath).ToBitmap();

			treeView.Nodes.Add(node);

			if (reserialize)
				RefreshAndReserializeProjects();
		}

		private void RefreshAndReserializeProjects()
		{
			RepositionProjectNodeIcons();

			_ide.AvailableProjects.Clear();

			foreach (DarkTreeNode node in treeView.Nodes)
				_ide.AvailableProjects.Add((Project)node.Tag);

			XmlHandling.UpdateProjectsXml(_ide.AvailableProjects);
		}

		private void RepositionProjectNodeIcons() // DarkUI sucks
		{
			const int LEFT_ICON_MARGIN = 8;
			const int LEFT_TEXT_MARGIN = 45;
			const int UNIFORM_ICON_SIZE = 32;

			foreach (DarkTreeNodeEx node in treeView.Nodes)
			{
				int iconYPos = (treeView.ItemHeight * node.VisibleIndex) + ((treeView.ItemHeight - UNIFORM_ICON_SIZE) / 2);

				node.IconArea = new Rectangle(new Point(LEFT_ICON_MARGIN, iconYPos), new Size(UNIFORM_ICON_SIZE, UNIFORM_ICON_SIZE));
				node.TextArea = new Rectangle(new Point(LEFT_TEXT_MARGIN, node.TextArea.Y), node.TextArea.Size);
				node.SubTextArea = new Rectangle(new Point(LEFT_TEXT_MARGIN + 1, node.SubTextArea.Y), node.SubTextArea.Size);
				node.ExtraIconArea = new Rectangle(new Point(node.ExtraIconArea.X + 6, node.ExtraIconArea.Y), node.ExtraIconArea.Size);
			}

			treeView.Invalidate();
		}

		private void SelectProjectOnList(string projectPath)
		{
			DarkTreeNode targetNode = treeView.Nodes
				.FirstOrDefault(node => node.Tag is Project p
					&& p.ProjectPath.Equals(projectPath, StringComparison.OrdinalIgnoreCase));

			if (targetNode == null)
				return;

			treeView.SelectNode(targetNode);
			CheckItemSelection();
		}

		private bool IsProjectOnList(Project project)
		{
			return treeView.Nodes
				.Any(node => node.Tag is Project p
					&& p.ProjectPath.Equals(project.ProjectPath, StringComparison.OrdinalIgnoreCase));
		}

		private bool IsValidGameLauncher()
		{
			if (!File.Exists(_selectedProject.LaunchFilePath))
			{
				try
				{
					_selectedProject.LaunchFilePath = FileFinder.GetLauncherPathFromProject(_selectedProject);
					_selectedProject.Save();

					return true;
				}
				catch (Exception ex)
				{
					DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					return false;
				}
			}
			else
				return true;
		}

		#endregion Other methods
	}
}
