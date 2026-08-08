using System;
using System.Windows;
using TombLib.Scripting.Cleaning;
using TombLib.Scripting.UI.Bases;

namespace TombLib.Scripting.UI.Cleaning;

/// <summary>
/// Applies formatter output to an editor while preserving the current scroll position.
/// </summary>
public sealed class TextEditorFormattingService
{
	/// <summary>
	/// Formats the current editor content.
	/// </summary>
	/// <param name="editor">The editor to update.</param>
	/// <param name="formatter">The formatter to apply.</param>
	/// <param name="trimOnly">Whether only trailing whitespace should be trimmed.</param>
	public void FormatDocument(TextEditorBase editor, ITextDocumentFormatter formatter, bool trimOnly = false)
	{
		ArgumentNullException.ThrowIfNull(editor);
		ArgumentNullException.ThrowIfNull(formatter);

		string formattedContent = formatter.FormatDocument(editor.Text, trimOnly);

		if (string.Equals(editor.Text, formattedContent, StringComparison.Ordinal))
			return;

		Vector scrollOffset = editor.TextArea.TextView.ScrollOffset;

		editor.SelectAll();
		editor.SelectedText = formattedContent;
		editor.ResetSelection();

		editor.ScrollToHorizontalOffset(scrollOffset.X);
		editor.ScrollToVerticalOffset(scrollOffset.Y);
	}
}
