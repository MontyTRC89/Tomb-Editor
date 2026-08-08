using DarkUI.Controls;
using System;
using System.Collections.Generic;

namespace TombLib.Scripting.UI.ContentNodes;

/// <summary>
/// Builds <see cref="DarkTreeNode"/> trees from content node data.
/// </summary>
public static class ContentNodeTreeBuilder
{
	/// <summary>
	/// Builds a flat list of nodes from the given data.
	/// </summary>
	/// <typeparam name="TNode">The node data type.</typeparam>
	/// <param name="nodes">The node data items.</param>
	/// <param name="textSelector">Selects the display text of a node.</param>
	/// <param name="tagSelector">Selects the optional tag of a node.</param>
	/// <returns>The built tree nodes.</returns>
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

	/// <summary>
	/// Builds a grouped list of nodes, one root node per group.
	/// </summary>
	/// <typeparam name="TGroup">The group data type.</typeparam>
	/// <typeparam name="TNode">The node data type.</typeparam>
	/// <param name="groups">The group data items.</param>
	/// <param name="headerSelector">Selects the header text of a group.</param>
	/// <param name="nodesSelector">Selects the node data items of a group.</param>
	/// <param name="textSelector">Selects the display text of a node.</param>
	/// <param name="tagSelector">Selects the optional tag of a node.</param>
	/// <returns>The built tree nodes.</returns>
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
