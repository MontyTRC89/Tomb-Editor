using DarkUI.Forms;
using System;
using System.IO;
using System.Windows.Forms;
using TombIDE.Shared;
using TombIDE.Shared.SharedClasses;

namespace TombIDE
{
	public partial class FormRenameProject : DarkForm
	{
		private Project _targetProject;

		#region Initialization

		public FormRenameProject(Project targetProject)
		{
			_targetProject = targetProject;

			InitializeComponent();
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);

			textBox_NewName.Text = _targetProject.Name;
			textBox_NewName.SelectAll();
		}

		#endregion Initialization

		#region Events

		private void button_Apply_Click(object sender, EventArgs e)
		{
			try
			{
				string newName = PathHelper.RemoveIllegalPathSymbols(textBox_NewName.Text.Trim());

				if (string.IsNullOrWhiteSpace(newName) || newName.ToLower() == "engine")
					throw new ArgumentException("Invalid name.");

				bool renameDirectory = checkBox_RenameDirectory.Checked;

				if (newName == _targetProject.Name)
				{
					// If the name hasn't changed, but the directory name is different and the user wants to rename it
					if (Path.GetFileName(_targetProject.ProjectPath) != newName && renameDirectory)
					{
						HandleDirectoryRenaming();

						_targetProject.Rename(newName, true);
						_targetProject.Save();
					}
					else
						DialogResult = DialogResult.Cancel;
				}
				else
				{
					// Check if a project with the same name already exists on the list
					foreach (Project project in XmlHandling.GetProjectsFromXml())
					{
						if (project.Name.ToLower() == newName.ToLower())
						{
							// Check if the project we found IS the current _ide.Project
							if (project.ProjectPath.ToLower() == _targetProject.ProjectPath.ToLower())
							{
								if (renameDirectory)
									HandleDirectoryRenaming();

								break;
							}
							else
								throw new ArgumentException("A project with the same name already exists on the list.");
						}
					}

					_targetProject.Rename(newName, renameDirectory);
					_targetProject.Save();
				}
			}
			catch (Exception ex)
			{
				DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

				DialogResult = DialogResult.None;
			}
		}

		#endregion Events

		#region Methods

		private void HandleDirectoryRenaming()
		{
			// Allow renaming directories to the same name, but with different letter cases
			// To do that, we must add a "_TEMP" suffix at the end of the directory name
			// _ide.Project.Rename() will then handle the rest

			string tempPath = _targetProject.ProjectPath + "_TEMP";
			Directory.Move(_targetProject.ProjectPath, tempPath);
		}

		#endregion Methods
	}
}
