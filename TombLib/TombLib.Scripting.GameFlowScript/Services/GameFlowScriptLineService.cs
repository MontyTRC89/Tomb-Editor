using System.Text.RegularExpressions;
using TombLib.Scripting.GameFlowScript.Resources;
using TombLib.Scripting.Text;

namespace TombLib.Scripting.GameFlowScript.Services;

/// <summary>
/// Default implementation of <see cref="IGameFlowScriptLineService"/>.
/// Provides line-level text operations using Core helpers and, where needed,
/// regex patterns that match legacy behavior.
/// </summary>
public class GameFlowScriptLineService : IGameFlowScriptLineService
{
	private const string CommentDelimiter = "//";

	// Legacy regex patterns retained for parity during Phase 9A extraction.
	private static readonly Regex SectionHeaderRegex = new(Patterns.Sections, RegexOptions.IgnoreCase | RegexOptions.Compiled);

	/// <inheritdoc />
	public string RemoveComments(string lineText)
		=> LineCommentHelper.RemoveLineComment(lineText, CommentDelimiter);

	/// <inheritdoc />
	public string EscapeComments(string lineText)
		=> LineCommentHelper.MaskLineComment(lineText, CommentDelimiter);

	/// <inheritdoc />
	public bool IsEmptyOrComments(string? lineText)
		=> string.IsNullOrWhiteSpace(lineText) || lineText!.TrimStart().StartsWith(CommentDelimiter, StringComparison.Ordinal);

	/// <inheritdoc />
	public bool IsSectionHeaderLine(string lineText)
		=> SectionHeaderRegex.IsMatch(lineText);

	/// <inheritdoc />
	public string? GetSectionHeaderText(string lineText)
	{
		Match match = SectionHeaderRegex.Match(lineText);

		if (!match.Success)
			return null;

		return match.Value.Trim().Trim(':');
	}
}
