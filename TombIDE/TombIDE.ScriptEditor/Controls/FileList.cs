using DarkUI.Controls;
using DarkUI.Forms;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using TombIDE.ScriptEditor.Forms;
using TombIDE.ScriptEditor.Helpers;
using TombIDE.Shared;
using TombIDE.Shared.SharedClasses;

namespace TombIDE.ScriptEditor.Controls
{
	internal partial class FileList : UserControl
	{
		private IDE _ide;

		#region Construction and public methods

		public FileList()
		{
			InitializeComponent();
		}

		public void Initialize(IDE ide)
		{
			_ide = ide;

			UpdateFileList();
		}

		public void UpdateFileList()
		{
			// Cache the current node selection in order to reselect it after the list has been updated
			string selectedNodeFullPath = null;

			if (treeView.SelectedNodes.Count > 0)
				selectedNodeFullPath = treeView.SelectedNodes[0].FullPath;

			string fileSearchPattern = "*" + string.Join("|*", SupportedFormats.Text);

			// Create a node with the full /Script/ folder file list of the project
			DarkTreeNode fullFileListNode = FileHelper.CreateFullFileListNode(_ide.Project.ScriptPath, fileSearchPattern, treeView);
			fullFileListNode.Expanded = true;

			// Remove all nodes from the treeView and apply the node we just got
			treeView.Nodes.Clear();
			treeView.Nodes.Add(fullFileListNode);

			// Reselect the cached node (if it exists)
			if (!string.IsNullOrEmpty(selectedNodeFullPath))
			{
				DarkTreeNode nodeToReselect = treeView.FindNode(selectedNodeFullPath);

				if (nodeToReselect != null)
					treeView.SelectNode(nodeToReselect);
			}
		}

		#endregion Construction and public methods

		#region Events

		private void sectionPanel_Resize(object sender, EventArgs e) => AdjustButtonSizes();

		private void button_EditScript_Click(object sender, EventArgs e) =>
			_ide.ScriptEditor_OpenFile(PathHelper.GetScriptFilePath(_ide.Project));

		private void button_EditStrings_Click(object sender, EventArgs e) =>
			_ide.ScriptEditor_OpenFile(PathHelper.GetLanguageFilePath(_ide.Project, _ide.Project.DefaultLanguage));

		private void button_OpenInExplorer_Click(object sender, EventArgs e) =>
			SharedMethods.OpenInExplorer(_ide.Project.ScriptPath);

		private void treeView_DoubleClick(object sender, EventArgs e) => OpenSelectedFile();
		private void treeView_SelectedNodesChanged(object sender, EventArgs e) => ToggleModificationButtons();

		private void menuItem_NewFile_Click(object sender, EventArgs e) => CreateNewFile();
		private void menuItem_NewFolder_Click(object sender, EventArgs e) => CreateNewFolder();
		private void menuItem_OpenInEditor_Click(object sender, EventArgs e) => OpenSelectedFile();
		private void menuItem_Rename_Click(object sender, EventArgs e) => RenameItem();
		private void menuItem_Delete_Click(object sender, EventArgs e) => DeleteItem();

		private void menuItem_OpenInExplorer_Click(object sender, EventArgs e) =>
			SharedMethods.OpenInExplorer(GetItemPathFromNode(treeView.SelectedNodes[0]));

		#endregion Events

		#region Event methods

		private void OpenSelectedFile()
		{
			if (treeView.SelectedNodes.Count == 0)
				return;

			DarkTreeNode selectedNode = treeView.SelectedNodes[0];

			if (IsDirectoryNode(selectedNode))
				return;

			FileInfo selectedNodeFileInfo = (FileInfo)selectedNode.Tag;

			if (!IsSuppoertdFileFormat(selectedNodeFileInfo))
				return;

			_ide.ScriptEditor_OpenFile(selectedNodeFileInfo.FullName);
		}

		private void CreateNewFile()
		{
			using (FormFileCreation form = new FormFileCreation(_ide, FileCreationMode.New, GetInitialNodePath()))
				if (form.ShowDialog(this) == DialogResult.OK)
					File.Create(form.NewFilePath).Close();
		}

		private void CreateNewFolder()
		{
			using (FormFolderCreation form = new FormFolderCreation(_ide, GetInitialNodePath()))
				if (form.ShowDialog(this) == DialogResult.OK)
					Directory.CreateDirectory(form.NewFolderPath);
		}

		private void RenameItem()
		{
			string targetItemPath = GetItemPathFromNode(treeView.SelectedNodes[0]);

			if (string.IsNullOrEmpty(targetItemPath))
				return;

			using (FormRenameItem form = new FormRenameItem(targetItemPath))
				form.ShowDialog(this);
		}

