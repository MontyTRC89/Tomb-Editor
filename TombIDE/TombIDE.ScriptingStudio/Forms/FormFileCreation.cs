using DarkUI.Forms;
using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using TombIDE.ScriptingStudio.Helpers;
using TombIDE.Shared;
using TombIDE.Shared.SharedClasses;

namespace TombIDE.ScriptingStudio.Forms
{
	internal partial class FormFileCreation : DarkForm
	{
		public string NewFilePath { get; private set; }

		private string _scriptRootFolderPath;
		private string _defaultFileExtension;
		private string[] _ignoredNodePaths;

		#region Construction

		public FormFileCreation(string scriptRootFolderPath, FileCreationMode mode, string defaultFileExtension,
			string initialNodePath = null, string initialFileName = null, params string[] ignoredNodePaths)
		{
			InitializeComponent();
			SwitchMode(mode);

			_scriptRootFolderPath = scriptRootFolderPath;
			_defaultFileExtension = defaultFileExtension;
			_ignoredNodePaths = ignoredNodePaths;

			FillFolderList();

			SetInitialNodePath(initialNodePath);
			SetInitialFileName(initialFileName);
		}

		#endregion Construction

		#region Events

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);

			comboBox_FileFormat.SelectedIndex = _defaultFileExtension.ToLower() switch
			{
				".json5" => 1,
				".lua" => 2,
				_ => 0
			};

			textBox_NewFileName.SelectAll();
		}

		private void button_Create_Click(object sender, EventArgs e)
		{
			try
			{
				string newFileName = PathHelper.RemoveIllegalPathSymbols(textBox_NewFileName.Text).Trim();

				if (string.IsNullOrWhiteSpace(newFileName))
					throw new ArgumentException("Invalid file name.");

				newFileName += GetSelectedExtension();

				string targetDirectory = ((DirectoryInfo)treeView.SelectedNodes[0].Tag).FullName;

				foreach (string file in Directory.GetFiles(targetDirectory, "*.*", SearchOption.TopDirectoryOnly))
					if (newFileName.Equals(Path.GetFileName(file), StringComparison.OrdinalIgnoreCase))
						throw new ArgumentException("A file with the same name already exists in that directory.");

				// // // //
				NewFilePath = Path.Combine(targetDirectory, newFileName);
				// // // //

				File.Create(NewFilePath).Close();
			}
			catch (Exception ex)
			{
				DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				DialogResult = DialogResult.None;
			}
		}

		#endregion Events

		#region Methods

		private void SwitchMode(FileCreationMode mode)
		{
			switch (mode)
			{
				case FileCreationMode.New:
					Text = "Creating New File...";
					label_Where.Text = "Where to Create:";
					button_Create.Text = "Create";
					break;

				case FileCreationMode.SavingAs:
					Text = "Saving As...";
					label_Where.Text = "Where to Save:";
					button_Create.Text = "Save";
					break;
			}
		}

		private void FillFolderList()
		{
			treeView.Nodes.Clear();
			treeView.Nodes.Add(FileTreeViewHelper.CreateFullFileListNode(_scriptRootFolderPath, true, true));

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

		private void SetInitialFileName(string initialFileName)
		{
			if (string.IsNullOrEmpty(initialFileName))
				textBox_NewFileName.Text = "untitled";
			else
				textBox_NewFileName.Text = initialFileName;
		}

		private string GetSelectedExtension()
		{
			switch (comboBox_FileFormat.SelectedIndex)
			{
				case 0: return ".txt";
				case 1: return ".json5";
				case 2: return ".lua";
			}

			return null;
		}

		#endregion Methods
	}
}
