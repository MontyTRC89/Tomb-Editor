namespace TombLib.Scripting.Presentation;

/// <summary>
/// Describes the current state of the text completion presentation.
/// </summary>
/// <param name="HasOpenWindow">Whether a completion window is currently open.</param>
/// <param name="IsRequestScheduled">Whether a completion request is currently scheduled.</param>
/// <param name="IsToolTipVisible">Whether the completion tooltip is visible.</param>
/// <param name="ToolTipContent">The content of the completion tooltip, when visible.</param>
public readonly record struct TextCompletionPresentationState(
	bool HasOpenWindow,
	bool IsRequestScheduled,
	bool IsToolTipVisible,
	object? ToolTipContent)
{
	/// <summary>
	/// Gets the empty presentation state with no window, request, tooltip or content.
	/// </summary>
	public static TextCompletionPresentationState Empty { get; } = new(false, false, false, null);
}
