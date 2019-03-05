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
		/// <summary>
		/// Configuration object.
		/// </summary>
		private Configuration _config = Configuration.Load();

		/// <summary>
		/// Used to stop the form from closing when an error occurred.
		/// </summary>
		private bool _errorOccurred = false;

		public FormProjectSetup()
		{
			InitializeComponent();

			// Hide NG_Center path selection by default
			label_NGCenterPath.Visible = false;
			textBox_NGCenterPath.Visible = false;
			button_BrowseNGCenter.Visible = false;
			groupBox_ProjectSetup.Height = 72;
			Height = 210;

			if (!string.IsNullOrWhiteSpace(_config.CachedNGCenterPath))
			{
				// Add NG_Center path which was already used in another project
				textBox_NGCenterPath.Text = _config.CachedNGCenterPath;
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

		private void button_Apply_Click(object sender, EventArgs e)
		{
			if (string.IsNullOrWhiteSpace(textBox_ProjectName.Text))
			{
				DarkMessageBox.Show(this, Resources.Messages.Error_NoProjectName, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				_errorOccurred = true;
				return;
			}

			if (string.IsNullOrWhiteSpace(textBox_GamePath.Text))
			{
				DarkMessageBox.Show(this, Resources.Messages.Error_NoGamePath, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				_errorOccurred = true;
				return;
			}

			if (!File.Exists(textBox_GamePath.Text))
			{
				DarkMessageBox.Show(this, Resources.Messages.Error_InvalidGamePath, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				_errorOccurred = true;
				return;
			}

			if (!radioButton_TR4.Checked && !radioButton_TRNG.Checked && !radioButton_TR5.Checked && !radioButton_TR5Main.Checked)
			{
				DarkMessageBox.Show(this, Resources.Messages.Error_NoEngineType, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				_errorOccurred = true;
				return;
			}

			if (radioButton_TRNG.Checked && string.IsNullOrWhiteSpace(textBox_NGCenterPath.Text))
			{
				DarkMessageBox.Show(this, Resources.Messages.Error_NoNGCenterPath, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				_errorOccurred = true;
				return;
			}

			if (radioButton_TRNG.Checked && !File.Exists(textBox_GamePath.Text))
			{
				DarkMessageBox.Show(this, Resources.Messages.Error_InvalidNGCenterPath, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				_errorOccurred = true;
				return;
			}

			string gamePath = Path.GetDirectoryName(textBox_GamePath.Text.Trim());
			DirectoryInfo gameDirectory = new DirectoryInfo(gamePath);

			bool scriptDirectoryFound = false;
			bool scriptFileFound = false;

			foreach (DirectoryInfo directory in gameDirectory.GetDirectories())
			{
				if (directory.Name.ToLower() == "script")
				{
					scriptDirectoryFound = true;

					foreach (FileInfo file in directory.GetFiles())
					{
						if (file.Name.ToLower() == "script.txt")
						{
							scriptFileFound = true;
							AddNewProject(gamePath, directory.FullName);
						}
					}
				}
			}

			if (!scriptDirectoryFound)
			{
				DarkMessageBox.Show(this, Resources.Messages.Error_NoScriptFolderFound, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				_errorOccurred = true;
				return;
			}

			if (!scriptFileFound)
			{
				DarkMessageBox.Show(this, Resources.Messages.Error_NoScriptFileFound, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				_errorOccurred = true;
			}
		}

		private void AddNewProject(string gamePath, string scriptPath)
		{
			List<Project> existingProjects = new List<Project>();

			using (StreamReader reader = new StreamReader("ScriptEditorProjects.xml"))
			{
				XmlSerializer serializer = new XmlSerializer(typeof(List<Project>));
				existingProjects = (List<Project>)serializer.Deserialize(reader);
			}

			string name = textBox_ProjectName.Text.Trim();

			foreach (Project proj in existingProjects)
			{
				if (name == proj.Name)
				{
					DarkMessageBox.Show(this,
						Resources.Messages.Error_ProjectNameAlreadyExists, "Error",
						MessageBoxButtons.OK, MessageBoxIcon.Error);

					_errorOccurred = true;
					return;
				}

				if (gamePath == proj.GamePath)
				{
					DarkMessageBox.Show(this,
						string.Format(Resources.Messages.Error_ProjectAlreadyExists, proj.Name), "Error",
						MessageBoxButtons.OK, MessageBoxIcon.Error);

					_errorOccurred = true;
					return;
				}
			}

			ScriptCompilers compiler = new ScriptCompilers();

			if (radioButton_TR4.Checked)
				compiler = ScriptCompilers.TRLELegacy;
			else if (radioButton_TRNG.Checked)
				compiler = ScriptCompilers.NGCenter;
			else if (radioButton_TR5.Checked)
				compiler = ScriptCompilers.TR5New;
			else if (radioButton_TR5Main.Checked)
				compiler = ScriptCompilers.TR5Main;

			Project newProject = new Project
			{
				Name = name,
				GamePath = gamePath,
				ScriptPath = scriptPath,
				Compiler = compiler
			};

			if (radioButton_TRNG.Checked)
			{
				string ngCenterPath = textBox_NGCenterPath.Text.Trim();

				newProject.NGCenterPath = ngCenterPath;

				_config.CachedNGCenterPath = ngCenterPath; // Remember this for other projects
				_config.Save();
			}

			existingProjects.Add(newProject);

			using (StreamWriter writer = new StreamWriter("ScriptEditorProjects.xml"))
				new XmlSerializer(typeof(List<Project>)).Serialize(writer, existingProjects);
		}

		private void button_BrowseGame_Click(object sender, EventArgs e)
		{
			OpenFileDialog dialog = new OpenFileDialog
			{
				Filter = "Game Executable (*.exe)|*.exe",
				Title = "Select your game's .exe file (Tomb4.exe or PCTomb5.exe)"
			};

			if (dialog.ShowDialog() == DialogResult.OK)
				textBox_GamePath.Text = dialog.FileName;
		}

		private void button_BrowseNGCenter_Click(object sender, EventArgs e)
		{
			OpenFileDialog dialog = new OpenFileDialog
			{
				Filter = "NG_Center Executable (*.exe)|*.exe",
				Title = "Select NG_Center.exe file"
			};

			if (dialog.ShowDialog() == DialogResult.OK)
				textBox_NGCenterPath.Text = dialog.FileName;
		}

		private void textBox_GamePath_TextChanged(object sender, EventArgs e)
		{
			if (textBox_GamePath.Text.ToLower().EndsWith("tomb4.exe"))
			{
				// Disable TR5 related RadioButtons
				radioButton_TR4.Enabled = true;
				radioButton_TRNG.Enabled = true;
				radioButton_TR5.Enabled = false;
				radioButton_TR5Main.Enabled = false;
			}
			else if (textBox_GamePath.Text.ToLower().EndsWith("pctomb5.exe"))
			{
				// Disable TR4 related RadioButtons
				radioButton_TR4.Enabled = false;
				radioButton_TRNG.Enabled = false;
				radioButton_TR5.Enabled = true;
				radioButton_TR5Main.Enabled = true;
			}
			else
			{
				// Re-Enable everything
				radioButton_TR4.Enabled = true;
				radioButton_TRNG.Enabled = true;
				radioButton_TR5.Enabled = true;
				radioButton_TR5Main.Enabled = true;
			}
		}

		private void radioButton_TRNG_CheckedChanged(object sender, EventArgs e)
		{
			if (radioButton_TRNG.Checked)
			{
				// Expand GroupBox and Window size to show NG_Center path selection
				groupBox_ProjectSetup.Height = 102;
				Height = 240;

				label_NGCenterPath.Visible = true;
				textBox_NGCenterPath.Visible = true;
				button_BrowseNGCenter.Visible = true;
			}
			else
			{
				groupBox_ProjectSetup.Height = 72;
				Height = 210;

				label_NGCenterPath.Visible = false;
				textBox_NGCenterPath.Visible = false;
				button_BrowseNGCenter.Visible = false;
			}
		}
	}
}
