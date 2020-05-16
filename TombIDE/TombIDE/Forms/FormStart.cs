using DarkUI.Controls;
using DarkUI.Forms;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using TombIDE.Shared;
using TombIDE.Shared.SharedClasses;

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

			ApplySavedSettings();

			FillProjectList();
		}

		public void OpenTrprojWithTombIDE(string trprojFilePath)
		{
			try
			{
				Project openedProject = Project.FromFile(trprojFilePath);

				string errorMessage;

				if (!ProjectChecker.IsValidProject(openedProject, out errorMessage))
					throw new ArgumentException(errorMessage);

				// Check if a project with the same name but different paths already exists on the list
				foreach (DarkTreeNode node in treeView.Nodes)
				{
					Project nodeProject = (Project)node.Tag;

					if (nodeProject.Name.ToLower() == openedProject.Name.ToLower()
					&& nodeProject.ProjectPath.ToLower() != openedProject.ProjectPath.ToLower())
					{
						if (AskForProjectNodeReplacement(openedProject, node))
							break; // The user confirmed replacing the node
						else
							return; // The user declined replacing the node
					}
				}

				if (!IsProjectOnList(openedProject))
					AddProjectToList(openedProject, true);

				_ide.IDEConfiguration.RememberedProject = openedProject.Name;

				// Continue code in Program.cs
			}
			catch (Exception ex)
			{
				DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			// Automatically open remembered project (if there is one)
			if (!string.IsNullOrWhiteSpace(_ide.IDEConfiguration.RememberedProject))
			{
				SelectProjectOnList(_ide.IDEConfiguration.RememberedProject);

				if (_selectedProject != null) // If the project was found on the list
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

			// Add nodes into the treeView for each project
			foreach (Project project in _ide.AvailableProjects)
			{
				if (ProjectChecker.IsValidProject(project))
					AddProjectToList(project, false);
			}

			// Update TombIDEProjects.xml with only valid projects
			RefreshAndReserializeProjects();
		}

		#endregion Initialization

		#region Events

		private void button_Main_New_Click(object sender, EventArgs e) => CreateNewProject();
		private void button_Main_Open_Click(object sender, EventArgs e) => OpenTrproj();
		private void button_Main_Import_Click(object sender, EventArgs e) => ImportExe();

		private void button_New_Click(object sender, EventArgs e) => CreateNewProject();
		private void button_Open_Click(object sender, EventArgs e) => OpenTrproj();
		private void button_Import_Click(object sender, EventArgs e) => ImportExe();
		private void button_Rename_Click(object sender, EventArgs e) => RenameProject();
		private void button_Delete_Click(object sender, EventArgs e) => DeleteProject();

		private void button_MoveUp_Click(object sender, EventArgs e)
		{
			treeView.MoveSelectedNodeUp();
			RefreshAndReserializeProjects();
		}

		private void button_MoveDown_Click(object sender, EventArgs e)
		{
			treeView.MoveSelectedNodeDown();
			RefreshAndReserializeProjects();
		}

		private void button_OpenFolder_Click(object sender, EventArgs e) =>
			SharedMethods.OpenInExplorer(((Project)treeView.SelectedNodes[0].Tag).ProjectPath);

		private void button_OpenProject_Click(object sender, EventArgs e) => OpenSelectedProject();

		private void treeView_MouseClick(object sender, MouseEventArgs e) => CheckItemSelection();

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
			using (FormProjectSetup form = new FormProjectSetup())
			{
				if (form.ShowDialog(this) == DialogResult.OK)
				{
					AddProjectToList(form.CreatedProject, true);
					SelectProjectOnList(form.CreatedProject.Name);
				}
			}
		}

		private void OpenTrproj()
		{
			using (OpenFileDialog dialog = new OpenFileDialog())
			{
				dialog.Title = "Select the .trproj file you want to open";
				dialog.Filter = "TombIDE Project Files|*.trproj";

				if (dialog.ShowDialog(this) == DialogResult.OK)
				{
					try
					{
						Project openedProject = Project.FromFile(dialog.FileName);

						string errorMessage;

						if (!ProjectChecker.IsValidProject(openedProject, out errorMessage))
							throw new ArgumentException(errorMessage);

						// Check if a project with the same name but different paths already exists on the list
						foreach (DarkTreeNode node in treeView.Nodes)
						{
							Project nodeProject = (Project)node.Tag;

							if (nodeProject.Name.ToLower() == openedProject.Name.ToLower()
							&& nodeProject.ProjectPath.ToLower() != openedProject.ProjectPath.ToLower())
							{
								if (AskForProjectNodeReplacement(openedProject, node))
									break; // The user confirmed replacing the node
								else
									return; // The user declined replacing the node
							}
						}

						if (!IsProjectOnList(openedProject))
							AddProjectToList(openedProject, true);

						SelectProjectOnList(openedProject.Name);
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
			using (OpenFileDialog dialog = new OpenFileDialog())
			{
				dialog.Title = "Select the .exe file of the game project you want to import";
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
							if (project.EnginePath.ToLower() == Path.GetDirectoryName(gameExeFilePath).ToLower())
								throw new ArgumentException("Project already exists on the list.\nIt's called \"" + project.Name + "\"");
						}

						using (FormImportProject form = new FormImportProject(gameExeFilePath, launcherFilePath))
						{
							if (form.ShowDialog(this) == DialogResult.OK)
							{
								AddProjectToList(form.ImportedProject, true);
								SelectProjectOnList(form.ImportedProject.Name);
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
			using (FormRenameProject form = new FormRenameProject(_selectedProject))
			{
				if (form.ShowDialog(this) == DialogResult.OK)
				{
					// Update the selected node
					treeView.SelectedNodes[0].Text = _selectedProject.Name;
					treeView.SelectedNodes[0].Tag = _selectedProject;

					// Update the TombIDEProjects.xml file as well
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
			button_Rename.Enabled = treeView.SelectedNodes.Count > 0;
			button_Delete.Enabled = treeView.SelectedNodes.Count > 0;
			button_MoveUp.Enabled = treeView.SelectedNodes.Count > 0;
			button_MoveDown.Enabled = treeView.SelectedNodes.Count > 0;
			button_OpenFolder.Enabled = treeView.SelectedNodes.Count > 0;
			button_OpenProject.Enabled = treeView.SelectedNodes.Count > 0;

			// // // // // // // //
			_selectedProject = treeView.SelectedNodes.Count > 0 ? (Project)treeView.SelectedNodes[0].Tag : null;
			// // // // // // // //
		}

		private void OpenSelectedProject()
		{
			if (_selectedProject == null) // If no project is selected
				return;

			string errorMessage;

			if (!ProjectChecker.IsValidProject(_selectedProject, out errorMessage))
			{
				DarkMessageBox.Show(this, "Failed to load project. " + errorMessage, "Error",
					MessageBoxButtons.OK, MessageBoxIcon.Error);

				return;
			}

			if (!IsValidGameLauncher())
				return;

			// Set RememberedProject if checkBox_Remember is checked
			if (checkBox_Remember.Checked)
				_ide.IDEConfiguration.RememberedProject = _selectedProject.Name;

			// Save settings and hide the current window
			SaveSettings();
			Hide();

			// Show the main form of TombIDE
			using (FormMain form = new FormMain(_ide, _selectedProject))
			{
				DialogResult result = form.ShowDialog(this);

				if (result == DialogResult.OK) // OK means the user wants to switch projects
				{
					// Reset the RememberedProject setting
					_ide.IDEConfiguration.RememberedProject = string.Empty;
					_ide.IDEConfiguration.Save();

					// Restart the application (without any arguments)
					Application.Exit();
					Process.Start(Assembly.GetExecutingAssembly().Location);
				}
				else if (result == DialogResult.Cancel) // Cancel means the user closed the program
					Application.Exit();
			}
		}

		#endregion Event methods

		#region Other methods

		private void AddProjectToList(Project project, bool reserialize)
		{
			// Create the node
			DarkTreeNode node = new DarkTreeNode
			{
				Text = project.Name,
				Tag = project
			};

			// Get the icon for the node
			if (!string.IsNullOrEmpty(project.LaunchFilePath))
			{
				if (File.Exists(project.LaunchFilePath))
					node.Icon = Icon.ExtractAssociatedIcon(project.LaunchFilePath).ToBitmap();
			}

			// Add the node to the list
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

			panel_Main_Buttons.Visible = treeView.Nodes.Count == 0;
		}

		private void RepositionProjectNodeIcons() // DarkUI sucks
		{
			foreach (DarkTreeNode node in treeView.Nodes)
			{
				int iconHeight = (40 * node.VisibleIndex) + 4;
				node.IconArea = new Rectangle(new Point(4, iconHeight), new Size(32, 32));
			}

			treeView.Invalidate();
		}

		private void SelectProjectOnList(string projectName)
		{
			foreach (DarkTreeNode node in treeView.Nodes)
			{
				if (((Project)node.Tag).Name.ToLower() == projectName.ToLower())
				{
					treeView.SelectNode(node);
					CheckItemSelection();

					break;
				}
			}
		}

		private bool AskForProjectNodeReplacement(Project openedProject, DarkTreeNode projectNode)
		{
			DialogResult result = DarkMessageBox.Show(this,
				"A project with the same name but different paths already exists on the list.\n" +
				"Would you like to replace the project on the list with the currently opened one?",
				"Project name duplicate found", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

			if (result == DialogResult.Yes)
			{
				treeView.Nodes.Remove(projectNode);
				RefreshAndReserializeProjects();

				AddProjectToList(openedProject, true);

				return true;
			}
			else
				return false;
		}

		private bool IsProjectOnList(Project project)
		{
			foreach (DarkTreeNode node in treeView.Nodes)
			{
				if (((Project)node.Tag).Name.ToLower() == project.Name.ToLower())
					return true;
			}

			return false;
		}

		private bool IsValidGameLauncher()
		{
			// Check if the launcher file exists, if not, then try to find it
			if (!File.Exists(_selectedProject.LaunchFilePath))
			{
				try
				{
					string launchFilePath = FileFinder.GetLauncherPathFromProject(_selectedProject);

					_selectedProject.LaunchFilePath = launchFilePath;
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
