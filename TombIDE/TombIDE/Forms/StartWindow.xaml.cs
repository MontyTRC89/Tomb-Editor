using DarkUI.Controls;
using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using TombIDE.Shared;
using TombIDE.Shared.SharedClasses;
using TombLib.LevelData;

namespace TombIDE.Forms
{
	public class Person
	{
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string PhoneNumber { get; set; }
		public DateTime DateOfBirth { get; set; }
	}

	/// <summary>
	/// Interaction logic for StartWindow.xaml
	/// </summary>
	public partial class StartWindow : Window
	{
		private IDE _ide;
		private Project _selectedProject;

		#region Initialization

		public StartWindow(IDE ide)
		{
			_ide = ide;

			InitializeComponent();

			ApplySavedSettings();

			FillProjectList();

			Person[] people = new[]
			{
				new Person { FirstName = "John", LastName = "Doe", PhoneNumber = "555-1234", DateOfBirth = new DateTime(1970, 1, 1) },
				new Person { FirstName = "John", LastName = "Doe", PhoneNumber = "555-1234", DateOfBirth = new DateTime(1970, 1, 1) },
				new Person { FirstName = "John", LastName = "Doe", PhoneNumber = "555-1234", DateOfBirth = new DateTime(1970, 1, 1) },
				new Person { FirstName = "John", LastName = "Doe", PhoneNumber = "555-1234", DateOfBirth = new DateTime(1970, 1, 1) },
				new Person { FirstName = "John", LastName = "Doe", PhoneNumber = "555-1234", DateOfBirth = new DateTime(1970, 1, 1) },
				new Person { FirstName = "John", LastName = "Doe", PhoneNumber = "555-1234", DateOfBirth = new DateTime(1970, 1, 1) },
				new Person { FirstName = "John", LastName = "Doe", PhoneNumber = "555-1234", DateOfBirth = new DateTime(1970, 1, 1) },
				new Person { FirstName = "John", LastName = "Doe", PhoneNumber = "555-1234", DateOfBirth = new DateTime(1970, 1, 1) },
			};

			//dataGrid.ItemsSource = people;
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
				MessageBox.Show(this, ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		protected override void OnInitialized(EventArgs e)
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

			base.OnInitialized(e);
		}

		protected override void OnClosed(EventArgs e)
		{
			SaveSettings();

			base.OnClosed(e);
		}

		private void ApplySavedSettings()
		{
			Width = _ide.IDEConfiguration.Start_WindowSize.Width;
			Height = _ide.IDEConfiguration.Start_WindowSize.Height;

			if (_ide.IDEConfiguration.Start_OpenMaximized)
				WindowState = WindowState.Maximized;
		}

		private void SaveSettings()
		{
			_ide.IDEConfiguration.Start_OpenMaximized = WindowState == WindowState.Maximized;

			if (WindowState == WindowState.Normal)
				_ide.IDEConfiguration.Start_WindowSize = new System.Drawing.Size((int)Width, (int)Height);
			else
				_ide.IDEConfiguration.Start_WindowSize = new System.Drawing.Size((int)RestoreBounds.Width, (int)RestoreBounds.Height);

			_ide.IDEConfiguration.Save();
		}

		private void FillProjectList()
		{
			//treeView.Nodes.Clear();

			IEnumerable<Project> validProjects = _ide.AvailableProjects
				.Where(project => ProjectChecker.IsValidProject(project));

			foreach (Project project in validProjects)
				AddProjectToList(project, false);

			RefreshAndReserializeProjects(); // Update TombIDEProjects.xml with only valid projects
		}

		#endregion Initialization

		#region Events

		private void contextMenuItem_Rename_Click(object sender, EventArgs e) => RenameProject();
		private void contextMenuItem_Delete_Click(object sender, EventArgs e) => DeleteProject();

		private void contextMenuItem_MoveUp_Click(object sender, EventArgs e)
		{
			//treeView.MoveSelectedNodeUp();
			RefreshAndReserializeProjects();
		}

		private void contextMenuItem_MoveDown_Click(object sender, EventArgs e)
		{
			//treeView.MoveSelectedNodeDown();
			RefreshAndReserializeProjects();
		}

		private void button_OpenProject_Click(object sender, EventArgs e) => OpenSelectedProject();

		private void treeView_SelectedNodesChanged(object sender, EventArgs e) => CheckItemSelection();

		private void treeView_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			if (e.LeftButton == MouseButtonState.Pressed)
				OpenSelectedProject();
		}

