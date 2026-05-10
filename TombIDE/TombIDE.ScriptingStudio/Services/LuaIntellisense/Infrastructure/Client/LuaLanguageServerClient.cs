#nullable enable

using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace TombIDE.ScriptingStudio.Services.LuaIntellisense;

/// <summary>
/// Hosts the LuaLS process, performs the LSP handshake, and transports JSON-RPC requests and notifications.
/// </summary>
internal sealed partial class LuaLanguageServerClient : ILuaLanguageServerClient
{
	private static readonly Logger Log = LogManager.GetCurrentClassLogger();

	private const int ReceiveChunkSize = 4096;
	private const int MaxPayloadByteCount = 64 * 1024 * 1024; // 64 MiB safety cap for inbound LSP payloads.
	private static readonly TimeSpan DisposeWaitTimeout = TimeSpan.FromSeconds(3);
	private static readonly byte[] HeaderTerminator = [(byte)'\r', (byte)'\n', (byte)'\r', (byte)'\n'];

	private readonly string _workspaceRootDirectoryPath;
	private readonly string _serverExecutablePath;
	private readonly Func<object> _settingsProvider;

	private readonly SemaphoreSlim _startLock = new(1, 1);
	private readonly SemaphoreSlim _writeLock = new(1, 1);
	private readonly CancellationTokenSource _lifetimeCts = new();
	private readonly ConcurrentDictionary<string, JsonElement> _pendingDiagnostics = new(StringComparer.OrdinalIgnoreCase);

	// Diagnostics arrive on the LSP read loop and are stored as the latest payload per file URI.
	// A bounded single-slot channel acts only as a wake signal for the pump, so bursty notifications
	// for the same file collapse to one queued wake-up instead of building an unbounded backlog.
	private readonly Channel<bool> _diagnosticsSignal = Channel.CreateBounded<bool>(
		new BoundedChannelOptions(1)
		{
			SingleReader = true,
			SingleWriter = true,
			AllowSynchronousContinuations = false,
			FullMode = BoundedChannelFullMode.DropWrite
		});

	private Task? _diagnosticsPumpTask;

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

	private long _requestId;
	private long _diagnosticsFallbackSequence;
	private long _transportGeneration;
	private long _activeTransportGeneration;
	private volatile bool _isDisposed;
	private volatile bool _isReady;

	private LuaLanguageServerTransportSession? _activeSession;
	private LuaTextDocumentSyncKind _textDocumentSyncKind = LuaTextDocumentSyncKind.Incremental;
	private string[] _semanticTokenTypes = [];
	private string[] _semanticTokenModifiers = [];
	private bool _supportsCompletionResolve;
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
	/// Gets the current transport generation for the active server session.
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
	/// Gets a value indicating whether the server supports semantic-token delta responses.
	/// </summary>
	public bool SupportsSemanticTokensDelta => _supportsSemanticTokensDelta;

	/// <summary>
	/// Occurs when the server publishes diagnostics for a tracked document.
	/// </summary>
	public event Action<JsonElement>? DiagnosticsPublished;

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

			JsonElement initializeResponse = await SendRequestCoreAsync(session,
				"initialize", BuildInitializeParams(), initializeTimeout.Token, allowDisposed: false).ConfigureAwait(false);
			CaptureServerCapabilities(initializeResponse);

			IsReady = true;

			await SendNotificationCoreAsync(session, "initialized", new { }, cancellationToken, allowDisposed: false).ConfigureAwait(false);

