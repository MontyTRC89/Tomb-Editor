namespace Nickelony.LanguageServer.Core;

/// <summary>
/// Represents a normalized workspace file change ready for forwarding.
/// </summary>
/// <param name="Path">The normalized file path.</param>
/// <param name="Kind">The effective file change kind.</param>
public readonly record struct WorkspaceFileChange(string Path, FileChangeKind Kind);
