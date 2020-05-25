﻿using DarkUI.Controls;
using DarkUI.Forms;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using TombIDE.Shared;
using TombIDE.Shared.SharedClasses;
using TombIDE.Shared.SharedForms;
using TombLib.LevelData;

namespace TombIDE.ProjectMaster
{
	public partial class SectionLevelList : UserControl
	{
		private IDE _ide;

		#region Initialization

		public SectionLevelList()
		{
			InitializeComponent();
		}

		public void Initialize(IDE ide)
		{
			_ide = ide;
			_ide.IDEEventRaised += OnIDEEventRaised;

			button_ViewFileNames.Checked = _ide.IDEConfiguration.ViewFileNames;

			FillLevelList(); // With levels taken from the .trproj file (current _ide.Project)
		}

		private void FillLevelList()
		{
			treeView.Nodes.Clear();

			// Add nodes into the treeView for each level
			foreach (ProjectLevel level in _ide.Project.Levels)
			{
				if (level.IsValidLevel())
					AddLevelToList(level);
			}

			// Update the .trproj file with only valid levels
			ReserializeTRPROJ();
		}

		private void AddLevelToList(ProjectLevel level, bool reserialize = false)
		{
			// Create the node
			DarkTreeNode node = new DarkTreeNode
			{
				Text = level.Name,
				Tag = level
			};

			// Adjust the node text if button_ViewFileNames is checked
			if (button_ViewFileNames.Checked)
			{
				if (level.SpecificFile == "$(LatestFile)")
					node.Text = level.Name + " (" + level.GetLatestPrj2File() + ")";
				else
					node.Text = level.Name + " (" + level.SpecificFile + ")";
			}

			// Mark external levels
			if (!level.FolderPath.StartsWith(_ide.Project.LevelsPath, StringComparison.OrdinalIgnoreCase))
				node.Text = _ide.IDEConfiguration.ExternalLevelPrefix + node.Text;

			// Add the node to the list
			treeView.Nodes.Add(node);

			if (reserialize)
				ReserializeTRPROJ();
		}

		#endregion Initialization

		#region Events

		private void OnIDEEventRaised(IIDEEvent obj)
		{
			if (obj is IDE.SelectedLevelSettingsChangedEvent)
			{
				// Update the text of the node (if the level name was changed ofc)
				if (button_ViewFileNames.Checked)
				{
					if (_ide.SelectedLevel.SpecificFile == "$(LatestFile)")
						treeView.SelectedNodes[0].Text = _ide.SelectedLevel.Name + " (" + _ide.SelectedLevel.GetLatestPrj2File() + ")";
					else
						treeView.SelectedNodes[0].Text = _ide.SelectedLevel.Name + " (" + _ide.SelectedLevel.SpecificFile + ")";
				}
				else
					treeView.SelectedNodes[0].Text = _ide.SelectedLevel.Name;

				// Mark external levels
				if (!_ide.SelectedLevel.FolderPath.StartsWith(_ide.Project.LevelsPath, StringComparison.OrdinalIgnoreCase))
					treeView.SelectedNodes[0].Text = _ide.IDEConfiguration.ExternalLevelPrefix + treeView.SelectedNodes[0].Text;

				// Update the node's Tag with the updated level
				treeView.SelectedNodes[0].Tag = _ide.SelectedLevel;

				// Send the new list (with the modified node) into the .trproj file
				ReserializeTRPROJ();
			}
			else if (obj is IDE.PRJ2FileDeletedEvent)
			{
				// Clear the selection immediately, otherwise many exceptions will happen!
				treeView.SelectedNodes.Clear();
				CheckItemSelection();

				RefreshLevelList();
			}
		}

		private void label_Hint_Click(object sender, EventArgs e) => ShowLevelSetupForm();