			await SendNotificationCoreAsync(session,
				"workspace/didChangeConfiguration",
				new { settings = _settingsProvider() },
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
		=> SendNotificationCoreAsync(method, parameters, cancellationToken, allowDisposed: false);

	/// <summary>
	/// Sends a JSON-RPC request to the language server and returns the raw response payload.
	/// </summary>
	/// <param name="method">The LSP method name.</param>
	/// <param name="parameters">The request payload.</param>
	/// <param name="cancellationToken">A token that can cancel the request.</param>
	/// <returns>The raw JSON response payload.</returns>
	public Task<JsonElement> SendRequestAsync(string method, object parameters, CancellationToken cancellationToken)
		=> SendRequestCoreAsync(method, parameters, cancellationToken, allowDisposed: false);

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
			// loading workspace-supplied plugins (runtime.plugin in .luarc.json). TombIDE has no
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

	private void CaptureServerCapabilities(JsonElement initializeResponse)
	{
		_supportsCompletionResolve = false;
		_supportsSemanticTokensDelta = false;
		_textDocumentSyncKind = LuaTextDocumentSyncKind.Incremental;
		_semanticTokenTypes = [];
		_semanticTokenModifiers = [];

		if (!initializeResponse.TryGetProperty("capabilities", out JsonElement capabilities))
			return;

		if (TryReadTextDocumentSyncKind(capabilities, out LuaTextDocumentSyncKind textDocumentSyncKind))
		{
			if (textDocumentSyncKind == LuaTextDocumentSyncKind.None)
			{
				throw new NotSupportedException(
					"The Lua language server does not advertise full or incremental text synchronization required by TombIDE.");
			}

			_textDocumentSyncKind = textDocumentSyncKind;
		}

		if (capabilities.TryGetProperty("completionProvider", out JsonElement completionProvider)
			&& completionProvider.TryGetProperty("resolveProvider", out JsonElement resolveProvider))
		{
			_supportsCompletionResolve = resolveProvider.ValueKind == JsonValueKind.True;
		}

		if (!capabilities.TryGetProperty("semanticTokensProvider", out JsonElement semanticTokensProvider))
			return;

		if (semanticTokensProvider.TryGetProperty("full", out JsonElement fullElement))
		{
			_supportsSemanticTokensDelta = fullElement.ValueKind == JsonValueKind.Object
				&& fullElement.TryGetProperty("delta", out JsonElement deltaElement)
				&& deltaElement.ValueKind == JsonValueKind.True;
		}

		if (!semanticTokensProvider.TryGetProperty("legend", out JsonElement legend))
			return;

		_semanticTokenTypes = ReadStringArray(legend, "tokenTypes");
		_semanticTokenModifiers = ReadStringArray(legend, "tokenModifiers");
	}

	private static bool TryReadTextDocumentSyncKind(JsonElement capabilities, out LuaTextDocumentSyncKind textDocumentSyncKind)
	{
		textDocumentSyncKind = LuaTextDocumentSyncKind.Incremental;

		if (!capabilities.TryGetProperty("textDocumentSync", out JsonElement textDocumentSyncElement))
			return false;

		if (textDocumentSyncElement.ValueKind == JsonValueKind.Number
			&& textDocumentSyncElement.TryGetInt32(out int rawSyncKind))
		{
			textDocumentSyncKind = ParseTextDocumentSyncKind(rawSyncKind);
			return true;
		}

		if (textDocumentSyncElement.ValueKind != JsonValueKind.Object)
			return false;

		if (!textDocumentSyncElement.TryGetProperty("change", out JsonElement changeElement)
			|| !changeElement.TryGetInt32(out rawSyncKind))
		{
			textDocumentSyncKind = LuaTextDocumentSyncKind.None;
			return true;
		}

		textDocumentSyncKind = ParseTextDocumentSyncKind(rawSyncKind);
		return true;
	}

	private static LuaTextDocumentSyncKind ParseTextDocumentSyncKind(int rawSyncKind) => rawSyncKind switch
	{
		0 => LuaTextDocumentSyncKind.None,
		1 => LuaTextDocumentSyncKind.Full,
		2 => LuaTextDocumentSyncKind.Incremental,
		_ => LuaTextDocumentSyncKind.None
	};

	private static string[] ReadStringArray(JsonElement parent, string propertyName)
	{
		if (!parent.TryGetProperty(propertyName, out JsonElement property) || property.ValueKind != JsonValueKind.Array)
			return [];

		var values = new List<string>();

		foreach (JsonElement item in property.EnumerateArray())
		{
			if (item.ValueKind == JsonValueKind.String)
			{
				string? value = item.GetString();

				if (!string.IsNullOrEmpty(value))
					values.Add(value);
			}
		}

		return [.. values];
	}

	private LuaLanguageServerTransportSession CreateTransportSession(Process process)
	{
		long generation = Interlocked.Increment(ref _transportGeneration);
		var session = new LuaLanguageServerTransportSession(generation,
			process,
			process.StandardOutput.BaseStream,
			process.StandardInput.BaseStream);

		session.ProcessExitedHandler = (_, _) => Process_Exited(session);
		process.Exited += session.ProcessExitedHandler;
		session.ReadLoopTask = Task.Run(() => ReadLoopAsync(session), CancellationToken.None);
		session.StderrLoopTask = Task.Run(() => ReadStandardErrorLoopAsync(session), CancellationToken.None);
		return session;
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

		FailPendingRequests(session, new IOException("The Lua language server process exited unexpectedly."));
	}

	private bool IsCurrentSession(LuaLanguageServerTransportSession session)
		=> ReferenceEquals(Volatile.Read(ref _activeSession), session);

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

	private static bool TryAcceptPayloadSize(int parsedContentLength, out int contentLength)
	{
		contentLength = 0;

		if (parsedContentLength <= 0)
			return false;

		if (parsedContentLength > MaxPayloadByteCount)
		{
			Log.Warn("Refusing Lua language server payload of {Bytes} bytes (cap is {Cap}).", parsedContentLength, MaxPayloadByteCount);
			return false;
		}

		contentLength = parsedContentLength;
		return true;
	}

	private void FailPendingRequests(LuaLanguageServerTransportSession session, Exception exception)
	{
		foreach (KeyValuePair<long, TaskCompletionSource<JsonElement>> pendingRequest in session.PendingRequests)
			pendingRequest.Value.TrySetException(exception);
	}

	private async Task DisposeSessionAsync(LuaLanguageServerTransportSession session)
	{
		Task? readLoopTask = session.ReadLoopTask;
		Task? stderrLoopTask = session.StderrLoopTask;
		Exception pendingRequestFailure = _isDisposed
			? new ObjectDisposedException(nameof(LuaLanguageServerClient))
			: new IOException("The Lua language server transport was closed.");

		FailPendingRequests(session, pendingRequestFailure);

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
			CleanupSessionResources(session);
		}

		await WaitForBackgroundLoopsAsync(readLoopTask, stderrLoopTask).ConfigureAwait(false);
	}

	private void CleanupSessionResources(LuaLanguageServerTransportSession session)
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

		ReturnReceiveBuffer(session);
		session.ReceiveBufferCount = 0;
		session.ReadLoopTask = null;
		session.StderrLoopTask = null;
	}

	private async Task TrySendShutdownAsync(LuaLanguageServerTransportSession session)
	{
		using var shutdownTimeout = new CancellationTokenSource(TimeSpan.FromSeconds(1));

		try
		{
			await SendRequestCoreAsync(session, "shutdown", new { }, shutdownTimeout.Token, allowDisposed: true).ConfigureAwait(false);
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
		_writeLock.Dispose();
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

		public EventHandler? ProcessExitedHandler { get; set; }

		public ConcurrentDictionary<long, TaskCompletionSource<JsonElement>> PendingRequests { get; } = new();

		public byte[] ReceiveBuffer { get; set; } = [];

		public int ReceiveBufferCount { get; set; }

		public Task? ReadLoopTask { get; set; }

		public Task? StderrLoopTask { get; set; }
	}
}
