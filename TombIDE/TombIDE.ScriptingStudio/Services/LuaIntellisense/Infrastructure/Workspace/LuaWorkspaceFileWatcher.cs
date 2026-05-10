#nullable enable

using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace TombIDE.ScriptingStudio.Services.LuaIntellisense;

/// <summary>
/// Watches the Lua workspace root for relevant external file changes (`*.lua`, `.luarc.json`,
/// `.luarc.jsonc`) and forwards them to LuaLS through `workspace/didChangeWatchedFiles`. LuaLS does
/// not poll the file system itself for clients that opt into this capability, so without this hook
/// changes made by Git pull, file copy, or any other out-of-band tool would not reach the language
/// server until the user touched the affected document inside TombIDE.
/// </summary>
internal sealed class LuaWorkspaceFileWatcher : IDisposable
{
	private static readonly Logger Log = LogManager.GetCurrentClassLogger();
	private static readonly TimeSpan DispatchDebounce = TimeSpan.FromMilliseconds(250);

	private readonly string _workspaceRootDirectoryPath;
	private readonly Func<FileChangeBatch, CancellationToken, Task> _dispatchAsync;
	private readonly Action<Exception?>? _watcherFailed;
	private readonly ConcurrentDictionary<string, FileChangeKind> _pendingChanges = new(StringComparer.OrdinalIgnoreCase);
	private readonly SemaphoreSlim _dispatchGate = new(1, 1);
	private readonly CancellationTokenSource _lifetimeCts = new();

	private FileSystemWatcher? _luaWatcher;
	private FileSystemWatcher? _apiDirectoryWatcher;
	private FileSystemWatcher? _configWatcher;
	private Timer? _debounceTimer;
	private int _activeDispatchCount;
	private int _dispatchResourcesDisposed;
	private int _watcherFailureReported;
	private volatile bool _isDisposed;

	/// <summary>
	/// Initializes a new instance of the <see cref="LuaWorkspaceFileWatcher"/> class.
	/// </summary>
	/// <param name="workspaceRootDirectoryPath">The workspace root directory to watch.</param>
	/// <param name="dispatchAsync">The callback that forwards coalesced changes to LuaLS.</param>
	public LuaWorkspaceFileWatcher(string workspaceRootDirectoryPath, Func<FileChangeBatch, CancellationToken, Task> dispatchAsync,
		Action<Exception?>? watcherFailed = null)
	{
		_workspaceRootDirectoryPath = workspaceRootDirectoryPath;
		_dispatchAsync = dispatchAsync;
		_watcherFailed = watcherFailed;
	}

	/// <summary>
	/// Starts watching the configured workspace for external file changes.
	/// </summary>
	/// <returns><see langword="true"/> when the watcher is running; otherwise, <see langword="false"/>.</returns>
	public bool Start()
	{
		if (_isDisposed || _luaWatcher is not null)
			return _luaWatcher is not null;

		if (!Directory.Exists(_workspaceRootDirectoryPath))
			return false;

		try
		{
			// .API/*.lua changes are already covered by the recursive Lua watcher.
			// This watcher exists so creating, deleting, or renaming the .API directory itself is also observed.
			_apiDirectoryWatcher = CreateWatcher(".API", includeSubdirectories: false);
			_luaWatcher = CreateWatcher("*.lua", includeSubdirectories: true);
			_configWatcher = CreateWatcher(".luarc.*", includeSubdirectories: false);
			_debounceTimer = new Timer(OnDebounceTick, state: null, dueTime: Timeout.Infinite, period: Timeout.Infinite);

			return true;
		}
		catch (Exception exception)
		{
			Log.Debug(exception, "Failed to start the Lua workspace file watcher for '{Workspace}'.", _workspaceRootDirectoryPath);
			Dispose();

			return false;
		}
	}

	private FileSystemWatcher CreateWatcher(string filter, bool includeSubdirectories)
	{
		var watcher = new FileSystemWatcher(_workspaceRootDirectoryPath, filter)
		{
			IncludeSubdirectories = includeSubdirectories,
			NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.CreationTime | NotifyFilters.DirectoryName,
			InternalBufferSize = 64 * 1024
		};

		watcher.Created += (_, e) => QueueChange(e.FullPath, FileChangeKind.Created);
		watcher.Changed += (_, e) => QueueChange(e.FullPath, FileChangeKind.Changed);
		watcher.Deleted += (_, e) => QueueChange(e.FullPath, FileChangeKind.Deleted);
		watcher.Renamed += (_, e) =>
		{
			QueueChange(e.OldFullPath, FileChangeKind.Deleted);
			QueueChange(e.FullPath, FileChangeKind.Created);
		};
		watcher.Error += (_, e) => HandleWatcherError(e.GetException());
		watcher.EnableRaisingEvents = true;
		return watcher;
	}

	private void QueueChange(string filePath, FileChangeKind kind)
	{
		if (_isDisposed || string.IsNullOrEmpty(filePath))
			return;

		// Coalesce multiple events for the same path: a Created event keeps winning over later Changed
		// events for the same file, while a transient delete-then-create collapse is reported as a
		// single Changed notification because the path exists again but its contents may have changed.
		_pendingChanges.AddOrUpdate(filePath, kind, (_, existing) => Combine(existing, kind));
		_debounceTimer?.Change(DispatchDebounce, Timeout.InfiniteTimeSpan);
	}

	private static FileChangeKind Combine(FileChangeKind existing, FileChangeKind incoming)
	{
		if (existing == FileChangeKind.Created && incoming == FileChangeKind.Deleted)
			return FileChangeKind.Deleted;

		if (existing == FileChangeKind.Deleted && incoming == FileChangeKind.Created)
			return FileChangeKind.Changed;

		return existing == FileChangeKind.Created ? FileChangeKind.Created : incoming;
	}

