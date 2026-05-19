using NLog;
using System.Collections.Concurrent;
using TombLib.Scripting.Lua.Objects;
using TombLib.Scripting.Lua.Services;
using TombLib.Scripting.Objects;

namespace TombLib.LanguageServer.Lua;

/// <summary>
/// Implements the Lua IntelliSense provider by synchronizing editor documents with LuaLS and caching its responses.
/// </summary>
public sealed partial class LuaLanguageServerIntellisenseProvider : ILuaIntellisenseProvider
{
	private static readonly Logger Log = LogManager.GetCurrentClassLogger();

	private static readonly TimeSpan DefaultRequestTimeout = TimeSpan.FromSeconds(10);
	private const int DefaultRequestTimeoutRestartThreshold = 2;
	private const int HardStartupFailureThreshold = 3;
	private const int MaxTrackedRequestOnlyDocuments = 16;

	internal static IReadOnlyList<WorkspaceWatchSpecification> WorkspaceWatchSpecifications { get; } = Array.AsReadOnly(
	[
		new WorkspaceWatchSpecification(".API", IncludeSubdirectories: false),
		new WorkspaceWatchSpecification("*.lua", IncludeSubdirectories: true),
		new WorkspaceWatchSpecification(".luarc.*", IncludeSubdirectories: false)
	]);

	private readonly string _workspaceRootDirectoryPath;
	private readonly ILanguageServerClient? _client;
	private readonly DocumentOperationScheduler _documentScheduler = new();
	private readonly LuaDocumentStore _documents = new();
	private readonly object _startupStateSyncRoot = new();
	private readonly object _requestTimeoutSyncRoot = new();
	private readonly ConcurrentDictionary<string, CancellationTokenSource> _semanticTokenRequests = new(StringComparer.OrdinalIgnoreCase);
	private readonly SemaphoreSlim _startLock = new(1, 1);
	private readonly CancellationTokenSource _disposeCts = new();
	private readonly TimeSpan _requestTimeout;
	private readonly int _requestTimeoutRestartThreshold;
	private readonly LuaWorkspaceChangeCoordinator _workspaceChanges;

	private bool _startupSucceeded;
	private int _consecutiveStartupFailures;
	private int _consecutiveRequestTimeouts;
	private long _timedOutRequestGeneration = -1;
	private long _restartRequestedGeneration = -1;
	private bool _permanentStartupFailureReported;
	private bool _transientStartupFailureReported;

	private int _disposeStarted;
	private volatile bool _isDisposed;

	/// <summary>
	/// Gets a value indicating whether IntelliSense requests can currently be served.
	/// </summary>
	public bool IsAvailable
	{
		get
		{
			if (_isDisposed || _client is null)
				return false;

			lock (_startupStateSyncRoot)
				return _consecutiveStartupFailures < HardStartupFailureThreshold;
		}
	}

	/// <summary>
	/// Gets a value indicating whether reference requests are supported by the active Lua language server.
	/// </summary>
	public bool SupportsReferences => !_isDisposed && _client is not null && _client.SupportsReferences;

	/// <summary>
	/// Gets a value indicating whether rename requests are supported by the active Lua language server.
	/// </summary>
	public bool SupportsRename => !_isDisposed && _client is not null && _client.SupportsRename;

	/// <summary>
	/// Gets a value indicating whether formatting requests are supported by the active Lua language server.
	/// </summary>
	public bool SupportsFormatting => !_isDisposed && _client is not null && _client.SupportsFormatting;

	/// <summary>
	/// Occurs when diagnostics for a tracked document change.
	/// </summary>
	/// <remarks>
	/// Diagnostics notifications may be delivered from background work. Consumers that touch UI controls must marshal to
	/// the UI thread. Once disposal begins, this event will not be raised again.
	/// </remarks>
	public event Action<string, IReadOnlyList<TextEditorDiagnostic>>? DiagnosticsUpdated;

	/// <summary>
	/// Occurs when semantic tokens for a tracked document change.
	/// </summary>
	/// <remarks>
	/// Semantic-token notifications may be delivered from background work. Consumers that touch UI controls must marshal
	/// to the UI thread. Once disposal begins, this event will not be raised again.
	/// </remarks>
	public event Action<string, IReadOnlyList<LuaSemanticToken>>? SemanticTokensUpdated;

	/// <summary>
	/// Occurs when repeated language-server startup failures should be surfaced to the user.
	/// </summary>
	/// <remarks>
	/// Startup-failure notifications may be delivered from background work. Consumers that touch UI controls must marshal
	/// to the UI thread. Once disposal begins, this event will not be raised again.
	/// </remarks>
	public event Action<LanguageServerStartupFailure>? StartupFailed;

