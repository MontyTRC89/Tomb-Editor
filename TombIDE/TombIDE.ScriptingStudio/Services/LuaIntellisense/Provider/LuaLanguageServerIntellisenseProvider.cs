#nullable enable

using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using TombLib.Scripting.Lua.Objects;
using TombLib.Scripting.Lua.Services;
using TombLib.Scripting.Objects;

namespace TombIDE.ScriptingStudio.Services.LuaIntellisense;

/// <summary>
/// Implements the Lua IntelliSense provider by synchronizing editor documents with LuaLS and caching its responses.
/// </summary>
internal sealed partial class LuaLanguageServerIntellisenseProvider : ILuaIntellisenseProvider
{
	private static readonly Logger Log = LogManager.GetCurrentClassLogger();

	private static readonly TimeSpan DefaultRequestTimeout = TimeSpan.FromSeconds(10);
	private const int DefaultRequestTimeoutRestartThreshold = 2;
	private const int HardStartupFailureThreshold = 3;

	private readonly string _workspaceApiDirectoryPath;
	private readonly string _workspaceRootDirectoryPath;
	private readonly ILuaLanguageServerClient? _client;
	private readonly LuaIntellisenseDocumentManager _documents = new();
	private readonly object _documentOperationSyncRoot = new();
	private readonly object _requestTimeoutSyncRoot = new();
	private readonly ConcurrentDictionary<string, CancellationTokenSource> _semanticTokenRequests = new(StringComparer.OrdinalIgnoreCase);
	private readonly SemaphoreSlim _startLock = new(1, 1);
	private readonly TimeSpan _requestTimeout;
	private readonly int _requestTimeoutRestartThreshold;
	private LuaWorkspaceFileWatcher? _workspaceFileWatcher;
	private Task _queuedDocumentOperation = Task.CompletedTask;

	private bool _startupSucceeded;
	private int _consecutiveStartupFailures;
	private int _consecutiveRequestTimeouts;
	private long _timedOutRequestGeneration = -1;
	private long _restartRequestedGeneration = -1;
	private bool _permanentStartupFailureReported;
	private bool _transientStartupFailureReported;
	private int _workspaceWatcherFailureReported;
	private volatile bool _isDisposed;

	/// <summary>
	/// Gets a value indicating whether IntelliSense requests can currently be served.
	/// </summary>
	public bool IsAvailable => !_isDisposed && _client is not null
		&& _consecutiveStartupFailures < HardStartupFailureThreshold;

	/// <summary>
	/// Occurs when diagnostics for a tracked document change.
	/// </summary>
	public event Action<string, IReadOnlyList<TextEditorDiagnostic>>? DiagnosticsUpdated;

	/// <summary>
	/// Occurs when semantic tokens for a tracked document change.
	/// </summary>
	public event Action<string, IReadOnlyList<LuaSemanticToken>>? SemanticTokensUpdated;

	/// <summary>
	/// Occurs when repeated language-server startup failures should be surfaced to the user.
	/// </summary>
	public event Action<LuaLanguageServerStartupFailure>? StartupFailed;

	/// <summary>
	/// Occurs when the external workspace watcher becomes unavailable for the rest of the session.
	/// </summary>
	public event Action<LuaWorkspaceWatcherFailure>? WorkspaceWatcherFailed;

	/// <summary>
	/// Initializes a new instance of the <see cref="LuaLanguageServerIntellisenseProvider"/> class.
	/// </summary>
	/// <param name="workspaceRootDirectoryPath">The root directory of the current Lua script workspace.</param>
	/// <param name="serverExecutablePath">The LuaLS executable path, or <see langword="null"/> when unavailable.</param>
	public LuaLanguageServerIntellisenseProvider(string workspaceRootDirectoryPath, string? serverExecutablePath)
		: this(workspaceRootDirectoryPath,
			CreateClient(workspaceRootDirectoryPath, serverExecutablePath),
			DefaultRequestTimeout,
			DefaultRequestTimeoutRestartThreshold)
	{ }

	internal LuaLanguageServerIntellisenseProvider(string workspaceRootDirectoryPath, ILuaLanguageServerClient? client,
		TimeSpan? requestTimeout = null, int requestTimeoutRestartThreshold = DefaultRequestTimeoutRestartThreshold)
	{
		_workspaceRootDirectoryPath = LuaLanguageServerPathHelper.NormalizeLocalPath(workspaceRootDirectoryPath);
		_workspaceApiDirectoryPath = Path.Combine(_workspaceRootDirectoryPath, ".API");
		_client = client;
		_requestTimeout = requestTimeout ?? DefaultRequestTimeout;
		_requestTimeoutRestartThreshold = Math.Max(1, requestTimeoutRestartThreshold);

		if (_client is not null)
		{
			_client.DiagnosticsPublished += HandleDiagnosticsPublished;
			_client.SemanticTokensRefreshRequested += HandleSemanticTokensRefreshRequested;
		}
	}