		private void DeleteItem()
		{
			DarkTreeNode selectedNode = treeView.SelectedNodes[0];

			string message;

			if (IsDirectoryNode(selectedNode))
				message = "Are you sure you want to move the \"" + treeView.SelectedNodes[0].Text + "\" folder into the recycle bin?";
			else if (IsFileNode(selectedNode))
				message = "Are you sure you want to move the \"" + treeView.SelectedNodes[0].Text + "\" file into the recycle bin?";
			else
				return;

			DialogResult result = DarkMessageBox.Show(this, message, "Are you sure?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

			if (result == DialogResult.Yes)
			{
				if (IsDirectoryNode(selectedNode))
					FileSystem.DeleteDirectory(GetItemPathFromNode(selectedNode), UIOption.AllDialogs, RecycleOption.SendToRecycleBin);
				else
					FileSystem.DeleteFile(GetItemPathFromNode(selectedNode), UIOption.AllDialogs, RecycleOption.SendToRecycleBin);
			}
		}

		private void AdjustButtonSizes() // 𝓡𝓮𝓼𝓹𝓸𝓷𝓼𝓲𝓿𝓮𝓷𝓮𝓼𝓼
		{
			button_EditScript.Width = (sectionPanel.Width / 2) - 5;
			button_EditStrings.Location = new Point(sectionPanel.Width / 2, button_EditStrings.Location.Y);

			if (sectionPanel.Width % 2 > 0)
				button_EditStrings.Width = (sectionPanel.Width / 2) - 5;
			else
				button_EditStrings.Width = (sectionPanel.Width / 2) - 6;
		}

		private void ToggleModificationButtons()
		{
			if (treeView.SelectedNodes.Count == 0)
				DisableContextMenuItems();
			else
			{
				DarkTreeNode selectedNode = treeView.SelectedNodes[0];

				if (IsDirectoryNode(selectedNode) || IsFileNode(selectedNode))
				{
					bool isModifiable = IsModifiableItemNode(selectedNode);

					menuItem_OpenInEditor.Enabled = IsFileNode(selectedNode);
					menuItem_Rename.Enabled = isModifiable;
					menuItem_Delete.Enabled = isModifiable;

					menuItem_OpenInExplorer.Enabled = true;
				}
				else
					DisableContextMenuItems();
			}
		}

		#endregion Event methods

		#region Other methods

		private string GetInitialNodePath()
		{
			if (treeView.SelectedNodes.Count == 0)
				return null;

			DarkTreeNode selectedNode = treeView.SelectedNodes[0];

			if (IsDirectoryNode(selectedNode))
				return selectedNode.FullPath;
			else if (IsFileNode(selectedNode))
			{
				string fileNameSubstring = selectedNode.FullPath.Split('\\').Last();
				return selectedNode.FullPath.Remove(selectedNode.FullPath.Length - (fileNameSubstring.Length + 1));
			}
			else
				return null;
		}

		private string GetItemPathFromNode(DarkTreeNode node)
		{
			if (IsDirectoryNode(node))
				return ((DirectoryInfo)node.Tag).FullName;
			else if (IsFileNode(node))
				return ((FileInfo)node.Tag).FullName;
			else
				return null;
		}

		private void DisableContextMenuItems()
		{
			menuItem_OpenInEditor.Enabled = false;
			menuItem_Rename.Enabled = false;
			menuItem_Delete.Enabled = false;
			menuItem_OpenInExplorer.Enabled = false;
		}

		private bool IsDirectoryNode(DarkTreeNode node)
		{
			return node.Tag is DirectoryInfo;
		}

		private bool IsFileNode(DarkTreeNode node)
		{
			return node.Tag is FileInfo;
		}

		private bool IsModifiableItemNode(DarkTreeNode node)
		{
			if (node == treeView.Nodes[0])
				return false;

			if (IsFileNode(node))
			{
				FileInfo nodeFileInfo = (FileInfo)node.Tag;

				if (nodeFileInfo.FullName.Equals(Path.Combine(_ide.Project.ScriptPath, "script.txt"), StringComparison.OrdinalIgnoreCase))
					return false; // Renaming or removing the script.txt file is not allowed

				if (nodeFileInfo.FullName.Equals(Path.Combine(_ide.Project.ScriptPath, _ide.Project.DefaultLanguage + ".txt"), StringComparison.OrdinalIgnoreCase))
					return false; // Renaming or removing the default language file is not allowed
			}

			return true;
		}

		private bool IsSuppoertdFileFormat(FileInfo fileInfo)
		{
			foreach (string format in SupportedFormats.Text)
				if (fileInfo.Extension.Equals(format, StringComparison.OrdinalIgnoreCase))
					return true;

			return false;
		}

		#endregion Other methods
	}
}
