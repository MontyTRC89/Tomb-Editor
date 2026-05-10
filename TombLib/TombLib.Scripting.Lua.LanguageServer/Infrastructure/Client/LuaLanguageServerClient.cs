using NLog;
using StreamJsonRpc;
using System.Diagnostics;
using System.Text.Json;

namespace TombLib.Scripting.Lua.LanguageServer;

/// <summary>
/// Hosts the LuaLS process, performs the LSP handshake, and transports JSON-RPC requests and notifications.
/// </summary>
public sealed partial class LuaLanguageServerClient : ILuaLanguageServerClient
{
	private static readonly Logger Log = LogManager.GetCurrentClassLogger();

	private static readonly TimeSpan DisposeWaitTimeout = TimeSpan.FromSeconds(3);

	private readonly string _workspaceRootDirectoryPath;
	private readonly string _serverExecutablePath;
	private readonly Func<object> _settingsProvider;

	private readonly SemaphoreSlim _startLock = new(1, 1);
	private readonly CancellationTokenSource _lifetimeCts = new();

	private static readonly string[] SupportedSemanticTokenTypes =
	[
		"namespace", "type", "class", "enum", "interface", "struct", "typeParameter",
		"parameter", "variable", "property", "enumMember", "event", "function", "method",
		"macro", "keyword", "modifier", "comment", "string", "number", "regexp",
		"operator", "decorator"
	];

	private static readonly string[] SupportedSemanticTokenModifiers =
	[
		"declaration", "definition", "readonly", "static", "deprecated", "abstract",
		"async", "modification", "documentation", "defaultLibrary", "global"
	];

	private long _transportGeneration;
	private long _activeTransportGeneration;
	private volatile bool _isDisposed;
	private volatile bool _isReady;

	private LuaLanguageServerTransportSession? _activeSession;
	private LuaTextDocumentSyncKind _textDocumentSyncKind = LuaTextDocumentSyncKind.Incremental;
	private string[] _semanticTokenTypes = [];
	private string[] _semanticTokenModifiers = [];
	private bool _supportsCompletionResolve;
	private bool? _supportsReferences;
	private bool? _supportsRename;
	private bool? _supportsFormatting;
	private bool _supportsSemanticTokensDelta;

	/// <summary>
	/// Gets a value indicating whether the client finished initialization and can accept requests.
	/// </summary>
	public bool IsReady
	{
		get => _isReady;
		private set => _isReady = value;
	}

	/// <summary>
	/// Gets the current transport generation for the active language-server session.
	/// </summary>
	public long TransportGeneration => Volatile.Read(ref _activeTransportGeneration);

	/// <summary>
	/// Gets the text-document synchronization mode negotiated with the server.
	/// </summary>
	public LuaTextDocumentSyncKind TextDocumentSyncKind => _textDocumentSyncKind;

	/// <summary>
	/// Gets the semantic token types advertised by the server.
	/// </summary>
	public IReadOnlyList<string> SemanticTokenTypes => _semanticTokenTypes;

	/// <summary>
	/// Gets the semantic token modifiers advertised by the server.
	/// </summary>
	public IReadOnlyList<string> SemanticTokenModifiers => _semanticTokenModifiers;

	/// <summary>
	/// Gets a value indicating whether the server supports completion-item resolve requests.
	/// </summary>
	public bool SupportsCompletionResolve => _supportsCompletionResolve;

	/// <summary>
	/// Gets a value indicating whether the server supports reference requests.
	/// </summary>
	public bool SupportsReferences => _supportsReferences ?? true;

	/// <summary>
	/// Gets a value indicating whether the server supports rename requests.
	/// </summary>
	public bool SupportsRename => _supportsRename ?? true;

	/// <summary>
	/// Gets a value indicating whether the server supports document formatting requests.
	/// </summary>
	public bool SupportsFormatting => _supportsFormatting ?? true;

	/// <summary>
	/// Gets a value indicating whether the server supports semantic-token delta responses.
	/// </summary>
	public bool SupportsSemanticTokensDelta => _supportsSemanticTokensDelta;

	/// <summary>
	/// Occurs when the server requests that semantic tokens be refreshed.
	/// </summary>
	public event Action? SemanticTokensRefreshRequested;

	/// <summary>
	/// Initializes a new instance of the <see cref="LuaLanguageServerClient"/> class.
	/// </summary>
	/// <param name="workspaceRootDirectoryPath">The normalized workspace root directory.</param>
	/// <param name="serverExecutablePath">The LuaLS executable path.</param>
	/// <param name="settingsProvider">Produces the current settings payload for <c>workspace/didChangeConfiguration</c>.</param>
	public LuaLanguageServerClient(string workspaceRootDirectoryPath, string serverExecutablePath, Func<object> settingsProvider)
	{
		_workspaceRootDirectoryPath = workspaceRootDirectoryPath;
		_serverExecutablePath = serverExecutablePath;
		_settingsProvider = settingsProvider;
	}

