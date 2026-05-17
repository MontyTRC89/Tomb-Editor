using NLog;
using StreamJsonRpc;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace TombLib.LanguageServer.Core;

/// <summary>
/// Hosts a language-server process, performs the LSP handshake, and transports JSON-RPC requests and notifications.
/// </summary>
public sealed partial class LanguageServerClient : ILanguageServerClient
{
	private static readonly Logger Log = LogManager.GetCurrentClassLogger();
	private static readonly IReadOnlyList<string> EmptyCapabilityList = Array.AsReadOnly(Array.Empty<string>());
	private static readonly JsonSerializerOptions ConfigurationJsonSerializerOptions = new()
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase
	};
	private readonly string _workspaceRootDirectoryPath;
	private readonly string _workspaceFolderName;
	private readonly string _serverExecutablePath;
	private readonly Func<object> _settingsProvider;
	private readonly Func<string, object?> _clientCapabilitiesProvider;
	private readonly Func<string, object?> _initializationOptionsProvider;
	private readonly Func<Process, CancellationToken, Task>? _processStartedTestHook;
	private readonly Func<CancellationToken, Task>? _sessionActivatedTestHook;
	private readonly Func<CancellationToken, Task>? _beforeInitializeRequestTestHook;
	private readonly TimeSpan _initializeTimeout;
	private readonly TimeSpan _shutdownRequestTimeout;
	private readonly TimeSpan _disposeWaitTimeout;
	private readonly SemaphoreSlim _startLock = new(1, 1);
	private readonly CancellationTokenSource _lifetimeCts = new();
	private readonly object _observedBackgroundLoopSyncRoot = new();
	private readonly object _backgroundLoopSyncRoot = new();
	private readonly object _failedSessionDisposalSyncRoot = new();
	private readonly HashSet<Task> _observedBackgroundLoopTerminations = [];
	private Task _queuedFailedSessionDisposal = Task.CompletedTask;
	private long _transportGeneration;
	private readonly object _publishedCapabilitySnapshotSyncRoot = new();
	private PublishedCapabilitySnapshot _publishedCapabilitySnapshot = CreateDefaultCapabilitySnapshot();
	private volatile bool _isDisposed;
	private int _disposeStarted;
	private LanguageServerTransportSession? _activeSession;

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
	public bool IsReady
	{
		get => Volatile.Read(ref _publishedCapabilitySnapshot).IsReady;
	}

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
	/// Gets a value indicating whether the server supports full semantic-token requests.
	/// </summary>
	public bool SupportsSemanticTokensFull => Volatile.Read(ref _publishedCapabilitySnapshot).SupportsSemanticTokensFull;

	/// <summary>
	/// Gets a value indicating whether the server supports semantic-token delta responses.
	/// </summary>
	public bool SupportsSemanticTokensDelta => Volatile.Read(ref _publishedCapabilitySnapshot).SupportsSemanticTokensDelta;

	/// <summary>
	/// Initializes a new instance of the <see cref="LanguageServerClient"/> class.
	/// </summary>
	/// <param name="workspaceRootDirectoryPath">The normalized workspace root directory.</param>
	/// <param name="serverExecutablePath">The language-server executable path.</param>
	/// <param name="options">Provides the host-specific settings, capabilities, and initialization payload factories.</param>
	public LanguageServerClient(string workspaceRootDirectoryPath, string serverExecutablePath, LanguageServerClientOptions options)
		: this(workspaceRootDirectoryPath, serverExecutablePath, options, processStartedTestHook: null, sessionActivatedTestHook: null, beforeInitializeRequestTestHook: null)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="LanguageServerClient"/> class.
	/// </summary>
	/// <param name="workspaceRootDirectoryPath">The normalized workspace root directory.</param>
	/// <param name="serverExecutablePath">The language-server executable path.</param>
	/// <param name="options">Provides the host-specific settings, capabilities, and initialization payload factories.</param>
	/// <param name="processStartedTestHook">A test seam invoked after the process starts but before session activation.</param>
	/// <param name="sessionActivatedTestHook">A test seam invoked after session activation but before handshake completion.</param>
	/// <param name="beforeInitializeRequestTestHook">A test seam invoked after the handshake timeout starts but before the initialize request is sent.</param>
	internal LanguageServerClient(string workspaceRootDirectoryPath, string serverExecutablePath, LanguageServerClientOptions options,
		Func<Process, CancellationToken, Task>? processStartedTestHook, Func<CancellationToken, Task>? sessionActivatedTestHook = null,
		Func<CancellationToken, Task>? beforeInitializeRequestTestHook = null)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(workspaceRootDirectoryPath);
		ArgumentException.ThrowIfNullOrWhiteSpace(serverExecutablePath);
		ArgumentNullException.ThrowIfNull(options);

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
		EnsureTransportBackgroundLoopsRunning(includeDiagnosticsPump: false);
	}

	/// <summary>
	/// Starts the language-server process and completes the initialize/initialized handshake.
	/// </summary>
	/// <param name="cancellationToken">A token that can cancel startup.</param>
	/// <returns><see langword="true"/> when startup succeeded; otherwise, <see langword="false"/>.</returns>
	public async Task<bool> StartAsync(CancellationToken cancellationToken)
	{
		ThrowIfDisposed(allowDisposed: false);

		if (IsReady)
			return true;

		using var disposeAwareStartupCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _lifetimeCts.Token);
		CancellationToken effectiveCancellationToken = disposeAwareStartupCts.Token;

		bool startLockHeld = false;
		Process? startedProcess = null;
		LanguageServerTransportSession? startedSession = null;
		bool sessionActivated = false;

		try
		{
			await _startLock.WaitAsync(effectiveCancellationToken).ConfigureAwait(false);
			startLockHeld = true;

			ThrowIfDisposed(allowDisposed: false);

			if (IsReady)
				return true;

			LanguageServerTransportSession? previousSession = DetachActiveSession();

			if (previousSession is not null)
			{
				Log.Info("Restarting language server transport by replacing generation {Generation}.", previousSession.Generation);
				await DisposeSessionAsync(previousSession).ConfigureAwait(false);
			}

			var startInfo = new ProcessStartInfo
			{
				FileName = _serverExecutablePath,
				WorkingDirectory = Path.GetDirectoryName(_serverExecutablePath) ?? Environment.CurrentDirectory,
				UseShellExecute = false,
				CreateNoWindow = true,
				RedirectStandardInput = true,
				RedirectStandardOutput = true,
				RedirectStandardError = true
			};

			startedProcess = new Process { StartInfo = startInfo, EnableRaisingEvents = true };

			if (!startedProcess.Start())
			{
				startedProcess.Dispose();
				startedProcess = null;
				return false;
			}

			if (OperatingSystem.IsWindows())
				ProcessJobObject.TryAssignProcess(startedProcess);

			if (_processStartedTestHook is not null)
				await _processStartedTestHook(startedProcess, effectiveCancellationToken).ConfigureAwait(false);

			LanguageServerTransportSession session = CreateTransportSession(startedProcess);
			startedSession = session;
			SetActiveSession(session);
			sessionActivated = true;

			Log.Info("Activated language server transport generation {Generation} for workspace '{Workspace}'; completing initialization handshake.",
				session.Generation,
				_workspaceRootDirectoryPath);

			if (_sessionActivatedTestHook is not null)
				await _sessionActivatedTestHook(effectiveCancellationToken).ConfigureAwait(false);

			EnsureTransportBackgroundLoopsRunning(includeDiagnosticsPump: true);

			// Complete the LSP handshake before marking the client ready for provider requests.
			using var initializeTimeout = CancellationTokenSource.CreateLinkedTokenSource(effectiveCancellationToken);
			initializeTimeout.CancelAfter(_initializeTimeout);

			if (_beforeInitializeRequestTestHook is not null)
				await _beforeInitializeRequestTestHook(initializeTimeout.Token).ConfigureAwait(false);

			InitializeResponse initializeResponse = await SendRequestCoreAsync<InitializeResponse>(session,
				"initialize", BuildInitializeParams(), initializeTimeout.Token, allowDisposed: false).ConfigureAwait(false);

			CaptureServerCapabilitiesForGeneration(session.Generation, initializeResponse);

			await SendNotificationCoreAsync(session, "initialized", new EmptyParams(), cancellationToken, allowDisposed: false).ConfigureAwait(false);

			await SendNotificationCoreAsync(session,
				"workspace/didChangeConfiguration",
				new DidChangeConfigurationParams(_settingsProvider()),
				cancellationToken,
				allowDisposed: false).ConfigureAwait(false);

			SetCapabilityReadinessForGeneration(session.Generation, true);

			Log.Info("Language server transport generation {Generation} completed initialization and is ready.", session.Generation);
			return true;
		}
		catch (OperationCanceledException) when (sessionActivated && !effectiveCancellationToken.IsCancellationRequested)
		{
			Log.Warn("Language server transport generation {Generation} did not complete initialization within {TimeoutMs} ms for workspace '{Workspace}'; tearing down the session and leaving the client not ready.",
				startedSession?.Generation ?? 0,
				(int)_initializeTimeout.TotalMilliseconds,
				_workspaceRootDirectoryPath);

			LogRecentStandardErrorContext(startedSession, "Initialization timeout");

			await DisposeActiveSessionAsync().ConfigureAwait(false);
			return false;
		}
		catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
		{
			if (!sessionActivated)
				await DisposeStartupSessionResourcesAsync(startedSession, startedProcess, isExpectedCancellation: true).ConfigureAwait(false);

			// Caller-driven cancellation should tear down the half-started process so later retries begin cleanly.
			await DisposeActiveSessionAsync().ConfigureAwait(false);
			throw;
		}
		catch (OperationCanceledException) when (_isDisposed)
		{
			if (!sessionActivated)
				await DisposeStartupSessionResourcesAsync(startedSession, startedProcess, isExpectedCancellation: true).ConfigureAwait(false);

			await DisposeActiveSessionAsync().ConfigureAwait(false);
			return false;
		}
		catch (Exception exception)
		{
			Log.Warn(exception,
				"Failed to start the language server (executable='{Executable}', workspace='{Workspace}', generation={Generation}, stage='{Stage}').",
				_serverExecutablePath,
				_workspaceRootDirectoryPath,
				startedSession?.Generation ?? 0,
				sessionActivated ? "initialization" : "startup");

			LogRecentStandardErrorContext(startedSession, sessionActivated ? "Initialization failure" : "Startup failure");

			if (!sessionActivated)
				await DisposeStartupSessionResourcesAsync(startedSession, startedProcess, isExpectedCancellation: false).ConfigureAwait(false);

			await DisposeActiveSessionAsync().ConfigureAwait(false);
			return false;
		}
		finally
		{
			if (startLockHeld)
				_startLock.Release();
		}
	}

	/// <summary>
	/// Sends a JSON-RPC notification to the language server.
	/// </summary>
	/// <param name="method">The LSP method name.</param>
	/// <param name="parameters">The notification payload.</param>
	/// <param name="cancellationToken">A token that can cancel the local dispatch attempt while the JSON-RPC notification task is still incomplete.</param>
	public async Task SendNotificationAsync(string method, object parameters, CancellationToken cancellationToken)
	{
		LanguageServerTransportSession session = GetRequiredReadySession(allowDisposed: false);

		try
		{
			await SendNotificationCoreAsync(session, method, parameters, cancellationToken, allowDisposed: false).ConfigureAwait(false);
		}
		catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
		{
			throw;
		}
		catch (Exception exception)
		{
			MarkTransportUnhealthyForGeneration(session.Generation);
			LogTransportOperationFailure("notification", method, session.Generation, exception);
			throw;
		}
	}

	/// <summary>
	/// Sends a JSON-RPC request to the language server and returns the typed response payload.
	/// </summary>
	/// <typeparam name="TResult">The typed response payload to deserialize.</typeparam>
	/// <param name="method">The LSP method name.</param>
	/// <param name="parameters">The request payload.</param>
	/// <param name="cancellationToken">A token that can cancel the request.</param>
	/// <returns>The typed response payload.</returns>
	public async Task<TResult> SendRequestAsync<TResult>(string method, object parameters, CancellationToken cancellationToken)
	{
		LanguageServerTransportSession session = GetRequiredReadySession(allowDisposed: false);
		TResult result;

		try
		{
			result = await SendRequestCoreAsync<TResult>(session, method, parameters, cancellationToken, allowDisposed: false).ConfigureAwait(false);
		}
		catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
		{
			throw;
		}
		catch (Exception exception)
		{
			MarkTransportUnhealthyForGeneration(session.Generation);
			LogTransportOperationFailure("request", method, session.Generation, exception);
			throw;
		}

		if (!CanAcceptRequestResultForSession(session))
		{
			Log.Debug(
				"Discarding language server request '{Method}' result from transport generation {Generation} because the transport was superseded or marked unavailable before completion.",
				method,
				session.Generation);
			throw new IOException("The language server transport changed before the request completed.");
		}

		return result;
	}

	/// <summary>
	/// Marks the current transport unhealthy so the provider restarts it on the next request.
	/// </summary>
	public void MarkTransportUnhealthy()
	{
		if (_isDisposed)
			return;

		PublishedCapabilitySnapshot snapshot = Volatile.Read(ref _publishedCapabilitySnapshot);
		MarkTransportUnhealthyForGeneration(snapshot.TransportGeneration);
	}

	/// <summary>
	/// Builds the initialize request payload for the active workspace.
	/// </summary>
	/// <returns>The initialize request payload.</returns>
	private object BuildInitializeParams() => new
	{
		processId = Environment.ProcessId,
		initializationOptions = _initializationOptionsProvider(_workspaceRootDirectoryPath),
		rootUri = LanguageServerPathHelper.CreateFileUri(_workspaceRootDirectoryPath),
		workspaceFolders = new[]
		{
			new
			{
				uri = LanguageServerPathHelper.CreateFileUri(_workspaceRootDirectoryPath),
				name = _workspaceFolderName
			}
		},
		capabilities = BuildClientCapabilitiesPayload()
	};

	private object BuildClientCapabilitiesPayload()
	{
		object? clientCapabilities = _clientCapabilitiesProvider(_workspaceRootDirectoryPath);

		if (clientCapabilities is null)
			return new { };

		JsonNode? capabilitiesNode = JsonSerializer.SerializeToNode(clientCapabilities);

		if (capabilitiesNode is not JsonObject capabilitiesObject)
			return clientCapabilities;

		EnforceDynamicRegistrationFalse(capabilitiesObject, "workspace", "didChangeWatchedFiles");

		string[] textDocumentCapabilityNames =
		[
			"completion",
			"hover",
			"definition",
			"references",
			"rename",
			"formatting",
			"signatureHelp",
			"semanticTokens",
			"publishDiagnostics"
		];

		for (int i = 0; i < textDocumentCapabilityNames.Length; i++)
			EnforceDynamicRegistrationFalse(capabilitiesObject, "textDocument", textDocumentCapabilityNames[i]);

		return capabilitiesObject;
	}

	private static void EnforceDynamicRegistrationFalse(JsonObject rootObject, string parentPropertyName, string capabilityPropertyName)
	{
		if (!TryGetJsonObjectProperty(rootObject, parentPropertyName, out JsonObject? parentObject))
			return;

		if (!TryGetJsonObjectProperty(parentObject, capabilityPropertyName, out JsonObject? capabilityObject))
			return;

		capabilityObject["dynamicRegistration"] = false;
	}

	private static bool TryGetJsonObjectProperty(JsonObject rootObject, string propertyName, [NotNullWhen(true)] out JsonObject? propertyObject)
	{
		propertyObject = null;

		if (!rootObject.TryGetPropertyValue(propertyName, out JsonNode? propertyNode))
			return false;

		propertyObject = propertyNode as JsonObject;
		return propertyObject is not null;
	}

	private static string GetWorkspaceFolderName(string workspaceRootDirectoryPath)
	{
		string trimmedWorkspaceRootDirectoryPath = workspaceRootDirectoryPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

		if (trimmedWorkspaceRootDirectoryPath.Length == 0)
			trimmedWorkspaceRootDirectoryPath = workspaceRootDirectoryPath;

		string folderName = Path.GetFileName(trimmedWorkspaceRootDirectoryPath);

		if (!string.IsNullOrEmpty(folderName))
			return folderName;

		string rootPath = Path.GetPathRoot(workspaceRootDirectoryPath) ?? string.Empty;
		string trimmedRootPath = rootPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

		if (!string.IsNullOrEmpty(trimmedRootPath))
			return trimmedRootPath;

		if (!string.IsNullOrEmpty(rootPath))
			return rootPath;

		return workspaceRootDirectoryPath;
	}

	/// <summary>
	/// Captures the server capabilities relevant to the host provider.
	/// </summary>
	/// <param name="initializeResponse">The initialize response received from the server.</param>
	private void CaptureServerCapabilities(InitializeResponse initializeResponse)
		=> CaptureServerCapabilitiesForGeneration(TransportGeneration, initializeResponse);

	/// <summary>
	/// Captures the server capabilities for one transport generation when it is still current.
	/// </summary>
	/// <param name="transportGeneration">The transport generation that owns the initialize response.</param>
	/// <param name="initializeResponse">The initialize response received from the server.</param>
	private void CaptureServerCapabilitiesForGeneration(long transportGeneration, InitializeResponse initializeResponse)
	{
		TextDocumentSyncKind textDocumentSyncKind = TextDocumentSyncKind.None;
		IReadOnlyList<string> semanticTokenTypes = EmptyCapabilityList;
		IReadOnlyList<string> semanticTokenModifiers = EmptyCapabilityList;
		bool supportsCompletionResolve = false;
		bool? supportsReferences = false;
		bool? supportsRename = false;
		bool? supportsFormatting = false;
		bool supportsSemanticTokensFull = false;
		bool supportsSemanticTokensDelta = false;

		ServerCapabilities capabilities = initializeResponse.Capabilities
			?? throw new NotSupportedException(
				"The language server did not advertise the full or incremental text synchronization required by the host provider.");

		TextDocumentSyncCapability textDocumentSync = capabilities.TextDocumentSync
			?? throw new NotSupportedException(
				"The language server did not advertise the full or incremental text synchronization required by the host provider.");

		if (textDocumentSync.Kind == TextDocumentSyncKind.None)
		{
			throw new NotSupportedException(
				"The language server did not advertise the full or incremental text synchronization required by the host provider.");
		}

		textDocumentSyncKind = textDocumentSync.Kind;

		supportsCompletionResolve = capabilities.CompletionProvider?.ResolveProvider == true;
		supportsReferences = capabilities.ReferencesProvider?.IsSupported == true;
		supportsRename = capabilities.RenameProvider?.IsSupported == true;
		supportsFormatting = capabilities.DocumentFormattingProvider?.IsSupported == true;

		if (capabilities.SemanticTokensProvider is { } semanticTokensProvider)
		{
			supportsSemanticTokensFull = semanticTokensProvider.Full?.IsSupported == true;
			supportsSemanticTokensDelta = semanticTokensProvider.Full?.SupportsDelta == true;

			if (semanticTokensProvider.Legend is { } legend)
			{
				semanticTokenTypes = Array.AsReadOnly(legend.TokenTypes ?? []);
				semanticTokenModifiers = Array.AsReadOnly(legend.TokenModifiers ?? []);
			}
		}

		PublishServerCapabilitiesForGeneration(
			transportGeneration,
			textDocumentSyncKind,
			semanticTokenTypes,
			semanticTokenModifiers,
			supportsCompletionResolve,
			supportsReferences,
			supportsRename,
			supportsFormatting,
			supportsSemanticTokensFull,
			supportsSemanticTokensDelta);
	}

	/// <summary>
	/// Creates a transport session from a started language-server process.
	/// </summary>
	/// <param name="process">The started language-server process.</param>
	/// <returns>The configured transport session.</returns>
	private LanguageServerTransportSession CreateTransportSession(Process process)
	{
		long generation = Interlocked.Increment(ref _transportGeneration);

		var session = new LanguageServerTransportSession(generation,
			process,
			process.StandardOutput.BaseStream,
			process.StandardInput.BaseStream);

		session.MessageHandler = CreateMessageHandler(session.ServerInputStream, session.ServerOutputStream);
		session.RpcTarget = new LanguageServerClientRpcTarget(this, generation);
		session.JsonRpc = CreateJsonRpc(session);
		session.RpcCompletionTask = session.JsonRpc.Completion;

		session.ProcessExitedHandler = (_, _) => Process_Exited(session);
		process.Exited += session.ProcessExitedHandler;
		session.StderrLoopTask = Task.Run(() => ReadStandardErrorLoopAsync(session), CancellationToken.None);
		session.JsonRpc.StartListening();

		return session;
	}

	/// <summary>
	/// Creates the JSON-RPC message handler for the transport streams.
	/// </summary>
	/// <param name="serverInputStream">The writable stream carrying host requests to the server.</param>
	/// <param name="serverOutputStream">The readable stream carrying server responses back to the host.</param>
	/// <returns>The configured message handler.</returns>
	private static HeaderDelimitedMessageHandler CreateMessageHandler(Stream serverInputStream, Stream serverOutputStream)
	{
		var formatter = new SystemTextJsonFormatter
		{
			JsonSerializerOptions = new JsonSerializerOptions
			{
				PropertyNameCaseInsensitive = true
			}
		};

		return new HeaderDelimitedMessageHandler(serverInputStream, serverOutputStream, formatter);
	}

	/// <summary>
	/// Creates the JSON-RPC transport bound to the supplied session.
	/// </summary>
	/// <param name="session">The transport session.</param>
	/// <returns>The configured JSON-RPC instance.</returns>
	private JsonRpc CreateJsonRpc(LanguageServerTransportSession session)
	{
		HeaderDelimitedMessageHandler messageHandler = session.MessageHandler
			?? throw new InvalidOperationException("The language server transport session is missing a JSON-RPC message handler.");

		LanguageServerClientRpcTarget rpcTarget = session.RpcTarget
			?? throw new InvalidOperationException("The language server transport session is missing a JSON-RPC callback target.");

		var jsonRpc = new JsonRpc(messageHandler, rpcTarget)
		{
			CancelLocallyInvokedMethodsWhenConnectionIsClosed = true
		};

		jsonRpc.Disconnected += (_, eventArgs) => JsonRpc_Disconnected(session, eventArgs);
		return jsonRpc;
	}

	/// <summary>
	/// Marks the supplied session as the currently active transport session.
	/// </summary>
	/// <param name="session">The active transport session.</param>
	private void SetActiveSession(LanguageServerTransportSession session)
	{
		lock (_publishedCapabilitySnapshotSyncRoot)
		{
			_activeSession = session;
			PublishCapabilitySnapshot(CreateDefaultCapabilitySnapshot(session.Generation));
		}
	}

	/// <summary>
	/// Gets the active transport session once initialization completed.
	/// </summary>
	/// <param name="allowDisposed">Whether disposed-state checks should be skipped.</param>
	/// <returns>The active transport session.</returns>
	private LanguageServerTransportSession GetRequiredReadySession(bool allowDisposed)
	{
		ThrowIfDisposed(allowDisposed);

		lock (_publishedCapabilitySnapshotSyncRoot)
		{
			PublishedCapabilitySnapshot snapshot = Volatile.Read(ref _publishedCapabilitySnapshot);
			LanguageServerTransportSession? session = _activeSession;

			if (!snapshot.IsReady)
				throw new IOException("The language server transport is not ready.");

			if (session is null || session.Generation != snapshot.TransportGeneration)
				throw new IOException("The language server transport is not available.");

			return session;
		}
	}

	/// <summary>
	/// Gets the active transport session or throws when none is available.
	/// </summary>
	/// <param name="allowDisposed">Whether disposed-state checks should be skipped.</param>
	/// <returns>The active transport session.</returns>
	private LanguageServerTransportSession GetRequiredActiveSession(bool allowDisposed)
	{
		ThrowIfDisposed(allowDisposed);

		LanguageServerTransportSession? session = Volatile.Read(ref _activeSession);

		if (session is null)
			throw new IOException("The language server transport is not available.");

		return session;
	}

	/// <summary>
	/// Detaches the current active transport session and clears ready-state tracking.
	/// </summary>
	/// <returns>The detached session, if one existed.</returns>
	private LanguageServerTransportSession? DetachActiveSession()
	{
		lock (_publishedCapabilitySnapshotSyncRoot)
		{
			LanguageServerTransportSession? session = _activeSession;

			if (session is null)
				return null;

			_activeSession = null;
			PublishCapabilitySnapshot(CreateDefaultCapabilitySnapshot());
			return session;
		}
	}

	/// <summary>
	/// Detaches the supplied session only when it is still the current active transport session.
	/// </summary>
	/// <param name="session">The session to detach.</param>
	/// <returns><see langword="true"/> when the session was detached; otherwise, <see langword="false"/>.</returns>
	private bool TryDetachSpecificActiveSession(LanguageServerTransportSession session)
	{
		lock (_publishedCapabilitySnapshotSyncRoot)
		{
			if (!ReferenceEquals(_activeSession, session))
				return false;

			_activeSession = null;
			PublishCapabilitySnapshot(CreateDefaultCapabilitySnapshot());
			return true;
		}
	}

	/// <summary>
	/// Disposes the currently active transport session, if any.
	/// </summary>
	private async Task DisposeActiveSessionAsync()
	{
		LanguageServerTransportSession? session = DetachActiveSession();

		if (session is not null)
			await DisposeSessionAsync(session).ConfigureAwait(false);
	}

	/// <summary>
	/// Handles unexpected server process exit for one transport session.
	/// </summary>
	/// <param name="session">The session whose process exited.</param>
	private void Process_Exited(LanguageServerTransportSession session)
	{
		if (!TryDetachSpecificActiveSession(session))
			return;

		DisposeFailedSessionInBackground(session, "process exit");

		int? exitCode = TryReadProcessExitCode(session.Process);

		if (!_isDisposed)
		{
			Log.Warn("Language server process for transport generation {Generation} in workspace '{Workspace}' exited unexpectedly{ExitCodeSuffix}; the host will recreate the session on the next startup attempt.",
				session.Generation,
				_workspaceRootDirectoryPath,
				exitCode is not null ? $" with code {exitCode.Value}" : string.Empty);

			LogRecentStandardErrorContext(session, "Unexpected process exit");
		}
	}

	/// <summary>
	/// Reports whether the supplied session is still the active session.
	/// </summary>
	/// <param name="session">The session to inspect.</param>
	/// <returns><see langword="true"/> when the session is still active.</returns>
	private bool IsCurrentSession(LanguageServerTransportSession session)
		=> ReferenceEquals(Volatile.Read(ref _activeSession), session);

	/// <summary>
	/// Reports whether one completed request result still belongs to the active ready transport session.
	/// </summary>
	/// <param name="session">The session that produced the request result.</param>
	/// <returns><see langword="true"/> when the request result still belongs to the active ready transport; otherwise, <see langword="false"/>.</returns>
	private bool CanAcceptRequestResultForSession(LanguageServerTransportSession session)
	{
		lock (_publishedCapabilitySnapshotSyncRoot)
		{
			PublishedCapabilitySnapshot snapshot = Volatile.Read(ref _publishedCapabilitySnapshot);
			return snapshot.IsReady
				&& snapshot.TransportGeneration == session.Generation
				&& ReferenceEquals(_activeSession, session);
		}
	}

	/// <summary>
	/// Reports whether the supplied transport generation still belongs to the active session.
	/// </summary>
	/// <param name="transportGeneration">The transport generation to inspect.</param>
	/// <returns><see langword="true"/> when the generation is still active.</returns>
	private bool IsActiveTransportGeneration(long transportGeneration)
		=> transportGeneration != 0 && transportGeneration == TransportGeneration;

	/// <summary>
	/// Reports whether the supplied transport generation is still the ready session that may publish server callbacks.
	/// </summary>
	/// <param name="transportGeneration">The transport generation to inspect.</param>
	/// <returns><see langword="true"/> when the generation still owns the published ready snapshot.</returns>
	private bool CanAcceptServerCallbacksForGeneration(long transportGeneration)
	{
		PublishedCapabilitySnapshot snapshot = Volatile.Read(ref _publishedCapabilitySnapshot);
		return snapshot.AcceptsServerCallbacks
			&& transportGeneration != 0
			&& snapshot.TransportGeneration == transportGeneration;
	}

	/// <summary>
	/// Handles JSON-RPC transport disconnection for the active session.
	/// </summary>
	/// <param name="session">The disconnected session.</param>
	/// <param name="eventArgs">The disconnect event arguments.</param>
	private void JsonRpc_Disconnected(LanguageServerTransportSession session, JsonRpcDisconnectedEventArgs eventArgs)
	{
		if (!TryDetachSpecificActiveSession(session))
		{
			Log.Debug("Ignoring disconnect from stale language server transport generation {Generation}: {Description}",
				session.Generation,
				eventArgs.Description);

			return;
		}

		DisposeFailedSessionInBackground(session, "transport disconnect");

		if (_isDisposed)
		{
			Log.Debug("Language server JSON-RPC transport generation {Generation} for workspace '{Workspace}' disconnected during client disposal: {Description}",
				session.Generation,
				_workspaceRootDirectoryPath,
				eventArgs.Description);

			return;
		}

		if (eventArgs.Reason == DisconnectedReason.LocallyDisposed)
		{
			Log.Info("Language server JSON-RPC transport generation {Generation} for workspace '{Workspace}' disconnected during expected local shutdown: {Description}",
				session.Generation,
				_workspaceRootDirectoryPath,
				eventArgs.Description);

			return;
		}

		Exception? exception = eventArgs.Exception;

		if (exception is not null)
		{
			Log.Warn(exception,
				"Language server JSON-RPC transport generation {Generation} for workspace '{Workspace}' disconnected unexpectedly (reason={Reason}); the host will recreate the session on the next startup attempt: {Description}",
				session.Generation,
				_workspaceRootDirectoryPath,
				eventArgs.Reason,
				eventArgs.Description);

			LogRecentStandardErrorContext(session, "Unexpected transport disconnect");

			return;
		}

		Log.Warn("Language server JSON-RPC transport generation {Generation} for workspace '{Workspace}' disconnected unexpectedly (reason={Reason}); the host will recreate the session on the next startup attempt: {Description}",
			session.Generation,
			_workspaceRootDirectoryPath,
			eventArgs.Reason,
			eventArgs.Description);

		LogRecentStandardErrorContext(session, "Unexpected transport disconnect");
	}

	/// <summary>
	/// Disposes a detached failed session without blocking the disconnect or process-exit callback thread.
	/// </summary>
	/// <param name="session">The detached failed session.</param>
	/// <param name="reason">The failure reason used for diagnostics when cleanup itself fails.</param>
	private void DisposeFailedSessionInBackground(LanguageServerTransportSession session, string reason)
	{
		lock (_failedSessionDisposalSyncRoot)
		{
			Task previousDisposal = _queuedFailedSessionDisposal;
			_queuedFailedSessionDisposal = Task.Run(
				() => DisposeFailedSessionAfterAsync(previousDisposal, session, reason));
		}
	}

	private async Task DisposeFailedSessionAfterAsync(Task previousDisposal, LanguageServerTransportSession session, string reason)
	{
		try
		{
			await previousDisposal.ConfigureAwait(false);
		}
		catch
		{
			// Later cleanup should still run even if earlier detached cleanup failed.
		}

		await DisposeFailedSessionAsync(session, reason).ConfigureAwait(false);
	}

	private async Task DisposeFailedSessionAsync(LanguageServerTransportSession session, string reason)
	{
		try
		{
			await DisposeSessionAsync(session).ConfigureAwait(false);
		}
		catch (Exception exception)
		{
			Log.Debug(exception,
				"Failed to dispose detached language server transport generation {Generation} after {Reason}.",
				session.Generation,
				reason);
		}
	}

	private Task GetQueuedFailedSessionDisposalTask()
	{
		lock (_failedSessionDisposalSyncRoot)
		{
			return _queuedFailedSessionDisposal;
		}
	}

	private async Task WaitForQueuedFailedSessionDisposalsAsync(Stopwatch disposeStopwatch)
	{
		while (true)
		{
			Task pendingDisposal = GetQueuedFailedSessionDisposalTask();

			await WaitWithDisposeBudgetAsync(
				pendingDisposal,
				disposeStopwatch,
				"Detached language server session cleanup did not complete within {TimeoutMs} ms during disposal.",
				"Detached language server session cleanup raised exceptions.").ConfigureAwait(false);

			if (ReferenceEquals(pendingDisposal, GetQueuedFailedSessionDisposalTask()))
				return;
		}
	}

	/// <summary>
	/// Logs one public request or notification failure with transport-generation context.
	/// </summary>
	/// <param name="operationKind">The operation kind, such as request or notification.</param>
	/// <param name="method">The JSON-RPC method name.</param>
	/// <param name="generation">The session generation that owned the operation.</param>
	/// <param name="exception">The transport failure.</param>
	private void LogTransportOperationFailure(string operationKind, string method, long generation, Exception exception)
	{
		if (_isDisposed)
			return;

		if (generation != 0 && generation != TransportGeneration)
		{
			Log.Debug(exception,
				"Language server {OperationKind} '{Method}' failed on stale transport generation {Generation} for workspace '{Workspace}'.",
				operationKind,
				method,
				generation,
				_workspaceRootDirectoryPath);
			return;
		}

		Log.Warn(exception,
			"Language server {OperationKind} '{Method}' failed on transport generation {Generation} for workspace '{Workspace}'; the host will recover by recreating the session when needed.",
			operationKind,
			method,
			generation,
			_workspaceRootDirectoryPath);
	}

	/// <summary>
	/// Logs recent stderr context for one session when a transport failure path needs more diagnostics.
	/// </summary>
	/// <param name="session">The session whose recent stderr should be reported.</param>
	/// <param name="context">The failure context label.</param>
	private static void LogRecentStandardErrorContext(LanguageServerTransportSession? session, string context)
	{
		string? recentStandardError = session?.GetRecentStandardErrorSummary();

		if (string.IsNullOrWhiteSpace(recentStandardError))
			return;

		Log.Warn("{Context} recent language server stderr: {RecentStandardError}", context, recentStandardError);
	}

	/// <summary>
	/// Starts one background loop task and attaches immediate fault observation.
	/// </summary>
	/// <param name="backgroundLoop">The background loop delegate.</param>
	/// <param name="loopName">The logical loop name used for diagnostics.</param>
	/// <returns>The started background loop task.</returns>
	private Task StartObservedBackgroundLoop(
		Func<Task> backgroundLoop,
		string loopName,
		bool markTransportUnhealthyOnUnexpectedTermination)
	{
		Task task = Task.Run(backgroundLoop, CancellationToken.None);
		ObserveBackgroundLoop(task, loopName, markTransportUnhealthyOnUnexpectedTermination);
		return task;
	}

	/// <summary>
	/// Ensures the callback pump and, optionally, the diagnostics pump are running for the current client instance.
	/// Completed or faulted pumps are recreated so a transport restart can recover callback delivery.
	/// </summary>
	/// <param name="includeDiagnosticsPump">Whether the diagnostics pump should also be ensured.</param>
	private void EnsureTransportBackgroundLoopsRunning(bool includeDiagnosticsPump)
	{
		lock (_backgroundLoopSyncRoot)
		{
			if (_callbackPumpTask.IsCompleted)
			{
				_callbackPumpTask = StartObservedBackgroundLoop(
					PumpCallbacksAsync,
					"callback dispatcher",
					markTransportUnhealthyOnUnexpectedTermination: true);
			}

			if (includeDiagnosticsPump && _diagnosticsPumpTask.IsCompleted)
			{
				_diagnosticsPumpTask = StartObservedBackgroundLoop(
					PumpDiagnosticsAsync,
					"diagnostics pump",
					markTransportUnhealthyOnUnexpectedTermination: true);
			}
		}
	}

	/// <summary>
	/// Observes one background loop task so unexpected termination is logged immediately while the client is still active.
	/// </summary>
	/// <param name="task">The background loop task to observe.</param>
	/// <param name="loopName">The logical loop name used for diagnostics.</param>
	private void ObserveBackgroundLoop(
		Task task,
		string loopName,
		bool markTransportUnhealthyOnUnexpectedTermination)
	{
		_ = task.ContinueWith(
			static (completedTask, state) =>
			{
				BackgroundLoopObservation observation = (BackgroundLoopObservation)state!;
				observation.Owner.LogUnexpectedBackgroundLoopTermination(
					completedTask,
					observation.LoopName,
					observation.MarkTransportUnhealthyOnUnexpectedTermination);
			},
			new BackgroundLoopObservation(this, loopName, markTransportUnhealthyOnUnexpectedTermination),
			CancellationToken.None,
			TaskContinuationOptions.ExecuteSynchronously,
			TaskScheduler.Default);
	}

	/// <summary>
	/// Logs unexpected background loop termination while the client is still active.
	/// </summary>
	/// <param name="task">The completed background loop task.</param>
	/// <param name="loopName">The logical loop name used for diagnostics.</param>
	private void LogUnexpectedBackgroundLoopTermination(
		Task task,
		string loopName,
		bool markTransportUnhealthyOnUnexpectedTermination)
	{
		if (_isDisposed || _lifetimeCts.IsCancellationRequested)
			return;

		if (!TryMarkObservedBackgroundLoopTermination(task))
			return;

		if (markTransportUnhealthyOnUnexpectedTermination)
			MarkTransportUnhealthy();

		if (task.IsFaulted && task.Exception is { } aggregateException)
		{
			Exception loggedException = aggregateException.Flatten().InnerExceptions.Count == 1
				? aggregateException.Flatten().InnerExceptions[0]
				: aggregateException.Flatten();

			Log.Warn(loggedException, "Language server background loop '{LoopName}' terminated unexpectedly while the client was still active.", loopName);
			return;
		}

		if (task.IsCanceled)
		{
			Log.Warn("Language server background loop '{LoopName}' was canceled unexpectedly while the client was still active.", loopName);
			return;
		}

		Log.Warn("Language server background loop '{LoopName}' completed unexpectedly while the client was still active.", loopName);
	}

	/// <summary>
	/// Records that one background loop termination has already been logged before disposal.
	/// </summary>
	/// <param name="task">The completed background loop task.</param>
	/// <returns><see langword="true"/> when the task was newly marked; otherwise, <see langword="false"/>.</returns>
	private bool TryMarkObservedBackgroundLoopTermination(Task task)
	{
		lock (_observedBackgroundLoopSyncRoot)
		{
			return _observedBackgroundLoopTerminations.Add(task);
		}
	}

	/// <summary>
	/// Reports whether the supplied background loop task already logged its unexpected termination before disposal.
	/// </summary>
	/// <param name="task">The completed background loop task.</param>
	/// <returns><see langword="true"/> when the task already logged unexpected termination; otherwise, <see langword="false"/>.</returns>
	private bool WasObservedBackgroundLoopTermination(Task task)
	{
		lock (_observedBackgroundLoopSyncRoot)
		{
			return _observedBackgroundLoopTerminations.Contains(task);
		}
	}

	/// <summary>
	/// Stores the state needed to log one observed background loop termination.
	/// </summary>
	/// <param name="Owner">The owning client.</param>
	/// <param name="LoopName">The logical loop name.</param>
	private readonly record struct BackgroundLoopObservation(
		LanguageServerClient Owner,
		string LoopName,
		bool MarkTransportUnhealthyOnUnexpectedTermination);

	/// <summary>
	/// Creates the default published capability snapshot for one transport generation.
	/// </summary>
	/// <param name="transportGeneration">The transport generation to publish.</param>
	/// <param name="isReady">Whether the transport completed initialization.</param>
	/// <returns>The default capability snapshot.</returns>
	private static PublishedCapabilitySnapshot CreateDefaultCapabilitySnapshot(long transportGeneration = 0, bool isReady = false)
		=> new(
			transportGeneration,
			isReady,
			transportGeneration != 0 && isReady,
			TextDocumentSyncKind.None,
			EmptyCapabilityList,
			EmptyCapabilityList,
			false,
			null,
			null,
			null,
			false,
			false);

	/// <summary>
	/// Publishes one immutable capability snapshot to concurrent readers with a single atomic swap.
	/// </summary>
	/// <param name="snapshot">The snapshot to publish.</param>
	private void PublishCapabilitySnapshot(PublishedCapabilitySnapshot snapshot)
		=> Volatile.Write(ref _publishedCapabilitySnapshot, snapshot);

	/// <summary>
	/// Publishes negotiated server capabilities when the target generation is still active.
	/// </summary>
	private void PublishServerCapabilitiesForGeneration(
		long transportGeneration,
		TextDocumentSyncKind textDocumentSyncKind,
		IReadOnlyList<string> semanticTokenTypes,
		IReadOnlyList<string> semanticTokenModifiers,
		bool supportsCompletionResolve,
		bool? supportsReferences,
		bool? supportsRename,
		bool? supportsFormatting,
		bool supportsSemanticTokensFull,
		bool supportsSemanticTokensDelta)
	{
		lock (_publishedCapabilitySnapshotSyncRoot)
		{
			PublishedCapabilitySnapshot currentSnapshot = Volatile.Read(ref _publishedCapabilitySnapshot);

			if (currentSnapshot.TransportGeneration != transportGeneration)
				return;

			PublishCapabilitySnapshot(new PublishedCapabilitySnapshot(
				currentSnapshot.TransportGeneration,
				currentSnapshot.IsReady,
				currentSnapshot.AcceptsServerCallbacks,
				textDocumentSyncKind,
				semanticTokenTypes,
				semanticTokenModifiers,
				supportsCompletionResolve,
				supportsReferences,
				supportsRename,
				supportsFormatting,
				supportsSemanticTokensFull,
				supportsSemanticTokensDelta));
		}
	}

	/// <summary>
	/// Marks one transport generation unhealthy only when it still owns the published snapshot.
	/// </summary>
	/// <param name="transportGeneration">The generation to mark unhealthy.</param>
	private void MarkTransportUnhealthyForGeneration(long transportGeneration)
	{
		if (!TryGetPublishedCapabilitySnapshotForGeneration(transportGeneration, out PublishedCapabilitySnapshot snapshot))
			return;

		lock (_publishedCapabilitySnapshotSyncRoot)
		{
			PublishedCapabilitySnapshot currentSnapshot = Volatile.Read(ref _publishedCapabilitySnapshot);

			if (currentSnapshot.TransportGeneration != transportGeneration)
				return;

			PublishCapabilitySnapshot(new PublishedCapabilitySnapshot(
				transportGeneration,
				IsReady: false,
				AcceptsServerCallbacks: false,
				TextDocumentSyncKind.None,
				EmptyCapabilityList,
				EmptyCapabilityList,
				false,
				null,
				null,
				null,
				false,
				false));
		}

		if (snapshot.IsReady && transportGeneration != 0)
			Log.Warn("Marked language server transport generation {Generation} unhealthy; the host will restart it before the next public request.", transportGeneration);
	}

	/// <summary>
	/// Updates only the readiness flag while keeping the rest of the published capability snapshot aligned.
	/// </summary>
	/// <param name="isReady">Whether the active transport is ready.</param>
	private void SetCapabilityReadiness(bool isReady)
	{
		PublishedCapabilitySnapshot snapshot = Volatile.Read(ref _publishedCapabilitySnapshot);
		SetCapabilityReadinessForGeneration(snapshot.TransportGeneration, isReady);
	}

	/// <summary>
	/// Updates the readiness flag for one transport generation when it still owns the published snapshot.
	/// </summary>
	/// <param name="transportGeneration">The generation whose readiness should change.</param>
	/// <param name="isReady">Whether the active transport is ready.</param>
	private void SetCapabilityReadinessForGeneration(long transportGeneration, bool isReady)
	{
		lock (_publishedCapabilitySnapshotSyncRoot)
		{
			PublishedCapabilitySnapshot snapshot = Volatile.Read(ref _publishedCapabilitySnapshot);

			if (snapshot.TransportGeneration != transportGeneration)
				return;

			if (!isReady)
			{
				PublishCapabilitySnapshot(CreateDefaultCapabilitySnapshot(snapshot.TransportGeneration));
				return;
			}

			PublishCapabilitySnapshot(snapshot with { IsReady = true, AcceptsServerCallbacks = true });
		}
	}

	/// <summary>
	/// Gets the published capability snapshot when it still belongs to the requested transport generation.
	/// </summary>
	/// <param name="transportGeneration">The generation that should own the snapshot.</param>
	/// <param name="snapshot">Receives the snapshot when the generation matches.</param>
	/// <returns><see langword="true"/> when the generation still owns the snapshot; otherwise, <see langword="false"/>.</returns>
	private bool TryGetPublishedCapabilitySnapshotForGeneration(long transportGeneration, out PublishedCapabilitySnapshot snapshot)
	{
		snapshot = Volatile.Read(ref _publishedCapabilitySnapshot);
		return snapshot.TransportGeneration == transportGeneration;
	}

	/// <summary>
	/// Reads standard-error output from the language-server process and logs non-empty lines.
	/// </summary>
	/// <param name="session">The transport session whose process stderr should be read.</param>
	private async Task ReadStandardErrorLoopAsync(LanguageServerTransportSession session)
	{
		try
		{
			while (!_isDisposed)
			{
				Process? process = session.Process;

				if (process is null || process.HasExited)
					break;

				string? line = await process.StandardError.ReadLineAsync(_lifetimeCts.Token).ConfigureAwait(false);

				if (line is null)
					break;

				if (!string.IsNullOrWhiteSpace(line))
				{
					session.RecordStandardErrorLine(line);
					Log.Debug("[LS stderr] {Line}", line);
				}
			}
		}
		catch (OperationCanceledException)
		{ }
		catch (Exception exception)
		{
			Log.Debug(exception, "Language server stderr read loop failed.");
		}
	}

	/// <summary>
	/// Logs a window message or log message notification from the language server.
	/// </summary>
	/// <param name="method">The originating method name.</param>
	/// <param name="parameters">The message payload.</param>
	private static void LogServerMessage(string method, WindowMessageParams parameters)
	{
		string? messageText = parameters.Message;

		if (string.IsNullOrWhiteSpace(messageText))
			return;

		int messageType = parameters.Type ?? 4;

		switch (messageType)
		{
			case 1: Log.Error("[LS {Method}] {Message}", method, messageText); break;
			case 2: Log.Warn("[LS {Method}] {Message}", method, messageText); break;
			case 3: Log.Info("[LS {Method}] {Message}", method, messageText); break;
			default: Log.Debug("[LS {Method}] {Message}", method, messageText); break;
		}
	}

	/// <summary>
	/// Attempts to read the exit code for a process that may already be disposed.
	/// </summary>
	/// <param name="process">The process to inspect.</param>
	/// <returns>The exit code, or <see langword="null"/> when unavailable.</returns>
	private static int? TryReadProcessExitCode(Process? process)
	{
		if (process is null)
			return null;

		try
		{
			return process.HasExited ? process.ExitCode : null;
		}
		catch (InvalidOperationException)
		{
			// Includes ObjectDisposedException; the process state is no longer accessible.
			return null;
		}
	}

	/// <summary>
	/// Disposes one transport session and all of its associated resources.
	/// </summary>
	/// <param name="session">The session to dispose.</param>
	private async Task DisposeSessionAsync(LanguageServerTransportSession session)
	{
		Task? rpcCompletionTask = session.RpcCompletionTask;
		Task? stderrLoopTask = session.StderrLoopTask;

		try
		{
			if (session.Process is not null && session.ProcessExitedHandler is not null)
				session.Process.Exited -= session.ProcessExitedHandler;
		}
		catch
		{
			// Ignore event detach failures.
		}

		try
		{
			if (session.Process is not null && !session.Process.HasExited)
			{
				Log.Info("Attempting graceful shutdown for language server transport generation {Generation} in workspace '{Workspace}'.",
					session.Generation,
					_workspaceRootDirectoryPath);

				await TrySendShutdownAsync(session).ConfigureAwait(false);
				await TrySendExitNotificationAsync(session).ConfigureAwait(false);

				if (!session.Process.HasExited)
				{
					Log.Warn("Language server transport generation {Generation} in workspace '{Workspace}' did not exit after graceful shutdown; forcing process termination.",
						session.Generation,
						_workspaceRootDirectoryPath);
					LogRecentStandardErrorContext(session, "Forced process termination");
					session.Process.Kill(true);
				}
			}
		}
		catch (Exception exception)
		{
			Log.Warn(exception, "Disposing language server transport generation {Generation} raised exceptions while stopping the server process.", session.Generation);
		}
		finally
		{
			try
			{
				session.JsonRpc?.Dispose();
			}
			catch
			{
				// Ignore JSON-RPC disposal failures.
			}

			try
			{
				if (session.MessageHandler is not null)
					await session.MessageHandler.DisposeAsync().ConfigureAwait(false);
			}
			catch
			{
				// Ignore message-handler disposal failures.
			}

			CleanupSessionResources(session);
		}

		await WaitForBackgroundLoopsAsync(rpcCompletionTask, stderrLoopTask).ConfigureAwait(false);
	}

	/// <summary>
	/// Disposes startup resources that failed before the session became active.
	/// </summary>
	/// <param name="session">The startup session, when transport construction completed.</param>
	/// <param name="process">The spawned process, when startup reached process launch.</param>
	/// <param name="isExpectedCancellation">Whether startup is being cleaned up due to expected cancellation rather than a startup failure.</param>
	private async Task DisposeStartupSessionResourcesAsync(LanguageServerTransportSession? session, Process? process, bool isExpectedCancellation)
	{
		if (session is not null)
		{
			await DisposeSessionAsync(session).ConfigureAwait(false);
			return;
		}

		if (process is null)
			return;

		try
		{
			if (!process.HasExited)
			{
				if (isExpectedCancellation)
					Log.Debug("Startup cleanup is terminating the language server process after cancellation before session activation completed.");
				else
					Log.Warn("Startup cleanup is forcing language server process termination before session activation completed.");

				process.Kill(true);
			}
		}
		catch (Exception exception)
		{
			if (isExpectedCancellation)
				Log.Debug(exception, "Startup cleanup failed while terminating the language server process after cancellation before session activation completed.");
			else
				Log.Warn(exception, "Startup cleanup failed while terminating the language server process after startup did not complete.");
		}
		finally
		{
			try
			{
				process.Dispose();
			}
			catch
			{
				// Ignore startup cleanup failures.
			}
		}
	}

	/// <summary>
	/// Releases the unmanaged and managed resources owned by a transport session.
	/// </summary>
	/// <param name="session">The session whose resources should be cleared.</param>
	private static void CleanupSessionResources(LanguageServerTransportSession session)
	{
		try
		{
			session.ServerInputStream.Dispose();
			session.ServerOutputStream.Dispose();
			session.Process?.Dispose();
		}
		catch
		{
			// Ignore stream disposal failures.
		}

		session.RpcCompletionTask = null;
		session.StderrLoopTask = null;
		session.JsonRpc = null;
		session.MessageHandler = null;
		session.RpcTarget = null;
	}

	/// <summary>
	/// Attempts to send a graceful shutdown request to the server.
	/// </summary>
	/// <param name="session">The session being shut down.</param>
	private async Task TrySendShutdownAsync(LanguageServerTransportSession session)
	{
		using var shutdownTimeout = new CancellationTokenSource(_shutdownRequestTimeout);
		Task<object?>? shutdownTask = null;

		try
		{
			shutdownTask = SendRequestCoreAsync<object?>(session, "shutdown", new EmptyParams(), shutdownTimeout.Token, allowDisposed: true);
			await shutdownTask.WaitAsync(_shutdownRequestTimeout).ConfigureAwait(false);
		}
		catch (TimeoutException)
		{
			shutdownTimeout.Cancel();

			if (shutdownTask is not null)
				ObserveLateShutdownTask(shutdownTask);

			Log.Warn("Language server transport generation {Generation} in workspace '{Workspace}' did not acknowledge shutdown within {TimeoutMs} ms; continuing teardown.",
				session.Generation,
				_workspaceRootDirectoryPath,
				(int)_shutdownRequestTimeout.TotalMilliseconds);
			LogRecentStandardErrorContext(session, "Shutdown timeout");
		}
		catch (OperationCanceledException) when (shutdownTimeout.IsCancellationRequested)
		{
			if (shutdownTask is not null)
				ObserveLateShutdownTask(shutdownTask);

			Log.Warn("Language server transport generation {Generation} in workspace '{Workspace}' did not acknowledge shutdown within {TimeoutMs} ms; continuing teardown.",
				session.Generation,
				_workspaceRootDirectoryPath,
				(int)_shutdownRequestTimeout.TotalMilliseconds);
			LogRecentStandardErrorContext(session, "Shutdown timeout");
		}
		catch (Exception exception)
		{
			Log.Warn(exception,
				"Sending the language server shutdown request during disposal failed for transport generation {Generation} in workspace '{Workspace}'; continuing teardown.",
				session.Generation,
				_workspaceRootDirectoryPath);
			LogRecentStandardErrorContext(session, "Shutdown failure");
		}
	}

	private static void ObserveLateShutdownTask(Task task)
	{
		_ = task.ContinueWith(
			static completedTask =>
			{
				if (completedTask.Exception is not null)
					_ = completedTask.Exception;
			},
			CancellationToken.None,
			TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.NotOnRanToCompletion,
			TaskScheduler.Default);
	}

	/// <summary>
	/// Attempts to queue the exit notification to the server as a best-effort local dispatch.
	/// </summary>
	/// <param name="session">The session being shut down.</param>
	private async Task TrySendExitNotificationAsync(LanguageServerTransportSession session)
	{
		try
		{
			await SendNotificationCoreAsync(session, "exit", new { }, CancellationToken.None, allowDisposed: true).ConfigureAwait(false);
		}
		catch (Exception exception)
		{
			Log.Debug(exception, "Sending the language server exit notification during disposal failed for transport generation {Generation}; continuing teardown.", session.Generation);
		}
	}

	/// <summary>
	/// Waits for the background read and stderr loops to finish within the dispose timeout.
	/// </summary>
	/// <param name="readLoopTask">The main JSON-RPC completion task.</param>
	/// <param name="stderrLoopTask">The standard-error read loop task.</param>
	private async Task WaitForBackgroundLoopsAsync(Task? readLoopTask, Task? stderrLoopTask)
	{
		Task combined = Task.WhenAll(
			readLoopTask ?? Task.CompletedTask,
			stderrLoopTask ?? Task.CompletedTask);

		try
		{
			await combined.WaitAsync(_disposeWaitTimeout).ConfigureAwait(false);
		}
		catch (TimeoutException)
		{
			Log.Warn("Language server background loops did not complete within {TimeoutMs} ms during disposal.",
				(int)_disposeWaitTimeout.TotalMilliseconds);
		}
		catch (Exception exception)
		{
			bool loggedSpecificLoop = false;
			loggedSpecificLoop |= TryLogCanceledBackgroundLoop(readLoopTask, "JSON-RPC completion");
			loggedSpecificLoop |= TryLogCanceledBackgroundLoop(stderrLoopTask, "stderr read");
			loggedSpecificLoop |= TryLogFaultedBackgroundLoop(readLoopTask, "JSON-RPC completion");
			loggedSpecificLoop |= TryLogFaultedBackgroundLoop(stderrLoopTask, "stderr read");

			if (!loggedSpecificLoop)
			{
				Log.Warn(exception, "Language server background loop failed during disposal.");
			}
		}
	}

	/// <summary>
	/// Logs cancellation for one background loop when disposal canceled it intentionally.
	/// </summary>
	/// <param name="task">The loop task to inspect.</param>
	/// <param name="loopName">The logical loop name for diagnostics.</param>
	/// <returns><see langword="true"/> when cancellation was logged; otherwise, <see langword="false"/>.</returns>
	private static bool TryLogCanceledBackgroundLoop(Task? task, string loopName)
	{
		if (task?.IsCanceled != true)
			return false;

		Log.Debug("Language server background loop '{LoopName}' was canceled during disposal.", loopName);
		return true;
	}

	/// <summary>
	/// Logs the failure for one background loop when it faulted during disposal.
	/// </summary>
	/// <param name="task">The loop task to inspect.</param>
	/// <param name="loopName">The logical loop name for diagnostics.</param>
	/// <returns><see langword="true"/> when a fault was logged; otherwise, <see langword="false"/>.</returns>
	private static bool TryLogFaultedBackgroundLoop(Task? task, string loopName)
	{
		if (task?.IsFaulted != true || task.Exception is not { } aggregateException)
			return false;

		Exception loggedException = aggregateException.Flatten().InnerExceptions.Count == 1
			? aggregateException.Flatten().InnerExceptions[0]
			: aggregateException.Flatten();

		Log.Warn(loggedException, "Language server background loop '{LoopName}' failed during disposal.", loopName);
		return true;
	}

	/// <summary>
	/// Begins disposal for the client if it has not already started.
	/// </summary>
	/// <returns><see langword="true"/> when the current caller should continue disposal; otherwise, <see langword="false"/>.</returns>
	private bool TryBeginDispose()
	{
		if (Interlocked.Exchange(ref _disposeStarted, 1) != 0)
			return false;

		_isDisposed = true;
		return true;
	}

	/// <summary>
	/// Stops the language-server process, completes pending requests, and releases transport resources.
	/// </summary>
	/// <returns>A task that completes when disposal finishes.</returns>
	private async Task DisposeCoreAsync()
	{
		var disposeStopwatch = Stopwatch.StartNew();

		_callbackSignal.Writer.TryComplete();
		_diagnosticsSignal.Writer.TryComplete();

		try
		{
			_lifetimeCts.Cancel();
		}
		catch (ObjectDisposedException)
		{ }

		await WaitWithDisposeBudgetAsync(
			DisposeActiveSessionAsync(),
			disposeStopwatch,
			"Disposing language server timed out after {TimeoutMs} ms; abandoning background tasks.",
			"Disposing language server raised exceptions.").ConfigureAwait(false);

		await WaitForQueuedFailedSessionDisposalsAsync(disposeStopwatch).ConfigureAwait(false);

		if (!ReferenceEquals(_diagnosticsPumpTask, Task.CompletedTask))
		{
			await WaitWithDisposeBudgetAsync(
				_diagnosticsPumpTask,
				disposeStopwatch,
				"Language server diagnostics pump did not complete within {TimeoutMs} ms during disposal.",
				"Disposing the language server diagnostics pump raised exceptions.").ConfigureAwait(false);
		}

		await WaitWithDisposeBudgetAsync(
			_callbackPumpTask,
			disposeStopwatch,
			"Language server callback dispatcher did not complete within {TimeoutMs} ms during disposal.",
			"Disposing the language server callback dispatcher raised exceptions.").ConfigureAwait(false);

		await DisposeStartLockAsync(GetRemainingDisposeBudget(disposeStopwatch)).ConfigureAwait(false);
		_lifetimeCts.Dispose();
	}

	/// <summary>
	/// Waits for one teardown task while spending from the caller's remaining disposal budget.
	/// </summary>
	/// <param name="task">The teardown task to await.</param>
	/// <param name="disposeStopwatch">Tracks the elapsed disposal time.</param>
	/// <param name="timeoutMessage">The warning logged when the remaining disposal budget is exhausted.</param>
	/// <param name="exceptionMessage">The warning logged when the teardown task faults.</param>
	private async Task WaitWithDisposeBudgetAsync(Task task, Stopwatch disposeStopwatch, string timeoutMessage, string exceptionMessage)
	{
		TimeSpan remainingDisposeBudget = GetRemainingDisposeBudget(disposeStopwatch);

		if (remainingDisposeBudget <= TimeSpan.Zero)
		{
			if (!task.IsCompleted)
				Log.Warn(timeoutMessage, (int)_disposeWaitTimeout.TotalMilliseconds);

			return;
		}

		try
		{
			await task.WaitAsync(remainingDisposeBudget).ConfigureAwait(false);
		}
		catch (TimeoutException)
		{
			Log.Warn(timeoutMessage, (int)_disposeWaitTimeout.TotalMilliseconds);
		}
		catch (Exception exception)
		{
			if (WasObservedBackgroundLoopTermination(task))
				return;

			Log.Warn(exception, exceptionMessage);
		}
	}

	/// <summary>
	/// Gets the remaining disposal budget shared by the teardown stages.
	/// </summary>
	/// <param name="disposeStopwatch">Tracks the elapsed disposal time.</param>
	/// <returns>The remaining shared disposal budget.</returns>
	private TimeSpan GetRemainingDisposeBudget(Stopwatch disposeStopwatch)
	{
		TimeSpan remainingDisposeBudget = _disposeWaitTimeout - disposeStopwatch.Elapsed;
		return remainingDisposeBudget > TimeSpan.Zero ? remainingDisposeBudget : TimeSpan.Zero;
	}

	/// <summary>
	/// Waits for the startup gate to become quiescent and then disposes it.
	/// </summary>
	/// <returns>A task that completes when the startup gate has been disposed or when cleanup timed out.</returns>
	private async Task DisposeStartLockAsync(TimeSpan waitTimeout)
	{
		bool startLockHeld = false;

		if (waitTimeout <= TimeSpan.Zero)
		{
			Log.Warn("Language server startup gate did not become available within {TimeoutMs} ms during disposal.",
				(int)_disposeWaitTimeout.TotalMilliseconds);
			return;
		}

		try
		{
			if (!await _startLock.WaitAsync(waitTimeout).ConfigureAwait(false))
			{
				Log.Warn("Language server startup gate did not become available within {TimeoutMs} ms during disposal.",
					(int)_disposeWaitTimeout.TotalMilliseconds);
				return;
			}

			startLockHeld = true;
		}
		catch (ObjectDisposedException)
		{
			return;
		}
		finally
		{
			if (startLockHeld)
			{
				try
				{
					_startLock.Release();
				}
				catch (ObjectDisposedException)
				{ }
			}
		}

		try
		{
			_startLock.Dispose();
		}
		catch (ObjectDisposedException)
		{ }
	}

	/// <summary>
	/// Throws when the client has been disposed and disposed access is not allowed.
	/// </summary>
	/// <param name="allowDisposed">Whether disposed access should be allowed.</param>
	private void ThrowIfDisposed(bool allowDisposed)
	{
		if (!allowDisposed)
			ObjectDisposedException.ThrowIf(_isDisposed, nameof(LanguageServerClient));
	}

	/// <summary>
	/// Stops the language-server process, completes pending requests, and releases transport resources.
	/// </summary>
	public void Dispose()
	{
		if (!TryBeginDispose())
			return;

		try
		{
			DisposeCoreAsync().GetAwaiter().GetResult();
		}
		finally
		{
			GC.SuppressFinalize(this);
		}
	}

	/// <summary>
	/// Stops the language-server process, completes pending requests, and releases transport resources asynchronously.
	/// </summary>
	/// <returns>A task that completes when disposal finishes.</returns>
	public async ValueTask DisposeAsync()
	{
		if (!TryBeginDispose())
			return;

		try
		{
			await DisposeCoreAsync().ConfigureAwait(false);
		}
		finally
		{
			GC.SuppressFinalize(this);
		}
	}
}
