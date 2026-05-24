namespace TombLib.Scripting.Completion;

/// <summary>
/// Describes a completion request against a concrete document snapshot.
/// </summary>
/// <remarks>
/// The shared completion contract uses an editor offset for the request point and leaves
/// protocol-specific line and column conversion at the provider boundary.
/// </remarks>
/// <param name="DocumentText">The current document snapshot text.</param>
/// <param name="CaretOffset">The zero-based caret offset within <paramref name="DocumentText"/>.</param>
/// <param name="Trigger">The reason the completion request was raised.</param>
/// <param name="ArgumentIndex">The optional active argument index for contextual completion scenarios.</param>
public readonly record struct TextCompletionContext(
	string DocumentText,
	int CaretOffset,
	TextCompletionTrigger Trigger = TextCompletionTrigger.Automatic,
	int ArgumentIndex = -1);