	/// <summary>
	/// Starts the LuaLS process and completes the initialize/initialized handshake.
	/// </summary>
	/// <param name="cancellationToken">A token that can cancel startup.</param>
	/// <returns><see langword="true"/> when startup succeeded; otherwise, <see langword="false"/>.</returns>
	public async Task<bool> StartAsync(CancellationToken cancellationToken)
	{
		if (IsReady)
			return true;

		await _startLock.WaitAsync(cancellationToken).ConfigureAwait(false);

		try
		{
			if (IsReady)
				return true;

			ThrowIfDisposed(allowDisposed: false);

			LuaLanguageServerTransportSession? previousSession = DetachActiveSession();

			if (previousSession is not null)
				await DisposeSessionAsync(previousSession).ConfigureAwait(false);

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

			var process = new Process { StartInfo = startInfo, EnableRaisingEvents = true };

			if (!process.Start())
			{
				process.Dispose();
				return false;
			}

			if (OperatingSystem.IsWindows())
				LuaProcessJobObject.TryAssignProcess(process);

			LuaLanguageServerTransportSession session = CreateTransportSession(process);
			SetActiveSession(session);

			_diagnosticsPumpTask ??= Task.Run(PumpDiagnosticsAsync, CancellationToken.None);

			// Complete the LSP handshake before marking the client ready for provider requests.
			using var initializeTimeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
			initializeTimeout.CancelAfter(TimeSpan.FromSeconds(10));

			LuaInitializeResponse initializeResponse = await SendRequestCoreAsync<LuaInitializeResponse>(session,
				"initialize", BuildInitializeParams(), initializeTimeout.Token, allowDisposed: false).ConfigureAwait(false);

			CaptureServerCapabilities(initializeResponse);

			IsReady = true;

			await SendNotificationCoreAsync(session, "initialized", new LuaEmptyParams(), cancellationToken, allowDisposed: false).ConfigureAwait(false);

			await SendNotificationCoreAsync(session,
				"workspace/didChangeConfiguration",
				new LuaDidChangeConfigurationParams(_settingsProvider()),
				cancellationToken,
				allowDisposed: false).ConfigureAwait(false);

			return true;
		}
		catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
		{
			// Caller-driven cancellation should tear down the half-started process so later retries begin cleanly.
			await DisposeActiveSessionAsync().ConfigureAwait(false);
			throw;
		}
		catch (Exception exception)
		{
			Log.Warn(exception, "Failed to start the Lua language server (executable='{Executable}', workspace='{Workspace}').",
				_serverExecutablePath, _workspaceRootDirectoryPath);

			await DisposeActiveSessionAsync().ConfigureAwait(false);
			return false;
		}
		finally
		{
			_startLock.Release();
		}
	}

	/// <summary>
	/// Sends a JSON-RPC notification to the language server.
	/// </summary>
	/// <param name="method">The LSP method name.</param>
	/// <param name="parameters">The notification payload.</param>
	/// <param name="cancellationToken">A token that can cancel the send operation.</param>
	public Task SendNotificationAsync(string method, object parameters, CancellationToken cancellationToken)
		=> SendNotificationCoreAsync(GetRequiredActiveSession(allowDisposed: false), method, parameters, cancellationToken, allowDisposed: false);

	/// <summary>
	/// Sends a JSON-RPC request to the language server and returns the typed response payload.
	/// </summary>
	/// <typeparam name="TResult">The typed response payload to deserialize.</typeparam>
	/// <param name="method">The LSP method name.</param>
	/// <param name="parameters">The request payload.</param>
	/// <param name="cancellationToken">A token that can cancel the request.</param>
	/// <returns>The typed response payload.</returns>
	public Task<TResult> SendRequestAsync<TResult>(string method, object parameters, CancellationToken cancellationToken)
		=> SendRequestCoreAsync<TResult>(GetRequiredActiveSession(allowDisposed: false), method, parameters, cancellationToken, allowDisposed: false);

	/// <summary>
	/// Marks the current transport unhealthy so the provider restarts it on the next request.
	/// </summary>
	public void MarkTransportUnhealthy()
	{
		if (_isDisposed)
			return;

		IsReady = false;
	}

