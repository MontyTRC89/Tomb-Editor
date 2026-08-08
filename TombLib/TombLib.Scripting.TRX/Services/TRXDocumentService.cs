using System;
using System.Text.RegularExpressions;
using TombLib.Scripting.Text;
using TombLib.Scripting.TRX.Resources;

namespace TombLib.Scripting.TRX.Services;

/// <summary>
/// Default implementation of <see cref="ITRXDocumentService"/>.
/// Provides document-level operations using <see cref="ITextSnapshot"/> and
/// the line service.
/// </summary>
public sealed class TRXDocumentService : ITRXDocumentService
{
	private readonly ITRXLineService _lineService;

	// Legacy regex pattern retained for exact parity.
	private static readonly Regex LevelCommentNameRegex = new(Patterns.LevelCommentName, RegexOptions.IgnoreCase);

	/// <summary>
	/// Initializes a new instance of the <see cref="TRXDocumentService"/> class.
	/// </summary>
	public TRXDocumentService(ITRXLineService lineService)
	{
		ArgumentNullException.ThrowIfNull(lineService);
		_lineService = lineService;
	}

	/// <inheritdoc />
	public bool IsLevelScriptDefined(ITextSnapshot source, string levelName)
	{
		ArgumentNullException.ThrowIfNull(source);
		ArgumentNullException.ThrowIfNull(levelName);

		foreach (ITextLine line in source.Lines)
		{
			string lineText = source.GetText(line.Offset, line.Length);

			if (TRXLevelNameParser.LevelPropertyRegex.IsMatch(lineText))
			{
				string scriptLevelName = TRXLevelNameParser.ExtractTitleName(_lineService.RemoveComments(lineText));

				if (scriptLevelName == levelName)
					return true;
			}
		}

		return false;
	}

	/// <inheritdoc />
	public int? FindDocumentLineOfLevel(ITextSnapshot source, string levelName)
	{
		ArgumentNullException.ThrowIfNull(source);
		ArgumentNullException.ThrowIfNull(levelName);

		foreach (ITextLine line in source.Lines)
		{
			string lineText = source.GetText(line.Offset, line.Length);

			// First check: title property match with StartsWith (legacy quirk).
			string normalizedTitle = TRXLevelNameParser.ExtractTitleName(lineText);

			if (normalizedTitle.StartsWith(levelName, StringComparison.Ordinal))
				return line.LineNumber;

			// Second check: comment name match (e.g. "// Level 1: Caves").
			Match commentMatch = LevelCommentNameRegex.Match(lineText);

			if (commentMatch.Success)
			{
				string matchedLevelName = commentMatch.Groups[3].Value.Trim();

				if (matchedLevelName.Equals(levelName, StringComparison.Ordinal))
					return line.LineNumber;
			}
		}

		return null;
	}
}
