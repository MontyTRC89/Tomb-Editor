using DarkUI.Controls;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TombLib.Scripting.Text;
using TombLib.Scripting.TRX.Resources;
using TombLib.Scripting.TRX.Services;
using TombLib.Scripting.UI.ContentNodes;

namespace TombLib.Scripting.TRX.ContentNodes;

/// <summary>
/// Builds content nodes for level names found in TRX documents.
/// </summary>
public sealed class TRXNodesProvider : ContentNodesProviderBase
{
	private static readonly Regex s_levelCommentRegex = new(Patterns.LevelCommentName, RegexOptions.IgnoreCase);

	private readonly ITRXLineService _lineService;

	/// <summary>
	/// Initializes a new instance of the <see cref="TRXNodesProvider"/> class.
	/// </summary>
	/// <param name="lineService">The line service used to strip comments from lines.</param>
	public TRXNodesProvider(ITRXLineService lineService)
	{
		ArgumentNullException.ThrowIfNull(lineService);
		_lineService = lineService;
	}

	/// <summary>
	/// Builds the level-name nodes for the given content and filter.
	/// </summary>
	/// <param name="content">The document content to scan.</param>
	/// <param name="filter">The filter used to match level names.</param>
	/// <returns>The content nodes that match the filter.</returns>
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
		if (TRXLevelNameParser.LevelPropertyRegex.IsMatch(lineText))
		{
			string strippedLineText = _lineService.RemoveComments(lineText);
			string levelName = TRXLevelNameParser.ExtractTitleName(strippedLineText);

			if (!string.IsNullOrWhiteSpace(levelName) && levelName.Contains(filter, StringComparison.OrdinalIgnoreCase))
				return levelName;
		}

		// The fallback runs against the raw line text so that a level-name comment is still
		// discoverable when the line also matches the title property (malformed mixed input).
		Match regexMatch = s_levelCommentRegex.Match(lineText);

		if (regexMatch.Success)
		{
			string levelName = regexMatch.Groups[3].Value.Trim();

			if (!string.IsNullOrWhiteSpace(levelName) && levelName.Contains(filter, StringComparison.OrdinalIgnoreCase))
				return levelName;
		}

		return null;
	}
}
