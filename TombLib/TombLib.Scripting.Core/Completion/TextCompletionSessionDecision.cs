using Nickelony.LanguageServer.Core.Completion;

namespace TombLib.Scripting.Completion;

/// <summary>
/// Describes how the editor should update the completion session after processing input.
/// </summary>
/// <param name="CloseWindow">Whether the active completion window should be dismissed.</param>
/// <param name="Items">The items to show when opening or refreshing a completion window.</param>
/// <param name="StartOffset">The zero-based start offset for replacement when opening the window.</param>
/// <param name="EndOffset">The zero-based end offset for replacement when opening the window.</param>
public readonly record struct TextCompletionSessionDecision(
	bool CloseWindow,
	IReadOnlyList<TextCompletionItem>? Items,
	int? StartOffset,
	int? EndOffset)
{
	/// <summary>
	/// Gets a decision that leaves the current completion window unchanged.
	/// </summary>
	public static TextCompletionSessionDecision None { get; } = new(false, null, null, null);

	/// <summary>
	/// Creates a decision that closes the active completion window.
	/// </summary>
	/// <returns>A closing completion-session decision.</returns>
	public static TextCompletionSessionDecision Close()
		=> new(true, null, null, null);

	/// <summary>
	/// Creates a decision that opens or refreshes the completion window for the supplied range.
	/// </summary>
	/// <param name="items">The completion items to display.</param>
	/// <param name="startOffset">The zero-based replacement start offset.</param>
	/// <param name="endOffset">The zero-based replacement end offset.</param>
	/// <returns>An opening completion-session decision.</returns>
	public static TextCompletionSessionDecision Open(IReadOnlyList<TextCompletionItem> items, int startOffset, int endOffset)
		=> new(false, items, startOffset, endOffset);
}