	private static ILuaLanguageServerClient? CreateClient(string workspaceRootDirectoryPath, string? serverExecutablePath)
	{
		if (string.IsNullOrWhiteSpace(serverExecutablePath))
			return null;

		string normalizedRoot = LuaLanguageServerPathHelper.NormalizeLocalPath(workspaceRootDirectoryPath);
		return new LuaLanguageServerClient(normalizedRoot, serverExecutablePath,
			() => LuaLanguageServerSettingsFactory.Create(normalizedRoot));
	}

	/// <summary>
	/// Gets the latest diagnostics cached for the specified document.
	/// </summary>
	/// <param name="filePath">The local file path.</param>
	/// <returns>The cached diagnostics, or an empty list when none are available.</returns>
	public IReadOnlyList<TextEditorDiagnostic> GetDiagnostics(string filePath)
	{
		if (_isDisposed)
			return [];

		if (!LuaLanguageServerPathHelper.TryNormalizeLocalPath(filePath, out string normalizedFilePath))
			return [];

		return _documents.GetDiagnostics(normalizedFilePath);
	}

	/// <summary>
	/// Gets the latest semantic tokens cached for the specified document.
	/// </summary>
	/// <param name="filePath">The local file path.</param>
	/// <returns>The cached semantic tokens, or an empty list when none are available.</returns>
	public IReadOnlyList<LuaSemanticToken> GetSemanticTokens(string filePath)
	{
		if (_isDisposed)
			return [];

		if (!LuaLanguageServerPathHelper.TryNormalizeLocalPath(filePath, out string normalizedFilePath))
			return [];

		return _documents.GetSemanticTokens(normalizedFilePath);
	}

	/// <summary>
	/// Opens a document in the provider and synchronizes its current content with LuaLS.
	/// </summary>
	/// <param name="filePath">The local file path.</param>
	/// <param name="content">The current document content.</param>
	public void OpenDocument(string filePath, string content)
		=> ObserveBackgroundTask(TrySynchronizeDocumentAsync(filePath, content, acquireOpenReference: true,
			refreshSemanticTokens: true, CancellationToken.None), "Document open");

	/// <summary>
	/// Pushes updated content for a document that is already tracked by the provider.
	/// </summary>
	/// <param name="filePath">The local file path.</param>
	/// <param name="content">The updated document content.</param>
	public void UpdateDocument(string filePath, string content)
		=> ObserveBackgroundTask(TrySynchronizeDocumentAsync(filePath, content, acquireOpenReference: false,
			refreshSemanticTokens: true, CancellationToken.None), "Document change");

	/// <summary>
	/// Closes a tracked document and releases its server-side state when the last open reference disappears.
	/// </summary>
	/// <param name="filePath">The local file path.</param>
	public void CloseDocument(string filePath)
	{
		if (_isDisposed || _client is null || !LuaLanguageServerPathHelper.TryNormalizeLocalPath(filePath, out string normalizedFilePath))
			return;

		ObserveBackgroundTask(CloseDocumentAsync(normalizedFilePath, CancellationToken.None), "Document close");
	}