		private void treeView_KeyDown(object sender, KeyEventArgs e)
		{
			//if (treeView.SelectedNodes.Count == 0)
			//	return;

			if (e.Key == Key.F2)
				RenameProject();

			if (e.Key == Key.Delete)
				DeleteProject();
		}

		private void treeView_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Up || e.Key == Key.Down)
				CheckItemSelection();
		}

		#endregion Events

		#region Event methods

		private void CreateNewProject()
		{
			using (var form = new FormProjectSetup())
			{
				if (form.ShowDialog() == System.Windows.Forms.DialogResult.OK)
				{
					AddProjectToList(form.CreatedProject, true);
					SelectProjectOnList(form.CreatedProject.ProjectPath);
				}
			}
		}

		private void OpenTrproj()
		{
			using (var dialog = new System.Windows.Forms.OpenFileDialog())
			{
				dialog.Title = "Select the .trproj file you want to open.";
				dialog.Filter = "TombIDE Project Files|*.trproj";

				if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
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
						MessageBox.Show(this, ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
					}
				}
			}
		}

		private void ImportExe()
		{
			using (var dialog = new System.Windows.Forms.OpenFileDialog())
			{
				dialog.Title = "Select the .exe file of the game project you want to import.";
				dialog.Filter = "Executable Files|*.exe";

				if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
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
							if (form.ShowDialog() == System.Windows.Forms.DialogResult.OK)
							{
								if (!IsProjectOnList(form.ImportedProject))
									AddProjectToList(form.ImportedProject, true);

								SelectProjectOnList(form.ImportedProject.ProjectPath);
							}
						}
					}
					catch (Exception ex)
					{
						MessageBox.Show(this, ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
					}
				}
			}
		}

		private void RenameProject()
		{
			using (var form = new FormRenameProject(_selectedProject))
			{
				if (form.ShowDialog() == System.Windows.Forms.DialogResult.OK)
				{
					//var node = treeView.SelectedNodes[0] as DarkTreeNodeEx;

					//node.Text = _selectedProject.Name;
					//node.SubText = _selectedProject.ProjectPath;
					//node.Tag = _selectedProject;

					RefreshAndReserializeProjects();
				}
			}
		}

		private void DeleteProject()
		{
			System.Windows.Forms.DialogResult result = DarkMessageBox.Show(
				"Are you sure you want to delete the \"" + _selectedProject.Name + "\" project from the list?\n" +
				"This process will NOT affect the project folder nor its files.", "Are you sure?",
				System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Question);

			if (result == System.Windows.Forms.DialogResult.Yes)
			{
				// Remove the node and clear selection
				//treeView.SelectedNodes[0].Remove();
				//treeView.SelectedNodes.Clear();

				CheckItemSelection();

				// Update TombIDEProjects.xml with the new list (without the removed node)
				RefreshAndReserializeProjects();
			}
		}

		private void CheckItemSelection()
		{
			//// Enable / Disable node specific buttons
			//contextMenuItem_Rename.Enabled = treeView.SelectedNodes.Count > 0;
			//contextMenuItem_Delete.Enabled = treeView.SelectedNodes.Count > 0;
			//contextMenuItem_MoveUp.Enabled = treeView.SelectedNodes.Count > 0;
			//contextMenuItem_MoveDown.Enabled = treeView.SelectedNodes.Count > 0;
			//contextMenuItem_OpenDirectory.Enabled = treeView.SelectedNodes.Count > 0;
			//contextMenuItem_OpenProject.Enabled = treeView.SelectedNodes.Count > 0;

			//checkBox_Remember.Enabled = treeView.SelectedNodes.Count > 0;
			//button_OpenProject.Enabled = treeView.SelectedNodes.Count > 0;

			//// // // // // // // //
			//_selectedProject = treeView.SelectedNodes.Count > 0 ? (Project)treeView.SelectedNodes[0].Tag : null;
			//// // // // // // // //
		}

		private void OpenSelectedProject()
		{
			if (_selectedProject == null)
				return;

			if (!ProjectChecker.IsValidProject(_selectedProject, out string errorMessage))
			{
				DarkMessageBox.Show("Failed to load project. " + errorMessage, "Error",
					System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);

				return;
			}

			if (!IsValidGameLauncher())
				return;

			//if (checkBox_Remember.IsChecked.Value)
			//	_ide.IDEConfiguration.RememberedProject = _selectedProject.ProjectPath;

			SaveSettings();
			Hide();

			using (var form = new FormMain(_ide, _selectedProject))
			{
				System.Windows.Forms.DialogResult result = form.ShowDialog();

				if (result == System.Windows.Forms.DialogResult.OK) // OK means the user wants to switch projects
				{
					// Reset the RememberedProject setting
					_ide.IDEConfiguration.RememberedProject = string.Empty;
					_ide.IDEConfiguration.Save();

					// Restart the application (without any arguments)
					Application.Current.Shutdown();
				}
				else if (result == System.Windows.Forms.DialogResult.Cancel) // Cancel means the user closed the program
					Application.Current.Shutdown();
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
				node.Icon = System.Drawing.Icon.ExtractAssociatedIcon(project.LaunchFilePath).ToBitmap();

			//treeView.Items.Add(node);

			if (reserialize)
				RefreshAndReserializeProjects();
		}

		private void RefreshAndReserializeProjects()
		{
			RepositionProjectNodeIcons();

			_ide.AvailableProjects.Clear();

			//foreach (DarkTreeNode node in treeView.Items)
			//	_ide.AvailableProjects.Add((Project)node.Tag);

			XmlHandling.UpdateProjectsXml(_ide.AvailableProjects);
		}

		private void RepositionProjectNodeIcons() // DarkUI sucks
		{
			const int LEFT_ICON_MARGIN = 8;
			const int LEFT_TEXT_MARGIN = 45;
			const int UNIFORM_ICON_SIZE = 32;

			//foreach (DarkTreeNodeEx node in treeView.Nodes)
			//{
			//	int iconYPos = (treeView.ItemHeight * node.VisibleIndex) + ((treeView.ItemHeight - UNIFORM_ICON_SIZE) / 2);

			//	node.IconArea = new System.Drawing.Rectangle(new Point(LEFT_ICON_MARGIN, iconYPos), new Size(UNIFORM_ICON_SIZE, UNIFORM_ICON_SIZE));
			//	node.TextArea = new System.Drawing.Rectangle(new Point(LEFT_TEXT_MARGIN, node.TextArea.Y), node.TextArea.Size);
			//	node.SubTextArea = new System.Drawing.Rectangle(new Point(LEFT_TEXT_MARGIN + 1, node.SubTextArea.Y), node.SubTextArea.Size);
			//	node.ExtraIconArea = new System.Drawing.Rectangle(new Point(node.ExtraIconArea.X + 6, node.ExtraIconArea.Y), node.ExtraIconArea.Size);
			//}

			//treeView.Invalidate();
		}

		private void SelectProjectOnList(string projectPath)
		{
			//DarkTreeNode targetNode = treeView.Items
			//	.FirstOrDefault(node => node.Tag is Project p
			//		&& p.ProjectPath.Equals(projectPath, StringComparison.OrdinalIgnoreCase));

			//if (targetNode == null)
			//	return;

			//treeView.SelectNode(targetNode);
			//CheckItemSelection();
		}

		private bool IsProjectOnList(Project project)
		{
			return false;

			//return treeView.Items
			//	.Any(node => node.Tag is Project p
			//		&& p.ProjectPath.Equals(project.ProjectPath, StringComparison.OrdinalIgnoreCase));
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
					DarkMessageBox.Show(ex.Message, "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
					return false;
				}
			}
			else
				return true;
		}

		#endregion Other methods

		private void Button_New_Click(object sender, RoutedEventArgs e)
		{
			CreateNewProject();
		}
		
		private void Button_Open_Click(object sender, RoutedEventArgs e)
		{
			OpenTrproj();
		}

		private void Button_Import_Click(object sender, RoutedEventArgs e)
		{
			ImportExe();
		}

	}
}
