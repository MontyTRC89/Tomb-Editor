using DarkUI.Controls;
using System;
using System.Collections.Generic;

namespace TombLib.Scripting.UI.ContentNodes
{
	public static class ContentNodeTreeBuilder
	{
		public static IReadOnlyList<DarkTreeNode> BuildFlatNodes<TNode>(
			IReadOnlyList<TNode> nodes,
			Func<TNode, string> textSelector,
			Func<TNode, object?>? tagSelector = null)
		{
			ArgumentNullException.ThrowIfNull(nodes);
			ArgumentNullException.ThrowIfNull(textSelector);

			var result = new List<DarkTreeNode>(nodes.Count);

			foreach (TNode node in nodes)
				result.Add(CreateNode(textSelector(node), tagSelector?.Invoke(node)));

			return result;
		}

		public static IReadOnlyList<DarkTreeNode> BuildGroupedNodes<TGroup, TNode>(
			IReadOnlyList<TGroup> groups,
			Func<TGroup, string> headerSelector,
			Func<TGroup, IReadOnlyList<TNode>> nodesSelector,
			Func<TNode, string> textSelector,
			Func<TNode, object?>? tagSelector = null)
		{
			ArgumentNullException.ThrowIfNull(groups);
			ArgumentNullException.ThrowIfNull(headerSelector);
			ArgumentNullException.ThrowIfNull(nodesSelector);
			ArgumentNullException.ThrowIfNull(textSelector);

			var result = new List<DarkTreeNode>(groups.Count);

			foreach (TGroup group in groups)
			{
				var rootNode = new DarkTreeNode(headerSelector(group));

				foreach (TNode node in nodesSelector(group))
					rootNode.Nodes.Add(CreateNode(textSelector(node), tagSelector?.Invoke(node)));

				rootNode.Expanded = true;
				result.Add(rootNode);
			}

			return result;
		}

		private static DarkTreeNode CreateNode(string text, object? tag)
			=> new(text) { Tag = tag };
	}
}
