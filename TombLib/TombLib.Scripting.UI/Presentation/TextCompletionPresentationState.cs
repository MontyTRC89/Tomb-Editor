namespace TombLib.Scripting.UI.Presentation;

public readonly record struct TextCompletionPresentationState(
	bool HasOpenWindow,
	bool IsRequestScheduled,
	bool IsToolTipVisible,
	object? ToolTipContent)
{
	public static TextCompletionPresentationState Empty { get; } = new(false, false, false, null);
}
