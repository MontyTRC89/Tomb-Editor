using DarkUI.Forms;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace TombIDE
{
	public partial class FormFindMusic : DarkForm
	{
		public string MusicArchiveFilePath { get; set; }

		public FormFindMusic()
		{
			InitializeComponent();
		}

		private void button_Download_Click(object sender, EventArgs e)
			=> Process.Start("https://mega.nz/file/f9llhQAY#y0RqaMhR4ghtQ-1IFAGbHep_FCmkV8Q66bzdMWVqtuY"); // Hardcoded links, yay!

		private void button_Browse_Click(object sender, EventArgs e)
		{
			using (var dialog = new OpenFileDialog())
			{
				dialog.Title = "Select the music.zip file.";
				dialog.Filter = "ZIP Files (.zip)|*.zip";

				if (dialog.ShowDialog(this) == DialogResult.OK)
					textBox_ArchivePath.Text = dialog.FileName;
			}
		}

		private void button_Continue_Click(object sender, EventArgs e)
		{
			try
			{
				string archivePath = textBox_ArchivePath.Text.Trim();

				if (!Path.GetExtension(archivePath).Equals(".zip", StringComparison.OrdinalIgnoreCase))
					throw new ArgumentException("Selected file is not a ZIP archive.");

				MusicArchiveFilePath = archivePath;
			}
			catch (Exception ex)
			{
				DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				DialogResult = DialogResult.None;
			}
		}

		private void textBox_ArchivePath_TextChanged(object sender, EventArgs e)
			=> button_Continue.Enabled = !string.IsNullOrWhiteSpace(textBox_ArchivePath.Text);
	}
}
