using DarkUI.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using TombIDE.Shared;
using TombLib.LevelData;
using TombLib.LevelData.IO;
using TombLib.Projects;

namespace TombIDE.ProjectMaster
{
	/* Warning */
	// This is the most fragile class in the whole project.
	// Modify at your own risk.

	public partial class SectionLevelProperties : UserControl
	{
		private IDE _ide;

		#region Initialization

		public SectionLevelProperties()
		{
			InitializeComponent();
		}

		public void Initialize(IDE ide)
		{
			_ide = ide;
			_ide.IDEEventRaised += OnIDEEventRaised;

			AddPinnedProgramsToContextMenu();
		}

		private void OnIDEEventRaised(IIDEEvent obj)
		{
			if (obj is IDE.SelectedLevelChangedEvent)
			{
				// Update the whole control depending on the level selection
				if (_ide.SelectedLevel != null)
				{
					tabControl.Enabled = true;

					// This will speed up some things
					if (tabControl.SelectedIndex == 0)
						UpdateSettings();
					else if (tabControl.SelectedIndex == 1)
						UpdateResourceList();
				}
				else
				{
					ClearEverything();
					tabControl.Enabled = false;
				}
			}
			else if (obj is IDE.ProgramButtonsChangedEvent)
			{
				// Cache the first 3 items, because they are important
				List<ToolStripItem> cachedItems = new List<ToolStripItem>
				{
					contextMenu.Items[0],
					contextMenu.Items[1],
					contextMenu.Items[2]
				};

				// Clear the whole list
				contextMenu.Items.Clear();

				// Re-Add the cached items
				contextMenu.Items.AddRange(cachedItems.ToArray());

				AddPinnedProgramsToContextMenu();
			}
		}

		private void AddPinnedProgramsToContextMenu()
		{
			foreach (string programPath in _ide.Configuration.PinnedProgramPaths)
			{
				string exeFileName = Path.GetFileName(programPath).ToLower();

				// Exclude these programs from the list
				if (exeFileName == "tombeditor.exe" || exeFileName == "wadtool.exe" || exeFileName == "tombide.exe"
					|| exeFileName == "ng_center.exe" || exeFileName == "tomb4.exe" || exeFileName == "pctomb5.exe")
					continue;

				// Get the ProductName and the icon of the program
				string programName = FileVersionInfo.GetVersionInfo(programPath).ProductName;
				Image image = ImageHandling.ResizeImage(Icon.ExtractAssociatedIcon(programPath).ToBitmap(), 16, 16);

				// Create the menu item
				ToolStripMenuItem item = new ToolStripMenuItem
				{
					Image = image,
					Text = "Open with " + programName,
					Tag = programPath
				};

				// Bind the OnContextMenuProgramClicked event method to the item and add it to the list
				item.Click += OnContextMenuProgramClicked;
				contextMenu.Items.Add(item);
			}
		}

		#endregion Initialization

		#region Events

		private void tabControl_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (tabControl.SelectedIndex == 0)
				UpdateSettings();
			else if (tabControl.SelectedIndex == 1)
				UpdateResourceList();
		}

		private void radioButton_LatestFile_CheckedChanged(object sender, EventArgs e)
		{
			if (_ide.SelectedLevel == null)
				return;

			// Check if the setting actually changed
			if (radioButton_LatestFile.Checked && _ide.SelectedLevel.SpecificFile != "$(LatestFile)")
			{
				ClearAndDisablePrj2FileList();

				_ide.SelectedLevel.SpecificFile = "$(LatestFile)";
				_ide.RaiseEvent(new IDE.SelectedLevelSettingsChangedEvent());
			}
		}

		private void radioButton_SpecificFile_CheckedChanged(object sender, EventArgs e)
		{
			if (_ide.SelectedLevel == null)
				return;

			// Check if the setting actually changed
			if (radioButton_SpecificFile.Checked && _ide.SelectedLevel.SpecificFile == "$(LatestFile)")
			{
				EnableAndFillPrj2FileList();

				_ide.SelectedLevel.SpecificFile = treeView_AllPrjFiles.SelectedNodes[0].Text;
				_ide.RaiseEvent(new IDE.SelectedLevelSettingsChangedEvent());
			}
		}

		private void treeView_AllPrjFiles_SelectedNodesChanged(object sender, EventArgs e)
		{
			if (_ide.SelectedLevel == null)
				return;

			if (treeView_AllPrjFiles.SelectedNodes.Count > 0)
			{
				// Check if the setting actually changed
				if (_ide.SelectedLevel.SpecificFile != treeView_AllPrjFiles.SelectedNodes[0].Text)
				{
					_ide.SelectedLevel.SpecificFile = treeView_AllPrjFiles.SelectedNodes[0].Text;
					_ide.RaiseEvent(new IDE.SelectedLevelSettingsChangedEvent());
				}
			}
		}

