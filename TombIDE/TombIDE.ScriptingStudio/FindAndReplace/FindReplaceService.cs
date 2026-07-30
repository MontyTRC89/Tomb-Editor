#nullable enable

using System;
using System.Text.RegularExpressions;
using TombLib.Scripting.UI.Bases;

namespace TombIDE.ScriptingStudio.FindAndReplace;

/// <summary>
/// Pure text-level find and replace operations with no UI dependencies.
/// </summary>
public sealed class FindReplaceService
{
	/// <summary>
	/// Builds a regex pattern from the find text and search options.
	/// </summary>
	public string BuildPattern(string findText, bool useRegex, bool matchWholeWord)
	{
		if (string.IsNullOrEmpty(findText))
			return string.Empty;

		string pattern = useRegex ? findText : Regex.Escape(findText);

		if (matchWholeWord)
			pattern = @"\b" + pattern + @"\b";

		return pattern;
	}

	/// <summary>
	/// Builds <see cref="RegexOptions"/> from the case-sensitive flag.
	/// </summary>
	public RegexOptions BuildRegexOptions(bool caseSensitive)
		=> caseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase;

	/// <summary>
	/// Returns the number of matches of <paramref name="pattern"/> in <paramref name="text"/>.
	/// </summary>
	public int CountMatches(string text, string pattern, RegexOptions options)
		=> string.IsNullOrEmpty(pattern) ? 0 : Regex.Matches(text, pattern, options).Count;

	/// <summary>
	/// Returns all matches of <paramref name="pattern"/> in <paramref name="text"/>.
	/// </summary>
	public MatchCollection FindAllMatches(string text, string pattern, RegexOptions options)
		=> Regex.Matches(text, pattern, options);

	/// <summary>
	/// Returns the text before the given <paramref name="selectionStartIndex"/>.
	/// Used to find the previous match relative to the current selection.
	/// </summary>
	public string GetTextBeforeSelection(string documentText, int selectionStartIndex)
		=> documentText.Substring(0, selectionStartIndex);

	/// <summary>
	/// Returns the text after the given <paramref name="selectionEndIndex"/>.
	/// Used to find the next match relative to the current selection.
	/// </summary>
	public string GetTextAfterSelection(string documentText, int selectionEndIndex)
		=> documentText.Substring(selectionEndIndex);

	/// <summary>
	/// Finds matches in the text section before or after the selection,
	/// depending on <paramref name="order"/>.
	/// </summary>
	public MatchCollection GetMatchesFromSection(
		FindingOrder order,
		string documentText,
		int selectionStart,
		int selectionLength,
		string pattern,
		RegexOptions options)
	{
		return order switch
		{
			FindingOrder.Previous => FindAllMatches(
				GetTextBeforeSelection(documentText, selectionStart), pattern, options),
			FindingOrder.Next => FindAllMatches(
				GetTextAfterSelection(documentText, selectionStart + selectionLength), pattern, options),
			_ => throw new ArgumentOutOfRangeException(nameof(order))
		};
	}

	/// <summary>
	/// Returns the last match in a collection (for upward/previous search).
	/// </summary>
	public Match? GetLastMatch(MatchCollection matches)
		=> matches.Count > 0 ? matches[matches.Count - 1] : null;

	/// <summary>
	/// Returns the first match in a collection (for downward/next search).
	/// </summary>
	public Match? GetFirstMatch(MatchCollection matches)
		=> matches.Count > 0 ? matches[0] : null;

	/// <summary>
	/// Computes the document-level offset of a match found in the text-after-selection section.
	/// The <paramref name="cutStringLength"/> is the length of the text before the section.
	/// </summary>
	public int GetAbsoluteMatchOffset(int cutStringLength, Match match)
		=> cutStringLength + match.Index;

	/// <summary>
	/// Replaces all matches of <paramref name="pattern"/> in <paramref name="text"/>
	/// with <paramref name="replacement"/>.
	/// </summary>
	public string ReplaceAll(string text, string pattern, string replacement, RegexOptions options)
		=> Regex.Replace(text, pattern, replacement, options);

	/// <summary>
	/// Builds a <see cref="FindReplaceSource"/> from a document's match collection,
	/// mapping each match to its line number and line text.
	/// </summary>
	public FindReplaceSource BuildFindReplaceSource(
		string documentName,
		TextEditorBase textEditor,
		string pattern,
		RegexOptions options)
	{
		ArgumentNullException.ThrowIfNull(textEditor);

		MatchCollection documentMatches = FindAllMatches(textEditor.Text, pattern, options);

		if (documentMatches.Count == 0)
			return new FindReplaceSource { Name = documentName };

		var source = new FindReplaceSource { Name = documentName };

		foreach (Match match in documentMatches)
		{
			var line = textEditor.Document.GetLineByOffset(match.Index);
			string lineText = textEditor.Document.GetText(line.Offset, line.Length);
			string matchSegmentText = match.Value;

			string lineTextBeforeMatch = lineText.Substring(0, match.Index - line.Offset);
			int matchSegmentIndex = Regex.Matches(lineTextBeforeMatch, Regex.Escape(match.Value)).Count;

			source.Add(new FindReplaceItem(line.LineNumber, lineText, matchSegmentText, matchSegmentIndex));
		}

		return source;
	}
}