	private object BuildInitializeParams() => new
	{
		processId = Environment.ProcessId,
		initializationOptions = new
		{
			changeConfiguration = true,
			viewDocument = true,
			// Do NOT set trustByClient = true: LuaLS uses that flag to skip the user prompt before
			// loading workspace-supplied plugins (runtime.plugin in .luarc.json). The host editor has no
			// equivalent workspace-trust gate, so leaving the prompt enabled keeps malicious or
			// accidental third-party Lua scripts from being executed silently inside the host process.
			trustByClient = false,
			useSemanticByRange = false
		},
		rootUri = LuaLanguageServerPathHelper.CreateFileUri(_workspaceRootDirectoryPath),
		workspaceFolders = new[]
		{
			new
			{
				uri = LuaLanguageServerPathHelper.CreateFileUri(_workspaceRootDirectoryPath),
				name = Path.GetFileName(_workspaceRootDirectoryPath)
			}
		},
		capabilities = new
		{
			workspace = new
			{
				workspaceFolders = true,
				configuration = true,
				didChangeWatchedFiles = new { dynamicRegistration = false }
			},
			textDocument = new
			{
				completion = new
				{
					contextSupport = true,
					completionItem = new
					{
						snippetSupport = false,
						// Prefer markdown so the editor's MdXaml renderer can show formatted documentation.
						documentationFormat = new[] { "markdown", "plaintext" },
						resolveSupport = new
						{
							properties = new[] { "detail", "documentation" }
						}
					}
				},
				hover = new
				{
					contentFormat = new[] { "markdown", "plaintext" }
				},
				definition = new
				{
					linkSupport = true
				},
				references = new
				{
					dynamicRegistration = false
				},
				rename = new
				{
					dynamicRegistration = false,
					prepareSupport = false
				},
				formatting = new
				{
					dynamicRegistration = false
				},
				publishDiagnostics = new
				{
					versionSupport = true
				},
				signatureHelp = new
				{
					signatureInformation = new
					{
						documentationFormat = new[] { "markdown", "plaintext" },
						parameterInformation = new
						{
							labelOffsetSupport = true
						}
					},
					contextSupport = true
				},
				semanticTokens = new
				{
					requests = new
					{
						range = false,
						full = new { delta = true }
					},
					tokenTypes = SupportedSemanticTokenTypes,
					tokenModifiers = SupportedSemanticTokenModifiers,
					formats = new[] { "relative" },
					multilineTokenSupport = false,
					overlappingTokenSupport = false,
					augmentsSyntaxTokens = true
				}
			}
		}
	};

	private void CaptureServerCapabilities(LuaInitializeResponse initializeResponse)
	{
		_supportsCompletionResolve = false;
		_supportsReferences = false;
		_supportsRename = false;
		_supportsFormatting = false;
		_supportsSemanticTokensDelta = false;
		_textDocumentSyncKind = LuaTextDocumentSyncKind.Incremental;
		_semanticTokenTypes = [];
		_semanticTokenModifiers = [];

		if (initializeResponse.Capabilities is not { } capabilities)
			return;

		if (capabilities.TextDocumentSync is { } textDocumentSync)
		{
			if (textDocumentSync.Kind == LuaTextDocumentSyncKind.None)
			{
				throw new NotSupportedException(
					"The Lua language server does not advertise full or incremental text synchronization required by the Lua IntelliSense provider.");
			}

			_textDocumentSyncKind = textDocumentSync.Kind;
		}

		_supportsCompletionResolve = capabilities.CompletionProvider?.ResolveProvider == true;
		_supportsReferences = capabilities.ReferencesProvider?.IsSupported == true;
		_supportsRename = capabilities.RenameProvider?.IsSupported == true;
		_supportsFormatting = capabilities.DocumentFormattingProvider?.IsSupported == true;

		if (capabilities.SemanticTokensProvider is not { } semanticTokensProvider)
			return;

		_supportsSemanticTokensDelta = semanticTokensProvider.Full?.SupportsDelta == true;

		if (semanticTokensProvider.Legend is not { } legend)
			return;

		_semanticTokenTypes = legend.TokenTypes ?? [];
		_semanticTokenModifiers = legend.TokenModifiers ?? [];
	}

	private LuaLanguageServerTransportSession CreateTransportSession(Process process)
	{
		long generation = Interlocked.Increment(ref _transportGeneration);

		var session = new LuaLanguageServerTransportSession(generation,
			process,
			process.StandardOutput.BaseStream,
			process.StandardInput.BaseStream);

		session.MessageHandler = CreateMessageHandler(session.OutputStream, session.InputStream);
		session.RpcTarget = new LuaLanguageServerClientRpcTarget(this, generation);
		session.JsonRpc = CreateJsonRpc(session);
		session.RpcCompletionTask = session.JsonRpc.Completion;

		session.ProcessExitedHandler = (_, _) => Process_Exited(session);
		process.Exited += session.ProcessExitedHandler;
		session.StderrLoopTask = Task.Run(() => ReadStandardErrorLoopAsync(session), CancellationToken.None);
		session.JsonRpc.StartListening();

		return session;
	}

