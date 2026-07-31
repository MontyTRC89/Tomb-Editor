namespace Nickelony.LanguageServer.Core.Diagnostics;

/// <summary>
/// Represents a single diagnostic produced for a document snapshot.
/// </summary>
public sealed class TextEditorDiagnostic
{
	/// <summary>
	/// Initializes a new instance of the <see cref="TextEditorDiagnostic"/> class.
	/// </summary>
	/// <param name="severity">The diagnostic severity.</param>
	/// <param name="message">The diagnostic message.</param>
	/// <param name="startOffset">The zero-based inclusive start offset.</param>
	/// <param name="endOffset">The zero-based exclusive end offset.</param>
	public TextEditorDiagnostic(TextEditorDiagnosticSeverity severity, string message, int startOffset, int endOffset)
	{
		Severity = severity;
		Message = message;
		StartOffset = Math.Max(0, startOffset);
		EndOffset = Math.Max(StartOffset + 1, endOffset);
	}

	/// <summary>
	/// Gets the diagnostic severity.
	/// </summary>
	public TextEditorDiagnosticSeverity Severity { get; }

	/// <summary>
	/// Gets the diagnostic message.
	/// </summary>
	public string Message { get; }

	/// <summary>
	/// Gets the zero-based inclusive start offset of the diagnostic span.
	/// </summary>
	public int StartOffset { get; }

	/// <summary>
	/// Gets the zero-based exclusive end offset of the diagnostic span.
	/// </summary>
	public int EndOffset { get; }

	/// <summary>
	/// Determines whether the diagnostic span contains the supplied offset.
	/// </summary>
	/// <param name="offset">The zero-based offset to check.</param>
	/// <returns><see langword="true"/> when the offset falls within the diagnostic span; otherwise, <see langword="false"/>.</returns>
	public bool ContainsOffset(int offset)
		=> offset >= StartOffset && offset < EndOffset;

	/// <summary>
	/// Determines whether the diagnostic span intersects the supplied range.
	/// </summary>
	/// <param name="startOffset">The zero-based inclusive start offset of the range.</param>
	/// <param name="endOffset">The zero-based exclusive end offset of the range.</param>
	/// <returns><see langword="true"/> when the ranges intersect; otherwise, <see langword="false"/>.</returns>
	public bool Intersects(int startOffset, int endOffset)
		=> endOffset > startOffset && EndOffset > startOffset && StartOffset < endOffset;
}
