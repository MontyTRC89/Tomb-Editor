using DarkUI.Controls;
using System.Collections.Generic;
using System.IO;
using TombIDE.ScriptEditor.Properties;

namespace TombIDE.ScriptEditor.Helpers
{
	internal static class FileTreeViewHelper
	{
		#region Public methods

		public static DarkTreeNode CreateFullFileListNode(string sourceDirectoryPath)
			=> CreateFullFileListNode(sourceDirectoryPath, string.Empty, false, null, false);

		public static DarkTreeNode CreateFullFileListNode(string sourceDirectoryPath, string fileSearchPattern)
			=> CreateFullFileListNode(sourceDirectoryPath, fileSearchPattern, false, null, false);

		public static DarkTreeNode CreateFullFileListNode(string sourceDirectoryPath, string fileSearchPattern, bool expandAllNodes)
			=> CreateFullFileListNode(sourceDirectoryPath, fileSearchPattern, false, null, expandAllNodes);

		public static DarkTreeNode CreateFullFileListNode(string sourceDirectoryPath, string fileSearchPattern, DarkTreeView expandedSourceTreeView)
			=> CreateFullFileListNode(sourceDirectoryPath, fileSearchPattern, false, expandedSourceTreeView, false);

		public static DarkTreeNode CreateFullFileListNode(string sourceDirectoryPath, bool directoriesOnly)
			=> CreateFullFileListNode(sourceDirectoryPath, string.Empty, directoriesOnly, null, false);

		public static DarkTreeNode CreateFullFileListNode(string sourceDirectoryPath, bool directoriesOnly, bool expandAllNodes)
			=> CreateFullFileListNode(sourceDirectoryPath, string.Empty, directoriesOnly, null, expandAllNodes);

		public static DarkTreeNode CreateFullFileListNode(string sourceDirectoryPath, bool directoriesOnly, DarkTreeView expandedSourceTreeView)
			=> CreateFullFileListNode(sourceDirectoryPath, string.Empty, directoriesOnly, expandedSourceTreeView, false);

		#endregion Public methods

		#region Private methods

		private static DarkTreeNode CreateFullFileListNode(string sourceDirectoryPath, string fileSearchPattern, bool directoriesOnly, DarkTreeView expandedSourceTreeView, bool expandAllNodes)
		{
			var sourceDirectory = new DirectoryInfo(sourceDirectoryPath);

			var node = new DarkTreeNode(sourceDirectory.Name)
			{
				Icon = Resources.Folder.ToBitmap(),
				Tag = sourceDirectory
			};

			FillNodeWithItems(node, fileSearchPattern, directoriesOnly, expandedSourceTreeView, expandAllNodes);

			if (expandAllNodes)
				node.Expanded = true;

			return node;
		}

		private static void FillNodeWithItems(DarkTreeNode node, string fileSearchPattern, bool directoriesOnly, DarkTreeView expandedSourceTreeView, bool expandAllNodes)
		{
			var stack = new Stack<DarkTreeNode>();
			stack.Push(node);

			while (stack.Count > 0)
			{
				DarkTreeNode currentNode = stack.Pop();
				var info = currentNode.Tag as DirectoryInfo;

				currentNode.Nodes.AddRange(GetDirectoryNodes(stack, info.GetDirectories(), expandedSourceTreeView, expandAllNodes));

				if (!directoriesOnly)
					foreach (string filter in fileSearchPattern.Split('|'))
						currentNode.Nodes.AddRange(GetFileNodes(info.GetFiles(filter)));

				if (expandedSourceTreeView != null)
				{
					DarkTreeNode expandedSourceNode = expandedSourceTreeView.FindNode(currentNode.FullPath);

					if (expandedSourceNode != null)
						currentNode.Expanded = expandedSourceNode.Expanded;
				}
				else if (expandAllNodes)
					currentNode.Expanded = true;
			}
		}

		private static IEnumerable<DarkTreeNode> GetDirectoryNodes(Stack<DarkTreeNode> loopStack, DirectoryInfo[] directories, DarkTreeView expandedSourceTreeView, bool expandAllNodes)
		{
			foreach (DirectoryInfo directory in directories)
			{
				var childDirectoryNode = new DarkTreeNode(directory.Name)
				{
					Icon = Resources.Folder.ToBitmap(),
					Tag = directory
				};

				if (expandedSourceTreeView != null)
				{
					DarkTreeNode expandedSourceChildNode = expandedSourceTreeView.FindNode(childDirectoryNode.FullPath);

					if (expandedSourceChildNode != null)
						childDirectoryNode.Expanded = expandedSourceChildNode.Expanded;
				}
				else if (expandAllNodes)
					childDirectoryNode.Expanded = true;

				loopStack.Push(childDirectoryNode);
				yield return childDirectoryNode;
			}
		}

		private static IEnumerable<DarkTreeNode> GetFileNodes(FileInfo[] files)
		{
			foreach (FileInfo file in files)
				yield return new DarkTreeNode(file.Name)
				{
					Icon = Resources.File.ToBitmap(),
					Tag = file
				};
		}

		#endregion Private methods
	}
}
