namespace TombLib.Scripting.Hover;

/// <summary>
/// Represents an in-process hover request against the current document snapshot.
/// </summary>
public sealed class TextHoverRequest
{
	/// <summary>
	/// Initializes a new instance of the <see cref="TextHoverRequest"/> class.
	/// </summary>
	/// <param name="documentText">The current document snapshot text.</param>
	/// <param name="hoveredOffset">The zero-based hovered offset within that snapshot.</param>
	public TextHoverRequest(string documentText, int hoveredOffset)
	{
		DocumentText = documentText ?? string.Empty;
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
