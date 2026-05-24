using System;
using System.Collections.Generic;
using ICSharpCode.AvalonEdit.Document;
using TextMateSharp.Grammars;
using TextMateSharp.Model;

namespace TombLib.Scripting.UI.Highlighting
{
	internal sealed class TextMateDocumentLineList : AbstractLineList
	{
		private readonly TextDocument _document;
		private readonly object _syncRoot = new object();
		private readonly List<string> _lineTexts = new List<string>();

		public TextMateDocumentLineList(TextDocument document)
		{
			_document = document ?? throw new ArgumentNullException(nameof(document));

			InitializeSnapshot();

			_document.Changed += Document_Changed;
		}

		public override void UpdateLine(int lineIndex)
			=> InvalidateLineRange(lineIndex, Math.Max(lineIndex, GetNumberOfLines() - 1));

		public override int GetNumberOfLines()
		{
			lock (_syncRoot)
				return _lineTexts.Count;
		}

		public override LineText GetLineTextIncludingTerminators(int lineIndex)
		{
			lock (_syncRoot)
			{
				if (lineIndex < 0 || lineIndex >= _lineTexts.Count)
					return new LineText(string.Empty);

				return new LineText(_lineTexts[lineIndex]);
			}
		}

		public override int GetLineLength(int lineIndex)
		{
			lock (_syncRoot)
			{
				if (lineIndex < 0 || lineIndex >= _lineTexts.Count)
					return 0;

				return _lineTexts[lineIndex].Length;
			}
		}

		public override void Dispose()
			=> _document.Changed -= Document_Changed;

		private void Document_Changed(object sender, DocumentChangeEventArgs e)
		{
			(int startLineIndex, int removedLineCount, int insertedLineCount) = GetChangeInfo(_document, e);
			int lineDelta = insertedLineCount - removedLineCount;

			lock (_syncRoot)
			{
				ReplaceSnapshotLines(startLineIndex, removedLineCount, insertedLineCount);

				if (lineDelta > 0)
				{
					for (int i = 0; i < lineDelta; i++)
						AddLine(startLineIndex + 1 + i);
				}
				else if (lineDelta < 0)
				{
					for (int i = 0; i < -lineDelta; i++)
					{
						int removeIndex = Math.Min(startLineIndex + 1, GetNumberOfLines() - 1);

						if (removeIndex >= 0)
							RemoveLine(removeIndex);
					}
				}

				UpdateLine(Math.Min(startLineIndex, Math.Max(0, GetNumberOfLines() - 1)));
			}
		}

		internal static (int StartLineIndex, int RemovedLineCount, int InsertedLineCount) GetChangeInfo(TextDocument document, DocumentChangeEventArgs change)
		{
			if (document is null)
				throw new ArgumentNullException(nameof(document));

			if (change is null)
				throw new ArgumentNullException(nameof(change));

			return (
				GetStartLineIndex(document, change.Offset),
				GetAffectedLineCount(change.RemovedText?.Text),
				GetAffectedLineCount(change.InsertedText?.Text));
		}

		private void InitializeSnapshot()
		{
			lock (_syncRoot)
			{
				_lineTexts.Clear();

				for (int i = 0; i < _document.LineCount; i++)
				{
					_lineTexts.Add(ReadDocumentLineText(i));
					AddLine(i);
				}
			}
		}

		private void ReplaceSnapshotLines(int startLineIndex, int removedLineCount, int insertedLineCount)
		{
			int safeStartLineIndex = Math.Max(0, Math.Min(startLineIndex, _lineTexts.Count));
			int removableLineCount = Math.Max(0, Math.Min(removedLineCount, _lineTexts.Count - safeStartLineIndex));

			if (removableLineCount > 0)
				_lineTexts.RemoveRange(safeStartLineIndex, removableLineCount);

			_lineTexts.InsertRange(safeStartLineIndex, ReadDocumentLines(safeStartLineIndex, insertedLineCount));
		}

		private List<string> ReadDocumentLines(int startLineIndex, int lineCount)
		{
			var lines = new List<string>();

			if (_document.LineCount == 0)
				return lines;

			int safeStartLineIndex = Math.Max(0, Math.Min(startLineIndex, _document.LineCount - 1));
			int safeLineCount = Math.Max(1, Math.Min(lineCount, _document.LineCount - safeStartLineIndex));

			for (int i = 0; i < safeLineCount; i++)
				lines.Add(ReadDocumentLineText(safeStartLineIndex + i));

			return lines;
		}

		private string ReadDocumentLineText(int lineIndex)
		{
			DocumentLine line = _document.GetLineByNumber(Math.Max(1, lineIndex + 1));
			return _document.GetText(line.Offset, line.TotalLength);
		}

		private static int GetStartLineIndex(TextDocument document, int offset)
		{
			if (document.LineCount == 0)
				return 0;

			int safeOffset = Math.Max(0, Math.Min(offset, document.TextLength));
			DocumentLine line = document.GetLineByOffset(safeOffset);
			return Math.Max(0, line.LineNumber - 1);
		}

		private static int CountLineBreaks(string text)
		{
			if (string.IsNullOrEmpty(text))
				return 0;

			int count = 0;

			for (int i = 0; i < text.Length; i++)
			{
				if (text[i] == '\r')
				{
					count++;

					if (i + 1 < text.Length && text[i + 1] == '\n')
						i++;

					continue;
				}

				if (text[i] == '\n')
					count++;
			}

			return count;
		}

		private static int GetAffectedLineCount(string text)
			=> CountLineBreaks(text) + 1;
	}
}