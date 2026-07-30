namespace TombLib.Scripting.Completion;

/// <summary>
/// Describes the insert and replace ranges supplied by a completion item.
/// </summary>
/// <remarks>
/// The contained ranges use zero-based protocol coordinates rather than editor offsets.
/// </remarks>
/// <param name="InsertRange">The insert range for the completion edit.</param>
/// <param name="ReplaceRange">The optional replace range; falls back to <paramref name="InsertRange"/> when <see langword="null"/>.</param>
public readonly record struct TextCompletionTextEdit(TextCompletionRange InsertRange, TextCompletionRange? ReplaceRange = null)
{
	/// <summary>
	/// Gets the effective replacement range, falling back to <see cref="InsertRange"/>.
	/// </summary>
	public TextCompletionRange ReplacementRange => ReplaceRange ?? InsertRange;
}
