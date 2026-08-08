using System;

namespace TombLib.Scripting.Completion;

/// <summary>
/// Describes a completion request against a concrete document snapshot.
/// </summary>
/// <remarks>
/// The shared completion contract uses an editor offset for the request point and leaves
/// protocol-specific line and column conversion at the provider boundary.
/// A default (<c>null</c>) context is not valid and is rejected by providers.
/// </remarks>
public sealed record TextCompletionContext
{
	/// <summary>
	/// Initializes a new instance of the <see cref="TextCompletionContext"/> class.
	/// </summary>
	/// <param name="documentText">The current document snapshot text.</param>
	/// <param name="caretOffset">The zero-based caret offset within <paramref name="documentText"/>.</param>
	/// <param name="trigger">The reason the completion request was raised.</param>
	/// <param name="argumentIndex">
	/// The active argument index for contextual completion scenarios, or <c>-1</c> to resolve
	/// it from the caret position. A default context must not be used because it would treat
	/// <c>0</c> as the active argument instead of resolving it from the caret.
	/// </param>
	/// <exception cref="ArgumentNullException"><paramref name="documentText"/> is null.</exception>
	/// <exception cref="ArgumentOutOfRangeException">
	/// <paramref name="caretOffset"/> is negative or greater than the length of <paramref name="documentText"/>,
	/// or <paramref name="argumentIndex"/> is less than <c>-1</c>.
	/// </exception>
	public TextCompletionContext(
		string documentText,
		int caretOffset,
		TextCompletionTrigger trigger = TextCompletionTrigger.Automatic,
		int argumentIndex = -1)
	{
		ArgumentNullException.ThrowIfNull(documentText);

		if (caretOffset < 0 || caretOffset > documentText.Length)
			throw new ArgumentOutOfRangeException(nameof(caretOffset));

		if (argumentIndex < -1)
			throw new ArgumentOutOfRangeException(nameof(argumentIndex));

		DocumentText = documentText;
		CaretOffset = caretOffset;
		Trigger = trigger;
		ArgumentIndex = argumentIndex;
	}

	/// <summary>
	/// Gets the current document snapshot text.
	/// </summary>
	public string DocumentText { get; }

	/// <summary>
	/// Gets the zero-based caret offset within <see cref="DocumentText"/>.
	/// </summary>
	public int CaretOffset { get; }

	/// <summary>
	/// Gets the reason the completion request was raised.
	/// </summary>
	public TextCompletionTrigger Trigger { get; }

	/// <summary>
	/// Gets the active argument index for contextual completion scenarios,
	/// or <c>-1</c> when the index should be resolved from the caret position.
	/// </summary>
	public int ArgumentIndex { get; }
}
