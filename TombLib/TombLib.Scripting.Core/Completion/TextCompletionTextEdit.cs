namespace TombLib.Scripting.Completion;

/// <summary>
/// Describes the insert and replace ranges supplied by a completion item.
/// </summary>
/// <remarks>
/// The contained ranges use zero-based protocol coordinates rather than editor offsets.
/// </remarks>
public readonly record struct TextCompletionTextEdit(TextCompletionRange InsertRange, TextCompletionRange? ReplaceRange = null)
{
	/// <summary>
	/// Gets the effective replacement range, falling back to <see cref="InsertRange"/>.
	/// </summary>
	public TextCompletionRange ReplacementRange => ReplaceRange ?? InsertRange;
}
