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

		#region Initialization

		public FormLevelSetup(IDE ide)
		{
			_ide = ide;

			InitializeComponent();
		}

		#endregion Initialization

		#region Events

		private void button_Create_Click(object sender, EventArgs e)
		{
			button_Create.Enabled = false;

			try
			{
				string levelName = SharedMethods.RemoveIllegalPathSymbols(textBox_LevelName.Text.Trim());
				levelName = LevelHandling.RemoveIllegalNameSymbols(levelName);

				if (string.IsNullOrWhiteSpace(levelName))
					throw new ArgumentException("You must enter a valid name for your level.");

				// Check for name duplicates
				foreach (ProjectLevel projectLevel in _ide.Project.Levels)
				{
					if (projectLevel.Name.ToLower() == levelName.ToLower())
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

				string prj2FilePath = Path.Combine(addedProjectLevel.FolderPath, addedProjectLevel.Name) + ".prj2";
				string exeFilePath = Path.Combine(_ide.Project.EnginePath, _ide.Project.GetExeFileName());

				string dataFileName = addedProjectLevel.Name.Replace(' ', '_') + _ide.Project.GetLevelFileExtension();
				string dataFilePath = Path.Combine(_ide.Project.EnginePath, "data", dataFileName);

				string projectSamplesPath = Path.Combine(_ide.Project.ProjectPath, "Sounds");

				level.Settings.LevelFilePath = prj2FilePath;

				level.Settings.GameDirectory = level.Settings.MakeRelative(_ide.Project.EnginePath, VariableType.LevelDirectory);
				level.Settings.GameExecutableFilePath = level.Settings.MakeRelative(exeFilePath, VariableType.LevelDirectory);
				level.Settings.GameLevelFilePath = level.Settings.MakeRelative(dataFilePath, VariableType.LevelDirectory);
				level.Settings.GameVersion = _ide.Project.GameVersion;

				level.Settings.SoundsCatalogs.Add(new ReferencedSoundsCatalog(level.Settings, projectSamplesPath));

				Prj2Writer.SaveToPrj2(prj2FilePath, level);

				if (checkBox_GenerateSection.Checked)
				{
					int ambientSoundID = (int)numeric_SoundID.Value;
					bool horizon = checkBox_EnableHorizon.Checked;

					List<string> scriptMessages = LevelHandling.GenerateSectionMessages(addedProjectLevel, ambientSoundID, horizon);

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
			SharedMethods.OpenFolderInExplorer(Path.Combine(_ide.Project.EnginePath, "audio"));

		#endregion Events
	}
}
