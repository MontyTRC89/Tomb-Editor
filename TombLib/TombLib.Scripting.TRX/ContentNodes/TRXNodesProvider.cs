#nullable enable

using DarkUI.Controls;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TombLib.Scripting.Text;
using TombLib.Scripting.TRX.Resources;
using TombLib.Scripting.TRX.Services;
using TombLib.Scripting.UI.ContentNodes;

namespace TombLib.Scripting.TRX.ContentNodes;

public sealed class TRXNodesProvider : ContentNodesProviderBase
{
	private readonly ITRXLineService _lineService;

	public TRXNodesProvider(ITRXLineService lineService)
	{
		ArgumentNullException.ThrowIfNull(lineService);
		_lineService = lineService;
	}

	protected override IReadOnlyList<DarkTreeNode> GetNodesCore(string content, string filter)
	{
		var nodes = new List<string>();
		var source = new StringTextSnapshot(content);

		foreach (ITextLine line in source.Lines)
		{
			string lineText = source.GetText(line.Offset, line.Length);
			string? levelNode = GetLevelNode(lineText, filter);

			if (levelNode is not null)
				nodes.Add(levelNode);
		}

		return ContentNodeTreeBuilder.BuildFlatNodes(nodes, node => node);
	}

	private string? GetLevelNode(string lineText, string filter)
	{
		var regex = new Regex(Patterns.LevelProperty, RegexOptions.IgnoreCase);

		if (regex.IsMatch(lineText))
		{
			lineText = _lineService.RemoveComments(lineText);
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
