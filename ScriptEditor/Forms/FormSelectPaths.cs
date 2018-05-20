using DarkUI.Forms;
using System;
using System.Windows.Forms;

namespace ScriptEditor
{
	public partial class FormSelectPaths : DarkForm
	{
		public FormSelectPaths()
		{
			InitializeComponent();
		}

		private void FormSelectPaths_Shown(object senderm, EventArgs e)
		{
			scriptPathTextBox.Text = Properties.Settings.Default.ScriptPath;
			gamePathTextBox.Text = Properties.Settings.Default.GamePath;
		}

		private void browseScriptButton_Click(object sender, EventArgs e)
		{
			FolderBrowserDialog fbd = new FolderBrowserDialog()
			{
				Description = "Select script folder:"
			};

			if (fbd.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
			{
				scriptPathTextBox.Text = fbd.SelectedPath;
			}
		}

		private void browseGameButton_Click(object sender, EventArgs e)
		{
			FolderBrowserDialog fbd = new FolderBrowserDialog()
			{
				Description = "Select game folder:"
			};

			if (fbd.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
			{
				gamePathTextBox.Text = fbd.SelectedPath;
			}
		}

		private void applyButton_Click(object sender, EventArgs e)
		{
			Properties.Settings.Default.ScriptPath = scriptPathTextBox.Text;
			Properties.Settings.Default.GamePath = gamePathTextBox.Text;
			Properties.Settings.Default.Save();
		}
	}
}
