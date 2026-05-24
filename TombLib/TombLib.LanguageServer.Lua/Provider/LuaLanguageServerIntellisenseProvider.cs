using NLog;
using System.Collections.Concurrent;
using TombLib.Scripting.Diagnostics;
using TombLib.Scripting.Core.Lua;

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
	private readonly TimeSpan _requestTimeout;
	private readonly int _requestTimeoutRestartThreshold;
	private readonly LuaWorkspaceChangeCoordinator _workspaceChanges;

	private readonly DocumentOperationScheduler _documentScheduler = new();
	private readonly LuaDocumentStore _documents = new();
	private readonly ConcurrentDictionary<string, CancellationTokenSource> _semanticTokenRequests = new(StringComparer.OrdinalIgnoreCase);

	private readonly object _startupStateSyncRoot = new();
	private readonly object _requestTimeoutSyncRoot = new();
	private readonly SemaphoreSlim _startLock = new(1, 1);
	private readonly CancellationTokenSource _disposeCts = new();

	private bool _startupSucceeded;
	private int _consecutiveStartupFailures;
	private int _consecutiveRequestTimeouts;
	private long _timedOutRequestGeneration = -1;
	private long _restartRequestedGeneration = -1;
	private bool _permanentStartupFailureReported;
	private bool _transientStartupFailureReported;

	private int _disposeStarted;
	private volatile bool _isDisposed;

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

	public bool SupportsReferences => !_isDisposed && _client is not null && _client.SupportsReferences;
	public bool SupportsRename => !_isDisposed && _client is not null && _client.SupportsRename;
	public bool SupportsFormatting => !_isDisposed && _client is not null && _client.SupportsFormatting;

	public event Action<string, IReadOnlyList<TextEditorDiagnostic>>? DiagnosticsUpdated;
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
	{
		return new(workspaceRootDirectoryPath, dispatchAsync, WorkspaceWatchSpecifications, watcherFailed);
	}

	public IReadOnlyList<TextEditorDiagnostic> GetDiagnostics(string filePath)
	{
		if (_isDisposed)
			return [];

		if (!LanguageServerPathHelper.TryNormalizeLocalPath(filePath, out string normalizedFilePath))
			return [];

		return _documents.GetDiagnostics(normalizedFilePath);
	}

	public IReadOnlyList<LuaSemanticToken> GetSemanticTokens(string filePath)
	{
		if (_isDisposed)
			return [];

		if (!LanguageServerPathHelper.TryNormalizeLocalPath(filePath, out string normalizedFilePath))
			return [];

		return _documents.GetSemanticTokens(normalizedFilePath);
	}

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

	public void UpdateDocument(string filePath, string content)
	{
		if (!LanguageServerPathHelper.TryNormalizeLocalPath(filePath, out string normalizedFilePath))
			return;

		ObserveBackgroundTask(QueueLatestDocumentUpdateAsync(normalizedFilePath, content), "Document change");
	}

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
