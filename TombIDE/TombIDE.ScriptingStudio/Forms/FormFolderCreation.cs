using DarkUI.Forms;
using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using TombIDE.ScriptingStudio.Helpers;
using TombIDE.Shared.SharedClasses;

namespace TombIDE.ScriptingStudio.Forms
{
	internal partial class FormFolderCreation : DarkForm
	{
		public string NewFolderPath { get; private set; }

		private string _scriptRootDirectoryPath;
		private string[] _ignoredNodePaths;

		#region Construction

		public FormFolderCreation(string scriptRootDirectoryPath, string initialNodePath, params string[] ignoredNodePaths)
		{
			InitializeComponent();

			_scriptRootDirectoryPath = scriptRootDirectoryPath;
			_ignoredNodePaths = ignoredNodePaths;

			FillFolderList();

			SetInitialNodePath(initialNodePath);
		}

		#endregion Construction

		#region Events

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
				string newFolderName = PathHelper.RemoveIllegalPathSymbols(textBox_NewFolderName.Text).Trim();

				if (string.IsNullOrWhiteSpace(newFolderName))
					throw new ArgumentException("Invalid folder name.");

				string targetDirectory = ((DirectoryInfo)treeView.SelectedNodes[0].Tag).FullName;

				foreach (string directory in Directory.GetDirectories(targetDirectory, "*.*", SearchOption.TopDirectoryOnly))
					if (newFolderName.Equals(Path.GetFileName(directory), StringComparison.OrdinalIgnoreCase))
						throw new ArgumentException("A folder with the same name already exists in that directory.");

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

		#endregion Events

		#region Methods

		private void FillFolderList()
		{
			treeView.Nodes.Clear();
			treeView.Nodes.Add(FileTreeViewHelper.CreateFullFileListNode(_scriptRootDirectoryPath, true, true));

			// Remove ignored nodes
			foreach (string ignoredNodePath in _ignoredNodePaths.Where(path => !string.IsNullOrWhiteSpace(path)))
				treeView.FindNode(ignoredNodePath)?.Remove();
		}

		private void SetInitialNodePath(string initialNodePath)
		{
			if (string.IsNullOrEmpty(initialNodePath))
				treeView.SelectNode(treeView.Nodes[0]);
			else
				treeView.SelectNode(treeView.FindNode(initialNodePath));
		}

		#endregion Methods
	}
}
