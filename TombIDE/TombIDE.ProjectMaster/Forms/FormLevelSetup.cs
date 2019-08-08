using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using TombIDE.Shared;
using TombLib.LevelData;
using TombLib.LevelData.IO;
using TombLib.Projects;

namespace TombIDE.ProjectMaster
{
	public partial class FormLevelSetup : DarkForm
	{
		private IDE _ide;

		public FormLevelSetup(IDE ide)
		{
			_ide = ide;

			InitializeComponent();
		}

		private void button_Create_Click(object sender, EventArgs e)
		{
			button_Create.Enabled = false;

			try
			{
				if (string.IsNullOrWhiteSpace(textBox_LevelName.Text))
					throw new ArgumentException("You must enter a name for your level.");

				string levelName = textBox_LevelName.Text.Trim();

				// Check for name duplicates
				foreach (ProjectLevel projectlevel in _ide.Project.Levels)
				{
					if (projectlevel.Name == levelName)
						throw new ArgumentException("A level with the same name already exists on the list.");
				}

				string levelFolderPath = Path.Combine(_ide.Project.LevelsPath, levelName);

				// Create the level folder
				if (!Directory.Exists(levelFolderPath))
					Directory.CreateDirectory(levelFolderPath);

				if (Directory.EnumerateFileSystemEntries(levelFolderPath).ToArray().Length > 0) // 99% this will never accidentally happen
					throw new ArgumentException("A folder with the same name as the \"Level name\" already exists in\n" +
						"the project's /Levels/ folder and it's not empty.");

				ProjectLevel addedProjectLevel = new ProjectLevel
				{
					Name = levelName,
					FolderPath = levelFolderPath
				};

				// Create a simple .prj2 file with pre-set project settings (game paths etc.)
				Level level = Level.CreateSimpleLevel();

				string dataFileName = addedProjectLevel.Name.Replace(' ', '_') + _ide.Project.GetLevelFileExtension();

				level.Settings.GameDirectory = _ide.Project.ProjectPath;
				level.Settings.GameExecutableFilePath = Path.Combine(_ide.Project.ProjectPath, _ide.Project.GetExeFileName());
				level.Settings.GameLevelFilePath = Path.Combine(_ide.Project.ProjectPath, "data", dataFileName);
				level.Settings.GameVersion = _ide.Project.GameVersion;

				string prj2FilePath = Path.Combine(addedProjectLevel.FolderPath, addedProjectLevel.Name) + ".prj2";
				Prj2Writer.SaveToPrj2(prj2FilePath, level);

				if (checkBox_GenerateSection.Checked)
				{
					List<string> scriptMessages = new List<string>
					{
						"\n[Level]",
						"Name= " + addedProjectLevel.Name,
						"Level= DATA\\" + addedProjectLevel.Name.ToUpper().Replace(' ', '_') + ", " + textBox_SoundID.Text.Trim(),
						"LoadCamera= 0, 0, 0, 0, 0, 0, 0",
						"Horizon= " + (checkBox_EnableHorizon.Checked? "ENABLED" : "DISABLED")
					};

					_ide.AddLevelToProject(addedProjectLevel, scriptMessages);
				}
				else
					_ide.AddLevelToProject(addedProjectLevel);
			}
			catch (Exception ex)
			{
				DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				button_Create.Enabled = true;
				DialogResult = DialogResult.None;
			}
		}

		private void checkBox_GenerateSection_CheckedChanged(object sender, EventArgs e)
		{
			if (checkBox_GenerateSection.Checked)
			{
				panel_ScriptSettings.Visible = true;
				panel_01.Height = 108;
				Height = 245;
			}
			else
			{
				panel_ScriptSettings.Visible = false;
				panel_01.Height = 35;
				Height = 172;
			}
		}

		private void button_OpenAudioFolder_Click(object sender, EventArgs e) =>
			SharedMethods.OpenFolderInExplorer(Path.Combine(_ide.Project.ProjectPath, "audio"));
	}
}
