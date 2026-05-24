using DarkUI.Controls;
using TombLib.Scripting.UI.ContentNodes;

namespace TombLib.Scripting.ClassicScript.ContentNodes;

public sealed class ClassicScriptNodesProvider : ContentNodesProviderBase
{
	private readonly ClassicScriptContentNodeService _nodeService = new();

	protected override IReadOnlyList<DarkTreeNode> GetNodesCore(string content, string filter)
		=> ContentNodeTreeBuilder.BuildGroupedNodes(
			_nodeService.GetNodeGroups(content, filter),
			group => group.Header,
			group => group.Nodes,
			node => node.Text,
			node => node.ObjectType);
}