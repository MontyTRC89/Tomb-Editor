#nullable enable

namespace TombIDE.ScriptingStudio.Services.LuaIntellisense;

/// <summary>
/// Describes a workspace-watcher failure that should be surfaced to the UI.
/// </summary>
/// <param name="Message">The user-facing failure message.</param>
internal readonly record struct LuaWorkspaceWatcherFailure(string Message);