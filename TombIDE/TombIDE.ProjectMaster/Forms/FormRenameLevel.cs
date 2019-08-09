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
				checkBox_RenameDirectory.Text = "Can't rename external level folders";
				checkBox_RenameDirectory.Checked = false;
				checkBox_RenameDirectory.Enabled = false;
			}

			// Check if there are errors in the script
			if (!_ide.IsLevelScriptDefined(_ide.SelectedLevel.Name) || !_ide.IsLanguageStringDefined(_ide.SelectedLevel.Name))
			{
				// Disable the checkBox if so
				checkBox_RenameScriptEntry.Checked = false;
				checkBox_RenameScriptEntry.Enabled = false;

				// Display ScriptError + LanguageError
				if (!_ide.IsLevelScriptDefined(_ide.SelectedLevel.Name) && !_ide.IsLanguageStringDefined(_ide.SelectedLevel.Name))
				{
					label_ScriptError.Visible = true;
					label_LanguageError.Visible = true;
				}
				// Display ScriptError only
				else if (!_ide.IsLevelScriptDefined(_ide.SelectedLevel.Name) && _ide.IsLanguageStringDefined(_ide.SelectedLevel.Name))
				{
					Height = 212;
					label_ScriptError.Visible = true;
				}
				// Display LanguageError only
				else if (_ide.IsLevelScriptDefined(_ide.SelectedLevel.Name) && !_ide.IsLanguageStringDefined(_ide.SelectedLevel.Name))
				{
					Height = 212;
					label_LanguageError.Visible = true;
				}
			}
			else // No errors
				Height = 193;
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
				string newName = textBox_NewName.Text.Trim();
				bool renameDirectory = checkBox_RenameDirectory.Checked;
				bool renameScriptEntry = checkBox_RenameScriptEntry.Checked;

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

				if (renameScriptEntry)
					_ide.RenameSelectedLevelScriptEntry(newName);

				_ide.SelectedLevel.Rename(newName, renameDirectory);
				_ide.RaiseEvent(new IDE.SelectedLevelSettingsChangedEvent());
			}
			catch (Exception ex)
			{
				DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				DialogResult = DialogResult.None;
			}
		}

		private void textBox_NewName_TextChanged(object sender, EventArgs e)
		{
			// If the name hasn't changed but the level folder name is different
			if (textBox_NewName.Text == _ide.SelectedLevel.Name && Path.GetFileName(_ide.SelectedLevel.FolderPath) != textBox_NewName.Text)
			{
				// If the level is not an external level
				if (_ide.SelectedLevel.FolderPath.StartsWith(_ide.Project.LevelsPath))
				{
					checkBox_RenameDirectory.Enabled = true;
					checkBox_RenameDirectory.Checked = true;
				}

				checkBox_RenameScriptEntry.Checked = false;
				checkBox_RenameScriptEntry.Enabled = false;
			}
			// If the name changed but the level folder name is the same
			else if (textBox_NewName.Text != _ide.SelectedLevel.Name && Path.GetFileName(_ide.SelectedLevel.FolderPath) == textBox_NewName.Text)
			{
				checkBox_RenameDirectory.Checked = false;
				checkBox_RenameDirectory.Enabled = false;

				// If there are no errors in the script (in this case, if no errors are displayed)
				if (!label_ScriptError.Visible && !label_LanguageError.Visible)
				{
					checkBox_RenameScriptEntry.Enabled = true;
					checkBox_RenameScriptEntry.Checked = true;
				}
			}
			// If the name hasn't changed and the level folder name is the same
			else if (textBox_NewName.Text == _ide.SelectedLevel.Name)
			{
				checkBox_RenameDirectory.Checked = false;
				checkBox_RenameDirectory.Enabled = false;

				checkBox_RenameScriptEntry.Checked = false;
				checkBox_RenameScriptEntry.Enabled = false;
			}
			else // Basically every other scenario
			{
				// If the level is not an external level
				if (_ide.SelectedLevel.FolderPath.StartsWith(_ide.Project.LevelsPath))
				{
					checkBox_RenameDirectory.Enabled = true;
					checkBox_RenameDirectory.Checked = true;
				}

				// If there are no errors in the script (in this case, if no errors are displayed)
				if (!label_ScriptError.Visible && !label_LanguageError.Visible)
				{
					checkBox_RenameScriptEntry.Enabled = true;
					checkBox_RenameScriptEntry.Checked = true;
				}
			}

			button_Apply.Enabled = !string.IsNullOrWhiteSpace(textBox_NewName.Text);
		}
	}
}
