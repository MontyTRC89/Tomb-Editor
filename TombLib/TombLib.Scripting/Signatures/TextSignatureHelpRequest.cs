using System;

namespace TombLib.Scripting.Signatures;

/// <summary>
/// Represents an in-process signature help request against an immutable document snapshot.
/// </summary>
/// <remarks>
/// Signature help requests use a zero-based document offset so they can be constructed directly
/// from editor caret positions without converting to line and column coordinates.
/// </remarks>
public sealed record TextSignatureHelpRequest
{
	/// <summary>
	/// Initializes a new instance of the <see cref="TextSignatureHelpRequest"/> class.
	/// </summary>
	/// <param name="documentText">The current document snapshot text.</param>
	/// <param name="caretOffset">The zero-based caret offset within that snapshot.</param>
	/// <exception cref="ArgumentOutOfRangeException">
	/// <paramref name="caretOffset"/> is negative or greater than the length of <paramref name="documentText"/>.
	/// </exception>
	public TextSignatureHelpRequest(string documentText, int caretOffset)
	{
		ArgumentNullException.ThrowIfNull(documentText);

		if (caretOffset < 0 || caretOffset > documentText.Length)
			throw new ArgumentOutOfRangeException(nameof(caretOffset));

		DocumentText = documentText;
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