		private void button_New_Click(object sender, EventArgs e) => ShowLevelSetupForm();
		private void button_Import_Click(object sender, EventArgs e) => ImportLevel();
		private void button_Rename_Click(object sender, EventArgs e) => ShowRenameLevelForm();
		private void button_Delete_Click(object sender, EventArgs e) => DeleteLevel();

		private void button_MoveUp_Click(object sender, EventArgs e)
		{
			treeView.MoveSelectedNodeUp();
			ReserializeTRPROJ();
		}

		private void button_MoveDown_Click(object sender, EventArgs e)
		{
			treeView.MoveSelectedNodeDown();
			ReserializeTRPROJ();
		}

		private void button_OpenInExplorer_Click(object sender, EventArgs e) =>
			SharedMethods.OpenInExplorer(((ProjectLevel)treeView.SelectedNodes[0].Tag).FolderPath);

		private void button_Refresh_Click(object sender, EventArgs e) => RefreshLevelList();

		private void button_ViewFileNames_Click(object sender, EventArgs e)
		{
			button_ViewFileNames.Checked = !button_ViewFileNames.Checked;

			_ide.IDEConfiguration.ViewFileNames = button_ViewFileNames.Checked;
			_ide.IDEConfiguration.Save();

			AdjustTextOfAllNodes();
		}

		private void treeView_MouseClick(object sender, MouseEventArgs e)
		{
			if (treeView.SelectedNodes.Count == 0)
				return;

			if (e.Button == MouseButtons.Right)
				contextMenu.Show(Cursor.Position);

			CheckItemSelection();
		}

		private void treeView_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
				OpenSelectedLevel();
		}

		private void treeView_KeyDown(object sender, KeyEventArgs e)
		{
			if (treeView.SelectedNodes.Count == 0)
				return;

			if (e.KeyCode == Keys.F2)
				ShowRenameLevelForm();

			if (e.KeyCode == Keys.Delete)
				DeleteLevel();
		}

