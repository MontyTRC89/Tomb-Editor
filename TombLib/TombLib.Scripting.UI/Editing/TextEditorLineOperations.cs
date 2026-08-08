using ICSharpCode.AvalonEdit.Document;
using System;
using System.Text.RegularExpressions;
using TombLib.Scripting.UI.Bases;

namespace TombLib.Scripting.UI.Editing;

/// <summary>
/// Provides shared line-based editor operations for small scripted document updates.
/// </summary>
public static class TextEditorLineOperations
{
	/// <summary>
	/// Replaces the first line whose selector returns replacement text.
	/// </summary>
	/// <param name="textEditor">The editor whose document should be updated.</param>
	/// <param name="replacementSelector">Returns the replacement text for a matching line, or <see langword="null" /> to skip the line.</param>
	/// <param name="scrollToLine">Whether to scroll the editor to the updated line.</param>
	/// <returns><see langword="true" /> when a matching line was replaced; otherwise, <see langword="false" />.</returns>
	public static bool TryReplaceFirstMatchingLine(TextEditorBase textEditor, Func<string, string?> replacementSelector, bool scrollToLine = true)
	{
		ArgumentNullException.ThrowIfNull(textEditor);
		ArgumentNullException.ThrowIfNull(replacementSelector);

		foreach (DocumentLine line in textEditor.Document.Lines)
		{
			string lineText = textEditor.Document.GetText(line.Offset, line.Length);
			string? replacementText = replacementSelector(lineText);

			if (replacementText is null)
				continue;

			textEditor.ReplaceLine(line, replacementText, true);

			if (scrollToLine)
				textEditor.ScrollToLine(line.LineNumber);

			return true;
		}

		return false;
	}

	/// <summary>
	/// Replaces the first occurrence of <paramref name="oldName"/> with <paramref name="newName"/>
	/// on a line that matches <paramref name="lineRegex"/>. The name is extracted from each matching
	/// line via <paramref name="nameExtractor"/> before comparison.
	/// </summary>
	/// <param name="textEditor">The editor whose document should be updated.</param>
	/// <param name="lineRegex">The regular expression used to identify candidate lines.</param>
	/// <param name="nameExtractor">
	/// Extracts the normalized name from a candidate line. Receives the full line text and
	/// the <paramref name="lineRegex"/> to remove the pattern; returns the cleaned name.
	/// </param>
	/// <param name="oldName">The name to search for.</param>
	/// <param name="newName">The replacement name.</param>
	/// <param name="scrollToLine">Whether to scroll the editor to the updated line.</param>
	/// <returns><see langword="true" /> when a matching line was replaced; otherwise, <see langword="false" />.</returns>
	public static bool TryReplaceFirstMatchingLine(
		TextEditorBase textEditor,
		Regex lineRegex,
		Func<string, Regex, string> nameExtractor,
		string oldName,
		string newName,
		bool scrollToLine = true)
	{
		ArgumentNullException.ThrowIfNull(textEditor);
		ArgumentNullException.ThrowIfNull(lineRegex);
		ArgumentNullException.ThrowIfNull(nameExtractor);
		ArgumentNullException.ThrowIfNull(oldName);
		ArgumentNullException.ThrowIfNull(newName);

		return TryReplaceFirstMatchingLine(textEditor, lineText => {
			if (!lineRegex.IsMatch(lineText))
				return null;

			string extractedName = nameExtractor(lineText, lineRegex);

			return extractedName == oldName
				? lineText.Replace(oldName, newName)
				: null;
		}, scrollToLine);
	}

	/// <summary>
	/// Assigns <paramref name="levelName"/> to the first "EMPTY STRING SLOT" line in the document.
	/// </summary>
	/// <param name="textEditor">The editor whose document should be updated.</param>
	/// <param name="levelName">The level name to write into the slot.</param>
	/// <returns><see langword="true" /> when a slot line was replaced; otherwise, <see langword="false" />.</returns>
	public static bool TryAssignStockLevelNameStringSlot(TextEditorBase textEditor, string levelName)
		=> TryReplaceFirstMatchingLine(
			textEditor,
			lineText => Regex.IsMatch(lineText, @"EMPTY\sSTRING\sSLOT\s\d+") ? levelName : null,
			scrollToLine: false);
}
