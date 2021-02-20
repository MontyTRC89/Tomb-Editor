using DarkUI.Forms;
using System;
using System.IO;
using System.Windows.Forms;
using TombIDE.Shared.SharedClasses;

namespace TombIDE.ScriptingStudio.Forms
{
	internal partial class FormRenameItem : DarkForm
	{
		private string _targetItemPath;

		#region Construction

		public FormRenameItem(string targetItemPath)
		{
			InitializeComponent();

			_targetItemPath = targetItemPath;
		}

		#endregion Construction

		#region Events

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);

			textBox_NewName.Text = Path.GetFileNameWithoutExtension(_targetItemPath);
			textBox_NewName.SelectAll();
		}

		private void button_Apply_Click(object sender, EventArgs e)
		{
			try
			{
				string newName = PathHelper.RemoveIllegalPathSymbols(textBox_NewName.Text).Trim();

				if (string.IsNullOrWhiteSpace(newName))
					throw new ArgumentException("Invalid name.");

				if (newName.Equals(Path.GetFileName(_targetItemPath), StringComparison.OrdinalIgnoreCase))
					DialogResult = DialogResult.Cancel;
				else
				{
					string newPath = Path.Combine(Path.GetDirectoryName(_targetItemPath), newName + Path.GetExtension(_targetItemPath));

					if (File.Exists(_targetItemPath))
						File.Move(_targetItemPath, newPath);
					else
						Directory.Move(_targetItemPath, newPath);
				}
			}
			catch (Exception ex)
			{
				DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				DialogResult = DialogResult.None;
			}
		}

		#endregion Events
	}
}
