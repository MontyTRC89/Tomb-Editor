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

		private void button_Apply_Click(object sender, EventArgs e)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(textBox_NewName.Text))
					throw new ArgumentException("Invalid name.");

				string newName = SharedMethods.RemoveIllegalSymbols(textBox_NewName.Text.Trim());
				bool renameDirectory = checkBox_RenameDirectory.Checked;

				if (newName == _ide.Project.Name)
				{
					// If the name hasn't changed, but the directory name is different and the user wants to rename it
					if (Path.GetFileName(_ide.Project.ProjectPath) != newName && renameDirectory)
					{
						_ide.Project.Rename(newName, true);
						XmlHandling.SaveTRPROJ(_ide.Project);

						_ide.RaiseEvent(new IDE.ActiveProjectRenamedEvent());
					}
					else
						DialogResult = DialogResult.Cancel;

					return;
				}

				// Check for name duplicates
				foreach (Project project in XmlHandling.GetProjectsFromXml())
				{
					if (project.Name == newName)
						throw new ArgumentException("A project with the same name already exists on the list.");
				}

				_ide.Project.Rename(newName, renameDirectory);
				XmlHandling.SaveTRPROJ(_ide.Project);

				_ide.RaiseEvent(new IDE.ActiveProjectRenamedEvent());
			}
			catch (Exception ex)
			{
				DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				DialogResult = DialogResult.None;
			}
		}
	}
}
