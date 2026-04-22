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
	private readonly ConcurrentDictionary<long, TaskCompletionSource<JsonElement>> _pendingRequests = new();
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
	private volatile bool _isDisposed;
	private volatile bool _isReady;

	private Process? _process;
	private Stream? _inputStream;
	private Stream? _outputStream;
	private byte[] _receiveBuffer = [];
	private int _receiveBufferCount;
	private Task? _readLoopTask;
	private Task? _stderrLoopTask;
	private LuaTextDocumentSyncKind _textDocumentSyncKind = LuaTextDocumentSyncKind.Incremental;
	private string[] _semanticTokenTypes = [];
	private string[] _semanticTokenModifiers = [];
	private bool _supportsCompletionResolve;
	private bool _supportsSemanticTokensDelta;

	public bool IsReady
	{
		get => _isReady;
		private set => _isReady = value;
	}

	public LuaTextDocumentSyncKind TextDocumentSyncKind => _textDocumentSyncKind;
	public IReadOnlyList<string> SemanticTokenTypes => _semanticTokenTypes;
	public IReadOnlyList<string> SemanticTokenModifiers => _semanticTokenModifiers;
	public bool SupportsCompletionResolve => _supportsCompletionResolve;
	public bool SupportsSemanticTokensDelta => _supportsSemanticTokensDelta;

	public event Action<JsonElement>? DiagnosticsPublished;
	public event Action? SemanticTokensRefreshRequested;

	public LuaLanguageServerClient(string workspaceRootDirectoryPath, string serverExecutablePath, Func<object> settingsProvider)
	{
		_workspaceRootDirectoryPath = workspaceRootDirectoryPath;
		_serverExecutablePath = serverExecutablePath;
		_settingsProvider = settingsProvider;
	}

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
			ResetProcessState();

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

			_process = new Process { StartInfo = startInfo, EnableRaisingEvents = true };
			_process.Exited += Process_Exited;

			if (!_process.Start())
				return false;

			if (OperatingSystem.IsWindows())
				LuaProcessJobObject.TryAssignProcess(_process);

			_inputStream = _process.StandardOutput.BaseStream;
			_outputStream = _process.StandardInput.BaseStream;

			_readLoopTask = Task.Run(ReadLoopAsync, CancellationToken.None);
			_stderrLoopTask = Task.Run(ReadStandardErrorLoopAsync, CancellationToken.None);
			_diagnosticsPumpTask ??= Task.Run(PumpDiagnosticsAsync, CancellationToken.None);

			using var initializeTimeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
			initializeTimeout.CancelAfter(TimeSpan.FromSeconds(10));

			JsonElement initializeResponse = await SendRequestAsync("initialize", BuildInitializeParams(), initializeTimeout.Token).ConfigureAwait(false);
			CaptureServerCapabilities(initializeResponse);

			IsReady = true;

			await SendNotificationAsync("initialized", new { }, cancellationToken).ConfigureAwait(false);
			await SendNotificationAsync("workspace/didChangeConfiguration", new { settings = _settingsProvider() }, cancellationToken).ConfigureAwait(false);

			return true;
		}
		catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
		{
			await DisposeProcessAsync().ConfigureAwait(false);
			throw;
		}
		catch (Exception exception)
		{
			Log.Warn(exception, "Failed to start the Lua language server (executable='{Executable}', workspace='{Workspace}').",
				_serverExecutablePath, _workspaceRootDirectoryPath);

			await DisposeProcessAsync().ConfigureAwait(false);
			return false;
		}
		finally
		{
			_startLock.Release();
		}
	}

	public Task SendNotificationAsync(string method, object parameters, CancellationToken cancellationToken)
		=> SendNotificationCoreAsync(method, parameters, cancellationToken, allowDisposed: false);

	public Task<JsonElement> SendRequestAsync(string method, object parameters, CancellationToken cancellationToken)
		=> SendRequestCoreAsync(method, parameters, cancellationToken, allowDisposed: false);

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

	private void Process_Exited(object? sender, EventArgs e)
	{
		IsReady = false;

		int? exitCode = TryReadProcessExitCode(sender as Process ?? _process);

		if (!_isDisposed)
			Log.Warn("Lua language server process exited unexpectedly{ExitCodeSuffix}.", exitCode is not null ? $" with code {exitCode.Value}" : string.Empty);

		FailPendingRequests(new IOException("The Lua language server process exited unexpectedly."));
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

	private void FailPendingRequests(Exception exception)
	{
		foreach (KeyValuePair<long, TaskCompletionSource<JsonElement>> pendingRequest in _pendingRequests)
			pendingRequest.Value.TrySetException(exception);
	}

	private async Task DisposeProcessAsync()
	{
		Task? readLoopTask = _readLoopTask;
		Task? stderrLoopTask = _stderrLoopTask;

		try
		{
			if (_process is not null && !_process.HasExited)
			{
				await TrySendShutdownAsync().ConfigureAwait(false);
				await TrySendExitNotificationAsync().ConfigureAwait(false);

				if (!_process.HasExited)
					_process.Kill(true);
			}
		}
		catch
		{
			// Ignore process disposal failures.
		}
		finally
		{
			ResetProcessState();
			IsReady = false;
		}

		await WaitForBackgroundLoopsAsync(readLoopTask, stderrLoopTask, _diagnosticsPumpTask).ConfigureAwait(false);
	}

	private async Task TrySendShutdownAsync()
	{
		using var shutdownTimeout = new CancellationTokenSource(TimeSpan.FromSeconds(1));

		try
		{
			await SendRequestCoreAsync("shutdown", new { }, shutdownTimeout.Token, allowDisposed: true).ConfigureAwait(false);
		}
		catch
		{
			// Ignore shutdown failures.
		}
	}

	private async Task TrySendExitNotificationAsync()
	{
		using var exitTimeout = new CancellationTokenSource(TimeSpan.FromSeconds(1));

		try
		{
			await SendNotificationCoreAsync("exit", new { }, exitTimeout.Token, allowDisposed: true).ConfigureAwait(false);
		}
		catch
		{
			// Ignore exit notification failures.
		}
	}

	private static async Task WaitForBackgroundLoopsAsync(Task? readLoopTask, Task? stderrLoopTask, Task? diagnosticsPumpTask)
	{
		Task combined = Task.WhenAll(
			readLoopTask ?? Task.CompletedTask,
			stderrLoopTask ?? Task.CompletedTask,
			diagnosticsPumpTask ?? Task.CompletedTask);

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

	private void ResetProcessState()
	{
		try
		{
			if (_process is not null)
				_process.Exited -= Process_Exited;
		}
		catch
		{
			// Ignore event detach failures.
		}

		try
		{
			_inputStream?.Dispose();
			_outputStream?.Dispose();
			_process?.Dispose();
		}
		catch
		{
			// Ignore stream disposal failures.
		}

		_inputStream = null;
		_outputStream = null;
		_process = null;
		ReturnReceiveBuffer();
		_receiveBufferCount = 0;
		_readLoopTask = null;
		_stderrLoopTask = null;
		_textDocumentSyncKind = LuaTextDocumentSyncKind.Incremental;
		_supportsCompletionResolve = false;
		_supportsSemanticTokensDelta = false;
		_semanticTokenTypes = [];
		_semanticTokenModifiers = [];
	}

	private void ThrowIfDisposed(bool allowDisposed)
	{
		if (!allowDisposed)
			ObjectDisposedException.ThrowIf(_isDisposed, nameof(LuaLanguageServerClient));
	}

	public void Dispose()
	{
		if (_isDisposed)
			return;

		_isDisposed = true;
		FailPendingRequests(new ObjectDisposedException(nameof(LuaLanguageServerClient)));
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
			if (!DisposeProcessAsync().Wait(DisposeWaitTimeout))
				Log.Warn("Disposing Lua language server timed out; abandoning background tasks.");
		}
		catch (AggregateException exception)
		{
			Log.Warn(exception.Flatten(), "Disposing Lua language server raised exceptions.");
		}

		_lifetimeCts.Dispose();
		_startLock.Dispose();
		_writeLock.Dispose();
	}
}
