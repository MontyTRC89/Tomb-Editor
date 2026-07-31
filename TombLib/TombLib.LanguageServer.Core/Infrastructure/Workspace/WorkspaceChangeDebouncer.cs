namespace TombLib.LanguageServer.Core;

/// <summary>
/// Debounces bursts of workspace file changes before dispatching them to the owner.
/// </summary>
internal sealed class WorkspaceChangeDebouncer : IDisposable
{
	/// <summary>
	/// Triggers dispatch of the currently accumulated workspace changes.
	/// </summary>
	private readonly Action _dispatchPendingChanges;

	/// <summary>
	/// Defines how long new changes delay the next dispatch.
	/// </summary>
	private readonly TimeSpan _debounceDelay;

	/// <summary>
	/// Stores the currently accumulated coalesced changes.
	/// </summary>
	private readonly WorkspaceChangeAccumulator _pendingChanges = new();

	/// <summary>
	/// Schedules the delayed dispatch callback.
	/// </summary>
	private readonly Timer _timer;

	/// <summary>
	/// Tracks whether the debouncer has been disposed.
	/// </summary>
	private volatile bool _isDisposed;

	/// <summary>
	/// Initializes a new instance of the <see cref="WorkspaceChangeDebouncer"/> class.
	/// </summary>
	/// <param name="debounceDelay">The delay applied after the most recent change.</param>
	/// <param name="dispatchPendingChanges">The callback that dispatches the accumulated changes.</param>
	public WorkspaceChangeDebouncer(TimeSpan debounceDelay, Action dispatchPendingChanges)
	{
		_debounceDelay = debounceDelay;
		_dispatchPendingChanges = dispatchPendingChanges;

		_timer = new Timer(OnDebounceTick, state: null, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
	}

	/// <summary>
	/// Gets a value indicating whether the debouncer has no pending changes.
	/// </summary>
	public bool IsEmpty => _pendingChanges.IsEmpty;

	/// <summary>
	/// Queues a single file change and restarts the debounce timer.
	/// </summary>
	/// <param name="filePath">The changed file path.</param>
	/// <param name="kind">The change kind.</param>
	public void Queue(string filePath, FileChangeKind kind)
	{
		if (_isDisposed || string.IsNullOrEmpty(filePath))
			return;

		_pendingChanges.Add(filePath, kind);
		ScheduleDispatch(_debounceDelay);
	}

	/// <summary>
	/// Drains the currently pending changes into a forwardable batch.
	/// </summary>
	/// <returns>The drained file-change batch.</returns>
	public FileChangeBatch DrainBatch()
		=> _pendingChanges.DrainBatch();

	/// <summary>
	/// Requeues a drained batch and schedules another debounced dispatch attempt.
	/// </summary>
	/// <param name="batch">The batch to requeue.</param>
	/// <param name="dispatchDelay">The delay before the next retry attempt.</param>
	public void Requeue(FileChangeBatch batch, TimeSpan? dispatchDelay = null)
	{
		if (_isDisposed || batch.Count == 0)
			return;

		for (int i = 0; i < batch.Count; i++)
			_pendingChanges.Add(batch.Entries[i].Path, batch.Entries[i].Kind);

		ScheduleDispatch(dispatchDelay ?? _debounceDelay);
	}

	/// <summary>
	/// Restores a drained batch without scheduling another timer tick.
	/// Callers use this when they will explicitly decide how recovery dispatch should proceed.
	/// </summary>
	/// <param name="batch">The batch to restore.</param>
	public void Restore(FileChangeBatch batch)
	{
		if (_isDisposed || batch.Count == 0)
			return;

		for (int i = 0; i < batch.Count; i++)
			_pendingChanges.Add(batch.Entries[i].Path, batch.Entries[i].Kind);
	}

	/// <summary>
	/// Stops the debounce timer without discarding the currently buffered changes.
	/// </summary>
	public void Stop()
	{
		if (_isDisposed)
			return;

		try
		{
			_timer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
		}
		catch (ObjectDisposedException)
		{ }
	}

	/// <summary>
	/// Stops the timer and releases the debouncer resources.
	/// </summary>
	public void Dispose()
	{
		if (_isDisposed)
			return;

		_isDisposed = true;
		_timer.Dispose();
	}

	/// <summary>
	/// Dispatches pending changes when the debounce timer fires.
	/// </summary>
	/// <param name="_">Unused timer state.</param>
	private void OnDebounceTick(object? _)
	{
		if (_isDisposed || _pendingChanges.IsEmpty)
			return;

		_dispatchPendingChanges();
	}

	private void ScheduleDispatch(TimeSpan dispatchDelay)
	{
		try
		{
			_timer.Change(dispatchDelay, Timeout.InfiniteTimeSpan);
		}
		catch (ObjectDisposedException)
		{
			if (!_isDisposed)
				_dispatchPendingChanges();
		}
	}
}
