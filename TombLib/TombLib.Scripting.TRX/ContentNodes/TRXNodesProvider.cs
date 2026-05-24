#nullable enable

using DarkUI.Controls;
using ICSharpCode.AvalonEdit.Document;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TombLib.Scripting.TRX.Parsers;
using TombLib.Scripting.TRX.Resources;
using TombLib.Scripting.UI.ContentNodes;

namespace TombLib.Scripting.TRX.ContentNodes;

public sealed class TRXNodesProvider : ContentNodesProviderBase
{
	protected override IReadOnlyList<DarkTreeNode> GetNodesCore(string content, string filter)
	{
		var nodes = new List<string>();
		var document = new TextDocument(content);

		foreach (DocumentLine line in document.Lines)
		{
			string lineText = document.GetText(line.Offset, line.Length);
			string? levelNode = GetLevelNode(lineText, filter);

			if (levelNode is not null)
				nodes.Add(levelNode);
		}

		return ContentNodeTreeBuilder.BuildFlatNodes(nodes, node => node);
	}

	private static string? GetLevelNode(string lineText, string filter)
	{
		var regex = new Regex(Patterns.LevelProperty, RegexOptions.IgnoreCase);

		if (regex.IsMatch(lineText))
		{
			lineText = LineParser.RemoveComments(lineText);
			string levelName = regex.Replace(lineText, string.Empty).Trim().TrimEnd(',').Trim('"');

			if (!string.IsNullOrWhiteSpace(levelName) && levelName.Contains(filter, StringComparison.OrdinalIgnoreCase))
				return levelName;
		}

		regex = new Regex(Patterns.LevelCommentName, RegexOptions.IgnoreCase);
		Match regexMatch = regex.Match(lineText);

		if (regexMatch.Success)
		{
			string levelName = regexMatch.Groups[3].Value.Trim();

			if (!string.IsNullOrWhiteSpace(levelName) && levelName.Contains(filter, StringComparison.OrdinalIgnoreCase))
				return levelName;
		}

		return null;
	}
}