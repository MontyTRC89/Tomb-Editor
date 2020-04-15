using DarkUI.Controls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using TombIDE.Shared;
using TombIDE.Shared.SharedClasses;

namespace TombIDE.ScriptEditor.Controls
{
	public partial class FileList : UserControl
	{
		private IDE _ide;

		#region Initialization

		public FileList()
		{
			InitializeComponent();
		}

		public void Initialize(IDE ide)
		{
			_ide = ide;

			scriptFolderWatcher.Path = _ide.Project.ScriptPath;

			UpdateTreeView();
		}

		#endregion Initialization

		#region Events

		private void sectionPanel_Resize(object sender, EventArgs e) => AdjustButtonSizes();

		private void button_EditScript_Click(object sender, EventArgs e) =>
			_ide.ScriptEditor_OpenFile(PathHelper.GetScriptFilePath(_ide.Project));

		private void button_EditStrings_Click(object sender, EventArgs e) =>
			_ide.ScriptEditor_OpenFile(PathHelper.GetLanguageFilePath(_ide.Project, _ide.Project.DefaultLanguage));

		private void button_OpenInExplorer_Click(object sender, EventArgs e) =>
			SharedMethods.OpenFolderInExplorer(_ide.Project.ScriptPath);

		private void treeView_DoubleClick(object sender, EventArgs e)
		{
			// If the user hasn't selected any node or the selected node is empty
			if (treeView.SelectedNodes.Count == 0 || string.IsNullOrWhiteSpace(treeView.SelectedNodes[0].Text))
				return;

			// If the selected node is not a .txt or .lua file
			if (!treeView.SelectedNodes[0].Text.ToLower().EndsWith(".txt") && !treeView.SelectedNodes[0].Text.ToLower().EndsWith(".lua"))
				return;

			_ide.ScriptEditor_OpenFile(treeView.SelectedNodes[0].Tag.ToString());
		}

		private void FolderWatcher_Changed(object sender, FileSystemEventArgs e) => UpdateTreeView();
		private void FolderWatcher_Renamed(object sender, RenamedEventArgs e) => UpdateTreeView();

		#endregion Events

		#region Methods

		private void UpdateTreeView()
		{
			treeView.Nodes.Clear();

			Stack<DarkTreeNode> stack = new Stack<DarkTreeNode>();
			DirectoryInfo scriptDirectory = new DirectoryInfo(_ide.Project.ScriptPath);

			DarkTreeNode node = new DarkTreeNode(Path.GetFileName(_ide.Project.ScriptPath))
			{
				Icon = Properties.Resources.folder.ToBitmap(),
				Tag = scriptDirectory
			};

			stack.Push(node);

			while (stack.Count > 0)
			{
				DarkTreeNode currentNode = stack.Pop();
				DirectoryInfo info = (DirectoryInfo)currentNode.Tag;

				foreach (DirectoryInfo directory in info.GetDirectories())
				{
					DarkTreeNode childDirectoryNode = new DarkTreeNode(directory.Name)
					{
						Icon = Properties.Resources.folder.ToBitmap(),
						Tag = directory
					};

					currentNode.Nodes.Add(childDirectoryNode);
					stack.Push(childDirectoryNode);
				}

				foreach (FileInfo file in info.GetFiles())
				{
					if (Path.GetExtension(file.Name).ToLower() == ".txt" || Path.GetExtension(file.Name).ToLower() == ".lua")
					{
						DarkTreeNode fileNode = new DarkTreeNode(file.Name)
						{
							Icon = Properties.Resources.file.ToBitmap(),
							Tag = file.FullName
						};

						currentNode.Nodes.Add(fileNode);
					}
				}
			}

			node.Expanded = true;
			treeView.Nodes.Add(node);
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

		#endregion Methods
	}
}
