using DarkUI.Controls;
using DarkUI.Forms;
using System;
using System.Drawing;
using System.IO;
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
					Project nodeProject = (Project)node.Tag;

					if (nodeProject.Name == addedProject.Name)
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

		protected override void OnLoad(EventArgs e)
		{
			// Automatically open remembered project (if there is one)
			if (!string.IsNullOrWhiteSpace(_ide.Configuration.RememberedProject))
			{
				// Find the node where the project.Name is the same as the RememberedProject string
				foreach (DarkTreeNode node in treeView.Nodes)
				{
					Project nodeProject = (Project)node.Tag;

					if (nodeProject.Name == _ide.Configuration.RememberedProject)
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

		private void SaveSettings()
		{
			_ide.Configuration.Start_OpenMaximized = WindowState == FormWindowState.Maximized;

			if (WindowState == FormWindowState.Normal)
				_ide.Configuration.Start_WindowSize = Size;
			else
				_ide.Configuration.Start_WindowSize = RestoreBounds.Size;

			_ide.Configuration.Save();
		}

		private void ApplySavedSettings()
		{
			Size = _ide.Configuration.Start_WindowSize;

			if (_ide.Configuration.Start_OpenMaximized)
				WindowState = FormWindowState.Maximized;
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
			string exeFilePath = Path.Combine(project.ProjectPath, project.GetExeFileName());
			Bitmap icon = Icon.ExtractAssociatedIcon(exeFilePath).ToBitmap();

			// Create the node
			DarkTreeNode node = new DarkTreeNode
			{
				Icon = icon,
				Text = project.Name,
				Tag = project
			};

			// Add the node to the list
			treeView.Nodes.Add(node);

			if (reserialize)
				RefreshAndReserializeProjects();
		}

		#endregion Initialization

		#region Events

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

		private void ShowProjectSetupForm()
		{
			using (FormProjectSetup form = new FormProjectSetup(_ide))
				form.ShowDialog(this); // After the form is done, it will trigger the IDE.ProjectAddedEvent
		}

		private void OpenTRPROJ()
		{
			OpenFileDialog dialog = new OpenFileDialog
			{
				Title = "Choose the .trproj file you want to open",
				Filter = "TombIDE Project Files|*.trproj"
			};

			if (dialog.ShowDialog(this) == DialogResult.OK)
			{
				try
				{
					Project openedProject = XmlHandling.ReadTRPROJ(dialog.FileName);

					// Check for name duplicates
					foreach (Project project in _ide.AvailableProjects)
					{
						if (project.Name == openedProject.Name)
							throw new ArgumentException("A project with the same name already exists on the list.");
					}

					// Check if the opened project is valid
					if (!openedProject.IsValidProject())
						throw new ArgumentException("Opened project is invalid. Please check if the project is correctly installed.");

					_ide.AddProjectToList(openedProject); // Triggers the IDE.ProjectAddedEvent
				}
				catch (Exception ex)
				{
					DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
		}

		private void ImportExe()
		{
			OpenFileDialog dialog = new OpenFileDialog
			{
				Title = "Choose the .exe file of the game project you want to import",
				Filter = "Executable Files|*.exe"
			};

			if (dialog.ShowDialog(this) == DialogResult.OK)
			{
				try
				{
					string exeFilePath = dialog.FileName;

					// Check if the imported .exe file is a TR game that's actually supported
					if (Path.GetFileName(exeFilePath).ToLower() != "tomb4.exe" && Path.GetFileName(exeFilePath).ToLower() != "pctomb5.exe")
						throw new ArgumentException("Invalid game .exe file.");

					// Check if a project that's using the same .exe file already exists on the list
					foreach (Project project in _ide.AvailableProjects)
					{
						if (project.ProjectPath == Path.GetDirectoryName(exeFilePath))
							throw new ArgumentException("Project already exists on the list.\nIt's called \"" + project.Name + "\"");
					}

					using (FormImportProject form = new FormImportProject(_ide, exeFilePath))
						form.ShowDialog(this); // After the form is done, it will trigger the IDE.ProjectAddedEvent
				}
				catch (Exception ex)
				{
					DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
		}

		private void ShowRenameProjectForm()
		{
			using (FormRenameProject form = new FormRenameProject(_ide))
				form.ShowDialog(this); // After the form is done, it will trigger the IDE.ActiveProjectRenamedEvent
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
			treeView.Invalidate();
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
			treeView.Invalidate();
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
			// Triggers the IDE.ActiveProjectChangedEvent
			if (treeView.SelectedNodes.Count > 0)
				_ide.Project = (Project)treeView.SelectedNodes[0].Tag;
			else
				_ide.Project = null;
		}

		private void OpenSelectedProject()
		{
			if (_ide.Project == null)
				return;

			if (!_ide.Project.IsValidProject())
				return;

			// Set the RememberedProject if checkBox_Remember is checked
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
					// Reset the RememberedProject
					_ide.Configuration.RememberedProject = string.Empty;
					_ide.Configuration.Save();

					// Restart the application
					Application.Restart();

					// Using Show(); instead would cause many ObjectDisposed exceptions
				}
				else if (result == DialogResult.Cancel) // Cancel means the user closed the program
					Application.Exit();
			}
		}

		#endregion Events

		#region Methods

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
			foreach (DarkTreeNode node in treeView.Nodes)
			{
				int iconHeight = (40 * node.VisibleIndex) + 4;
				node.IconArea = new Rectangle(new Point(4, iconHeight), new Size(32, 32));
			}
		}

		#endregion Methods
	}
}
