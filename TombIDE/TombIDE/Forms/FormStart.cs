using DarkUI.Controls;
using DarkUI.Forms;
using System;
using System.Collections.Generic;
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
	public partial class FormStart : DarkForm
	{
		private IDE _ide;

		#region Initialization

		public FormStart(IDE ide)
		{
			_ide = ide;
			_ide.IDEEventRaised += OnIDEEventRaised;

			InitializeComponent();

			ApplySavedSettings();
			FillProjectList(); // _ide.AvailableProjects is taken from the TombIDEProjects.xml file
		}

		public void OpenTRPROJWithTombIDE(string path)
		{
			try
			{
				Project openedProject = XmlHandling.ReadTRPROJ(path);

				if (!openedProject.IsValidProject())
					throw new ArgumentException("Opened project is invalid. Please check if the project is correctly installed.");

				// Check if a project with the same name but different paths already exists on the list
				foreach (DarkTreeNode node in treeView.Nodes)
				{
					Project project = (Project)node.Tag;

					if (project.Name.ToLower() == openedProject.Name.ToLower() && project.ProjectPath.ToLower() != openedProject.ProjectPath.ToLower())
					{
						DialogResult result = DarkMessageBox.Show(this,
							"A project with the same name but different paths already exists on the list.\n" +
							"Would you like to replace the project on the list with the currently opened one?",
							"Project name duplicate found", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

						if (result == DialogResult.Yes)
						{
							treeView.Nodes.Remove(node);
							RefreshAndReserializeProjects();

							break;
						}
						else
							return;
					}
				}

				if (!IsProjectOnList(openedProject))
					_ide.AddProjectToList(openedProject); // Triggers IDE.ProjectAddedEvent

				_ide.Configuration.RememberedProject = openedProject.Name;

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
			if (!string.IsNullOrWhiteSpace(_ide.Configuration.RememberedProject))
			{
				foreach (DarkTreeNode node in treeView.Nodes)
				{
					if (((Project)node.Tag).Name.ToLower() == _ide.Configuration.RememberedProject.ToLower())
					{
						treeView.SelectNode(node);
						CheckItemSelection();

						OpenSelectedProject();
						return;
					}
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
			Size = _ide.Configuration.Start_WindowSize;

			if (_ide.Configuration.Start_OpenMaximized)
				WindowState = FormWindowState.Maximized;
		}

		private void SaveSettings()
		{
			_ide.Configuration.Start_OpenMaximized = WindowState == FormWindowState.Maximized;

			if (WindowState == FormWindowState.Normal)
				_ide.Configuration.Start_WindowSize = Size;
			else
				_ide.Configuration.Start_WindowSize = RestoreBounds.Size;

			_ide.Configuration.Save();
		}

		private void FillProjectList()
		{
			treeView.Nodes.Clear();

			// Add nodes into the treeView for each project
			foreach (Project project in _ide.AvailableProjects)
			{
				if (project.IsValidProject())
					AddProjectToList(project);
			}

			// Update TombIDEProjects.xml with only valid projects
			RefreshAndReserializeProjects();
		}

		private void AddProjectToList(Project project, bool reserialize = false)
		{
			// Create the node
			DarkTreeNode node = new DarkTreeNode
			{
				Text = project.Name,
				Tag = project
			};

			if (!string.IsNullOrEmpty(project.LaunchFilePath))
				node.Icon = Icon.ExtractAssociatedIcon(project.LaunchFilePath).ToBitmap();

			// Add the node to the list
			treeView.Nodes.Add(node);

			if (reserialize)
				RefreshAndReserializeProjects();
		}

		#endregion Initialization

		#region Events

		private void OnIDEEventRaised(IIDEEvent obj)
		{
			if (obj is IDE.ProjectAddedEvent)
			{
				// Add the project to the list
				Project addedProject = ((IDE.ProjectAddedEvent)obj).AddedProject;
				AddProjectToList(addedProject, true);

				// Select the new project node
				foreach (DarkTreeNode node in treeView.Nodes)
				{
					if (((Project)node.Tag).Name.ToLower() == addedProject.Name.ToLower())
					{
						treeView.SelectNode(node);
						CheckItemSelection();

						break;
					}
				}
			}
			else if (obj is IDE.ActiveProjectRenamedEvent)
			{
				// Update the selected node
				treeView.SelectedNodes[0].Text = _ide.Project.Name;
				treeView.SelectedNodes[0].Tag = _ide.Project;

				// Update the TombIDEProjects.xml file as well
				RefreshAndReserializeProjects();
			}
		}

		private void button_Main_New_Click(object sender, EventArgs e) => ShowProjectSetupForm();
		private void button_Main_Open_Click(object sender, EventArgs e) => OpenTRPROJ();
		private void button_Main_Import_Click(object sender, EventArgs e) => ImportExe();

		private void button_New_Click(object sender, EventArgs e) => ShowProjectSetupForm();
		private void button_Open_Click(object sender, EventArgs e) => OpenTRPROJ();
		private void button_Import_Click(object sender, EventArgs e) => ImportExe();
		private void button_Rename_Click(object sender, EventArgs e) => ShowRenameProjectForm();
		private void button_Delete_Click(object sender, EventArgs e) => DeleteProject();
		private void button_MoveUp_Click(object sender, EventArgs e) => MoveProjectUpOnList();
		private void button_MoveDown_Click(object sender, EventArgs e) => MoveProjectDownOnList();

		private void button_OpenFolder_Click(object sender, EventArgs e) =>
			SharedMethods.OpenFolderInExplorer(((Project)treeView.SelectedNodes[0].Tag).ProjectPath);

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
				ShowRenameProjectForm();

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

		private void ShowProjectSetupForm()
		{
			using (FormProjectSetup form = new FormProjectSetup(_ide))
				form.ShowDialog(this); // After the form is done, it will trigger IDE.ProjectAddedEvent
		}

		private void OpenTRPROJ()
		{
			using (OpenFileDialog dialog = new OpenFileDialog())
			{
				dialog.Title = "Choose the .trproj file you want to open";
				dialog.Filter = "TombIDE Project Files|*.trproj";

				if (dialog.ShowDialog(this) == DialogResult.OK)
				{
					try
					{
						Project openedProject = XmlHandling.ReadTRPROJ(dialog.FileName);

						if (!openedProject.IsValidProject())
							throw new ArgumentException("Opened project is invalid. Please check if the project is correctly installed.");

						// Check for name duplicates
						foreach (Project project in _ide.AvailableProjects)
						{
							if (project.Name.ToLower() == openedProject.Name.ToLower())
								throw new ArgumentException("A project with the same name already exists on the list.");
						}

						_ide.AddProjectToList(openedProject); // Triggers IDE.ProjectAddedEvent
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
				dialog.Title = "Choose the .exe file of the game project you want to import";
				dialog.Filter = "Executable Files|*.exe";

				if (dialog.ShowDialog(this) == DialogResult.OK)
				{
					string selectedExePath = dialog.FileName;

					string selectedExeName = Path.GetFileName(selectedExePath);
					string selectedExeDirectoryName = Path.GetFileName(Path.GetDirectoryName(selectedExePath));

					try
					{
						string gameExeFilePath = string.Empty;
						string launcherFilePath = string.Empty;

						string engineDirectory = Path.Combine(Path.GetDirectoryName(selectedExePath), "Engine");

						if (selectedExeName.ToLower() == "tomb4.exe" && selectedExeDirectoryName.ToLower() != "engine")
						{
							gameExeFilePath = selectedExePath;
							launcherFilePath = selectedExePath;
						}
						else if (selectedExeName.ToLower() == "tomb4.exe" && selectedExeDirectoryName.ToLower() == "engine")
						{
							gameExeFilePath = selectedExePath;

							string prevDirectory = Path.GetDirectoryName(Path.GetDirectoryName(selectedExePath));
							List<string> launcherFiles = Project.GetLauncherExecutablesFromDirectory(prevDirectory);

							if (launcherFiles.Count == 1)
								launcherFilePath = launcherFiles.First();
							else if (launcherFiles.Count > 1)
							{
								DarkMessageBox.Show(this, "Selected project contains more than one launcher executable.\n" +
									"Please specify which one you want to use.", "Warning",
										MessageBoxButtons.OK, MessageBoxIcon.Warning);

								using (FormFileSelection form = new FormFileSelection(launcherFiles.ToArray()))
								{
									if (form.ShowDialog(this) == DialogResult.OK)
										launcherFilePath = form.SelectedFile;
									else
										return;
								}
							}
							else
								throw new ArgumentException("Selected project doesn't contain any launcher executable.\n" +
									"Please check if the project is correctly installed.");
						}
						else if (selectedExeName.ToLower() != "tomb4.exe" && Directory.Exists(engineDirectory))
						{
							foreach (string file in Directory.GetFiles(engineDirectory, "*.exe", SearchOption.TopDirectoryOnly))
							{
								if (Path.GetFileName(file).ToLower() == "tomb4.exe")
								{
									gameExeFilePath = file;
									launcherFilePath = selectedExePath;

									break;
								}
							}
						}

						if (string.IsNullOrEmpty(gameExeFilePath) || string.IsNullOrEmpty(launcherFilePath))
							throw new ArgumentException("Invalid game .exe file.");

						// Check if a project that's using the same .exe file exists on the list
						foreach (Project project in _ide.AvailableProjects)
						{
							if (project.EnginePath.ToLower() == Path.GetDirectoryName(gameExeFilePath).ToLower())
								throw new ArgumentException("Project already exists on the list.\nIt's called \"" + project.Name + "\"");
						}

						using (FormImportProject form = new FormImportProject(_ide, gameExeFilePath, launcherFilePath))
							form.ShowDialog(this); // After the form is done, it will trigger IDE.ProjectAddedEvent
					}
					catch (Exception ex)
					{
						DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					}
				}
			}
		}

		private void ShowRenameProjectForm()
		{
			using (FormRenameProject form = new FormRenameProject(_ide))
				form.ShowDialog(this); // After the form is done, it will trigger IDE.ActiveProjectRenamedEvent
		}

		private void DeleteProject()
		{
			Project affectedProject = (Project)treeView.SelectedNodes[0].Tag;

			DialogResult result = DarkMessageBox.Show(this,
				"Are you sure you want to delete the \"" + affectedProject.Name + "\" project from the list?\n" +
				"This process will NOT affect the project folder nor its files.", "Are you sure?",
				MessageBoxButtons.YesNo, MessageBoxIcon.Question);

			if (result == DialogResult.Yes)
			{
				// Remove the node and clear selection
				treeView.SelectedNodes[0].Remove();
				treeView.SelectedNodes.Clear();
				CheckItemSelection();

				// Update TombIDEProjects.xml with the new list (without the deleted node)
				RefreshAndReserializeProjects();
			}
		}

		private void MoveProjectUpOnList()
		{
			if (treeView.SelectedNodes[0].VisibleIndex == 0)
				return; // Node can't be moved higher because it's already at 0

			DarkTreeNode[] cachedNodes = treeView.Nodes.ToArray();
			treeView.Nodes.Clear();

			for (int i = 0; i < cachedNodes.Length; i++)
			{
				if (i + 1 == treeView.SelectedNodes[0].VisibleIndex)
				{
					treeView.Nodes.Add(cachedNodes[i + 1]);
					treeView.Nodes.Add(cachedNodes[i]);
					i++;
				}
				else
					treeView.Nodes.Add(cachedNodes[i]);
			}

			treeView.ScrollTo(treeView.SelectedNodes[0].FullArea.Location);
			RefreshAndReserializeProjects();
		}

		private void MoveProjectDownOnList()
		{
			if (treeView.SelectedNodes[0].VisibleIndex == treeView.Nodes.Count - 1)
				return; // Node can't be moved lower because it's already at the bottom

			DarkTreeNode[] cachedNodes = treeView.Nodes.ToArray();
			treeView.Nodes.Clear();

			for (int i = 0; i < cachedNodes.Length; i++)
			{
				if (i == treeView.SelectedNodes[0].VisibleIndex)
				{
					treeView.Nodes.Add(cachedNodes[i + 1]);
					treeView.Nodes.Add(cachedNodes[i]);
					i++;
				}
				else
					treeView.Nodes.Add(cachedNodes[i]);
			}

			treeView.ScrollTo(treeView.SelectedNodes[0].FullArea.Location);
			RefreshAndReserializeProjects();
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

			// Set the active project if a node is selected
			// Triggers IDE.ActiveProjectChangedEvent
			_ide.Project = treeView.SelectedNodes.Count > 0 ? (Project)treeView.SelectedNodes[0].Tag : null;
		}

		private void OpenSelectedProject()
		{
			if (_ide.Project == null)
				return;

			if (!_ide.Project.IsValidProject())
				return;

			if (!File.Exists(_ide.Project.LaunchFilePath))
			{
				string[] exeFiles = Directory.GetFiles(_ide.Project.ProjectPath, "*.exe", SearchOption.TopDirectoryOnly);

				if (exeFiles.Length > 1)
				{
					DarkMessageBox.Show(this, "Couldn't find the project's launch.exe file. Please select its new location.", "Warning",
						MessageBoxButtons.OK, MessageBoxIcon.Warning);

					ShowFindLauncherDialog(_ide.Project);
				}
				else
				{
					_ide.Project.LaunchFilePath = exeFiles.First();
					XmlHandling.SaveTRPROJ(_ide.Project);
				}
			}

			// Set RememberedProject if checkBox_Remember is checked
			if (checkBox_Remember.Checked)
				_ide.Configuration.RememberedProject = _ide.Project.Name;

			SaveSettings();

			// Hide the project selection window (this one)
			Hide();

			// Show the main form of TombIDE
			using (FormMain form = new FormMain(_ide))
			{
				DialogResult result = form.ShowDialog(this);

				if (result == DialogResult.OK) // OK means the user wants to switch projects
				{
					// Reset the RememberedProject setting
					_ide.Configuration.RememberedProject = string.Empty;
					_ide.Configuration.Save();

					// Restart the application
					Application.Exit();
					Process.Start(Assembly.GetExecutingAssembly().Location);

					// Using Show() instead would cause ObjectDisposed exceptions
				}
				else if (result == DialogResult.Cancel) // Cancel means the user closed the program
					Application.Exit();
			}
		}

		private void ShowFindLauncherDialog(Project project)
		{
			try
			{
				using (OpenFileDialog dialog = new OpenFileDialog())
				{
					dialog.Title = "Choose the launch.exe file of the current game project";
					dialog.Filter = "Executable Files|*.exe";
					dialog.InitialDirectory = project.ProjectPath;

					if (dialog.ShowDialog(this) == DialogResult.OK)
					{
						if (Path.GetDirectoryName(dialog.FileName).ToLower() != project.ProjectPath.ToLower())
							throw new ArgumentException("This is not the project folder. Please try again.");

						project.LaunchFilePath = dialog.FileName;
						XmlHandling.SaveTRPROJ(project);
					}
				}
			}
			catch (Exception ex)
			{
				DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				ShowFindLauncherDialog(project);
			}
		}

		#endregion Event methods

		#region Other methods

		private void RefreshAndReserializeProjects()
		{
			RepositionProjectNodeIcons();
			treeView.Invalidate();

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

		#endregion Other methods
	}
}
