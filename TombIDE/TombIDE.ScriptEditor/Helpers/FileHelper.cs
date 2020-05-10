using DarkUI.Controls;
using System.Collections.Generic;
using System.IO;

namespace TombIDE.ScriptEditor.Helpers
{
	internal static class FileHelper
	{
		#region Public methods

		public static DarkTreeNode CreateFullFileListNode(string sourceDirectoryPath)
		{ return CreateFullFileListNode(sourceDirectoryPath, string.Empty, false, null, false); }

		public static DarkTreeNode CreateFullFileListNode(string sourceDirectoryPath, string fileSearchPattern)
		{ return CreateFullFileListNode(sourceDirectoryPath, fileSearchPattern, false, null, false); }

		public static DarkTreeNode CreateFullFileListNode(string sourceDirectoryPath, string fileSearchPattern, bool expandAllNodes)
		{ return CreateFullFileListNode(sourceDirectoryPath, fileSearchPattern, false, null, expandAllNodes); }

		public static DarkTreeNode CreateFullFileListNode(string sourceDirectoryPath, string fileSearchPattern, DarkTreeView expandedSourceTreeView)
		{ return CreateFullFileListNode(sourceDirectoryPath, fileSearchPattern, false, expandedSourceTreeView, false); }

		public static DarkTreeNode CreateFullFileListNode(string sourceDirectoryPath, bool directoriesOnly)
		{ return CreateFullFileListNode(sourceDirectoryPath, string.Empty, directoriesOnly, null, false); }

		public static DarkTreeNode CreateFullFileListNode(string sourceDirectoryPath, bool directoriesOnly, bool expandAllNodes)
		{ return CreateFullFileListNode(sourceDirectoryPath, string.Empty, directoriesOnly, null, expandAllNodes); }

		public static DarkTreeNode CreateFullFileListNode(string sourceDirectoryPath, bool directoriesOnly, DarkTreeView expandedSourceTreeView)
		{ return CreateFullFileListNode(sourceDirectoryPath, string.Empty, directoriesOnly, expandedSourceTreeView, false); }

		public static void DeleteFiles(string[] files)
		{
			foreach (string file in files)
				if (File.Exists(file))
					File.Delete(file);
		}

		#endregion Public methods

		#region Private methods

		private static DarkTreeNode CreateFullFileListNode(
			string sourceDirectoryPath, string fileSearchPattern, bool directoriesOnly, DarkTreeView expandedSourceTreeView, bool expandAllNodes)
		{
			DirectoryInfo sourceDirectory = new DirectoryInfo(sourceDirectoryPath);

			DarkTreeNode node = new DarkTreeNode(sourceDirectory.Name)
			{
				Icon = Properties.Resources.folder.ToBitmap(),
				Tag = sourceDirectory
			};

			FillNodeWithItems(ref node, fileSearchPattern, directoriesOnly, expandedSourceTreeView, expandAllNodes);

			if (expandAllNodes)
				node.Expanded = true;

			return node;
		}

		private static void FillNodeWithItems(
			ref DarkTreeNode node, string fileSearchPattern, bool directoriesOnly, DarkTreeView expandedSourceTreeView, bool expandAllNodes)
		{
			Stack<DarkTreeNode> stack = new Stack<DarkTreeNode>();
			stack.Push(node);

			while (stack.Count > 0)
			{
				DarkTreeNode currentNode = stack.Pop();
				DirectoryInfo info = (DirectoryInfo)currentNode.Tag;

				currentNode.Nodes.AddRange(GetDirectoryNodes(ref stack, info.GetDirectories(), expandedSourceTreeView, expandAllNodes));

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

		private static List<DarkTreeNode> GetDirectoryNodes(
			ref Stack<DarkTreeNode> stack, DirectoryInfo[] directories, DarkTreeView expandedSourceTreeView, bool expandAllNodes)
		{
			List<DarkTreeNode> nodes = new List<DarkTreeNode>();

			foreach (DirectoryInfo directory in directories)
			{
				DarkTreeNode childDirectoryNode = new DarkTreeNode(directory.Name)
				{
					Icon = Properties.Resources.folder.ToBitmap(),
					Tag = directory
				};

				nodes.Add(childDirectoryNode);
				stack.Push(childDirectoryNode);

				if (expandedSourceTreeView != null)
				{
					DarkTreeNode expandedSourceChildNode = expandedSourceTreeView.FindNode(childDirectoryNode.FullPath);

					if (expandedSourceChildNode != null)
						childDirectoryNode.Expanded = expandedSourceChildNode.Expanded;
				}
				else if (expandAllNodes)
					childDirectoryNode.Expanded = true;
			}

			return nodes;
		}

		private static List<DarkTreeNode> GetFileNodes(FileInfo[] files)
		{
			List<DarkTreeNode> nodes = new List<DarkTreeNode>();

			foreach (FileInfo file in files)
			{
				DarkTreeNode fileNode = new DarkTreeNode(file.Name)
				{
					Icon = Properties.Resources.file.ToBitmap(),
					Tag = file
				};

				nodes.Add(fileNode);
			}

			return nodes;
		}

		#endregion Private methods
	}
}
