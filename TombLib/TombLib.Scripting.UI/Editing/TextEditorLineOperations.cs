#nullable enable

using ICSharpCode.AvalonEdit.Document;
using System;
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
}