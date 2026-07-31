namespace TombLib.LanguageServer.Core;

/// <summary>
/// Watches the workspace root for configured file patterns and forwards coalesced changes to the owner.
/// A startup failure disposes the instance, so recovery requires creating a replacement watcher.
/// </summary>
public sealed partial class WorkspaceFileWatcher : IDisposable, IAsyncDisposable
{
	private const int DispatchFailureWarningThreshold = 3;
	private const int DispatchFailureEscalationThreshold = 5;

	private readonly ILogger _logger;

	private static readonly TimeSpan DispatchDebounce = TimeSpan.FromMilliseconds(250);
	private static readonly TimeSpan MaxDispatchRetryDelay = TimeSpan.FromSeconds(5.0f);

	// Watcher registration state.
	private readonly string _workspaceRootDirectoryPath;
	private readonly IReadOnlyList<WorkspaceWatchSpecification> _watchSpecifications;
	private readonly Func<string, WorkspaceWatchSpecification, FileSystemWatcher> _fileSystemWatcherFactory;
	private readonly List<FileSystemWatcher> _watchers = [];
	private readonly object _watchersSyncRoot = new();
	private readonly Action<WorkspaceFileWatcher, Exception?>? _watcherFailed;
	private int _watcherFailureReported;

	// Dispatch and disposal lifecycle state. The completion source lets sync and async disposal
	// wait on the same deferred finalization path when a dispatch is still in flight.
	private readonly Func<FileChangeBatch, CancellationToken, Task> _dispatchAsync;
	private readonly WorkspaceChangeDebouncer _pendingChanges;
	private readonly WorkspaceChangeAccumulator _disposeRetryChanges = new();
	private readonly CancellationTokenSource _lifetimeCts = new();
	private readonly SemaphoreSlim _dispatchGate = new(1, 1);
	private readonly object _dispatchLifecycleSyncRoot = new();
	private TaskCompletionSource<bool>? _disposeCompletionSource;
	private volatile bool _isDisposed;
	private int _disposeStarted;
	private bool _flushDisposeRetryChanges = true;
	private int _activeDispatchCount;
	private bool _disposeFinalizationStarted;
	private int _dispatchResourcesDisposed;
	private int _consecutiveDispatchFailures;

	/// <summary>
	/// Gets a value indicating whether any file-system watchers are currently active.
	/// </summary>
	internal bool HasActiveWatchers
	{
		get
		{
			lock (_watchersSyncRoot)
				return _watchers.Count > 0;
		}
	}

	/// <summary>
	/// Gets the number of active file-system watchers.
	/// </summary>
	internal int ActiveWatcherCount
	{
		get
		{
			lock (_watchersSyncRoot)
				return _watchers.Count;
		}
	}

	/// <summary>
	/// Gets a value indicating whether the watcher has been disposed.
	/// </summary>
	internal bool IsDisposed => _isDisposed;

	/// <summary>
	/// Initializes a new instance of the <see cref="WorkspaceFileWatcher"/> class.
	/// </summary>
	/// <param name="workspaceRootDirectoryPath">The workspace root directory to watch.</param>
	/// <param name="dispatchAsync">The callback that forwards coalesced changes to the owner.</param>
	/// <param name="watchSpecifications">The explicit file patterns that should be watched under the workspace root.</param>
	/// <param name="watcherFailed">The callback that reports an unrecoverable watcher failure to the owner.</param>
	/// <param name="fileSystemWatcherFactory">Creates one file-system watcher for a watch specification.</param>
	/// <param name="logger">The logger instance, or <see langword="null"/> for a no-op logger.</param>
	public WorkspaceFileWatcher(
		string workspaceRootDirectoryPath,
		Func<FileChangeBatch, CancellationToken, Task> dispatchAsync,
		IReadOnlyList<WorkspaceWatchSpecification> watchSpecifications,
		Action<WorkspaceFileWatcher, Exception?>? watcherFailed = null,
		Func<string, WorkspaceWatchSpecification, FileSystemWatcher>? fileSystemWatcherFactory = null,
		ILogger<WorkspaceFileWatcher>? logger = null)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(workspaceRootDirectoryPath);

		if (watchSpecifications.Count == 0)
			throw new ArgumentException("At least one watch specification is required.", nameof(watchSpecifications));

		_logger = logger ?? NullLogger<WorkspaceFileWatcher>.Instance;

		_workspaceRootDirectoryPath = workspaceRootDirectoryPath;
		_dispatchAsync = dispatchAsync;
		_watchSpecifications = watchSpecifications;
		_watcherFailed = watcherFailed;
		_fileSystemWatcherFactory = fileSystemWatcherFactory ?? CreateFileSystemWatcher;

		_pendingChanges = new WorkspaceChangeDebouncer(DispatchDebounce, () => _ = DispatchPendingChangesAsync());
	}
}
