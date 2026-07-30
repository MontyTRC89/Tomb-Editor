using DarkUI.Controls;
using TombLib.Scripting.GameFlowScript.Services;
using TombLib.Scripting.UI.ContentNodes;

namespace TombLib.Scripting.GameFlowScript.ContentNodes;

public sealed class GameFlowNodesProvider : ContentNodesProviderBase
{
    private readonly GameFlowContentNodeService _nodeService;

    public GameFlowNodesProvider(IGameFlowScriptLineService lineService)
    {
        ArgumentNullException.ThrowIfNull(lineService);
        _nodeService = new GameFlowContentNodeService(lineService);
    }

    protected override IReadOnlyList<DarkTreeNode> GetNodesCore(string content, string filter)
        => ContentNodeTreeBuilder.BuildGroupedNodes(
            _nodeService.GetNodeGroups(content, filter),
            group => group.Header,
            group => group.Nodes,
            node => node.Text,
            node => node.ObjectType);
}
