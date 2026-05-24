namespace TombLib.Scripting.Signatures;

/// <summary>
/// Represents an in-process signature-help request against the current document snapshot.
/// </summary>
public sealed class TextSignatureHelpRequest
{
	/// <summary>
	/// Initializes a new instance of the <see cref="TextSignatureHelpRequest"/> class.
	/// </summary>
	/// <param name="documentText">The current document snapshot text.</param>
	/// <param name="caretOffset">The zero-based caret offset within that snapshot.</param>
	public TextSignatureHelpRequest(string documentText, int caretOffset)
	{
		DocumentText = documentText ?? string.Empty;
		CaretOffset = caretOffset;
	}

	/// <summary>
	/// Gets the current document snapshot text.
	/// </summary>
	public string DocumentText { get; }

	/// <summary>
	/// Gets the zero-based caret offset within <see cref="DocumentText"/>.
	/// </summary>
	public int CaretOffset { get; }
}
