#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using DarkUI.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TombIDE.ScriptingStudio.UI;
using TombLib.Scripting.UI.ContentNodes;
using TombLib.Scripting.UI.Editors;
using TombLib.WPF.Services.Abstract;

namespace TombIDE.ScriptingStudio.DocumentOutline;

public sealed partial class DocumentOutlineViewModel : ObservableObject, IDisposable
{
	private readonly ILocalizationService _localizationService;
	private readonly DocumentOutlineNodesProviderFactory _nodesProviderFactory;
	private readonly ContentNodesRefreshCoordinator _refreshCoordinator;

	private DocumentMode _documentMode;
	private IEditorControl? _editorControl;

	internal DocumentOutlineViewModel(
		ILocalizationService localizationService,
		DocumentOutlineNodesProviderFactory? nodesProviderFactory = null)
	{
		ArgumentNullException.ThrowIfNull(localizationService);

		_nodesProviderFactory = nodesProviderFactory;
		_localizationService = localizationService.WithKeysFor(this);
		_refreshCoordinator = new ContentNodesRefreshCoordinator();
	}

	public ObservableCollection<DocumentOutlineNodeViewModel> Nodes { get; } = [];

	public ContentNodesProviderBase? NodesProvider { get; private set; }

	public string Title => _localizationService["Title"];

	public bool IsEmpty => Nodes.Count == 0;

	public DocumentMode DocumentMode
	{
		get => _documentMode;
		set
		{
			if (_documentMode == value)
				return;

			_documentMode = value;
			UpdateNodesProvider();
		}
	}

	public IEditorControl? EditorControl
	{
		get => _editorControl;
		set
		{
			if (ReferenceEquals(_editorControl, value))
				return;

			if (_editorControl is not null)
				_editorControl.ContentChangedWorkerRunCompleted -= EditorControl_ContentChangedWorkerRunCompleted;

			_editorControl = value;

			if (_editorControl is not null)
				_editorControl.ContentChangedWorkerRunCompleted += EditorControl_ContentChangedWorkerRunCompleted;

			RefreshNodes();
		}
	}

	[ObservableProperty]
	private string _searchText = string.Empty;

	[ObservableProperty]
	private DocumentOutlineNodeViewModel? _selectedNode;

	partial void OnSelectedNodeChanged(DocumentOutlineNodeViewModel? value)
	{
		// Clear IsSelected on all nodes except the newly selected one.
		foreach (DocumentOutlineNodeViewModel node in Nodes)
			ClearSelectionExcept(node, value);
	}

	private static void ClearSelectionExcept(DocumentOutlineNodeViewModel node, DocumentOutlineNodeViewModel? except)
	{
		if (ReferenceEquals(node, except))
			return;

		node.IsSelected = false;

		foreach (DocumentOutlineNodeViewModel child in node.Children)
			ClearSelectionExcept(child, except);
	}

	public void Dispose()
	{
		_refreshCoordinator.InvalidatePendingRequests();

		if (_editorControl is not null)
			_editorControl.ContentChangedWorkerRunCompleted -= EditorControl_ContentChangedWorkerRunCompleted;
	}

	public bool SelectNode(string nodeText)
	{
		foreach (DocumentOutlineNodeViewModel node in Nodes)
			node.ClearSelection();

		foreach (DocumentOutlineNodeViewModel node in Nodes)
		{
			if (!node.TrySelect(nodeText))
				continue;

			SelectedNode = FindSelectedNode(node);
			return true;
		}

		SelectedNode = null;
		return false;
	}

	partial void OnSearchTextChanged(string value)
		=> RefreshNodes();

	private void ApplyNodes(IReadOnlyList<DarkTreeNode> nodes)
	{
		string? selectedNodeText = SelectedNode?.Text;

		Nodes.Clear();

		foreach (DarkTreeNode node in nodes)
			Nodes.Add(DocumentOutlineNodeViewModel.FromDarkTreeNode(node));

		if (!string.IsNullOrWhiteSpace(selectedNodeText) && !SelectNode(selectedNodeText))
			SelectedNode = null;

		OnPropertyChanged(nameof(IsEmpty));
	}

	private bool CanApplyRefreshResult(ContentNodesProviderBase nodesProvider)
		=> ReferenceEquals(nodesProvider, NodesProvider);

	private void EditorControl_ContentChangedWorkerRunCompleted(object? sender, EventArgs e)
		=> RefreshNodes();

	private static DocumentOutlineNodeViewModel? FindSelectedNode(DocumentOutlineNodeViewModel node)
	{
		if (node.IsSelected)
			return node;

		foreach (DocumentOutlineNodeViewModel child in node.Children)
		{
			DocumentOutlineNodeViewModel? selectedNode = FindSelectedNode(child);

			if (selectedNode is not null)
				return selectedNode;
		}

		return null;
	}

	private void RefreshNodes()
	{
		if (EditorControl is null || NodesProvider is null)
		{
			_refreshCoordinator.InvalidatePendingRequests();
			ApplyNodes([]);
			return;
		}

		string filter = string.IsNullOrWhiteSpace(SearchText)
			? string.Empty
			: SearchText.Trim();

		ContentNodesProviderBase nodesProvider = NodesProvider;
		string content = EditorControl.Content;

		_refreshCoordinator.RequestRefresh(nodesProvider, content, filter, CanApplyRefreshResult, ApplyNodes);
	}

	private void UpdateNodesProvider()
	{
		NodesProvider = _nodesProviderFactory.Create(_documentMode);
		_refreshCoordinator.InvalidatePendingRequests();

		RefreshNodes();
	}
}