	private void OnDebounceTick(object? _)
	{
		if (_isDisposed || _pendingChanges.IsEmpty)
			return;

		_ = DispatchPendingChangesAsync();
	}

	private async Task DispatchPendingChangesAsync()
	{
		Interlocked.Increment(ref _activeDispatchCount);

		try
		{
			await DispatchPendingChangesCoreAsync().ConfigureAwait(false);
		}
		finally
		{
			if (Interlocked.Decrement(ref _activeDispatchCount) == 0 && _isDisposed)
				DisposeDispatchResources();
		}
	}

	private async Task DispatchPendingChangesCoreAsync()
	{
		bool dispatchGateHeld = false;

		try
		{
			await _dispatchGate.WaitAsync(_lifetimeCts.Token).ConfigureAwait(false);
			dispatchGateHeld = true;

			if (_pendingChanges.IsEmpty)
				return;

			var batch = new FileChangeBatch();

			foreach (var entry in _pendingChanges)
			{
				if (_pendingChanges.TryRemove(entry.Key, out FileChangeKind removedKind))
					batch.Add(entry.Key, removedKind);
			}

			if (batch.Count == 0)
				return;

			await _dispatchAsync(batch, _lifetimeCts.Token).ConfigureAwait(false);
		}
		catch (OperationCanceledException)
		{ }
		catch (ObjectDisposedException)
		{ }
		catch (Exception exception)
		{
			Log.Debug(exception, "Lua workspace file watcher dispatch failed.");
		}
		finally
		{
			if (dispatchGateHeld)
			{
				try
				{
					_dispatchGate.Release();
				}
				catch (ObjectDisposedException)
				{ }
			}
		}
	}

	private void HandleWatcherError(Exception? exception)
	{
		if (_isDisposed)
			return;

		StopWatching();

		if (!_pendingChanges.IsEmpty)
			_ = DispatchPendingChangesAsync();

		if (Interlocked.Exchange(ref _watcherFailureReported, 1) != 0)
			return;

		Log.Warn(exception,
			"Lua workspace file watcher was disabled for '{Workspace}'. External workspace changes will no longer be forwarded to LuaLS until TombIDE is restarted.",
			_workspaceRootDirectoryPath);

		try
		{
			_watcherFailed?.Invoke(exception);
		}
		catch (Exception callbackException)
		{
			Log.Warn(callbackException, "Lua workspace watcher failure handler threw.");
		}
	}

	private void StopWatching()
	{
		TryDisposeAndClear(ref _apiDirectoryWatcher, nameof(_apiDirectoryWatcher));
		TryDisposeAndClear(ref _luaWatcher, nameof(_luaWatcher));
		TryDisposeAndClear(ref _configWatcher, nameof(_configWatcher));
		TryDisposeAndClear(ref _debounceTimer, nameof(_debounceTimer));
	}

	/// <summary>
	/// Releases all native file-system watchers and pending dispatch resources.
	/// </summary>
	public void Dispose()
	{
		if (_isDisposed)
			return;

		_isDisposed = true;

		StopWatching();

		try { _lifetimeCts.Cancel(); } catch (ObjectDisposedException) { }

		if (Volatile.Read(ref _activeDispatchCount) == 0)
			DisposeDispatchResources();
	}

	private void DisposeDispatchResources()
	{
		if (Interlocked.Exchange(ref _dispatchResourcesDisposed, 1) != 0)
			return;

		_lifetimeCts.Dispose();
		_dispatchGate.Dispose();
	}

	private static void TryDisposeAndClear<T>(ref T? disposable, string resourceName) where T : class, IDisposable
	{
		T? value = disposable;
		disposable = null;

		if (value is null)
			return;

		try
		{
			value.Dispose();
		}
		catch (Exception exception)
		{
			Log.Debug(exception, "Failed to dispose Lua workspace watcher resource '{ResourceName}'.", resourceName);
		}
	}

	internal void QueueChangeForTest(string filePath, FileChangeKind kind)
		=> QueueChange(filePath, kind);

	internal Task DispatchPendingChangesForTestAsync()
		=> DispatchPendingChangesAsync();

	internal void ReportErrorForTest(Exception? exception)
		=> HandleWatcherError(exception);

	internal bool HasActiveWatchers
		=> _apiDirectoryWatcher is not null || _luaWatcher is not null || _configWatcher is not null;
}

/// <summary>
/// Identifies the file-system change kind reported to LuaLS.
/// </summary>
internal enum FileChangeKind
{
	/// <summary>
	/// A file or directory was created.
	/// </summary>
	Created = 1,

	/// <summary>
	/// A file or directory changed in place.
	/// </summary>
	Changed = 2,

	/// <summary>
	/// A file or directory was deleted.
	/// </summary>
	Deleted = 3
}

/// <summary>
/// Represents a coalesced batch of workspace file changes ready to forward to LuaLS.
/// </summary>
internal sealed class FileChangeBatch
{
	private readonly List<(string Path, FileChangeKind Kind)> _entries = [];

	/// <summary>
	/// Gets the number of coalesced entries in the batch.
	/// </summary>
	public int Count => _entries.Count;

	/// <summary>
	/// Gets the coalesced file-change entries.
	/// </summary>
	public IReadOnlyList<(string Path, FileChangeKind Kind)> Entries => _entries;

	/// <summary>
	/// Adds a file-change entry to the batch.
	/// </summary>
	/// <param name="path">The changed local path.</param>
	/// <param name="kind">The coalesced change kind.</param>
	public void Add(string path, FileChangeKind kind) => _entries.Add((path, kind));
}
