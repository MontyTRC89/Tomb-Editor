using NLog;
using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace TombLib.Scripting.UI.Documents;

/// <summary>
/// Persists editor content to the backing file and keeps an optional backup in sync, running the
/// work on a background task. Requests are latest-request-wins: concurrent calls coalesce into a
/// single pass over the most recent content, discarding intermediate states.
/// </summary>
public sealed class ContentChangedWorker : IDisposable
{
	private static readonly Logger Log = LogManager.GetCurrentClassLogger();

	// Properties

	private volatile string _filePath = string.Empty;

	/// <summary>
	/// Gets or sets the path of the file the content is persisted to.
	/// </summary>
	public string FilePath
	{
		get => _filePath;
		set
		{
			if (_isDisposed)
				return;

			if (string.IsNullOrEmpty(value) || !value.Equals(_filePath, StringComparison.OrdinalIgnoreCase))
			{
				DeleteBackupFile();

				lock (_syncRoot)
				{
					_stateVersion++;
					_latestRequestId++;
					_hasPendingRequest = false;
				}
			}

			_filePath = value;
		}
	}

	private volatile bool _createBackupFiles;

	/// <summary>
	/// Gets or sets whether backup files are created when the content changes.
	/// </summary>
	public bool CreateBackupFiles
	{
		get => _createBackupFiles;
		set
		{
			if (_isDisposed)
				return;

			if (value == false)
				DeleteBackupFile();

			_createBackupFiles = value;
		}
	}

	/// <summary>
	/// Gets whether a persistence run is currently in progress.
	/// </summary>
	public bool IsBusy => _isDisposed ? false : _isBusy;

	// Events

	/// <summary>
	/// Raised when a persistence run completes.
	/// </summary>
	public event RunWorkerCompletedEventHandler? RunWorkerCompleted;

	// Fields

	private readonly Dispatcher _dispatcher;
	private readonly object _syncRoot = new();

	private Task? _processingTask;
	private volatile bool _isBusy;
	private volatile bool _isDisposed;
	private bool _hasPendingRequest;
	private int _latestRequestId;
	private int _stateVersion;
	private string _pendingEditorContent = string.Empty;
	private string _persistedContent = string.Empty;

	// Construction

	/// <summary>
	/// Initializes a new instance of the <see cref="ContentChangedWorker"/> class on the current dispatcher thread.
	/// </summary>
	public ContentChangedWorker()
	{
		_dispatcher = Dispatcher.CurrentDispatcher;
		FilePath = string.Empty;
		CreateBackupFiles = true;
	}

	// Disposal

	/// <summary>
	/// Invalidates any pending or in-flight persistence work and deletes the backup file, if one exists.
	/// An in-flight pass that already started writing is invalidated by the request version bump and
	/// removes its own backup once the write completes. Disposal is idempotent; a disposed worker must
	/// not be reused.
	/// </summary>
	public void Dispose()
	{
		if (_isDisposed)
			return;

		_isDisposed = true;

		lock (_syncRoot)
		{
			_hasPendingRequest = false;
			_isBusy = false;
			_latestRequestId++;
			_stateVersion++;
		}

		DeleteBackupFileByOriginalPath(FilePath);
	}

	// Public methods

	/// <summary>
	/// Schedules the given editor content to be persisted, coalescing concurrent requests into one run.
	/// </summary>
	/// <param name="editorContent">The editor content to persist.</param>
	public void Run(string editorContent)
	{
		if (_isDisposed || string.IsNullOrEmpty(FilePath))
			return;

		lock (_syncRoot)
		{
			if (_isDisposed)
				return;

			_pendingEditorContent = editorContent ?? string.Empty;
			_hasPendingRequest = true;
			_latestRequestId++;
			_isBusy = true;

			if (_processingTask is null || _processingTask.IsCompleted)
				_processingTask = ProcessLatestRequestAsync();
		}
	}

	/// <summary>
	/// Records the given content as the persisted baseline and cancels any pending persistence run.
	/// </summary>
	/// <param name="persistedContent">The persisted content.</param>
	public void SetPersistedContent(string persistedContent)
	{
		if (_isDisposed)
			return;

		int stateVersion;
		int requestId;
		string filePath = FilePath;

		lock (_syncRoot)
		{
			if (_isDisposed)
				return;

			_persistedContent = persistedContent ?? string.Empty;
			_pendingEditorContent = _persistedContent;
			_hasPendingRequest = false;
			stateVersion = ++_stateVersion;
			requestId = ++_latestRequestId;
			_isBusy = false;
		}

		_ = SynchronizeBackupStateAsync(filePath, _persistedContent, false, stateVersion, requestId);
	}

