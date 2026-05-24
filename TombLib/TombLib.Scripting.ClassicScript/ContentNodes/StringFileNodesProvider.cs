using DarkUI.Controls;
using ICSharpCode.AvalonEdit.Document;
using TombLib.Scripting.ClassicScript.Parsers;
using TombLib.Scripting.UI.ContentNodes;

namespace TombLib.Scripting.ClassicScript.ContentNodes;

public sealed class StringFileNodesProvider : ContentNodesProviderBase
{
	protected override IReadOnlyList<DarkTreeNode> GetNodesCore(string content, string filter)
	{
		var nodes = new List<DarkTreeNode>();
		var document = new TextDocument(content);

		foreach (DocumentLine line in document.Lines)
		{
			string lineText = document.GetText(line.Offset, line.Length);

			if (LineParser.IsSectionHeaderLine(lineText))
			{
				string headerText = LineParser.GetSectionHeaderText(lineText);

				if (headerText.Contains(filter, StringComparison.OrdinalIgnoreCase))
					nodes.Add(new DarkTreeNode(headerText));
			}
		}

		return nodes;
	}
}