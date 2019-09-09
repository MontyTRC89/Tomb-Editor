using DarkUI.Forms;
using System;
using System.IO;
using System.Windows.Forms;
using TombIDE.Shared;

namespace TombIDE.ProjectMaster
{
	public partial class FormRenameLauncher : DarkForm
	{
		private IDE _ide;

		#region Initialization

		public FormRenameLauncher(IDE ide)
		{
			_ide = ide;

			InitializeComponent();
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);

			textBox_NewName.Text = Path.GetFileNameWithoutExtension(_ide.Project.LaunchFilePath);
			textBox_NewName.SelectAll();
		}

		#endregion Initialization

		#region Events

		private void button_Apply_Click(object sender, EventArgs e)
		{
			try
			{
				string newName = SharedMethods.RemoveIllegalPathSymbols(textBox_NewName.Text.Trim());

				if (string.IsNullOrWhiteSpace(newName))
					throw new ArgumentException("Invalid file name.");

				if (newName == Path.GetFileName(_ide.Project.LaunchFilePath))
					DialogResult = DialogResult.Cancel;
				else
				{
					string newPath = Path.Combine(Path.GetDirectoryName(_ide.Project.LaunchFilePath), newName + ".exe");

					File.Move(_ide.Project.LaunchFilePath, newPath);

					_ide.Project.LaunchFilePath = newPath;
					XmlHandling.SaveTRPROJ(_ide.Project);
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
