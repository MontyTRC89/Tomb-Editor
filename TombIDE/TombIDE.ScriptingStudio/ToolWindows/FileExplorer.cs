using DarkUI.Controls;
using DarkUI.Docking;
using DarkUI.Forms;
using Microsoft.VisualBasic.FileIO;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using TombIDE.ScriptingStudio.Forms;
using TombIDE.ScriptingStudio.Helpers;
using TombIDE.ScriptingStudio.Objects;
using TombIDE.Shared;
using TombIDE.Shared.SharedClasses;
using TombLib.Scripting.Enums;

namespace TombIDE.ScriptingStudio.ToolWindows
{
	public partial class FileExplorer : DarkToolWindow
	{
		#region Properties

		public string RootDirectoryPath
		{
			get => fileSystemWatcher.Path;
			set
			{
				fileSystemWatcher.Path = value;
				folderWatcher.Path = value;

				UpdateFileList();
			}
		}

		public string Filter
		{
			get => fileSystemWatcher.Filter;
			set
			{
				fileSystemWatcher.Filter = value;
				UpdateFileList();
			}
		}

		public NotifyFilters NotifyFilter
		{
			get => fileSystemWatcher.NotifyFilter;
			set => fileSystemWatcher.NotifyFilter = value;
		}

		#endregion Properties

		#region Construction

		public FileExplorer() : this(string.Empty)
		{ }
		public FileExplorer(string rootDirectoryPath) : this(rootDirectoryPath, string.Empty)
		{ }
		public FileExplorer(string rootDirectoryPath, string filter)
		{
			InitializeComponent();
			DockText = Strings.Default.FileExplorer;

			menuItem_NewFile.Text = Strings.Default.NewFile;
			menuItem_NewFolder.Text = Strings.Default.NewFolder;
			menuItem_ViewInEditor.Text = Strings.Default.ViewInDefaultEditor;
			menuItem_ViewCode.Text = Strings.Default.ViewCode;
			menuItem_Rename.Text = Strings.Default.RenameItem;
			menuItem_Delete.Text = Strings.Default.DeleteItem;
			menuItem_OpenInExplorer.Text = Strings.Default.OpenItemInExplorer;

			if (LicenseManager.UsageMode != LicenseUsageMode.Designtime && !string.IsNullOrEmpty(rootDirectoryPath))
			{
				RootDirectoryPath = rootDirectoryPath;
				Filter = filter;
			}
		}

		#endregion Construction

		#region Events

		public event FileOpenedEventHandler FileOpened;
		protected virtual void OnFileOpened(FileOpenedEventArgs e)
			=> FileOpened?.Invoke(this, e);

		public event FileSystemEventHandler FileChanged;
		protected virtual void OnFileChanged(object sender, FileSystemEventArgs e)
			=> FileChanged?.Invoke(sender, e);

		public event FileSystemEventHandler FileCreated;
		protected virtual void OnFileCreated(object sender, FileSystemEventArgs e)
		{
			UpdateFileList();
			FileCreated?.Invoke(sender, e);
		}

		public event FileSystemEventHandler FileDeleted;
		protected virtual void OnFileDeleted(object sender, FileSystemEventArgs e)
		{
			UpdateFileList();
			FileDeleted?.Invoke(sender, e);
		}

		public event RenamedEventHandler FileRenamed;
		protected virtual void OnFileRenamed(object sender, RenamedEventArgs e)
		{
			UpdateFileList();
			FileRenamed?.Invoke(sender, e);
		}

		private void treeView_DoubleClick(object sender, EventArgs e) => OpenSelectedFile();
		private void treeView_SelectedNodesChanged(object sender, EventArgs e) => ToggleModificationButtons();

		private void menuItem_NewFile_Click(object sender, EventArgs e) => CreateNewFile();
		private void menuItem_NewFolder_Click(object sender, EventArgs e) => CreateNewFolder();
		private void menuItem_ViewInEditor_Click(object sender, EventArgs e) => OpenSelectedFile();
		private void menuItem_ViewCode_Click(object sender, EventArgs e) => OpenSelectedFile(EditorType.Text);
		private void menuItem_Rename_Click(object sender, EventArgs e) => RenameItem();
		private void menuItem_Delete_Click(object sender, EventArgs e) => DeleteItem();

		private void menuItem_OpenInExplorer_Click(object sender, EventArgs e)
			=> SharedMethods.OpenInExplorer(GetItemPathFromNode(treeView.SelectedNodes[0]));

		#endregion Events

		#region Event methods

		private void OpenSelectedFile(EditorType editorType = EditorType.Default)
		{
			if (treeView.SelectedNodes.Count == 0)
				return;

			DarkTreeNode selectedNode = treeView.SelectedNodes[0];

			if (IsDirectoryNode(selectedNode))
				return;

			var selectedNodeFileInfo = selectedNode.Tag as FileInfo;

			if (!IsSuppoertdFileFormat(selectedNodeFileInfo))
				return;

			OnFileOpened(new FileOpenedEventArgs(selectedNodeFileInfo.FullName, editorType));
		}

