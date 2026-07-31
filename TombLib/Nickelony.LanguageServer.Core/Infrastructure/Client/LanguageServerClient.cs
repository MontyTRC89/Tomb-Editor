using System.Diagnostics;
using System.Text.Json;

namespace Nickelony.LanguageServer.Core;

/// <summary>
/// Hosts a language-server process, performs the LSP handshake, and transports JSON-RPC requests and notifications.
/// </summary>
public sealed partial class LanguageServerClient : ILanguageServerClient
{
	private readonly ILogger _logger;

	private static readonly IReadOnlyList<string> EmptyCapabilityList = Array.AsReadOnly(Array.Empty<string>());

	private static readonly JsonSerializerOptions ConfigurationJsonSerializerOptions = new()
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase
	};

	// Host configuration and startup options.
	private readonly string _workspaceRootDirectoryPath;
	private readonly string _workspaceFolderName;
	private readonly string _serverExecutablePath;
	private readonly Func<object> _settingsProvider;
	private readonly Func<string, object?> _clientCapabilitiesProvider;
	private readonly Func<string, object?> _initializationOptionsProvider;
	private readonly TimeSpan _initializeTimeout;
	private readonly TimeSpan _shutdownRequestTimeout;
	private readonly TimeSpan _disposeWaitTimeout;
	private readonly object _settingsSnapshotSyncRoot = new();

	// Test seams.
	private readonly Func<Process, CancellationToken, Task>? _processStartedTestHook;
	private readonly Func<CancellationToken, Task>? _sessionActivatedTestHook;
	private readonly Func<CancellationToken, Task>? _beforeInitializeRequestTestHook;

	// Transport lifetime coordination.
	private readonly SemaphoreSlim _startLock = new(1, 1);
	private readonly CancellationTokenSource _lifetimeCts = new();
	private readonly object _observedBackgroundLoopSyncRoot = new();
	private readonly object _backgroundLoopSyncRoot = new();
	private readonly object _failedSessionDisposalSyncRoot = new();
	private readonly HashSet<Task> _observedBackgroundLoopTerminations = [];
	private Task _queuedFailedSessionDisposal = Task.CompletedTask;
	private long _transportGeneration;

	// Published transport state read by the public capability surface.
	private readonly object _publishedCapabilitySnapshotSyncRoot = new();
	private CachedSettingsSnapshot? _cachedSettingsSnapshot;
	private PublishedCapabilitySnapshot _publishedCapabilitySnapshot = CreateDefaultCapabilitySnapshot();
	private LanguageServerTransportSession? _activeSession;
	private volatile bool _isDisposed;
	private int _disposeStarted;

	/// <summary>
	/// Stores the current settings payload together with its serialized settings element.
	/// </summary>
	private sealed record CachedSettingsSnapshot(object SettingsPayload, JsonElement SettingsElement);

	/// <summary>
	/// Stores the immutable transport and capability state exposed through the public client surface.
	/// </summary>
	private sealed record PublishedCapabilitySnapshot(
		long TransportGeneration,
		bool IsReady,
		bool AcceptsServerCallbacks,
		TextDocumentSyncKind TextDocumentSyncKind,
		IReadOnlyList<string> SemanticTokenTypes,
		IReadOnlyList<string> SemanticTokenModifiers,
		bool SupportsCompletionResolve,
		bool? SupportsReferences,
		bool? SupportsRename,
		bool? SupportsFormatting,
		bool SupportsSemanticTokensFull,
		bool SupportsSemanticTokensDelta);

	/// <summary>
	/// Gets a value indicating whether the client finished initialization and can accept requests.
	/// </summary>
	public bool IsReady => Volatile.Read(ref _publishedCapabilitySnapshot).IsReady;

	/// <summary>
	/// Gets the current transport generation for the active language-server session.
	/// </summary>
	public long TransportGeneration => Volatile.Read(ref _publishedCapabilitySnapshot).TransportGeneration;

	/// <summary>
	/// Gets the text-document synchronization mode negotiated with the server.
	/// </summary>
	public TextDocumentSyncKind TextDocumentSyncKind => Volatile.Read(ref _publishedCapabilitySnapshot).TextDocumentSyncKind;

	/// <summary>
	/// Gets the semantic token types advertised by the server.
	/// The returned list is a read-only snapshot for the current transport generation.
	/// </summary>
	public IReadOnlyList<string> SemanticTokenTypes => Volatile.Read(ref _publishedCapabilitySnapshot).SemanticTokenTypes;

	/// <summary>
	/// Gets the semantic token modifiers advertised by the server.
	/// The returned list is a read-only snapshot for the current transport generation.
	/// </summary>
	public IReadOnlyList<string> SemanticTokenModifiers => Volatile.Read(ref _publishedCapabilitySnapshot).SemanticTokenModifiers;

	/// <summary>
	/// Gets a value indicating whether the server supports completion-item resolve requests.
	/// </summary>
	public bool SupportsCompletionResolve => Volatile.Read(ref _publishedCapabilitySnapshot).SupportsCompletionResolve;

	/// <summary>
	/// Gets a value indicating whether the server supports reference requests.
	/// </summary>
	public bool SupportsReferences => Volatile.Read(ref _publishedCapabilitySnapshot).SupportsReferences == true;

	/// <summary>
	/// Gets a value indicating whether the server supports rename requests.
	/// </summary>
	public bool SupportsRename => Volatile.Read(ref _publishedCapabilitySnapshot).SupportsRename == true;

	/// <summary>
	/// Gets a value indicating whether the server supports document formatting requests.
	/// </summary>
	public bool SupportsFormatting => Volatile.Read(ref _publishedCapabilitySnapshot).SupportsFormatting == true;

	/// <summary>
	/// Gets a value indicating whether the server supports full semantic token requests.
	/// </summary>
	public bool SupportsSemanticTokensFull => Volatile.Read(ref _publishedCapabilitySnapshot).SupportsSemanticTokensFull;

	/// <summary>
	/// Gets a value indicating whether the server supports semantic token delta responses.
	/// </summary>
	public bool SupportsSemanticTokensDelta => Volatile.Read(ref _publishedCapabilitySnapshot).SupportsSemanticTokensDelta;

	/// <summary>
	/// Initializes a new instance of the <see cref="LanguageServerClient"/> class.
	/// </summary>
	/// <param name="workspaceRootDirectoryPath">The workspace root directory to host.</param>
	/// <param name="serverExecutablePath">The language-server executable path.</param>
	/// <param name="options">Provides the host-specific settings, capabilities, and initialization payload factories.</param>
	/// <param name="logger">The logger instance, or <see langword="null"/> for a no-op logger.</param>
	public LanguageServerClient(string workspaceRootDirectoryPath, string serverExecutablePath, LanguageServerClientOptions options, ILogger<LanguageServerClient>? logger = null)
		: this(workspaceRootDirectoryPath, serverExecutablePath, options, logger, processStartedTestHook: null, sessionActivatedTestHook: null, beforeInitializeRequestTestHook: null)
	{ }

	/// <summary>
	/// Initializes a new instance of the <see cref="LanguageServerClient"/> class.
	/// </summary>
	/// <param name="workspaceRootDirectoryPath">The workspace root directory to host.</param>
	/// <param name="serverExecutablePath">The language-server executable path.</param>
	/// <param name="options">Provides the host-specific settings, capabilities, and initialization payload factories.</param>
	/// <param name="processStartedTestHook">A test seam invoked after the process starts but before session activation.</param>
	/// <param name="sessionActivatedTestHook">A test seam invoked after session activation but before handshake completion.</param>
	/// <param name="beforeInitializeRequestTestHook">A test seam invoked after the handshake timeout starts but before the initialize request is sent.</param>
	internal LanguageServerClient(string workspaceRootDirectoryPath, string serverExecutablePath, LanguageServerClientOptions options,
		ILogger<LanguageServerClient>? logger,
		Func<Process, CancellationToken, Task>? processStartedTestHook, Func<CancellationToken, Task>? sessionActivatedTestHook = null,
		Func<CancellationToken, Task>? beforeInitializeRequestTestHook = null)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(workspaceRootDirectoryPath);
		ArgumentException.ThrowIfNullOrWhiteSpace(serverExecutablePath);

		_logger = logger ?? NullLogger<LanguageServerClient>.Instance;

		string normalizedWorkspaceRootDirectoryPath = LanguageServerPathHelper.NormalizeLocalPath(workspaceRootDirectoryPath);

		_workspaceRootDirectoryPath = normalizedWorkspaceRootDirectoryPath;
		_workspaceFolderName = GetWorkspaceFolderName(normalizedWorkspaceRootDirectoryPath);
		_serverExecutablePath = serverExecutablePath;
		_settingsProvider = options.SettingsProvider;
		_clientCapabilitiesProvider = options.ClientCapabilitiesProvider;
		_initializationOptionsProvider = options.InitializationOptionsProvider;
		_initializeTimeout = options.InitializeTimeout;
		_shutdownRequestTimeout = options.ShutdownRequestTimeout;
		_disposeWaitTimeout = options.DisposeWaitTimeout;
		_processStartedTestHook = processStartedTestHook;
		_sessionActivatedTestHook = sessionActivatedTestHook;
		_beforeInitializeRequestTestHook = beforeInitializeRequestTestHook;

		CompletionResponseJsonConverter.InitializeLogger(_logger);

		if (OperatingSystem.IsWindows())
			ProcessJobObject.InitializeLogger(_logger);

		_diagnosticsPublishedSubscribers = new(
			static (handler, parameters) => handler(parameters),
			exception => _logger.LogWarning(exception, "Diagnostics handler threw; later subscribers will still be notified."));

		_semanticTokensRefreshSubscribers = new(
			static handler => handler(),
			exception => _logger.LogWarning(exception, "Semantic tokens refresh request handler threw; later subscribers will still be notified."));

		EnsureTransportBackgroundLoopsRunning(includeDiagnosticsPump: false);
	}
}
