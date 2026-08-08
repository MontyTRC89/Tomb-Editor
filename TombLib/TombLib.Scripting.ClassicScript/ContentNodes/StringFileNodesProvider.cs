using DarkUI.Controls;
using TombLib.Scripting.ClassicScript.Services;
using TombLib.Scripting.Text;
using TombLib.Scripting.UI.ContentNodes;

namespace TombLib.Scripting.ClassicScript.ContentNodes;

/// <summary>
/// Provides the content nodes for a ClassicScript strings file.
/// </summary>
public sealed class StringFileNodesProvider : ContentNodesProviderBase
{
	private readonly IClassicScriptLineService _lineService;

	/// <summary>
	/// Initializes a new instance of the <see cref="StringFileNodesProvider"/> class.
	/// </summary>
	/// <param name="lineService">The line service used to identify section headers.</param>
	public StringFileNodesProvider(IClassicScriptLineService lineService)
	{
		ArgumentNullException.ThrowIfNull(lineService);
		_lineService = lineService;
	}

	/// <inheritdoc/>
	protected override IReadOnlyList<DarkTreeNode> GetNodesCore(string content, string filter)
	{
		var nodes = new List<DarkTreeNode>();
		var source = new StringTextSnapshot(content);

		foreach (ITextLine line in source.Lines)
		{
			string lineText = source.GetText(line.Offset, line.Length);

			if (_lineService.IsSectionHeaderLine(lineText))
			{
				string? headerText = _lineService.GetSectionHeaderText(lineText);

				if (headerText is not null && headerText.Contains(filter, StringComparison.OrdinalIgnoreCase))
					nodes.Add(new DarkTreeNode(headerText));
			}
		}

		return nodes;
	}
}
