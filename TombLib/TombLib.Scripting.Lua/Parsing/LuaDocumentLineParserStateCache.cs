using ICSharpCode.AvalonEdit.Document;
using System;
using System.Collections.Generic;

namespace TombLib.Scripting.Lua.Parsing;

/// <summary>
/// Caches per-line Lua parser continuation state so comment and long-string checks stay fast after edits.
/// </summary>
internal sealed class LuaDocumentLineParserStateCache
{
	private readonly object _syncRoot = new();
	private readonly TextDocument _document;
	private readonly List<LuaLineParserState> _cachedLineStartStates = [];

	/// <summary>
	/// Initializes a new instance of the <see cref="LuaDocumentLineParserStateCache"/> class.
	/// </summary>
	/// <param name="document">The document whose line-start parser state should be cached.</param>
	public LuaDocumentLineParserStateCache(TextDocument document)
	{
		_document = document ?? throw new ArgumentNullException(nameof(document));
		_document.Changed += Document_Changed;
	}

	/// <summary>
	/// Gets the parser continuation state that applies at the start of the specified one-based line.
	/// </summary>
	/// <param name="lineNumber">The one-based document line number.</param>
	/// <returns>The cached or computed line-start parser state.</returns>
	public LuaLineParserState GetLineStartState(int lineNumber)
	{
		if (lineNumber <= 1)
			return default;

		lock (_syncRoot)
		{
			if (_document.LineCount == 0)
				return default;

			int targetLineNumber = Math.Max(1, Math.Min(lineNumber, _document.LineCount));

			EnsureFirstLineStateCached();
			EnsureStatesCachedThrough(targetLineNumber);
			return _cachedLineStartStates[targetLineNumber - 1];
		}
	}

	private void Document_Changed(object? sender, DocumentChangeEventArgs e)
	{
		lock (_syncRoot)
		{
			if (_cachedLineStartStates.Count == 0)
				return;

			int firstAffectedLineNumber = GetSafeLineNumberForOffset(e.Offset);
			int preservedLineCount = Math.Max(0, firstAffectedLineNumber - 1);

			if (_cachedLineStartStates.Count > preservedLineCount)
				_cachedLineStartStates.RemoveRange(preservedLineCount, _cachedLineStartStates.Count - preservedLineCount);

			if (_cachedLineStartStates.Count > _document.LineCount)
				_cachedLineStartStates.RemoveRange(_document.LineCount, _cachedLineStartStates.Count - _document.LineCount);
		}
	}

	private void EnsureFirstLineStateCached()
	{
		if (_document.LineCount > 0 && _cachedLineStartStates.Count == 0)
			_cachedLineStartStates.Add(default);
	}

	private void EnsureStatesCachedThrough(int lineNumber)
	{
		int targetLineNumber = Math.Min(lineNumber, _document.LineCount);

		if (targetLineNumber <= 0)
			return;

		while (_cachedLineStartStates.Count < targetLineNumber)
		{
			int previousLineNumber = _cachedLineStartStates.Count;
			DocumentLine previousLine = _document.GetLineByNumber(previousLineNumber);
			LuaLineParser.IsInsideCommentOrString(_document.GetText(previousLine), _cachedLineStartStates[previousLineNumber - 1], out LuaLineParserState nextState);
			_cachedLineStartStates.Add(nextState);
		}
	}

	private int GetSafeLineNumberForOffset(int offset)
	{
		if (_document.LineCount == 0)
			return 1;

		int safeOffset = Math.Max(0, Math.Min(offset, _document.TextLength));
		return _document.GetLineByOffset(safeOffset).LineNumber;
	}
}