	private static HeaderDelimitedMessageHandler CreateMessageHandler(Stream outputStream, Stream inputStream)
	{
		var formatter = new SystemTextJsonFormatter
		{
			JsonSerializerOptions = new JsonSerializerOptions
			{
				PropertyNameCaseInsensitive = true
			}
		};

		return new HeaderDelimitedMessageHandler(outputStream, inputStream, formatter);
	}

	private JsonRpc CreateJsonRpc(LuaLanguageServerTransportSession session)
	{
		HeaderDelimitedMessageHandler messageHandler = session.MessageHandler
			?? throw new InvalidOperationException("The Lua language server transport session is missing a JSON-RPC message handler.");

		LuaLanguageServerClientRpcTarget rpcTarget = session.RpcTarget
			?? throw new InvalidOperationException("The Lua language server transport session is missing a JSON-RPC callback target.");

		var jsonRpc = new JsonRpc(messageHandler, rpcTarget)
		{
			CancelLocallyInvokedMethodsWhenConnectionIsClosed = true
		};

		jsonRpc.Disconnected += (_, eventArgs) => JsonRpc_Disconnected(session, eventArgs);
		return jsonRpc;
	}

	private void SetActiveSession(LuaLanguageServerTransportSession session)
	{
		_activeSession = session;
		Volatile.Write(ref _activeTransportGeneration, session.Generation);
	}

	private LuaLanguageServerTransportSession GetRequiredActiveSession(bool allowDisposed)
	{
		ThrowIfDisposed(allowDisposed);

		LuaLanguageServerTransportSession? session = Volatile.Read(ref _activeSession);

		if (session is null)
			throw new IOException("The Lua language server transport is not available.");

		return session;
	}

	private LuaLanguageServerTransportSession? DetachActiveSession()
	{
		LuaLanguageServerTransportSession? session = Interlocked.Exchange(ref _activeSession, null);
		Volatile.Write(ref _activeTransportGeneration, 0);

		IsReady = false;
		return session;
	}

	private async Task DisposeActiveSessionAsync()
	{
		LuaLanguageServerTransportSession? session = DetachActiveSession();

		if (session is not null)
			await DisposeSessionAsync(session).ConfigureAwait(false);
	}

	private void Process_Exited(LuaLanguageServerTransportSession session)
	{
		bool isCurrentSession = IsCurrentSession(session);

		if (isCurrentSession)
			IsReady = false;

		int? exitCode = TryReadProcessExitCode(session.Process);

		if (!_isDisposed && isCurrentSession)
			Log.Warn("Lua language server process exited unexpectedly{ExitCodeSuffix}.", exitCode is not null ? $" with code {exitCode.Value}" : string.Empty);
	}

	private bool IsCurrentSession(LuaLanguageServerTransportSession session)
		=> ReferenceEquals(Volatile.Read(ref _activeSession), session);

	private void JsonRpc_Disconnected(LuaLanguageServerTransportSession session, JsonRpcDisconnectedEventArgs eventArgs)
	{
		if (!IsCurrentSession(session))
			return;

		IsReady = false;

		if (_isDisposed)
			return;

		Exception? exception = eventArgs.Exception;

		if (exception is not null)
		{
			Log.Warn(exception, "Lua language server JSON-RPC transport disconnected: {Description}", eventArgs.Description);
			return;
		}

		Log.Warn("Lua language server JSON-RPC transport disconnected: {Description}", eventArgs.Description);
	}

