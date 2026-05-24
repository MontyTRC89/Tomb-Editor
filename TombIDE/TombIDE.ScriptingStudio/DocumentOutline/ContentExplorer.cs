#nullable enable

using DarkUI.Controls;
using DarkUI.Docking;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using TombIDE.ScriptingStudio.UI;
using TombIDE.Shared;
using TombLib.Scripting;
using TombLib.Scripting.UI.ContentNodes;
using TombLib.Scripting.UI.Editors;

namespace TombIDE.ScriptingStudio.DocumentOutline
{
	public partial class ContentExplorer : DarkToolWindow
	{
		private readonly ContentNodesRefreshCoordinator _refreshCoordinator;
		private readonly ContentNodesProviderFactory _nodesProviderFactory;
		private IEditorControl? _editorControl;

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

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public IEditorControl? EditorControl
		{
			get => _editorControl;
			set
			{
				if (_editorControl is not null)
					_editorControl.ContentChangedWorkerRunCompleted -= EditorControl_ContentChangedWorkerRunCompleted;

				_editorControl = value;

				if (_editorControl is not null)
					_editorControl.ContentChangedWorkerRunCompleted += EditorControl_ContentChangedWorkerRunCompleted;

				UpdateTreeView();
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public ContentNodesProviderBase? NodesProvider { get; private set; }

		#endregion Properties

		#region Construction

		public ContentExplorer()
			: this(new ContentNodesProviderFactory())
		{
		}

		internal ContentExplorer(ContentNodesProviderFactory nodesProviderFactory)
		{
			ArgumentNullException.ThrowIfNull(nodesProviderFactory);

			_refreshCoordinator = new ContentNodesRefreshCoordinator();
			_nodesProviderFactory = nodesProviderFactory;
			InitializeComponent();

			DockText = Strings.Default.ContentExplorer;
			searchTextBox.SearchText = Strings.Default.SearchContent;
		}

		#endregion Construction

		#region Events

		public event ObjectClickedEventHandler? ObjectClicked;
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

		private void EditorControl_ContentChangedWorkerRunCompleted(object? sender, EventArgs e)
			=> UpdateTreeView();

		#endregion Events

		#region Methods

		public void SelectNode(string nodeText)
		{
			DarkTreeNode? node = treeView.Nodes.Find(x => x.Text == nodeText.Trim('[').Trim(']'));

			if (node is not null)
				treeView.SelectNode(node);

			treeView.Invalidate();
		}

		private void UpdateTreeView()
		{
			if (EditorControl is null || NodesProvider is null)
			{
				_refreshCoordinator.InvalidatePendingRequests();
				ClearTreeView();
				return;
			}

			string filter = string.IsNullOrWhiteSpace(searchTextBox.Text)
				? string.Empty
				: searchTextBox.Text.Trim();

			ContentNodesProviderBase nodesProvider = NodesProvider;
			string content = EditorControl.Content;

			_refreshCoordinator.RequestRefresh(nodesProvider, content, filter, CanApplyRefreshResult, ApplyNodes);
		}

		private void UpdateNodesProvider()
		{
			NodesProvider = _nodesProviderFactory.Create(_documentMode);
			_refreshCoordinator.InvalidatePendingRequests();

			UpdateTreeView();
		}

		private bool CanApplyRefreshResult(ContentNodesProviderBase nodesProvider)
			=> !IsDisposed && !Disposing && ReferenceEquals(nodesProvider, NodesProvider);

		private void ApplyNodes(IReadOnlyList<DarkTreeNode> nodes)
		{
			treeView.Nodes.Clear();

			if (nodes.Count > 0)
				treeView.Nodes.AddRange(nodes.ToArray());

			treeView.Invalidate();
		}

		private void ClearTreeView()
		{
			treeView.Nodes.Clear();
			treeView.Invalidate();
		}

		#endregion Methods
	}
}
