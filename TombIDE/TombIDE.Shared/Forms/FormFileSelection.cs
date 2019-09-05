using DarkUI.Controls;
using DarkUI.Forms;
using System;
using System.IO;
using System.Windows.Forms;

namespace TombIDE.Shared
{
	public partial class FormFileSelection : DarkForm
	{
		public string SelectedFile { get; internal set; }

		public FormFileSelection(string[] fileList)
		{
			InitializeComponent();

			foreach (string file in fileList)
			{
				DarkTreeNode node = new DarkTreeNode
				{
					Text = Path.GetFileName(file),
					Tag = file
				};

				treeView.Nodes.Add(node);
			}
		}

		private void treeView_SelectedNodesChanged(object sender, EventArgs e) =>
			button_Confirm.Enabled = treeView.SelectedNodes.Count != 0;

		private void treeView_DoubleClick(object sender, EventArgs e)
		{
			if (treeView.SelectedNodes.Count == 0)
				return;

			SelectedFile = treeView.SelectedNodes[0].Tag.ToString();
			DialogResult = DialogResult.OK;
		}

		private void button_Confirm_Click(object sender, EventArgs e) =>
			SelectedFile = treeView.SelectedNodes[0].Tag.ToString();
	}
}
