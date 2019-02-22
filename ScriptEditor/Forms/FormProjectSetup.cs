using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using System.Xml.Serialization;
using TombLib.Script;

namespace ScriptEditor
{
	public partial class FormProjectSetup : DarkForm
	{
		private bool _errorOccurred = false;

		public FormProjectSetup()
		{
			InitializeComponent();

			// Hide NG_Center path selection by default
			ngFolderLabel.Visible = false;
			ngCenterPathTextBox.Visible = false;
			ngCenterBrowseButton.Visible = false;
			projectSetupGroup.Height = 72;
			Height = 210;
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);

			if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.NGCenterPath))
			{
				// Add NG_Center path which was already used in another project
				ngCenterPathTextBox.Text = Properties.Settings.Default.NGCenterPath;
			}
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			base.OnClosing(e);

			if (_errorOccurred)
			{
				_errorOccurred = false;
				e.Cancel = true;
			}
		}

		private void applyButton_Click(object sender, EventArgs e)
		{
			if (string.IsNullOrWhiteSpace(nameTextBox.Text))
			{
				DarkMessageBox.Show(this, Resources.Messages.Error_NoProjectName, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				_errorOccurred = true;
				return;
			}

			if (string.IsNullOrWhiteSpace(gamePathTextBox.Text))
			{
				DarkMessageBox.Show(this, Resources.Messages.Error_NoGamePath, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				_errorOccurred = true;
				return;
			}

			if (!File.Exists(gamePathTextBox.Text))
			{
				DarkMessageBox.Show(this, Resources.Messages.Error_InvalidGamePath, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				_errorOccurred = true;
				return;
			}

			if (!tr4RadioButton.Checked && !trngRadioButton.Checked && !tr5RadioButton.Checked && !tr5mainRadioButton.Checked)
			{
				DarkMessageBox.Show(this, Resources.Messages.Error_NoEngineType, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				_errorOccurred = true;
				return;
			}

			if (trngRadioButton.Checked && string.IsNullOrWhiteSpace(ngCenterPathTextBox.Text))
			{
				DarkMessageBox.Show(this, Resources.Messages.Error_NoNGCenterPath, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				_errorOccurred = true;
				return;
			}

			if (trngRadioButton.Checked && !File.Exists(gamePathTextBox.Text))
			{
				DarkMessageBox.Show(this, Resources.Messages.Error_InvalidNGCenterPath, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				_errorOccurred = true;
				return;
			}

			string scriptFolderPath = Path.GetDirectoryName(gamePathTextBox.Text) + @"\Script";

			if (!Directory.Exists(scriptFolderPath))
			{
				DarkMessageBox.Show(this, Resources.Messages.Error_NoScriptFolderFound, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				_errorOccurred = true;
				return;
			}

			string scriptFilePath = scriptFolderPath + @"\script.txt";

			if (!File.Exists(scriptFilePath))
			{
				DarkMessageBox.Show(this, Resources.Messages.Error_NoScriptFileFound, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				_errorOccurred = true;
				return;
			}

			AddNewProject();
		}

		private void AddNewProject()
		{
			List<Project> existingProjects = new List<Project>();

			using (StreamReader stream = new StreamReader("Projects.xml"))
			{
				XmlSerializer serializer = new XmlSerializer(typeof(List<Project>));
				existingProjects = (List<Project>)serializer.Deserialize(stream);
			}

			foreach (Project proj in existingProjects)
			{
				if (nameTextBox.Text.Trim() == proj.Name)
				{
					DarkMessageBox.Show(this, Resources.Messages.Error_ProjectAlreadyExists, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					_errorOccurred = true;
					return;
				}
			}

			ScriptCompilers compiler = new ScriptCompilers();

			if (tr4RadioButton.Checked)
				compiler = ScriptCompilers.TRLELegacy;
			else if (trngRadioButton.Checked)
				compiler = ScriptCompilers.NGCenter;
			else if (tr5RadioButton.Checked)
				compiler = ScriptCompilers.TR5New;
			else if (tr5mainRadioButton.Checked)
				compiler = ScriptCompilers.TR5Main;

			Project newProject = new Project();

			newProject.Name = nameTextBox.Text.Trim();
			newProject.GamePath = Path.GetDirectoryName(gamePathTextBox.Text.Trim());
			newProject.Compiler = compiler;

			if (trngRadioButton.Checked)
			{
				newProject.NGCenterPath = ngCenterPathTextBox.Text.Trim();

				Properties.Settings.Default.NGCenterPath = ngCenterPathTextBox.Text; // Remember this for other projects
				Properties.Settings.Default.Save();
			}

			existingProjects.Add(newProject);

			using (StreamWriter stream = new StreamWriter("Projects.xml"))
				new XmlSerializer(typeof(List<Project>)).Serialize(stream, existingProjects);
		}

		private void gameBrowseButton_Click(object sender, EventArgs e)
		{
			OpenFileDialog dialog = new OpenFileDialog();
			dialog.Filter = "Game Executable|*.exe";
			dialog.Title = "Select your game's .exe file (Tomb4.exe or PCTomb5.exe)";

			if (dialog.ShowDialog() == DialogResult.OK)
				gamePathTextBox.Text = dialog.FileName;
		}

		private void ngCenterBrowseButton_Click(object sender, EventArgs e)
		{
			OpenFileDialog dialog = new OpenFileDialog();
			dialog.Filter = "NG_Center Executable|*.exe";
			dialog.Title = "Select NG_Center.exe file";

			if (dialog.ShowDialog() == DialogResult.OK)
				ngCenterPathTextBox.Text = dialog.FileName;
		}

		private void gamePathTextBox_TextChanged(object sender, EventArgs e)
		{
			if (gamePathTextBox.Text.ToLower().EndsWith("tomb4.exe"))
			{
				// Disable TR5 related RadioButtons
				tr4RadioButton.Enabled = true;
				trngRadioButton.Enabled = true;
				tr5RadioButton.Enabled = false;
				tr5mainRadioButton.Enabled = false;
			}
			else if (gamePathTextBox.Text.ToLower().EndsWith("pctomb5.exe"))
			{
				// Disable TR4 related RadioButtons
				tr4RadioButton.Enabled = false;
				trngRadioButton.Enabled = false;
				tr5RadioButton.Enabled = true;
				tr5mainRadioButton.Enabled = true;
			}
			else
			{
				// Re-Enable everything
				tr4RadioButton.Enabled = true;
				trngRadioButton.Enabled = true;
				tr5RadioButton.Enabled = true;
				tr5mainRadioButton.Enabled = true;
			}
		}

		private void trngRadioButton_CheckedChanged(object sender, EventArgs e)
		{
			if (trngRadioButton.Checked)
			{
				// Expand GroupBox and Window size to show NG_Center path selection
				projectSetupGroup.Height = 102;
				Height = 240;

				ngFolderLabel.Visible = true;
				ngCenterPathTextBox.Visible = true;
				ngCenterBrowseButton.Visible = true;
			}
			else
			{
				projectSetupGroup.Height = 72;
				Height = 210;

				ngFolderLabel.Visible = false;
				ngCenterPathTextBox.Visible = false;
				ngCenterBrowseButton.Visible = false;
			}
		}
	}
}