	/// <summary>
	/// Occurs when the external workspace watcher becomes unavailable for the rest of the session.
	/// </summary>
	/// <remarks>
	/// Workspace-watcher notifications may be delivered from background work. Consumers that touch UI controls must
	/// marshal to the UI thread. Once disposal begins, this event will not be raised again.
	/// </remarks>
	public event Action<WorkspaceWatcherFailure>? WorkspaceWatcherFailed;

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

	internal LuaLanguageServerIntellisenseProvider(string workspaceRootDirectoryPath, ILanguageServerClient? client,
		TimeSpan? requestTimeout = null,
		int requestTimeoutRestartThreshold = DefaultRequestTimeoutRestartThreshold,
		Func<string, Func<FileChangeBatch, CancellationToken, Task>, Action<WorkspaceFileWatcher, Exception?>, WorkspaceFileWatcher>? workspaceFileWatcherFactory = null)
	{
		_workspaceRootDirectoryPath = LanguageServerPathHelper.NormalizeLocalPath(workspaceRootDirectoryPath);
		_client = client;
		_requestTimeout = requestTimeout ?? DefaultRequestTimeout;
		_requestTimeoutRestartThreshold = Math.Max(1, requestTimeoutRestartThreshold);
		_workspaceChanges = new LuaWorkspaceChangeCoordinator(
			_workspaceRootDirectoryPath,
			WorkspaceWatchSpecifications,
			workspaceFileWatcherFactory ?? CreateWorkspaceFileWatcher,
			() => _client,
			() => _isDisposed,
			EnsureStartedAsync,
			MarkWorkspaceTransportUnavailable,
			RaiseWorkspaceWatcherFailed);

		if (_client is not null)
		{
			_client.DiagnosticsPublished += HandleDiagnosticsPublished;
			_client.SemanticTokensRefreshRequested += HandleSemanticTokensRefreshRequested;
		}
	}

	private static ILanguageServerClient? CreateClient(string workspaceRootDirectoryPath, string? serverExecutablePath)
	{
		if (string.IsNullOrWhiteSpace(serverExecutablePath))
			return null;

		string normalizedRoot = LanguageServerPathHelper.NormalizeLocalPath(workspaceRootDirectoryPath);

		return new LanguageServerClient(normalizedRoot, serverExecutablePath, new LanguageServerClientOptions(
			() => LuaLanguageServerSettingsFactory.Create(normalizedRoot))
		{
			ClientCapabilitiesProvider = _ => LuaLanguageServerClientCapabilitiesFactory.Create(),
			InitializationOptionsProvider = _ => LuaLanguageServerInitializationOptionsFactory.Create()
		});
	}

	private static WorkspaceFileWatcher CreateWorkspaceFileWatcher(
		string workspaceRootDirectoryPath,
		Func<FileChangeBatch, CancellationToken, Task> dispatchAsync,
		Action<WorkspaceFileWatcher, Exception?> watcherFailed)
		=> new(workspaceRootDirectoryPath, dispatchAsync, WorkspaceWatchSpecifications, watcherFailed);

	/// <summary>
	/// Gets the latest diagnostics cached for the specified document.
	/// </summary>
	/// <param name="filePath">The local file path.</param>
	/// <returns>The cached diagnostics, or an empty list when none are available.</returns>
	public IReadOnlyList<TextEditorDiagnostic> GetDiagnostics(string filePath)
	{
		if (_isDisposed)
			return [];

		if (!LanguageServerPathHelper.TryNormalizeLocalPath(filePath, out string normalizedFilePath))
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

		if (!LanguageServerPathHelper.TryNormalizeLocalPath(filePath, out string normalizedFilePath))
			return [];

		return _documents.GetSemanticTokens(normalizedFilePath);
	}

	/// <summary>
	/// Opens a document in the provider and synchronizes its current content with LuaLS.
	/// </summary>
	/// <param name="filePath">The local file path.</param>
	/// <param name="content">The current document content.</param>
	public void OpenDocument(string filePath, string content)
	{
		if (!LanguageServerPathHelper.TryNormalizeLocalPath(filePath, out string normalizedFilePath))
			return;

		CancelQueuedDocumentUpdate(normalizedFilePath);

		ObserveBackgroundTask(SynchronizeDocumentAsync(normalizedFilePath, content,
			acquireOpenReference: true,
			acquireRequestReference: false,
			refreshSemanticTokens: true,
			CancellationToken.None), "Document open");
	}