	private async Task<bool> EnsureStartedAsync(CancellationToken cancellationToken)
	{
		if (_client is null || _consecutiveStartupFailures >= HardStartupFailureThreshold)
			return false;

		bool shieldCancellationForRestart = _startupSucceeded && !_client.IsReady;
		CancellationToken startupCancellationToken = shieldCancellationForRestart
			? CancellationToken.None
			: cancellationToken;

		// Fast path: once the client is healthy, keep the workspace watcher alive and avoid taking the startup lock.
		if (_startupSucceeded && _client.IsReady)
		{
			EnsureWorkspaceFileWatcherStarted();
			return true;
		}

		await _startLock.WaitAsync(startupCancellationToken).ConfigureAwait(false);

		try
		{
			IReadOnlyList<LuaDocumentSnapshot> documentsToReopen = [];

			// Re-check state after taking the lock so concurrent callers share the same restart/startup work.
			if (_consecutiveStartupFailures >= HardStartupFailureThreshold)
				return false;

			if (_startupSucceeded && _client.IsReady)
			{
				EnsureWorkspaceFileWatcherStarted();
				return true;
			}

			if (_startupSucceeded && !_client.IsReady)
			{
				Log.Info("Lua language server connection dropped, restarting and reopening tracked documents.");
				documentsToReopen = _documents.PrepareForRestart();
			}

			// Start the transport, then replay tracked documents when this is a restart rather than a cold start.
			_startupSucceeded = await _client.StartAsync(startupCancellationToken).ConfigureAwait(false);

			if (_startupSucceeded && documentsToReopen.Count > 0)
			{
				_startupSucceeded = await ReopenTrackedDocumentsAsync(documentsToReopen, startupCancellationToken).ConfigureAwait(false);

				if (!_startupSucceeded)
					Log.Warn("Failed to replay tracked documents after Lua language server restart.");
			}

			if (_startupSucceeded)
			{
				_consecutiveStartupFailures = 0;
				ResetRequestTimeoutTracking(_client.TransportGeneration);
				_transientStartupFailureReported = false;
				_permanentStartupFailureReported = false;
				EnsureWorkspaceFileWatcherStarted();
			}
			else
			{
				// Record repeated failures so IntelliSense eventually stops advertising availability until restart.
				_consecutiveStartupFailures++;
				bool isPermanentFailure = _consecutiveStartupFailures >= HardStartupFailureThreshold;

				if (isPermanentFailure)
				{
					Log.Error("Lua language server failed to start {Count} times consecutively for workspace '{Workspace}'; IntelliSense is now disabled until the editor is restarted.",
						_consecutiveStartupFailures, _workspaceRootDirectoryPath);
				}
				else
				{
					Log.Warn("Failed to start the Lua language server for workspace '{Workspace}' (attempt {Attempt}/{Threshold}).",
						_workspaceRootDirectoryPath, _consecutiveStartupFailures, HardStartupFailureThreshold);
				}

				ReportStartupFailure(isPermanentFailure);
			}

			return _startupSucceeded;
		}
		finally
		{
			_startLock.Release();
		}
	}

	private void EnsureWorkspaceFileWatcherStarted()
	{
		if (_workspaceFileWatcher is not null || _client is null || string.IsNullOrEmpty(_workspaceRootDirectoryPath))
			return;

		var watcher = new LuaWorkspaceFileWatcher(_workspaceRootDirectoryPath, DispatchWorkspaceFileChangesAsync, HandleWorkspaceWatcherFailed);

		if (!watcher.Start())
			return;

		_workspaceFileWatcher = watcher;
	}

	private async Task DispatchWorkspaceFileChangesAsync(FileChangeBatch batch, CancellationToken cancellationToken)
	{
		if (_client is null || _isDisposed || batch.Count == 0)
			return;

		// Normalize every path once, drop invalid entries, and track whether any change affects LuaLS configuration.
		var changes = new List<object>(batch.Count);
		bool shouldRefreshConfiguration = false;

		foreach ((string path, FileChangeKind kind) in batch.Entries)
		{
			if (!LuaLanguageServerPathHelper.TryNormalizeLocalPath(path, out string normalizedPath))
				continue;

			shouldRefreshConfiguration |= IsWorkspaceConfigurationPath(normalizedPath);

			changes.Add(new
			{
				uri = LuaLanguageServerPathHelper.CreateFileUri(normalizedPath),
				type = (int)kind
			});
		}

		if (changes.Count == 0)
			return;

		try
		{
			// Configuration-affecting files must refresh settings before the watched-files notification lands.
			if (shouldRefreshConfiguration)
			{
				await _client.SendNotificationAsync("workspace/didChangeConfiguration",
					new { settings = LuaLanguageServerSettingsFactory.Create(_workspaceRootDirectoryPath) },
					cancellationToken).ConfigureAwait(false);
			}

			await _client.SendNotificationAsync("workspace/didChangeWatchedFiles",
				new { changes }, cancellationToken).ConfigureAwait(false);
		}
		catch (OperationCanceledException)
		{ }
		catch (Exception exception)
		{
			Log.Debug(exception, "Failed to forward workspace file changes to the Lua language server.");
		}
	}

