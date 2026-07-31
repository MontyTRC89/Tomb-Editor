namespace Nickelony.LanguageServer.Core;

/// <summary>
/// Describes a workspace-watcher failure that should be surfaced to the UI.
/// </summary>
/// <param name="Message">The user-facing failure message.</param>
public readonly record struct WorkspaceWatcherFailure(string Message);