		private void treeView_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down)
				CheckItemSelection();
		}

		private void menuItem_OpenLevel_Click(object sender, EventArgs e) => OpenSelectedLevel();

		private void ShowLevelSetupForm()
		{
			using (FormLevelSetup form = new FormLevelSetup(_ide.Project))
			{
				if (form.ShowDialog(this) == DialogResult.OK)
					OnLevelAdded(form.CreatedLevel, form.GeneratedScriptLines);
			}
		}

		private void ImportLevel()
		{
			using (OpenFileDialog dialog = new OpenFileDialog())
			{
				dialog.Title = "Choose the .prj2 file you want to import";
				dialog.Filter = "Tomb Editor Levels|*.prj2";

				if (dialog.ShowDialog(this) == DialogResult.OK)
				{
					try
					{
						if (dialog.FileName.StartsWith(_ide.Project.LevelsPath, StringComparison.OrdinalIgnoreCase))
							throw new ArgumentException("You cannot import levels which are already inside the project's /Levels/ folder.");

						if (ProjectLevel.IsBackupFile(Path.GetFileName(dialog.FileName)))
							throw new ArgumentException("You cannot import backup files.");

						using (FormImportLevel form = new FormImportLevel(_ide.Project, dialog.FileName))
						{
							if (form.ShowDialog(this) == DialogResult.OK)
								OnLevelAdded(form.ImportedLevel, form.GeneratedScriptLines);
						}
					}
					catch (Exception ex)
					{
						DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					}
				}
			}
		}

		private void ShowRenameLevelForm()
		{
			using (FormRenameLevel form = new FormRenameLevel(_ide))
				form.ShowDialog(this); // After the form is done, it will trigger IDE.SelectedLevelSettingsChangedEvent
		}

		private void DeleteLevel()
		{
			ProjectLevel affectedLevel = (ProjectLevel)treeView.SelectedNodes[0].Tag;

			// We can't allow deleting directories of external levels, so we must handle them differently

			// Internal level paths always start with the project's LevelsPath
			bool isInternalLevel = affectedLevel.FolderPath.StartsWith(_ide.Project.LevelsPath, StringComparison.OrdinalIgnoreCase);
			string message;

			if (isInternalLevel)
			{
				message = "Are you sure you want to delete the \"" + affectedLevel.Name + "\" level?\n" +
					"This will send the level folder with all its files into the recycle bin.";
			}
			else
			{
				message = "Are you sure you want to delete the \"" + affectedLevel.Name + "\" level from the list?\n" +
					"Since this is an external level, this process will NOT affect the level folder nor its files.";
			}

			DialogResult result = DarkMessageBox.Show(this, message, "Are you sure?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

			if (result == DialogResult.Yes)
			{
				if (isInternalLevel) // Move the level folder into the recycle bin in this case
					FileSystem.DeleteDirectory(affectedLevel.FolderPath, UIOption.AllDialogs, RecycleOption.SendToRecycleBin);

				// Remove the node and clear selection
				treeView.SelectedNodes[0].Remove();
				treeView.SelectedNodes.Clear();
				CheckItemSelection();

				// Send the new list (without the removed node) into the .trproj file
				ReserializeTRPROJ();
			}
		}

		private void RefreshLevelList()
		{
			string cachedNodeText = string.Empty;

			if (treeView.SelectedNodes.Count > 0)
			{
				cachedNodeText = treeView.SelectedNodes[0].Text;

				// Reinitialize the selected level (That's for treeView_Resources in SectionLevelProperties)
				_ide.SelectedLevel = null;
				_ide.SelectedLevel = (ProjectLevel)treeView.SelectedNodes[0].Tag;
			}

			// Scan the project's /Levels/ folder using FormLoading
			using (FormLoading form = new FormLoading(_ide))
			{
				if (form.ShowDialog(this) == DialogResult.OK)
				{
					// Refresh the level list
					FillLevelList();

					// If a node was selected, reselect it after refreshing
					if (!string.IsNullOrEmpty(cachedNodeText))
					{
						foreach (DarkTreeNode node in treeView.Nodes)
						{
							if (node.Text.ToLower() == cachedNodeText.ToLower())
							{
								treeView.SelectNode(node);
								treeView.ScrollTo(node.FullArea.Location);

								break;
							}
						}
					}

					CheckItemSelection();
				}
			}
		}

		private void AdjustTextOfAllNodes()
		{
			if (button_ViewFileNames.Checked)
			{
				foreach (DarkTreeNode node in treeView.Nodes)
				{
					ProjectLevel nodeLevel = (ProjectLevel)node.Tag;

					if (nodeLevel.SpecificFile == "$(LatestFile)")
						node.Text = nodeLevel.Name + " (" + nodeLevel.GetLatestPrj2File() + ")";
					else
						node.Text = nodeLevel.Name + " (" + nodeLevel.SpecificFile + ")";

					// Mark external levels
					if (!nodeLevel.FolderPath.StartsWith(_ide.Project.LevelsPath, StringComparison.OrdinalIgnoreCase))
						node.Text = _ide.IDEConfiguration.ExternalLevelPrefix + node.Text;
				}
			}
			else
			{
				foreach (DarkTreeNode node in treeView.Nodes)
				{
					ProjectLevel nodeLevel = (ProjectLevel)node.Tag;
					node.Text = nodeLevel.Name;

					// Mark external levels
					if (!nodeLevel.FolderPath.StartsWith(_ide.Project.LevelsPath, StringComparison.OrdinalIgnoreCase))
						node.Text = _ide.IDEConfiguration.ExternalLevelPrefix + node.Text;
				}
			}
		}

		private void CheckItemSelection()
		{
			// Enable / Disable node specific buttons
			button_Rename.Enabled = treeView.SelectedNodes.Count > 0;
			button_Delete.Enabled = treeView.SelectedNodes.Count > 0;
			button_MoveUp.Enabled = treeView.SelectedNodes.Count > 0;
			button_MoveDown.Enabled = treeView.SelectedNodes.Count > 0;
			button_OpenInExplorer.Enabled = treeView.SelectedNodes.Count > 0;

			// Set the SelectedLevel variable if a node is selected
			// Triggers IDE.SelectedLevelChangedEvent
			if (treeView.SelectedNodes.Count > 0)
			{
				ProjectLevel nodeLevel = (ProjectLevel)treeView.SelectedNodes[0].Tag;

				// Validation check
				if (!nodeLevel.IsValidLevel())
				{
					treeView.SelectedNodes.Clear();
					CheckItemSelection(); // Recursion

					RefreshLevelList();
					return;
				}

				_ide.SelectedLevel = nodeLevel;
			}
			else
				_ide.SelectedLevel = null;
		}

		private void OpenSelectedLevel()
		{
			if (_ide.SelectedLevel == null)
				return;

			// Validation check
			if (!_ide.SelectedLevel.IsValidLevel())
			{
				treeView.SelectedNodes.Clear();
				CheckItemSelection();

				RefreshLevelList();
				return;
			}

			string prj2Path = string.Empty;

			if (_ide.SelectedLevel.SpecificFile == "$(LatestFile)")
				prj2Path = Path.Combine(_ide.SelectedLevel.FolderPath, _ide.SelectedLevel.GetLatestPrj2File());
			else
				prj2Path = Path.Combine(_ide.SelectedLevel.FolderPath, _ide.SelectedLevel.SpecificFile);

			ProcessStartInfo startInfo = new ProcessStartInfo
			{
				FileName = Path.Combine(DefaultPaths.GetProgramDirectory(), "TombEditor.exe"),
				Arguments = "\"" + prj2Path + "\""
			};

			Process.Start(startInfo);
		}

		#endregion Events

		#region Methods

		private void OnLevelAdded(ProjectLevel addedLevel, List<string> scriptLines)
		{
			AddLevelToList(addedLevel, true);

			// Select the new level node
			foreach (DarkTreeNode node in treeView.Nodes)
			{
				if (((ProjectLevel)node.Tag).Name.ToLower() == addedLevel.Name.ToLower())
				{
					treeView.SelectNode(node);
					CheckItemSelection();

					break;
				}
			}

			if (scriptLines != null && scriptLines.Count > 0)
			{
				_ide.ScriptEditor_AppendScriptLines(scriptLines);
				_ide.ScriptEditor_AddNewLevelString(addedLevel.Name);
			}
		}

		public void ReserializeTRPROJ()
		{
			treeView.Invalidate();

			_ide.Project.Levels.Clear();

			foreach (DarkTreeNode node in treeView.Nodes)
				_ide.Project.Levels.Add((ProjectLevel)node.Tag);

			_ide.Project.Save();

			label_Hint.Visible = treeView.Nodes.Count == 0;
		}

		#endregion Methods

		private void menuItem_Build_Click(object sender, EventArgs e)
		{
			BatchCompileList batchList = new BatchCompileList();

			string prj2Path;

			if (_ide.SelectedLevel.SpecificFile == "$(LatestFile)")
				prj2Path = Path.Combine(_ide.SelectedLevel.FolderPath, _ide.SelectedLevel.GetLatestPrj2File());
			else
				prj2Path = Path.Combine(_ide.SelectedLevel.FolderPath, _ide.SelectedLevel.SpecificFile);

			batchList.Files.Add(prj2Path);

			string batchListFilePath = Path.Combine(Path.GetTempPath(), "tide_batch.xml");
			BatchCompileList.SaveToXml(batchListFilePath, batchList);

			ProcessStartInfo startInfo = new ProcessStartInfo
			{
				FileName = Path.Combine(DefaultPaths.GetProgramDirectory(), "TombEditor.exe"),
				Arguments = "\"" + batchListFilePath + "\""
			};

			Process.Start(startInfo);
		}
	}
}
