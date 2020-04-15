using DarkUI.Forms;
using System;
using System.IO;
using System.Windows.Forms;
using TombIDE.Shared;
using TombIDE.Shared.SharedClasses;

namespace TombIDE
{
	public partial class FormSaveAs : DarkForm
	{
		public string NewFilePath { get; private set; }

		private IDE _ide;

		public FormSaveAs(IDE ide)
		{
			InitializeComponent();

			_ide = ide;
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);

			comboBox_FileFormat.SelectedIndex = 0;

			textBox_NewFileName.Text = "untitled";
			textBox_NewFileName.SelectAll();
		}

		private void button_Save_Click(object sender, EventArgs e)
		{
			try
			{
				string newFileName = PathHelper.RemoveIllegalPathSymbols(textBox_NewFileName.Text.Trim());

				if (string.IsNullOrWhiteSpace(newFileName))
					throw new ArgumentException("Invalid file name.");

				switch (comboBox_FileFormat.SelectedIndex)
				{
					case 0:
						newFileName += ".txt";
						break;

					case 1:
						newFileName += ".lua";
						break;
				}

				string[] files = Directory.GetFiles(_ide.Project.ScriptPath, "*.*", SearchOption.TopDirectoryOnly);

				foreach (string file in files)
				{
					if (Path.GetFileName(file).ToLower() == newFileName.ToLower())
						throw new ArgumentException("A file with the same name already exists in the project's /Script/ folder.");
				}

				// // // //
				NewFilePath = Path.Combine(_ide.Project.ScriptPath, newFileName);
				// // // //
			}
			catch (Exception ex)
			{
				DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				DialogResult = DialogResult.None;
			}
		}
	}
}
