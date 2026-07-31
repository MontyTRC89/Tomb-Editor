namespace Nickelony.LanguageServer.Core;

/// <summary>
/// Describes the outcome of attempting to start a workspace file watcher.
/// </summary>
public enum WorkspaceWatcherStartStatus
{
	/// <summary>
	/// The watcher started successfully.
	/// </summary>
	Started,

	/// <summary>
	/// The watcher was already running.
	/// </summary>
	AlreadyRunning,

	/// <summary>
	/// The watcher could not start because it was already disposed.
	/// </summary>
	Disposed,

	/// <summary>
	/// The watcher could not start because the workspace root path does not exist.
	/// </summary>
	WorkspaceRootMissing,

	/// <summary>
	/// The watcher failed to start because watcher creation or activation threw.
	/// </summary>
	StartupFailed
}