	/// <summary>
	/// Reports whether the given content differs from the persisted baseline.
	/// </summary>
	/// <param name="editorContent">The content to compare.</param>
	/// <returns><c>true</c> when the content changed; otherwise, <c>false</c>.</returns>
	public bool HasChanges(string editorContent)
	{
		if (_isDisposed)
			return false;

		lock (_syncRoot)
			return !string.Equals(editorContent ?? string.Empty, _persistedContent, StringComparison.Ordinal);
	}

	/// <summary>
	/// Creates a backup file for the given editor content.
	/// </summary>
	/// <param name="editorContent">The editor content to back up.</param>
	public void CreateBackupFile(string editorContent)
	{
		if (_isDisposed)
			return;

		_ = SynchronizeBackupStateAsync(FilePath, editorContent ?? string.Empty, true, CaptureStateVersion(), CaptureRequestId());
	}

	/// <summary>
	/// Deletes the backup file for the current file path, if one exists.
	/// </summary>
	public void DeleteBackupFile()
	{
		if (_isDisposed)
			return;

		DeleteBackupFileByOriginalPath(FilePath);
	}

	// Private methods

	private async Task ProcessLatestRequestAsync()
	{
		while (true)
		{
			string editorContent;
			string persistedContent;
			string filePath;
			int stateVersion;
			int requestId;

			lock (_syncRoot)
			{
				if (!_hasPendingRequest)
				{
					_isBusy = false;
					return;
				}

				editorContent = _pendingEditorContent;
				persistedContent = _persistedContent;
				filePath = _filePath;
				stateVersion = _stateVersion;
				requestId = _latestRequestId;
				_hasPendingRequest = false;
			}

			bool isChanged = !string.Equals(editorContent, persistedContent, StringComparison.Ordinal);
			Exception? error = null;

			try
			{
				await SynchronizeBackupStateAsync(filePath, editorContent, isChanged, stateVersion, requestId).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				Log.Warn(ex, "Failed to synchronize the backup state for '{Path}'.", filePath);
				error = ex;
				isChanged = true;
			}

			if (!IsLatestRequest(requestId, stateVersion))
				continue;

			await _dispatcher.InvokeAsync(() =>
			{
				if (!IsLatestRequest(requestId, stateVersion))
					return;

				lock (_syncRoot)
					_isBusy = false;

				RunWorkerCompleted?.Invoke(this, new RunWorkerCompletedEventArgs(isChanged, error, false));
			});

			return;
		}
	}

	private async Task SynchronizeBackupStateAsync(string originalFilePath, string editorContent, bool isChanged, int stateVersion, int requestId)
	{
		if (_isDisposed || string.IsNullOrWhiteSpace(originalFilePath))
			return;

		string backupFilePath = GetBackupFilePath(originalFilePath);

		if (!CreateBackupFiles || !isChanged)
		{
			DeleteBackupFileByBackupPath(backupFilePath);
			return;
		}

		await File.WriteAllTextAsync(backupFilePath, editorContent ?? string.Empty).ConfigureAwait(false);

		if (!IsLatestRequest(requestId, stateVersion) || !string.Equals(originalFilePath, FilePath, StringComparison.OrdinalIgnoreCase))
			DeleteBackupFileByBackupPath(backupFilePath);
	}

	private static string GetBackupFilePath(string originalFilePath)
		=> originalFilePath + ".backup";

	private void DeleteBackupFileByOriginalPath(string originalFilePath)
	{
		if (string.IsNullOrWhiteSpace(originalFilePath))
			return;

		DeleteBackupFileByBackupPath(GetBackupFilePath(originalFilePath));
	}

	private void DeleteBackupFileByBackupPath(string backupFilePath)
	{
		try
		{
			if (File.Exists(backupFilePath))
				File.Delete(backupFilePath);
		}
		catch (Exception ex)
		{
			Log.Warn(ex, "Failed to delete the backup file '{Path}' during cleanup.", backupFilePath);
		}
	}

	private bool IsLatestRequest(int requestId, int stateVersion)
	{
		lock (_syncRoot)
			return requestId == _latestRequestId && stateVersion == _stateVersion && !_hasPendingRequest;
	}

	private int CaptureStateVersion()
	{
		lock (_syncRoot)
			return _stateVersion;
	}

	private int CaptureRequestId()
	{
		lock (_syncRoot)
			return _latestRequestId;
	}
}
