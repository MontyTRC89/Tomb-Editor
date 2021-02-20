using DarkUI.Controls;
using ICSharpCode.AvalonEdit.Document;
using System;
using System.Collections.Generic;
using TombLib.Scripting.Bases;
using TombLib.Scripting.ClassicScript.Parsers;
using TombLib.Scripting.Extensions;

namespace TombLib.Scripting.ClassicScript.Workers
{
	public class StringFileNodesProvider : ContentNodesProviderBase
	{
		protected override IEnumerable<DarkTreeNode> GetNodes(string content)
		{
			var document = new TextDocument(content);

			// Add all subnodes
			foreach (DocumentLine line in document.Lines)
			{
				string lineText = document.GetText(line.Offset, line.Length);

				if (LineParser.IsSectionHeaderLine(lineText))
				{
					string headerText = LineParser.GetSectionHeaderText(lineText);

					if (headerText.Contains(Filter, StringComparison.OrdinalIgnoreCase))
						yield return new DarkTreeNode(headerText);
				}
			}
		}
	}
}
