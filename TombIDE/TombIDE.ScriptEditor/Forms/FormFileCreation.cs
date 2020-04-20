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
	public enum FileCreationMode
	{
		New,
		Saving
	}

	public partial class FormFileCreation : DarkForm
	{
		public string NewFilePath { get; private set; }

		private IDE _ide;

		public FormFileCreation(IDE ide, string initialNodePath, FileCreationMode mode)
		{
			InitializeComponent();

			_ide = ide;

			UpdateFolderList();

			if (string.IsNullOrEmpty(initialNodePath))
				treeView.SelectNode(treeView.Nodes[0]);
			else
				treeView.SelectNode(treeView.FindNode(initialNodePath));

			switch (mode)
			{
				case FileCreationMode.New:
					Text = "Creating New File...";
					label_Where.Text = "Where to Create:";
					button_Create.Text = "Create";
					break;

				case FileCreationMode.Saving:
					Text = "Saving As...";
					label_Where.Text = "Where to Save:";
					button_Create.Text = "Save";
					break;
			}
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);

			comboBox_FileFormat.SelectedIndex = 0;

			textBox_NewFileName.Text = "untitled";
			textBox_NewFileName.SelectAll();
		}

		private void button_Create_Click(object sender, EventArgs e)
		{
			try
			{
				string newFileName = PathHelper.RemoveIllegalPathSymbols(textBox_NewFileName.Text.Trim());

				if (string.IsNullOrWhiteSpace(newFileName))
					throw new ArgumentException("Invalid file name.");

				switch (comboBox_FileFormat.SelectedIndex)
				{
					case 0:
						newFileName += ".txt";
						break;

					case 1:
						newFileName += ".lua";
						break;
				}

				string targetDirectory = ((DirectoryInfo)treeView.SelectedNodes[0].Tag).FullName;
				string[] files = Directory.GetFiles(targetDirectory, "*.*", SearchOption.TopDirectoryOnly);

				foreach (string file in files)
				{
					if (Path.GetFileName(file).ToLower() == newFileName.ToLower())
						throw new ArgumentException("A file with the same name already exists in that directory.");
				}

				// // // //
				NewFilePath = Path.Combine(targetDirectory, newFileName);
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
