using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Indentation;
using System;

namespace TombLib.Scripting.Lua.Editing;

internal sealed class LuaAutoIndentationStrategy : IIndentationStrategy
{
	private readonly TextEditorOptions _options;

	public LuaAutoIndentationStrategy(TextEditorOptions options)
	{
		ArgumentNullException.ThrowIfNull(options);
		_options = options;
	}

	public void IndentLine(TextDocument document, DocumentLine line)
	{
		ArgumentNullException.ThrowIfNull(document);
		ArgumentNullException.ThrowIfNull(line);

		string lineText = document.GetText(line);
		string desiredIndentation = GetDesiredIndentation(document, line, lineText);
		ReplaceLeadingWhitespace(document, line, lineText, desiredIndentation);
	}

	public void IndentLines(TextDocument document, int beginLine, int endLine)
	{
		ArgumentNullException.ThrowIfNull(document);

		if (document.LineCount == 0)
			return;

		int startLine = Math.Max(1, Math.Min(beginLine, document.LineCount));
		int lastLine = Math.Max(startLine, Math.Min(endLine, document.LineCount));

		document.BeginUpdate();

		try
		{
			for (int lineNumber = startLine; lineNumber <= lastLine; lineNumber++)
				IndentLine(document, document.GetLineByNumber(lineNumber));
		}
		finally
		{
			document.EndUpdate();
		}
	}

	private string GetDesiredIndentation(TextDocument document, DocumentLine line, string lineText)
	{
		ArgumentNullException.ThrowIfNull(document);
		ArgumentNullException.ThrowIfNull(line);

		if (line.PreviousLine is null)
			return LuaIndentationStrategy.GetLeadingWhitespace(lineText);

		DocumentLine previousLine = line.PreviousLine;
		string previousLineText = document.GetText(previousLine);
		string previousLineIndentation = LuaIndentationStrategy.GetLeadingWhitespace(previousLineText);

		return LuaIndentationStrategy.GetDesiredIndentation(
			previousLineText,
			lineText,
			previousLineIndentation,
			LuaIndentationStrategy.CreateIndentationUnit(_options.ConvertTabsToSpaces, _options.IndentationSize, 4),
			ShouldUseSmartIndent(document, previousLine));
	}

	private static bool ShouldUseSmartIndent(TextDocument document, DocumentLine previousLine)
	{
		ArgumentNullException.ThrowIfNull(document);
		ArgumentNullException.ThrowIfNull(previousLine);

		if (previousLine.Length == 0)
			return true;

		return !LuaEditorInteractionRules.IsInsideCommentOrString(document, previousLine.EndOffset - 1);
	}

	private static void ReplaceLeadingWhitespace(TextDocument document, DocumentLine line, string lineText, string desiredIndentation)
	{
		ArgumentNullException.ThrowIfNull(document);
		ArgumentNullException.ThrowIfNull(line);

		int leadingWhitespaceLength = 0;

		while (leadingWhitespaceLength < lineText.Length
			&& (lineText[leadingWhitespaceLength] == ' ' || lineText[leadingWhitespaceLength] == '\t'))
		{
			leadingWhitespaceLength++;
		}

		if (leadingWhitespaceLength == desiredIndentation.Length
			&& string.CompareOrdinal(lineText, 0, desiredIndentation, 0, leadingWhitespaceLength) == 0)
		{
			return;
		}

		document.Replace(line.Offset, leadingWhitespaceLength, desiredIndentation);
	}
}