	/// <summary>
	/// Pushes updated content for a document that is already tracked by the provider.
	/// </summary>
	/// <param name="filePath">The local file path.</param>
	/// <param name="content">The updated document content.</param>
	public void UpdateDocument(string filePath, string content)
	{
		if (!LanguageServerPathHelper.TryNormalizeLocalPath(filePath, out string normalizedFilePath))
			return;

		ObserveBackgroundTask(QueueLatestDocumentUpdateAsync(normalizedFilePath, content), "Document change");
	}

	/// <summary>
	/// Closes a tracked document and releases its server-side state when the last open reference disappears.
	/// </summary>
	/// <param name="filePath">The local file path.</param>
	public void CloseDocument(string filePath)
	{
		if (_isDisposed || _client is null || !LanguageServerPathHelper.TryNormalizeLocalPath(filePath, out string normalizedFilePath))
			return;

		CancelQueuedDocumentUpdate(normalizedFilePath);

		ObserveBackgroundTask(CloseDocumentAsync(normalizedFilePath, CancellationToken.None), "Document close");
	}

	private void MarkWorkspaceTransportUnavailable(long transportGeneration)
	{
		ILanguageServerClient? client = _client;

		if (client is null)
			return;

		try
		{
			if (client.TryMarkTransportUnhealthy(transportGeneration))
				MarkStartupTransportUnavailable();
		}
		catch (Exception exception)
		{
			Log.Debug(exception, "Failed to mark the Lua language server transport unhealthy after a workspace-watcher send failure.");
		}
	}

	private void RaiseDiagnosticsUpdated(string filePath, IReadOnlyList<TextEditorDiagnostic> diagnostics)
	{
		if (_isDisposed)
			return;

		InvokeSubscribersSafely(
			DiagnosticsUpdated,
			handler => ((Action<string, IReadOnlyList<TextEditorDiagnostic>>)handler)(filePath, diagnostics),
			"Lua diagnostics subscriber");
	}

	private void RaiseSemanticTokensUpdated(string filePath, IReadOnlyList<LuaSemanticToken> semanticTokens)
	{
		if (_isDisposed)
			return;

		InvokeSubscribersSafely(
			SemanticTokensUpdated,
			handler => ((Action<string, IReadOnlyList<LuaSemanticToken>>)handler)(filePath, semanticTokens),
			"Lua semantic-token subscriber");
	}

	private void RaiseStartupFailed(LanguageServerStartupFailure failure)
	{
		if (_isDisposed)
			return;

		InvokeSubscribersSafely(
			StartupFailed,
			handler => ((Action<LanguageServerStartupFailure>)handler)(failure),
			"Lua IntelliSense startup-failure subscriber");
	}

	private void RaiseWorkspaceWatcherFailed(WorkspaceWatcherFailure failure)
	{
		if (_isDisposed)
			return;

		InvokeSubscribersSafely(
			WorkspaceWatcherFailed,
			handler => ((Action<WorkspaceWatcherFailure>)handler)(failure),
			"Lua workspace-watcher subscriber");
	}

	private void InvokeSubscribersSafely(Delegate? handlers, Action<Delegate> invoke, string subscriberDescription)
	{
		if (handlers is null)
			return;

		foreach (Delegate handler in handlers.GetInvocationList())
		{
			if (_isDisposed)
				return;

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

	private int GetConsecutiveStartupFailures()
	{
		lock (_startupStateSyncRoot)
			return _consecutiveStartupFailures;
	}

	private bool GetStartupSucceeded()
	{
		lock (_startupStateSyncRoot)
			return _startupSucceeded;
	}

	private bool TryMarkStartupFailureReported(bool isPermanentFailure)
	{
		lock (_startupStateSyncRoot)
		{
			if (isPermanentFailure)
			{
				if (_permanentStartupFailureReported)
					return false;

				_permanentStartupFailureReported = true;
				return true;
			}

			if (_transientStartupFailureReported)
				return false;

			_transientStartupFailureReported = true;
			return true;
		}
	}

	private void MarkStartupTransportUnavailable()
	{
		lock (_startupStateSyncRoot)
			_startupSucceeded = false;
	}

	private void ResetStartupStateAfterSuccessfulStart()
	{
		lock (_startupStateSyncRoot)
		{
			_startupSucceeded = true;
			_consecutiveStartupFailures = 0;
			_transientStartupFailureReported = false;
			_permanentStartupFailureReported = false;
		}
	}

	private int RegisterStartupFailure()
	{
		lock (_startupStateSyncRoot)
		{
			_startupSucceeded = false;
			return ++_consecutiveStartupFailures;
		}
	}

	private void SetStartupSucceeded(bool startupSucceeded)
	{
		lock (_startupStateSyncRoot)
			_startupSucceeded = startupSucceeded;
	}
}
