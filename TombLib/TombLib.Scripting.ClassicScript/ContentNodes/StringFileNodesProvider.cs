using DarkUI.Controls;
using TombLib.Scripting.ClassicScript.Services;
using TombLib.Scripting.Text;
using TombLib.Scripting.UI.ContentNodes;

namespace TombLib.Scripting.ClassicScript.ContentNodes;

public sealed class StringFileNodesProvider : ContentNodesProviderBase
{
	private readonly IClassicScriptLineService _lineService;

	public StringFileNodesProvider(IClassicScriptLineService lineService)
	{
		_lineService = lineService ?? throw new ArgumentNullException(nameof(lineService));
	}

	protected override IReadOnlyList<DarkTreeNode> GetNodesCore(string content, string filter)
	{
		var nodes = new List<DarkTreeNode>();
		var source = new StringTextSnapshot(content);

		foreach (ITextLine line in source.Lines)
		{
			string lineText = source.GetText(line.Offset, line.Length);

			if (_lineService.IsSectionHeaderLine(lineText))
			{
				string headerText = _lineService.GetSectionHeaderText(lineText);

				if (headerText.Contains(filter, StringComparison.OrdinalIgnoreCase))
					nodes.Add(new DarkTreeNode(headerText));
			}
		}

		return nodes;
	}
}