		private void checkBox_ShowAllFiles_CheckedChanged(object sender, EventArgs e)
		{
			if (radioButton_LatestFile.Checked)
				return;

			UpdatePrjFileList();

			// If the user unchecked the checkBox and the SpecificFile was a backup file
			if (!checkBox_ShowAllFiles.Checked && ProjectLevel.IsBackupFile(_ide.SelectedLevel.SpecificFile))
				treeView_AllPrjFiles.SelectNode(treeView_AllPrjFiles.Nodes[0]); // Select something else since the item is no longer on the list
		}

		private void treeView_Resources_MouseClick(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right)
				ShowResourceContextMenu();
		}

		private void treeView_Resources_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
				OpenSelectedResource();
		}

		private void menuItem_Open_Click(object sender, EventArgs e) => OpenSelectedResource();

		private void menuItem_OpenFolder_Click(object sender, EventArgs e) =>
			SharedMethods.OpenFolderInExplorer(Path.GetDirectoryName(treeView_Resources.SelectedNodes[0].Text));

		private void OpenSelectedResource()
		{
			if (treeView_Resources.SelectedNodes.Count == 0)
				return;

			foreach (DarkTreeNode node in treeView_Resources.Nodes) // Check if the clicked node is not a default node
			{
				if (treeView_Resources.SelectedNodes[0] == node)
					return;
			}

			ProcessStartInfo startInfo = new ProcessStartInfo
			{
				FileName = treeView_Resources.SelectedNodes[0].Text
			};

			Process.Start(startInfo);
		}

		private void ShowResourceContextMenu()
		{
			if (treeView_Resources.SelectedNodes.Count == 0)
				return;

			foreach (DarkTreeNode node in treeView_Resources.Nodes) // Check if the clicked node is not a default node
			{
				if (treeView_Resources.SelectedNodes[0] == node)
					return;
			}

			contextMenu.Show(Cursor.Position);
		}

		private void OnContextMenuProgramClicked(object sender, EventArgs e)
		{
			string programPath = ((ToolStripMenuItem)sender).Tag.ToString();

			ProcessStartInfo startInfo = new ProcessStartInfo
			{
				Arguments = "\"" + treeView_Resources.SelectedNodes[0].Text + "\"",
				FileName = programPath
			};

			Process.Start(startInfo);
		}

		#endregion Events

		#region Methods

		private void UpdateSettings()
		{
			// Update the radio buttons
			radioButton_LatestFile.Checked = _ide.SelectedLevel.SpecificFile == "$(LatestFile)";
			radioButton_SpecificFile.Checked = _ide.SelectedLevel.SpecificFile != "$(LatestFile)";

			// Update the .prj2 file list depending on which radio button was checked
			if (radioButton_LatestFile.Checked)
				ClearAndDisablePrj2FileList();
			else
				EnableAndFillPrj2FileList();
		}

		private void UpdateResourceList()
		{
			treeView_Resources.SelectedNodes.Clear();
			treeView_Resources.Nodes.Clear();

			if (_ide.SelectedLevel == null)
				return;

			string prj2Path = string.Empty;

			if (_ide.SelectedLevel.SpecificFile == "$(LatestFile)")
				prj2Path = Path.Combine(_ide.SelectedLevel.FolderPath, _ide.SelectedLevel.GetLatestPrj2File());
			else
				prj2Path = Path.Combine(_ide.SelectedLevel.FolderPath, _ide.SelectedLevel.SpecificFile);

			Level level = Prj2Loader.LoadFromPrj2(prj2Path, null);
			LevelSettings settings = new LevelSettings();

			AddDefaultResourceNodes(level);

			AddTextureFileNodes(level, settings);
			AddWadFileNodes(level, settings);
			AddGeometryFileNodes(level, settings);

			treeView_Resources.Invalidate();
		}

		private void ClearEverything() // Used when _ide.SelectedLevel is null
		{
			radioButton_LatestFile.Checked = false;
			radioButton_SpecificFile.Checked = false;

			treeView_AllPrjFiles.SelectedNodes.Clear();
			treeView_AllPrjFiles.Nodes.Clear();
			treeView_AllPrjFiles.Invalidate();

			checkBox_ShowAllFiles.Checked = false;

			treeView_Resources.SelectedNodes.Clear();
			treeView_Resources.Nodes.Clear();
			treeView_Resources.Invalidate();
		}

		private void EnableAndFillPrj2FileList()
		{
			treeView_AllPrjFiles.Enabled = true;
			checkBox_ShowAllFiles.Enabled = true;

			// When the user switched the _ide.SelectedLevel and the current _ide.SelectedLevel 's SpecificFile is a backup file,
			// then check this checkbox, otherwise it will reset the SpecificFile to a non-backup file and we don't want that
			checkBox_ShowAllFiles.Checked = ProjectLevel.IsBackupFile(_ide.SelectedLevel.SpecificFile);

			UpdatePrjFileList();
		}

		private void ClearAndDisablePrj2FileList()
		{
			treeView_AllPrjFiles.SelectedNodes.Clear();
			treeView_AllPrjFiles.Nodes.Clear();

			checkBox_ShowAllFiles.Checked = false;

			treeView_AllPrjFiles.Enabled = false;
			checkBox_ShowAllFiles.Enabled = false;
		}

		private void UpdatePrjFileList()
		{
			treeView_AllPrjFiles.Nodes.Clear();

			DirectoryInfo directoryInfo = new DirectoryInfo(_ide.SelectedLevel.FolderPath);

			foreach (FileInfo fileInfo in directoryInfo.GetFiles("*.prj2", SearchOption.TopDirectoryOnly))
			{
				// Don't show backup files if checkBox_ShowAllFiles is unchecked
				if (!checkBox_ShowAllFiles.Checked && ProjectLevel.IsBackupFile(fileInfo.Name))
					continue;

				// Create the .prj2 file node
				DarkTreeNode node = new DarkTreeNode
				{
					Text = fileInfo.Name,
					Tag = fileInfo.FullName
				};

				// Add the node to the .prj2 file list
				treeView_AllPrjFiles.Nodes.Add(node);
			}

			// Select the SpecificFile node (if the file exists on the list)
			foreach (DarkTreeNode node in treeView_AllPrjFiles.Nodes)
			{
				if (node.Text == _ide.SelectedLevel.SpecificFile)
				{
					treeView_AllPrjFiles.SelectNode(node);
					break;
				}
			}

			if (treeView_AllPrjFiles.SelectedNodes.Count == 0)
				treeView_AllPrjFiles.SelectNode(treeView_AllPrjFiles.Nodes[0]); // Select the first node if no file was found

			treeView_AllPrjFiles.Invalidate();
		}

		private void AddDefaultResourceNodes(Level level)
		{
			DarkTreeNode texturesNode = new DarkTreeNode
			{
				Icon = Properties.Resources.image_file.ToBitmap(),
				Text = "Textures",
			};

			DarkTreeNode wadFilesNode = new DarkTreeNode
			{
				Icon = Properties.Resources.wad_file.ToBitmap(),
				Text = "Wad Files",
			};

			DarkTreeNode geometryNode = new DarkTreeNode
			{
				Icon = Properties.Resources.obj_file.ToBitmap(),
				Text = "Imported Geometry",
			};

			if (level.Settings.Textures.Count > 0)
				treeView_Resources.Nodes.Add(texturesNode);

			if (level.Settings.Wads.Count > 0)
				treeView_Resources.Nodes.Add(wadFilesNode);

			if (level.Settings.ImportedGeometries.Count > 0)
				treeView_Resources.Nodes.Add(geometryNode);
		}

		private void AddTextureFileNodes(Level level, LevelSettings settings)
		{
			foreach (LevelTexture texture in level.Settings.Textures)
			{
				string textureFilePath = settings.MakeAbsolute(texture.Image.FileName);

				if (!File.Exists(textureFilePath))
					continue;

				DarkTreeNode node = new DarkTreeNode
				{
					Icon = Properties.Resources.image_file.ToBitmap(),
					Text = textureFilePath,
				};

				treeView_Resources.Nodes[0].Nodes.Add(node);
				treeView_Resources.Nodes[0].Expanded = true;
			}
		}

		private void AddWadFileNodes(Level level, LevelSettings settings)
		{
			foreach (ReferencedWad wad in level.Settings.Wads)
			{
				string wadFilePath = settings.MakeAbsolute(wad.Wad.FileName);

				if (!File.Exists(wadFilePath))
					continue;

				DarkTreeNode node = new DarkTreeNode
				{
					Icon = Properties.Resources.wad_file.ToBitmap(),
					Text = wadFilePath,
				};

				treeView_Resources.Nodes[1].Nodes.Add(node);
				treeView_Resources.Nodes[1].Expanded = true;
			}
		}

		private void AddGeometryFileNodes(Level level, LevelSettings settings)
		{
			foreach (ImportedGeometry geometry in level.Settings.ImportedGeometries)
			{
				string geometryFilePath = settings.MakeAbsolute(geometry.Info.Path);

				if (!File.Exists(geometryFilePath))
					continue;

				DarkTreeNode node = new DarkTreeNode
				{
					Icon = Properties.Resources.obj_file.ToBitmap(),
					Text = geometryFilePath,
				};

				treeView_Resources.Nodes[2].Nodes.Add(node);
				treeView_Resources.Nodes[2].Expanded = true;
			}
		}

		#endregion Methods
	}
}
