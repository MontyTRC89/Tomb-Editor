using DarkUI.Controls;
using System.Collections.Generic;
using TombLib.Scripting.ClassicScript.Services;
using TombLib.Scripting.ClassicScript.Types;
using TombLib.Scripting.UI.ContentNodes;

namespace TombLib.Scripting.ClassicScript.ContentNodes;

/// <summary>
/// Provides the content nodes for a ClassicScript document.
/// </summary>
public sealed class ClassicScriptNodesProvider : ContentNodesProviderBase
{
	private readonly ClassicScriptContentNodeService _nodeService;

	/// <summary>
	/// Initializes a new instance of the <see cref="ClassicScriptNodesProvider"/> class.
	/// </summary>
	/// <param name="lineService">The line service used to analyze document lines.</param>
	public ClassicScriptNodesProvider(IClassicScriptLineService lineService)
	{
		_nodeService = new ClassicScriptContentNodeService(lineService);
	}

	/// <inheritdoc/>
	protected override IReadOnlyList<DarkTreeNode> GetNodesCore(string content, string filter)
	{
		return ContentNodeTreeBuilder.BuildGroupedNodes(
			_nodeService.GetNodeGroups(content, filter),
			group => group.Header,
			group => group.Nodes,
			node => node.Text,
			node => new ClassicScriptObjectDiscriminator(node.ObjectType));
	}
}
