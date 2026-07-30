#nullable enable

using ICSharpCode.AvalonEdit.Document;
using System;
using System.Text.RegularExpressions;
using TombIDE.ScriptingStudio.FindAndReplace;
using TombLib.Scripting.Editing;
using TombLib.Scripting.UI.Bases;

namespace TombIDE.ScriptingStudio.Navigation;

internal static class EditorNavigationHelper
{
	public static EditorNavigationLocation CreateLocation(TextEditorBase textEditor)
		=> new(
			textEditor.FilePath,
			textEditor.CaretOffset,
			textEditor.SelectionStart,
			textEditor.SelectionLength,
			textEditor.CurrentRow);

	public static EditorNavigationLocation CreateDefinitionLocation(
		TextEditorBase textEditor,
		string filePath,
		int lineNumber,
		int columnNumber)
	{
		int offset = GetOffset(textEditor, lineNumber, columnNumber);

		return new EditorNavigationLocation(filePath, offset, offset, 0, lineNumber);
	}

	public static EditorNavigationLocation CreateRangeLocation(
		TextEditorBase textEditor,
		string filePath,
		TextDocumentRange range)
	{
		int startOffset = GetOffset(textEditor, range.StartLineNumber, range.StartColumnNumber);
		int endOffset = GetOffset(textEditor, range.EndLineNumber, range.EndColumnNumber);
		int selectionLength = Math.Max(0, endOffset - startOffset);

		return new EditorNavigationLocation(filePath, startOffset, startOffset, selectionLength, range.StartLineNumber);
	}

	public static void ApplyLocation(TextEditorBase textEditor, EditorNavigationLocation location)
	{
		int documentLength = textEditor.Document.TextLength;
		int selectionStart = Math.Max(0, Math.Min(location.SelectionStart, documentLength));
		int selectionLength = Math.Max(0, Math.Min(location.SelectionLength, documentLength - selectionStart));
		int caretOffset = Math.Max(0, Math.Min(location.CaretOffset, documentLength));

		textEditor.Focus();
		textEditor.CaretOffset = caretOffset;
		textEditor.Select(selectionStart, selectionLength);
		textEditor.ScrollToLine(GetPreferredLine(textEditor, location, selectionStart, caretOffset));
	}

	public static bool TryCreateSearchResultLocation(
		TextEditorBase textEditor,
		string filePath,
		FindReplaceItem item,
		out EditorNavigationLocation? location)
	{
		location = null;

		if (item.LineNumber < 1 || item.LineNumber > textEditor.Document.LineCount)
			return false;

		DocumentLine line = textEditor.Document.GetLineByNumber(item.LineNumber);
		string lineText = textEditor.Document.GetText(line.Offset, line.Length);
		MatchCollection matches = Regex.Matches(lineText, item.MatchSegmentText);

		if (item.MatchSegmentIndex < 0 || item.MatchSegmentIndex >= matches.Count)
		{
			location = new EditorNavigationLocation(filePath, line.Offset, line.Offset, 0, line.LineNumber);
			return true;
		}

		Match match = matches[item.MatchSegmentIndex];
		int selectionStart = line.Offset + match.Index;

		location = new EditorNavigationLocation(filePath, selectionStart, selectionStart, match.Length, line.LineNumber);
		return true;
	}

	private static int GetPreferredLine(
		TextEditorBase textEditor,
		EditorNavigationLocation location,
		int selectionStart,
		int caretOffset)
	{
		if (location.PreferredLine is int preferredLine)
			return Math.Max(1, Math.Min(preferredLine, textEditor.Document.LineCount));

		int offset = selectionStart > 0 || location.SelectionLength > 0
			? selectionStart
			: caretOffset;

		return textEditor.Document.GetLineByOffset(offset).LineNumber;
	}

	private static int GetOffset(TextEditorBase textEditor, int lineNumber, int columnNumber)
	{
		int safeLineNumber = Math.Max(1, Math.Min(lineNumber, textEditor.Document.LineCount));
		DocumentLine documentLine = textEditor.Document.GetLineByNumber(safeLineNumber);
		int safeColumnNumber = Math.Max(1, Math.Min(columnNumber, documentLine.Length + 1));
		return documentLine.Offset + safeColumnNumber - 1;
	}
}
