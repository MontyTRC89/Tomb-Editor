﻿using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using TombIDE.Shared;
using TombIDE.Shared.SharedClasses;
using TombLib.LevelData;
using TombLib.LevelData.IO;

namespace TombIDE.ProjectMaster
{
	public partial class FormLevelSetup : DarkForm
	{
		public ProjectLevel CreatedLevel { get; private set; }
		public List<string> GeneratedScriptLines { get; private set; }

		private Project _targetProject;

		#region Initialization

		public FormLevelSetup(Project targetProject)
		{
			_targetProject = targetProject;

			InitializeComponent();

			if (targetProject.GameVersion == TRVersion.Game.TR1 || targetProject.GameVersion == TRVersion.Game.TombEngine)
			{
				checkBox_GenerateSection.Checked = checkBox_GenerateSection.Visible = false;
				panel_ScriptSettings.Visible = false;
			}
			else if (targetProject.GameVersion != TRVersion.Game.TR4 && targetProject.GameVersion != TRVersion.Game.TRNG)
			{
				checkBox_EnableHorizon.Visible = false;
				panel_ScriptSettings.Height -= 35;
			}

			if (_targetProject.GameVersion == TRVersion.Game.TR2)
				numeric_SoundID.Value = 33;
			else if (_targetProject.GameVersion == TRVersion.Game.TR3)
				numeric_SoundID.Value = 28;
		}

		#endregion Initialization

		#region Events

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);

			textBox_LevelName.Text = "New Level";
			textBox_LevelName.Focus();
			textBox_LevelName.SelectAll();
		}

		private void textBox_LevelName_TextChanged(object sender, EventArgs e)
		{
			if (!checkBox_CustomFileName.Checked)
				textBox_CustomFileName.Text = textBox_LevelName.Text.Trim().Replace(' ', '_');
		}

		private void checkBox_CustomFileName_CheckedChanged(object sender, EventArgs e)
		{
			if (checkBox_CustomFileName.Checked)
				textBox_CustomFileName.Enabled = true;
			else
			{
				textBox_CustomFileName.Enabled = false;
				textBox_CustomFileName.Text = textBox_LevelName.Text.Trim().Replace(' ', '_');
			}
		}

		private void textBox_CustomFileName_TextChanged(object sender, EventArgs e)
		{
			int cachedCaretPosition = textBox_CustomFileName.SelectionStart;

			textBox_CustomFileName.Text = textBox_CustomFileName.Text.Replace(' ', '_');
			textBox_CustomFileName.SelectionStart = cachedCaretPosition;
		}

		private void button_Create_Click(object sender, EventArgs e)
		{
			button_Create.Enabled = false;

			try
			{
				string levelName = PathHelper.RemoveIllegalPathSymbols(textBox_LevelName.Text.Trim());
				levelName = LevelHandling.RemoveIllegalNameSymbols(levelName);

				if (string.IsNullOrWhiteSpace(levelName))
					throw new ArgumentException("You must enter a valid name for your level.");

				// Check for name duplicates
				foreach (ProjectLevel projectLevel in _targetProject.Levels)
				{
					if (projectLevel.Name.ToLower() == levelName.ToLower())
						throw new ArgumentException("A level with the same name already exists on the list.");
				}

				string dataFileName = textBox_CustomFileName.Text.Trim();

				if (string.IsNullOrWhiteSpace(dataFileName))
					throw new ArgumentException("You must specify the custom PRJ2 / DATA file name.");

				string levelFolderPath = Path.Combine(_targetProject.LevelsPath, levelName);

				// Create the level folder
				if (!Directory.Exists(levelFolderPath))
					Directory.CreateDirectory(levelFolderPath);

				if (Directory.EnumerateFileSystemEntries(levelFolderPath).ToArray().Length > 0) // 99% this will never accidentally happen
					throw new ArgumentException("A folder with the same name as the \"Level name\" already exists in\n" +
						"the project's /Levels/ folder and it's not empty.");

				ProjectLevel createdLevel = new ProjectLevel
				{
					Name = levelName,
					DataFileName = dataFileName,
					FolderPath = levelFolderPath
				};

				// Create a simple .prj2 file with pre-set project settings (game paths etc.)
				Level level = Level.CreateSimpleLevel();

				string prj2FilePath = Path.Combine(createdLevel.FolderPath, createdLevel.DataFileName) + ".prj2";
				string exeFilePath = Path.Combine(_targetProject.EnginePath, _targetProject.GetExeFileName());

				string dataFilePath = Path.Combine(_targetProject.EnginePath, "data", createdLevel.DataFileName + _targetProject.GetLevelFileExtension());

				level.Settings.LevelFilePath = prj2FilePath;

				level.Settings.GameDirectory = level.Settings.MakeRelative(_targetProject.EnginePath, VariableType.LevelDirectory);
				level.Settings.GameExecutableFilePath = level.Settings.MakeRelative(exeFilePath, VariableType.LevelDirectory);
				level.Settings.GameLevelFilePath = level.Settings.MakeRelative(dataFilePath, VariableType.LevelDirectory);
				level.Settings.GameVersion = _targetProject.GameVersion;

				level.Settings.WadSoundPaths.Clear();
				level.Settings.WadSoundPaths.Add(new WadSoundPath(LevelSettings.VariableCreate(VariableType.LevelDirectory) + LevelSettings.Dir + ".." + LevelSettings.Dir + ".." + LevelSettings.Dir + "Sounds"));

				if (_targetProject.GameVersion == TRVersion.Game.TR3)
				{
					level.Settings.AgressiveTexturePacking = true;
					level.Settings.TexturePadding = 1;
				}

				level.Settings.LoadDefaultSoundCatalog();

				Prj2Writer.SaveToPrj2(prj2FilePath, level);

				if (checkBox_GenerateSection.Checked)
				{
					int ambientSoundID = (int)numeric_SoundID.Value;
					bool horizon = checkBox_EnableHorizon.Checked;

					// // // //
					GeneratedScriptLines = LevelHandling.GenerateScriptLines(createdLevel, _targetProject.GameVersion, ambientSoundID, horizon);
					// // // //
				}

				// // // //
				CreatedLevel = createdLevel;
				// // // //
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
			panel_ScriptSettings.Enabled = checkBox_GenerateSection.Checked;
		}

		private void button_OpenAudioFolder_Click(object sender, EventArgs e)
		{
			if (_targetProject.GameVersion == TRVersion.Game.TR1)
				SharedMethods.OpenInExplorer(Path.Combine(_targetProject.EnginePath, "music"));
			else
				SharedMethods.OpenInExplorer(Path.Combine(_targetProject.EnginePath, "audio"));
		}

		#endregion Events
	}
}
