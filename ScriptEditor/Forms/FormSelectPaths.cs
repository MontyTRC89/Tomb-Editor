using DarkUI.Forms;
using System;
using System.Windows.Forms;
using TombLib.Script;

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
            tbNgCenter.Text = Properties.Settings.Default.NgCenterPath;
            comboCompiler.SelectedIndex = (int)Properties.Settings.Default.Compiler;
		}

		private void browseScriptButton_Click(object sender, EventArgs e)
		{
			FolderBrowserDialog fbd = new FolderBrowserDialog()
			{
				Description = "Select script folder:"
			};

			if (fbd.ShowDialog(this) == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
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

			if (fbd.ShowDialog(this) == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
			{
				gamePathTextBox.Text = fbd.SelectedPath;
			}
		}

		private void applyButton_Click(object sender, EventArgs e)
		{
            var compiler = (ScriptCompilers)comboCompiler.SelectedIndex;

            if (compiler == ScriptCompilers.NGCenter && tbNgCenter.Text.Trim() == "")
            {
                DarkMessageBox.Show(this, "You must enter NG_Center.exe's path", "Error",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Properties.Settings.Default.ScriptPath = scriptPathTextBox.Text;
			Properties.Settings.Default.GamePath = gamePathTextBox.Text;
            Properties.Settings.Default.NgCenterPath = tbNgCenter.Text;
            Properties.Settings.Default.Compiler = compiler;
			Properties.Settings.Default.Save();
		}

        private void butNgCenter_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog()
            {
                Description = "Select NG_Center.exe folder:"
            };

            if (fbd.ShowDialog(this) == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
            {
                tbNgCenter.Text = fbd.SelectedPath;
            }
        }
    }
}
