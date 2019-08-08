using DarkUI.Forms;
using System;
using System.IO;
using System.Windows.Forms;
using TombIDE.Shared;
using TombLib.Projects;

namespace TombIDE.ProjectMaster
{
	public partial class FormRenameLevel : DarkForm
	{
		private IDE _ide;

		public FormRenameLevel(IDE ide)
		{
			_ide = ide;

			InitializeComponent();

			// Disable renaming external level folders (level folders which are outside of the project's /Levels/ folder)
			if (!_ide.SelectedLevel.FolderPath.StartsWith(_ide.Project.LevelsPath))
			{
				checkBox_RenameDirectory.Checked = false;
				checkBox_RenameDirectory.Enabled = false;
				checkBox_RenameDirectory.Visible = false;
			}
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);

			textBox_NewName.Text = _ide.SelectedLevel.Name;
			textBox_NewName.SelectAll();
		}

		private void button_Apply_Click(object sender, EventArgs e)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(textBox_NewName.Text))
					throw new ArgumentException("Invalid name.");

				string newName = textBox_NewName.Text.Trim();
				bool renameDirectory = checkBox_RenameDirectory.Checked;

				if (newName == _ide.SelectedLevel.Name)
				{
					// If the name hasn't changed, but the directory name is different and the user wants to rename it
					if (Path.GetFileName(_ide.SelectedLevel.FolderPath) != newName && renameDirectory)
					{
						_ide.SelectedLevel.Rename(newName, true);
						_ide.RaiseEvent(new IDE.SelectedLevelSettingsChangedEvent());
					}
					else
						DialogResult = DialogResult.Cancel;

					return;
				}

				// Check for name duplicates
				foreach (ProjectLevel level in _ide.Project.Levels)
				{
					if (level.Name == newName)
						throw new ArgumentException("A level with the same name already exists on the list.");
				}

				_ide.SelectedLevel.Rename(newName, renameDirectory);
				_ide.RaiseEvent(new IDE.SelectedLevelSettingsChangedEvent());
			}
			catch (Exception ex)
			{
				DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				DialogResult = DialogResult.None;
			}
		}
	}
}
