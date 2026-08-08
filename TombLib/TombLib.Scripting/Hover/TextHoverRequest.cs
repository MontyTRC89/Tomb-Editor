namespace TombLib.Scripting.Hover;

/// <summary>
/// Represents an in-process hover request against an immutable document snapshot.
/// </summary>
/// <remarks>
/// Hover requests use a zero-based document offset so they can be constructed directly from
/// editor caret positions without converting to line and column coordinates.
/// </remarks>
public sealed record TextHoverRequest
{
	/// <summary>
	/// Initializes a new instance of the <see cref="TextHoverRequest"/> class.
	/// </summary>
	/// <param name="documentText">The current document snapshot text.</param>
	/// <param name="hoveredOffset">The zero-based hovered offset within that snapshot.</param>
	/// <exception cref="ArgumentNullException"><paramref name="documentText"/> is null.</exception>
	/// <exception cref="ArgumentOutOfRangeException">
	/// <paramref name="hoveredOffset"/> is negative or greater than the length of <paramref name="documentText"/>.
	/// </exception>
	public TextHoverRequest(string documentText, int hoveredOffset)
	{
		ArgumentNullException.ThrowIfNull(documentText);

		if (hoveredOffset < 0 || hoveredOffset > documentText.Length)
			throw new ArgumentOutOfRangeException(nameof(hoveredOffset));

		DocumentText = documentText;
		HoveredOffset = hoveredOffset;
	}

	/// <summary>
	/// Gets the current document snapshot text.
	/// </summary>
	public string DocumentText { get; }

	/// <summary>
	/// Gets the zero-based hovered offset within <see cref="DocumentText"/>.
	/// </summary>
	public int HoveredOffset { get; }
}
