using DarkUI.Forms;
using System;
using System.IO;
using System.Windows.Forms;
using TombIDE.Shared;
using TombLib.Projects;

namespace TombIDE
{
	public partial class FormRenameProject : DarkForm
	{
		private IDE _ide;

		#region Initialization

		public FormRenameProject(IDE ide)
		{
			_ide = ide;

			InitializeComponent();
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);

			textBox_NewName.Text = _ide.Project.Name;
			textBox_NewName.SelectAll();
		}

		#endregion Initialization

		#region Events

		private void button_Apply_Click(object sender, EventArgs e)
		{
			try
			{
				string newName = SharedMethods.RemoveIllegalPathSymbols(textBox_NewName.Text.Trim());

				if (string.IsNullOrWhiteSpace(newName) || newName.ToLower() == "engine")
					throw new ArgumentException("Invalid name.");

				bool renameDirectory = checkBox_RenameDirectory.Checked;

				if (newName == _ide.Project.Name)
				{
					// If the name hasn't changed, but the directory name is different and the user wants to rename it
					if (Path.GetFileName(_ide.Project.ProjectPath) != newName && renameDirectory)
					{
						HandleDirectoryRenaming(newName);

						_ide.Project.Rename(newName, true);
						XmlHandling.SaveTRPROJ(_ide.Project);

						_ide.RaiseEvent(new IDE.ActiveProjectRenamedEvent());
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
							if (project.ProjectPath.ToLower() == _ide.Project.ProjectPath.ToLower())
							{
								if (renameDirectory)
									HandleDirectoryRenaming(newName);

								break;
							}
							else
								throw new ArgumentException("A project with the same name already exists on the list.");
						}
					}

					_ide.Project.Rename(newName, renameDirectory);
					XmlHandling.SaveTRPROJ(_ide.Project);

					_ide.RaiseEvent(new IDE.ActiveProjectRenamedEvent());
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

		private void HandleDirectoryRenaming(string newName)
		{
			// Allow renaming directories to the same name, but with different letter cases
			// To do that, we must add a "_TEMP" suffix at the end of the directory name
			// _ide.Project.Rename() will then handle the rest

			string tempPath = _ide.Project.ProjectPath + "_TEMP";
			Directory.Move(_ide.Project.ProjectPath, tempPath);
		}

		#endregion Methods
	}
}