	private async Task ReadStandardErrorLoopAsync(LuaLanguageServerTransportSession session)
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
					Log.Debug("[LuaLS stderr] {Line}", line);
			}
		}
		catch (OperationCanceledException)
		{ }
		catch
		{
			// Ignore stderr read failures.
		}
	}

	private static void LogServerMessage(string method, LuaWindowMessageParams parameters)
	{
		string? messageText = parameters.Message;

		if (string.IsNullOrWhiteSpace(messageText))
			return;

		int messageType = parameters.Type ?? 4;

		switch (messageType)
		{
			case 1: Log.Error("[LuaLS {Method}] {Message}", method, messageText); break;
			case 2: Log.Warn("[LuaLS {Method}] {Message}", method, messageText); break;
			case 3: Log.Info("[LuaLS {Method}] {Message}", method, messageText); break;
			default: Log.Debug("[LuaLS {Method}] {Message}", method, messageText); break;
		}
	}

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

	private async Task DisposeSessionAsync(LuaLanguageServerTransportSession session)
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
				await TrySendShutdownAsync(session).ConfigureAwait(false);
				await TrySendExitNotificationAsync(session).ConfigureAwait(false);

				if (!session.Process.HasExited)
					session.Process.Kill(true);
			}
		}
		catch
		{
			// Ignore process disposal failures.
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

	private static void CleanupSessionResources(LuaLanguageServerTransportSession session)
	{
		try
		{
			session.InputStream.Dispose();
			session.OutputStream.Dispose();
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

	private async Task TrySendShutdownAsync(LuaLanguageServerTransportSession session)
	{
		try
		{
			Task<object?> shutdownTask = SendRequestCoreAsync<object?>(session, "shutdown", new LuaEmptyParams(), CancellationToken.None, allowDisposed: true);
			await shutdownTask.WaitAsync(TimeSpan.FromSeconds(1)).ConfigureAwait(false);
		}
		catch
		{
			// Ignore shutdown failures.
		}
	}

	private async Task TrySendExitNotificationAsync(LuaLanguageServerTransportSession session)
	{
		using var exitTimeout = new CancellationTokenSource(TimeSpan.FromSeconds(1));

		try
		{
			await SendNotificationCoreAsync(session, "exit", new { }, exitTimeout.Token, allowDisposed: true).ConfigureAwait(false);
		}
		catch
		{
			// Ignore exit notification failures.
		}
	}

	private static async Task WaitForBackgroundLoopsAsync(Task? readLoopTask, Task? stderrLoopTask)
	{
		Task combined = Task.WhenAll(
			readLoopTask ?? Task.CompletedTask,
			stderrLoopTask ?? Task.CompletedTask);

		try
		{
			await combined.WaitAsync(DisposeWaitTimeout).ConfigureAwait(false);
		}
		catch (TimeoutException)
		{
			Log.Warn("Lua language server background loops did not complete within the dispose timeout.");
		}
		catch
		{
			// Ignore loop completion failures during disposal.
		}
	}

	private void ThrowIfDisposed(bool allowDisposed)
	{
		if (!allowDisposed)
			ObjectDisposedException.ThrowIf(_isDisposed, nameof(LuaLanguageServerClient));
	}

	/// <summary>
	/// Stops the language-server process, completes pending requests, and releases transport resources.
	/// </summary>
	public void Dispose()
	{
		if (_isDisposed)
			return;

		_isDisposed = true;
		_diagnosticsSignal.Writer.TryComplete();

		try
		{
			_lifetimeCts.Cancel();
		}
		catch (ObjectDisposedException)
		{ }

		try
		{
			// DisposeProcessAsync also waits for the read/stderr loops, so by the time it returns no
			// background task should still be holding the write semaphore. We then release the locks.
			if (!DisposeActiveSessionAsync().Wait(DisposeWaitTimeout))
				Log.Warn("Disposing Lua language server timed out; abandoning background tasks.");
		}
		catch (AggregateException exception)
		{
			Log.Warn(exception.Flatten(), "Disposing Lua language server raised exceptions.");
		}

		if (_diagnosticsPumpTask is not null)
		{
			try
			{
				if (!_diagnosticsPumpTask.Wait(DisposeWaitTimeout))
					Log.Warn("Lua language server diagnostics pump did not complete within the dispose timeout.");
			}
			catch (AggregateException exception)
			{
				Log.Warn(exception.Flatten(), "Disposing the Lua language server diagnostics pump raised exceptions.");
			}
		}

		_lifetimeCts.Dispose();
		_startLock.Dispose();
	}

	private sealed class LuaLanguageServerTransportSession
	{
		public LuaLanguageServerTransportSession(long generation, Process? process, Stream inputStream, Stream outputStream)
		{
			Generation = generation;
			Process = process;
			InputStream = inputStream;
			OutputStream = outputStream;
		}

		public long Generation { get; }

		public Process? Process { get; }

		public Stream InputStream { get; }

		public Stream OutputStream { get; }

		public HeaderDelimitedMessageHandler? MessageHandler { get; set; }

		public JsonRpc? JsonRpc { get; set; }

		public LuaLanguageServerClientRpcTarget? RpcTarget { get; set; }

		public EventHandler? ProcessExitedHandler { get; set; }

		public Task? RpcCompletionTask { get; set; }

		public Task? StderrLoopTask { get; set; }
	}
}
