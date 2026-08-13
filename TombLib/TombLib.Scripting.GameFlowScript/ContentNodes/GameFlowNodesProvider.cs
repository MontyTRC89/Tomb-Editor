using DarkUI.Controls;
using System;
using System.Collections.Generic;
using TombLib.Scripting.GameFlowScript.Services;
using TombLib.Scripting.GameFlowScript.Types;
using TombLib.Scripting.UI.ContentNodes;

namespace TombLib.Scripting.GameFlowScript.ContentNodes;

/// <summary>
/// Provides the content nodes for a GameFlow script document.
/// </summary>
public sealed class GameFlowNodesProvider : ContentNodesProviderBase
{
	private readonly GameFlowContentNodeService _nodeService;

	/// <summary>
	/// Initializes a new instance of the <see cref="GameFlowNodesProvider"/> class.
	/// </summary>
	/// <param name="lineService">The line service used to analyze document lines.</param>
	public GameFlowNodesProvider(IGameFlowScriptLineService lineService)
	{
		ArgumentNullException.ThrowIfNull(lineService);
		_nodeService = new(lineService);
	}

	/// <inheritdoc/>
	protected override IReadOnlyList<DarkTreeNode> GetNodesCore(string content, string filter)
	{
		return ContentNodeTreeBuilder.BuildGroupedNodes(
			_nodeService.GetNodeGroups(content, filter),
			group => group.Header,
			group => group.Nodes,
			node => node.Text,
			node => new GameFlowObjectDiscriminator(node.ObjectType));
	}
}
