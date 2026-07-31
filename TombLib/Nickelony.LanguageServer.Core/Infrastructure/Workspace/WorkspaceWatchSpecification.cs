namespace Nickelony.LanguageServer.Core;

/// <summary>
/// Describes one file-system watch pattern used by the workspace watcher.
/// </summary>
/// <param name="Filter">The <see cref="FileSystemWatcher"/> filter pattern.</param>
/// <param name="IncludeSubdirectories">Whether matching should recurse into subdirectories.</param>
public readonly record struct WorkspaceWatchSpecification(string Filter, bool IncludeSubdirectories);
