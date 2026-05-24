#nullable enable

using DarkUI.Controls;
using DarkUI.Docking;
using System;
using System.Windows.Forms;
using TombIDE.Shared;

namespace TombIDE.ScriptingStudio.FindAndReplace
{
	public partial class SearchResults : DarkToolWindow
	{
		private readonly Action<string, FindReplaceItem>? _navigateToSearchResult;

		public SearchResults(Action<string, FindReplaceItem>? navigateToSearchResult)
		{
			InitializeComponent();
			DockText = Strings.Default.SearchResults;

			_navigateToSearchResult = navigateToSearchResult;
		}

		public void UpdateResults(FindReplaceEventArgs e)
		{
			treeView.Nodes.Clear();

			foreach (FindReplaceSource source in e.SourceCollection)
			{
				var sourceNode =
					new DarkTreeNode(string.Format(Strings.Default.MatchSourceNodeText, source.Name, source.Count))
					{
						Tag = source.Name
					};

				foreach (FindReplaceItem item in source)
					sourceNode.Nodes.Add(
						new DarkTreeNode(string.Format(Strings.Default.SingleMatchNodeText, item.LineNumber, item.LineText))
						{
							Tag = item
						});

				sourceNode.Expanded = true;
				treeView.Nodes.Add(sourceNode);
			}
		}

		private void treeView_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			if (IsRootNode())
				return;

			if (treeView.SelectedNodes.Count == 0)
				return;

			var item = treeView.SelectedNodes[0].Tag as FindReplaceItem;
			if (item is null)
				return;

			string? sourceFilePath = treeView.SelectedNodes[0].ParentNode?.Tag?.ToString();
			if (string.IsNullOrWhiteSpace(sourceFilePath))
				return;

			_navigateToSearchResult?.Invoke(sourceFilePath, item);
		}

		private bool IsRootNode()
		{
			foreach (DarkTreeNode node in treeView.Nodes)
				if (treeView.SelectedNodes[0] == node)
					return true;

			return false;
		}
	}
}