		public void CreateNewFile()
		{
			using (var form = new FormFileCreation(RootDirectoryPath, FileCreationMode.New, GetInitialNodePath()))
				if (form.ShowDialog(this) == DialogResult.OK)
				{
					File.Create(form.NewFilePath).Close();
					OnFileOpened(new FileOpenedEventArgs(form.NewFilePath));
				}
		}

		public void CreateNewFolder()
		{
			using (var form = new FormFolderCreation(RootDirectoryPath, GetInitialNodePath()))
				if (form.ShowDialog(this) == DialogResult.OK)
					Directory.CreateDirectory(form.NewFolderPath);
		}

		private void RenameItem()
		{
			string targetItemPath = GetItemPathFromNode(treeView.SelectedNodes[0]);

			if (string.IsNullOrEmpty(targetItemPath))
				return;

			using (var form = new FormRenameItem(targetItemPath))
				form.ShowDialog(this);
		}

		private void DeleteItem()
		{
			DarkTreeNode selectedNode = treeView.SelectedNodes[0];

			string message;

			if (IsFileNode(selectedNode))
				message = string.Format(Strings.Default.AskMoveFileToBin, selectedNode.Text);
			else if (IsDirectoryNode(selectedNode))
				message = string.Format(Strings.Default.AskMoveFolderToBin, selectedNode.Text);
			else
				return;

			DialogResult result = DarkMessageBox.Show(this,
				message, Strings.Default.AreYouSure, MessageBoxButtons.YesNo, MessageBoxIcon.Question);

			if (result == DialogResult.Yes)
			{
				if (IsFileNode(selectedNode))
					FileSystem.DeleteFile(GetItemPathFromNode(selectedNode), UIOption.AllDialogs, RecycleOption.SendToRecycleBin);
				else if (IsDirectoryNode(selectedNode))
					FileSystem.DeleteDirectory(GetItemPathFromNode(selectedNode), UIOption.AllDialogs, RecycleOption.SendToRecycleBin);
			}
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

					menuItem_ViewCode.Enabled = IsFileNode(selectedNode);
					menuItem_ViewInEditor.Enabled = IsFileNode(selectedNode);
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

		public void UpdateFileList()
		{
			if (string.IsNullOrEmpty(fileSystemWatcher.Path))
				return;

			// Cache the current node selection in order to reselect it after the list has been updated
			string selectedNodeFullPath = null;

			if (treeView.SelectedNodes.Count > 0)
				selectedNodeFullPath = treeView.SelectedNodes[0].FullPath;

			// Create a node with the full /Script/ folder file list of the project
			DarkTreeNode fullFileListNode = FileTreeViewHelper.CreateFullFileListNode(RootDirectoryPath, Filter, treeView);
			fullFileListNode.Expanded = true;

			// Remove all nodes from the treeView
			treeView.Nodes.Clear();

			// Apply the node we just created
			treeView.Nodes.Add(fullFileListNode);

			// Reselect the cached node (if it exists)
			if (!string.IsNullOrEmpty(selectedNodeFullPath))
			{
				DarkTreeNode nodeToReselect = treeView.FindNode(selectedNodeFullPath);

				if (nodeToReselect != null)
					treeView.SelectNode(nodeToReselect);
			}
		}

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
			if (IsFileNode(node))
				return (node.Tag as FileInfo).FullName;
			else if (IsDirectoryNode(node))
				return (node.Tag as DirectoryInfo).FullName;
			else
				return null;
		}

		private void DisableContextMenuItems()
		{
			menuItem_ViewInEditor.Enabled = false;
			menuItem_Rename.Enabled = false;
			menuItem_Delete.Enabled = false;
			menuItem_OpenInExplorer.Enabled = false;
		}

		private bool IsFileNode(DarkTreeNode node) => node.Tag is FileInfo;
		private bool IsDirectoryNode(DarkTreeNode node) => node.Tag is DirectoryInfo;

		private bool IsModifiableItemNode(DarkTreeNode node)
		{
			if (node == treeView.Nodes[0])
				return false;

			if (IsFileNode(node))
			{
				var nodeFileInfo = node.Tag as FileInfo;

				if (nodeFileInfo.FullName.Equals(Path.Combine(RootDirectoryPath, "script.txt"), StringComparison.OrdinalIgnoreCase))
					return false; // Renaming or removing the script.txt file is not allowed

				if (nodeFileInfo.FullName.Equals(Path.Combine(RootDirectoryPath, "english.txt"), StringComparison.OrdinalIgnoreCase))
					return false; // Renaming or removing the default language file is not allowed
			}

			return true;
		}

		private bool IsSuppoertdFileFormat(FileInfo fileInfo)
		{
			if (Filter == "*.*")
				return true;

			foreach (string format in Filter.Split('|'))
				if (fileInfo.Extension.Equals(format.TrimStart('*'), StringComparison.OrdinalIgnoreCase))
					return true;

			return false;
		}

		#endregion Other methods
	}
}
