namespace TombLib.Scripting.Text;

/// <summary>
/// Represents an immutable snapshot of text used by provider services.
/// Implementations never observe later document edits and may be retained freely;
/// they capture their content at construction time.
/// Offsets are zero-based. Line numbers are one-based, matching the AvalonEdit model.
/// </summary>
public interface ITextSnapshot
{
	/// <summary>
	/// Gets the optional file name associated with this text source.
	/// </summary>
	string? FileName { get; }

	/// <summary>
	/// Gets the total number of characters in the text.
	/// </summary>
	int TextLength { get; }

	/// <summary>
	/// Gets the total number of lines in the text.
	/// A non-empty text that does not end with a newline still has at least one line.
	/// </summary>
	int LineCount { get; }

	/// <summary>
	/// Gets the character at the specified zero-based offset.
	/// </summary>
	/// <param name="offset">The zero-based offset of the character to retrieve.</param>
	/// <returns>The character at the specified offset.</returns>
	/// <exception cref="ArgumentOutOfRangeException">
	/// <paramref name="offset"/> is negative or not less than <see cref="TextLength"/>.
	/// </exception>
	char GetCharAt(int offset);

	/// <summary>
	/// Retrieves the text within the specified range.
	/// </summary>
	/// <param name="offset">The zero-based start offset of the text to retrieve.</param>
	/// <param name="length">The number of characters to retrieve.</param>
	/// <returns>The text in the specified range.</returns>
	/// <exception cref="ArgumentOutOfRangeException">
	/// <paramref name="offset"/> or <paramref name="length"/> is negative,
	/// or the specified range extends beyond <see cref="TextLength"/>.
	/// </exception>
	string GetText(int offset, int length);

	/// <summary>
	/// Gets the line that contains the specified zero-based offset.
	/// </summary>
	/// <param name="offset">The zero-based offset to locate.</param>
	/// <returns>The line containing the specified offset.</returns>
	/// <exception cref="ArgumentOutOfRangeException">
	/// <paramref name="offset"/> is negative or greater than <see cref="TextLength"/>.
	/// </exception>
	ITextLine GetLineByOffset(int offset);

	/// <summary>
	/// Gets the line with the specified one-based line number.
	/// </summary>
	/// <param name="lineNumber">The one-based line number to retrieve.</param>
	/// <returns>The line at the specified line number.</returns>
	/// <exception cref="ArgumentOutOfRangeException">
	/// <paramref name="lineNumber"/> is less than 1 or greater than <see cref="LineCount"/>.
	/// </exception>
	ITextLine GetLineByNumber(int lineNumber);

	/// <summary>
	/// Enumerates all lines in the text, in order.
	/// </summary>
	IEnumerable<ITextLine> Lines { get; }
}
