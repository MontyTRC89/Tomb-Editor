#nullable enable

using ICSharpCode.AvalonEdit.Document;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;

namespace TombLib.Scripting.UI.Documents;

[SupportedOSPlatform("windows")]
internal sealed class BookmarkCoordinator
{
	private readonly Func<TextDocument> _documentProvider;
	private readonly Action? _onBookmarksChanged;
	private readonly List<TextAnchor> _bookmarkAnchors = [];

	public BookmarkCoordinator(Func<TextDocument> documentProvider, Action? onBookmarksChanged = null)
	{
		_documentProvider = documentProvider ?? throw new ArgumentNullException(nameof(documentProvider));
		_onBookmarksChanged = onBookmarksChanged;
	}

	public IReadOnlyList<DocumentLine> GetBookmarkedLines()
		=> CollectBookmarkedLines(GetDocument());

	public void ToggleBookmark(int caretOffset)
	{
		TextDocument document = GetDocument();

		if (document.LineCount == 0)
			return;

		DocumentLine currentLine = document.GetLineByOffset(document.ClampOffset(caretOffset));
		TextAnchor? bookmarkAnchor = FindBookmarkAnchor(document, currentLine);

		if (bookmarkAnchor is null)
			AddBookmark(document, currentLine);
		else
			_bookmarkAnchors.Remove(bookmarkAnchor);

		_onBookmarksChanged?.Invoke();
	}

	public DocumentLine? GetNextBookmarkLine(int caretOffset)
		=> GetAdjacentBookmarkLine(caretOffset, findNext: true);

	public DocumentLine? GetPreviousBookmarkLine(int caretOffset)
		=> GetAdjacentBookmarkLine(caretOffset, findNext: false);

	public void Clear()
	{
		_bookmarkAnchors.Clear();
		_onBookmarksChanged?.Invoke();
	}

	public void Save(string filePath)
	{
		if (string.IsNullOrWhiteSpace(filePath))
			return;

		List<DocumentLine> bookmarkedLines = CollectBookmarkedLines(GetDocument());
		string bookmarkFileName = GetBookmarkFilePath(filePath);

		try
		{
			if (bookmarkedLines.Count > 0)
			{
				File.WriteAllLines(bookmarkFileName, bookmarkedLines.Select(line => line.LineNumber.ToString()));
			}
			else if (File.Exists(bookmarkFileName))
			{
				File.Delete(bookmarkFileName);
			}
		}
		catch
		{
			// Too bad.
		}
	}

	public void Restore(string filePath)
	{
		_bookmarkAnchors.Clear();

		if (string.IsNullOrWhiteSpace(filePath))
			return;

		string bookmarkFileName = GetBookmarkFilePath(filePath);

		if (!File.Exists(bookmarkFileName))
			return;

		TextDocument document = GetDocument();

		if (document.LineCount == 0)
			return;

		try
		{
			foreach (string line in File.ReadAllLines(bookmarkFileName))
			{
				if (int.TryParse(line, out int lineNumber) && lineNumber >= 1 && lineNumber <= document.LineCount)
				{
					DocumentLine documentLine = document.GetLineByNumber(lineNumber);

					if (FindBookmarkAnchor(document, documentLine) is null)
						AddBookmark(document, documentLine);
				}
			}
		}
		catch
		{
			// Too bad.
		}
	}

	private DocumentLine? GetAdjacentBookmarkLine(int caretOffset, bool findNext)
	{
		TextDocument document = GetDocument();
		List<DocumentLine> bookmarkedLines = CollectBookmarkedLines(document);

		if (document.LineCount == 0 || bookmarkedLines.Count == 0)
			return null;

		DocumentLine currentLine = document.GetLineByOffset(document.ClampOffset(caretOffset));

		return findNext
			? bookmarkedLines.FirstOrDefault(line => line.LineNumber > currentLine.LineNumber) ?? bookmarkedLines[0]
			: bookmarkedLines.LastOrDefault(line => line.LineNumber < currentLine.LineNumber) ?? bookmarkedLines[bookmarkedLines.Count - 1];
	}

	private static string GetBookmarkFilePath(string filePath)
		=> filePath + ".bkmrk";

	private TextDocument GetDocument()
		=> _documentProvider();

	private List<DocumentLine> CollectBookmarkedLines(TextDocument document)
	{
		var bookmarkedLines = new List<DocumentLine>();
		var invalidAnchors = new List<TextAnchor>();
		var seenLineNumbers = new HashSet<int>();

		foreach (TextAnchor anchor in _bookmarkAnchors)
		{
			DocumentLine? line = GetBookmarkedLine(document, anchor);

			if (line is null)
			{
				invalidAnchors.Add(anchor);
				continue;
			}

			if (seenLineNumbers.Add(line.LineNumber))
				bookmarkedLines.Add(line);
		}

		foreach (TextAnchor anchor in invalidAnchors)
			_bookmarkAnchors.Remove(anchor);

		bookmarkedLines.Sort((left, right) => left.LineNumber.CompareTo(right.LineNumber));
		return bookmarkedLines;
	}

	private void AddBookmark(TextDocument document, DocumentLine line)
	{
		if (line is null)
			return;

		TextAnchor anchor = document.CreateAnchor(line.Offset);
		anchor.MovementType = AnchorMovementType.BeforeInsertion;
		anchor.SurviveDeletion = true;
		_bookmarkAnchors.Add(anchor);
	}

	private TextAnchor? FindBookmarkAnchor(TextDocument document, DocumentLine line)
	{
		if (line is null)
			return null;

		foreach (TextAnchor anchor in _bookmarkAnchors)
		{
			DocumentLine? bookmarkedLine = GetBookmarkedLine(document, anchor);

			if (bookmarkedLine is not null && bookmarkedLine.LineNumber == line.LineNumber)
				return anchor;
		}

		return null;
	}

	private static DocumentLine? GetBookmarkedLine(TextDocument document, TextAnchor anchor)
	{
		if (anchor is null || anchor.IsDeleted || document.LineCount == 0)
			return null;

		int offset = document.ClampOffset(anchor.Offset);
		return document.GetLineByOffset(offset);
	}
}
