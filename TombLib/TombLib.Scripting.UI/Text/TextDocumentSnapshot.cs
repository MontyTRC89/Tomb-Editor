using System;
using System.Collections.Generic;
using ICSharpCode.AvalonEdit.Document;
using TombLib.Scripting.Text;

namespace TombLib.Scripting.UI.Text;

/// <summary>
/// An <see cref="ITextSnapshot"/> that adapts an AvalonEdit <see cref="ICSharpCode.AvalonEdit.Document.TextDocument"/>.
/// This type lives in the UI layer because it depends on AvalonEdit.
/// </summary>
public sealed class TextDocumentSnapshot : ITextSnapshot
{
    private readonly TextDocument _document;

    /// <summary>
    /// Initializes a new instance of the <see cref="TextDocumentSnapshot"/> class.
    /// </summary>
    /// <param name="document">The AvalonEdit document to adapt. Must not be null.</param>
    /// <exception cref="ArgumentNullException"><paramref name="document"/> is null.</exception>
    public TextDocumentSnapshot(TextDocument document)
    {
        _document = document ?? throw new ArgumentNullException(nameof(document));
    }

    /// <inheritdoc />
    public string? FileName => _document.FileName;

    /// <inheritdoc />
    public int TextLength => _document.TextLength;

    /// <inheritdoc />
    public int LineCount => _document.LineCount;

    /// <inheritdoc />
    public char GetCharAt(int offset)
    {
        if (offset < 0 || offset >= _document.TextLength)
            throw new ArgumentOutOfRangeException(nameof(offset));

        return _document.GetCharAt(offset);
    }

    /// <inheritdoc />
    public string GetText(int offset, int length)
    {
        if (offset < 0 || length < 0 || offset + length > _document.TextLength)
            throw new ArgumentOutOfRangeException();

        return _document.GetText(offset, length);
    }

    /// <inheritdoc />
    public ITextLine GetLineByOffset(int offset)
    {
        if (offset < 0 || offset > _document.TextLength)
            throw new ArgumentOutOfRangeException(nameof(offset));

        return new DocumentLineAdapter(_document.GetLineByOffset(offset));
    }

    /// <inheritdoc />
    public ITextLine GetLineByNumber(int lineNumber)
    {
        if (lineNumber < 1 || lineNumber > _document.LineCount)
            throw new ArgumentOutOfRangeException(nameof(lineNumber));

        return new DocumentLineAdapter(_document.GetLineByNumber(lineNumber));
    }

    /// <inheritdoc />
    public IEnumerable<ITextLine> Lines
    {
        get
        {
            foreach (IDocumentLine line in _document.Lines)
                yield return new DocumentLineAdapter(line);
        }
    }

    /// <summary>
    /// Adapts an AvalonEdit <see cref="IDocumentLine"/> to <see cref="ITextLine"/>.
    /// </summary>
    private sealed class DocumentLineAdapter : ITextLine
    {
        private readonly IDocumentLine _line;

        public DocumentLineAdapter(IDocumentLine line)
        {
            _line = line;
        }

        public int Offset => _line.Offset;

        public int Length => _line.Length;

        public int EndOffset => _line.EndOffset;

        public int LineNumber => _line.LineNumber;
    }
}
