using DarkUI.Controls;
using DarkUI.Docking;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using TombIDE.ScriptingStudio.UI;
using TombIDE.Shared;
using TombLib.Scripting;
using TombLib.Scripting.Bases;
using TombLib.Scripting.ClassicScript.Workers;
using TombLib.Scripting.Interfaces;
using TombLib.Scripting.Objects;

namespace TombIDE.ScriptingStudio.ToolWindows
{
	public partial class ContentExplorer : DarkToolWindow
	{
		#region Properties

		private DocumentMode _documentMode;
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public DocumentMode DocumentMode
		{
			get => _documentMode;
			set
			{
				_documentMode = value;
				UpdateNodesProvider();
			}
		}

		private IEditorControl _editorControl;
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public IEditorControl EditorControl
		{
			get => _editorControl;
			set
			{
				if (_editorControl != null)
					EditorControl.ContentChangedWorkerRunCompleted -= EditorControl_ContentChangedWorkerRunCompleted;

				_editorControl = value;

				if (_editorControl != null)
					_editorControl.ContentChangedWorkerRunCompleted += EditorControl_ContentChangedWorkerRunCompleted;

				UpdateTreeView();
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public ContentNodesProviderBase NodesProvider { get; set; }

		#endregion Properties

		#region Construction

		public ContentExplorer()
		{
			InitializeComponent();

			DockText = Strings.Default.ContentExplorer;
			searchTextBox.SearchText = Strings.Default.SearchContent;
		}

		#endregion Construction

		#region Events

		public event ObjectClickedEventHandler ObjectClicked;
		protected virtual void OnObjectClicked(ObjectClickedEventArgs e)
			=> ObjectClicked?.Invoke(this, e);

		private void textBox_Search_TextChanged(object sender, EventArgs e)
			=> UpdateTreeView();

		private void treeView_Click(object sender, EventArgs e)
		{
			if (treeView.SelectedNodes.Count > 0)
			{
				DarkTreeNode selectedNode = treeView.SelectedNodes.First();
				OnObjectClicked(new ObjectClickedEventArgs(selectedNode.Text, selectedNode.Tag));
			}
		}

		private void EditorControl_ContentChangedWorkerRunCompleted(object sender, EventArgs e)
			=> UpdateTreeView();

		private void NodesProvider_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			treeView.Nodes.Clear();

			if (e.Result is IEnumerable<DarkTreeNode> nodes && nodes.Count() > 0)
				treeView.Nodes.AddRange(nodes);

			treeView.Invalidate();
		}

		#endregion Events

		#region Methods

		private void UpdateTreeView()
		{
			if (EditorControl != null && NodesProvider != null && !NodesProvider.IsBusy)
			{
				string filter = string.Empty;

				if (!string.IsNullOrWhiteSpace(searchTextBox.Text))
					filter = searchTextBox.Text.Trim();

				NodesProvider.Filter = filter;
				NodesProvider.RunWorkerAsync(EditorControl.Content);
			}
			else if (EditorControl == null || NodesProvider == null)
			{
				treeView.Nodes.Clear();
				treeView.Invalidate();
			}
		}

		private void UpdateNodesProvider()
		{
			if (NodesProvider != null)
				NodesProvider.RunWorkerCompleted -= NodesProvider_RunWorkerCompleted;

			switch (_documentMode)
			{
				case DocumentMode.ClassicScript: NodesProvider = new ClassicScriptNodesProvider(); break;
				case DocumentMode.Lua: NodesProvider = null; break; // TODO
				case DocumentMode.Strings: NodesProvider = new StringFileNodesProvider(); break;
				default: NodesProvider = null; break;
			}

			if (NodesProvider != null)
				NodesProvider.RunWorkerCompleted += NodesProvider_RunWorkerCompleted;
		}

		#endregion Methods
	}
}