	private bool IsWorkspaceConfigurationPath(string normalizedPath)
	{
		if (string.Equals(normalizedPath, _workspaceApiDirectoryPath, StringComparison.OrdinalIgnoreCase))
			return true;

		string apiDirectoryPrefix = _workspaceApiDirectoryPath + Path.DirectorySeparatorChar;

		if (normalizedPath.StartsWith(apiDirectoryPrefix, StringComparison.OrdinalIgnoreCase))
			return true;

		string fileName = Path.GetFileName(normalizedPath);
		return string.Equals(fileName, ".luarc.json", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(fileName, ".luarc.jsonc", StringComparison.OrdinalIgnoreCase);
	}

	private void ReportStartupFailure(bool isPermanentFailure)
	{
		if (isPermanentFailure)
		{
			if (_permanentStartupFailureReported)
				return;

			_permanentStartupFailureReported = true;
		}
		else
		{
			if (_transientStartupFailureReported)
				return;

			_transientStartupFailureReported = true;
		}

		LuaLanguageServerStartupFailure failure = isPermanentFailure
			? new LuaLanguageServerStartupFailure(
				"The bundled Lua language server failed to start repeatedly and Lua IntelliSense is now disabled until TombIDE is restarted. See the log for technical details.",
				true)
			: new LuaLanguageServerStartupFailure(
				"The bundled Lua language server failed to start. Lua IntelliSense will remain unavailable until TombIDE can start the server successfully. TombIDE will retry automatically when Lua IntelliSense is requested again.",
				false);

		RaiseStartupFailed(failure);
	}

	private void HandleWorkspaceWatcherFailed(Exception? exception)
	{
		if (_isDisposed || Interlocked.Exchange(ref _workspaceWatcherFailureReported, 1) != 0)
			return;

		Log.Warn(exception,
			"Lua workspace watching is disabled for '{Workspace}'. Open editors will keep working, but external Lua workspace changes will not be forwarded until TombIDE is restarted.",
			_workspaceRootDirectoryPath);

		RaiseWorkspaceWatcherFailed(new LuaWorkspaceWatcherFailure(
			"The Lua workspace file watcher encountered an internal error and has been disabled for this session.\n\n" +
			"Lua IntelliSense will continue to work for files edited inside TombIDE, but external workspace changes - such as Git pull updates, generated .API files, or .luarc changes - will no longer be forwarded until TombIDE is restarted."));
	}

	private void RaiseDiagnosticsUpdated(string filePath, IReadOnlyList<TextEditorDiagnostic> diagnostics)
		=> InvokeSubscribersSafely(
			DiagnosticsUpdated,
			handler => ((Action<string, IReadOnlyList<TextEditorDiagnostic>>)handler)(filePath, diagnostics),
			"Lua diagnostics subscriber");

	private void RaiseSemanticTokensUpdated(string filePath, IReadOnlyList<LuaSemanticToken> semanticTokens)
		=> InvokeSubscribersSafely(
			SemanticTokensUpdated,
			handler => ((Action<string, IReadOnlyList<LuaSemanticToken>>)handler)(filePath, semanticTokens),
			"Lua semantic-token subscriber");

	private void RaiseStartupFailed(LuaLanguageServerStartupFailure failure)
		=> InvokeSubscribersSafely(
			StartupFailed,
			handler => ((Action<LuaLanguageServerStartupFailure>)handler)(failure),
			"Lua IntelliSense startup-failure subscriber");

	private void RaiseWorkspaceWatcherFailed(LuaWorkspaceWatcherFailure failure)
		=> InvokeSubscribersSafely(
			WorkspaceWatcherFailed,
			handler => ((Action<LuaWorkspaceWatcherFailure>)handler)(failure),
			"Lua workspace-watcher subscriber");

	private void InvokeSubscribersSafely(Delegate? handlers, Action<Delegate> invoke, string subscriberDescription)
	{
		if (handlers is null)
			return;

		foreach (Delegate handler in handlers.GetInvocationList())
		{
			try
			{
				invoke(handler);
			}
			catch (Exception exception)
			{
				Log.Warn(exception, "{SubscriberDescription} threw; later subscribers will still be notified.", subscriberDescription);
			}
		}
	}

	/// <summary>
	/// Releases the language-server client, workspace watcher, and any in-flight semantic-token requests.
	/// </summary>
	public void Dispose()
	{
		if (_isDisposed)
			return;

		_isDisposed = true;

		if (_client is not null)
		{
			_client.DiagnosticsPublished -= HandleDiagnosticsPublished;
			_client.SemanticTokensRefreshRequested -= HandleSemanticTokensRefreshRequested;
		}

		if (_workspaceFileWatcher is not null)
		{
			try
			{
				_workspaceFileWatcher.Dispose();
			}
			catch (Exception exception)
			{
				Log.Debug(exception, "Failed to dispose the Lua workspace file watcher.");
			}
		}

		CancelAllSemanticTokenRequests();
		_startLock.Dispose();
		_client?.Dispose();
	}
}
