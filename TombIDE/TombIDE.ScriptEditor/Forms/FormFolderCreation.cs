using DarkUI.Controls;
using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using TombIDE.Shared;
using TombIDE.Shared.SharedClasses;

namespace TombIDE
{
	public partial class FormFolderCreation : DarkForm
	{
		public string NewFolderPath { get; private set; }

		private IDE _ide;

		public FormFolderCreation(IDE ide, string initialNodePath)
		{
			InitializeComponent();

			_ide = ide;

			UpdateFolderList();

			if (string.IsNullOrEmpty(initialNodePath))
				treeView.SelectNode(treeView.Nodes[0]);
			else
				treeView.SelectNode(treeView.FindNode(initialNodePath));
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);

			textBox_NewFolderName.Text = "New Folder";
			textBox_NewFolderName.SelectAll();
		}

		private void button_Create_Click(object sender, EventArgs e)
		{
			try
			{
				string newFolderName = PathHelper.RemoveIllegalPathSymbols(textBox_NewFolderName.Text.Trim());

				if (string.IsNullOrWhiteSpace(newFolderName))
					throw new ArgumentException("Invalid folder name.");

				string targetDirectory = ((DirectoryInfo)treeView.SelectedNodes[0].Tag).FullName;
				string[] directories = Directory.GetDirectories(targetDirectory, "*.*", SearchOption.TopDirectoryOnly);

				foreach (string directory in directories)
				{
					if (Path.GetFileName(directory).ToLower() == newFolderName.ToLower())
						throw new ArgumentException("A folder with the same name already exists in that directory.");
				}

				// // // //
				NewFolderPath = Path.Combine(targetDirectory, newFolderName);
				// // // //
			}
			catch (Exception ex)
			{
				DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				DialogResult = DialogResult.None;
			}
		}

		private void UpdateFolderList()
		{
			treeView.Nodes.Clear();

			Stack<DarkTreeNode> stack = new Stack<DarkTreeNode>();
			DirectoryInfo scriptDirectory = new DirectoryInfo(_ide.Project.ScriptPath);

			DarkTreeNode node = new DarkTreeNode(Path.GetFileName(_ide.Project.ScriptPath))
			{
				Icon = ScriptEditor.Properties.Resources.folder.ToBitmap(),
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
						Icon = ScriptEditor.Properties.Resources.folder.ToBitmap(),
						Tag = directory
					};

					currentNode.Nodes.Add(childDirectoryNode);
					stack.Push(childDirectoryNode);

					childDirectoryNode.Expanded = true;
					currentNode.Expanded = true;
				}
			}

			node.Expanded = true;
			treeView.Nodes.Add(node);
		}
	}
}
