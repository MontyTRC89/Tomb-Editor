#nullable enable

using TombLib.Scripting.UI.Editing;

namespace TombIDE.ScriptingStudio.TextEditing;

internal enum TextWorkspaceCommandStatus
{
	Applied,
	NoChanges,
	Cancelled
}

internal readonly record struct TextWorkspaceCommandResult(TextWorkspaceCommandStatus Status, TextWorkspaceEditTransaction? Transaction)
{
	public static TextWorkspaceCommandResult NoChanges { get; } = new(TextWorkspaceCommandStatus.NoChanges, null);

	public static TextWorkspaceCommandResult Cancelled { get; } = new(TextWorkspaceCommandStatus.Cancelled, null);

	public static TextWorkspaceCommandResult Applied(TextWorkspaceEditTransaction transaction) => new(TextWorkspaceCommandStatus.Applied, transaction);
}
