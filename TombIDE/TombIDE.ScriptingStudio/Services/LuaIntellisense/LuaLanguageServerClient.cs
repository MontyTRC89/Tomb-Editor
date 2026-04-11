using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace TombIDE.ScriptingStudio.Services.LuaIntellisense
{
	internal sealed class LuaLanguageServerClient : IDisposable
	{
		private readonly string _workspaceRootDirectoryPath;
		private readonly string _serverExecutablePath;
		private readonly Func<object> _settingsProvider;

		private readonly SemaphoreSlim _startLock = new(1, 1);
		private readonly SemaphoreSlim _writeLock = new(1, 1);
		private readonly ConcurrentDictionary<long, TaskCompletionSource<JsonElement>> _pendingRequests = new();

		private long _requestId;
		private bool _isDisposed;

		private Process _process;
		private Stream _inputStream;
		private Stream _outputStream;
		private Task _readLoopTask;
		private Task _stderrLoopTask;

		public bool IsReady { get; private set; }

		public event Action<JsonElement> DiagnosticsPublished;

		public LuaLanguageServerClient(string workspaceRootDirectoryPath, string serverExecutablePath, Func<object> settingsProvider)
		{
			_workspaceRootDirectoryPath = workspaceRootDirectoryPath ?? throw new ArgumentNullException(nameof(workspaceRootDirectoryPath));
			_serverExecutablePath = serverExecutablePath ?? throw new ArgumentNullException(nameof(serverExecutablePath));
			_settingsProvider = settingsProvider ?? throw new ArgumentNullException(nameof(settingsProvider));
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

				ThrowIfDisposed();
				ResetProcessState();

				var startInfo = new ProcessStartInfo
				{
					FileName = _serverExecutablePath,
					WorkingDirectory = Path.GetDirectoryName(_serverExecutablePath),
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

				_inputStream = _process.StandardOutput.BaseStream;
				_outputStream = _process.StandardInput.BaseStream;

				_readLoopTask = Task.Run(ReadLoopAsync);
				_stderrLoopTask = Task.Run(ReadStandardErrorLoopAsync);

				using var initializeTimeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
				initializeTimeout.CancelAfter(TimeSpan.FromSeconds(10));

				await SendRequestAsync("initialize", BuildInitializeParams(), initializeTimeout.Token).ConfigureAwait(false);

				IsReady = true;

				await SendNotificationAsync("initialized", new { }, cancellationToken).ConfigureAwait(false);
				await SendNotificationAsync("workspace/didChangeConfiguration", new { settings = _settingsProvider() }, cancellationToken).ConfigureAwait(false);

				return true;
			}
			catch
			{
				await DisposeProcessAsync().ConfigureAwait(false);
				return false;
			}
			finally
			{
				_startLock.Release();
			}
		}

		public Task SendNotificationAsync(string method, object parameters, CancellationToken cancellationToken)
			=> WriteMessageAsync(new { jsonrpc = "2.0", method, @params = parameters }, cancellationToken);

		public async Task<JsonElement> SendRequestAsync(string method, object parameters, CancellationToken cancellationToken)
		{
			ThrowIfDisposed();

			long requestId = Interlocked.Increment(ref _requestId);
			var responseSource = new TaskCompletionSource<JsonElement>(TaskCreationOptions.RunContinuationsAsynchronously);

			if (!_pendingRequests.TryAdd(requestId, responseSource))
				throw new InvalidOperationException("Unable to track a new language server request.");

			using var registration = cancellationToken.Register(() => responseSource.TrySetCanceled(cancellationToken));

			try
			{
				await WriteMessageAsync(new { jsonrpc = "2.0", id = requestId, method, @params = parameters }, cancellationToken).ConfigureAwait(false);
				return await responseSource.Task.ConfigureAwait(false);
			}
			finally
			{
				_pendingRequests.TryRemove(requestId, out _);
			}
		}

		private object BuildInitializeParams()
			=> new
			{
				processId = Process.GetCurrentProcess().Id,
				rootUri = CreateFileUri(_workspaceRootDirectoryPath),
				workspaceFolders = new[]
				{
					new
					{
						uri = CreateFileUri(_workspaceRootDirectoryPath),
						name = Path.GetFileName(_workspaceRootDirectoryPath)
					}
				},
				capabilities = new
				{
					workspace = new
					{
						workspaceFolders = true,
						configuration = true
					},
					textDocument = new
					{
						completion = new
						{
							completionItem = new
							{
								snippetSupport = false,
								documentationFormat = new[] { "plaintext", "markdown" }
							}
						},
						hover = new
						{
							contentFormat = new[] { "plaintext", "markdown" }
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
								documentationFormat = new[] { "plaintext", "markdown" },
								parameterInformation = new
								{
									labelOffsetSupport = true
								}
							},
							contextSupport = true
						}
					}
				}
			};

		private async Task ReadLoopAsync()
		{
			try
			{
				while (!_isDisposed)
				{
					Dictionary<string, string> headers = await ReadHeadersAsync().ConfigureAwait(false);

					if (headers is null || !headers.TryGetValue("Content-Length", out string contentLengthValue)
						|| !int.TryParse(contentLengthValue, out int contentLength) || contentLength <= 0)
						break;

					byte[] payloadBytes = await ReadPayloadAsync(contentLength).ConfigureAwait(false);

					if (payloadBytes is null)
						break;

					using JsonDocument document = JsonDocument.Parse(payloadBytes);
					await HandleMessageAsync(document.RootElement).ConfigureAwait(false);
				}
			}
			catch (Exception exception)
			{
				FailPendingRequests(exception);
			}
			finally
			{
				IsReady = false;
			}
		}

		private async Task ReadStandardErrorLoopAsync()
		{
			try
			{
				while (!_isDisposed && _process is not null && !_process.HasExited)
				{
					string line = await _process.StandardError.ReadLineAsync().ConfigureAwait(false);

					if (line is null)
						break;

					if (!string.IsNullOrWhiteSpace(line))
						Debug.WriteLine($"[LuaLS stderr] {line}");
				}
			}
			catch
			{
				// Ignore stderr read failures.
			}
		}

		private async Task<Dictionary<string, string>> ReadHeadersAsync()
		{
			var buffer = new List<byte>();
			var singleByte = new byte[1];

			while (true)
			{
				int bytesRead = await _inputStream.ReadAsync(singleByte.AsMemory(0, 1)).ConfigureAwait(false);

				if (bytesRead == 0)
					return null;

				buffer.Add(singleByte[0]);

				if (buffer.Count >= 4
					&& buffer[^4] == '\r'
					&& buffer[^3] == '\n'
					&& buffer[^2] == '\r'
					&& buffer[^1] == '\n')
					break;
			}

			string headerText = Encoding.ASCII.GetString(buffer.ToArray());
			var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

			foreach (string line in headerText.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries))
			{
				int separatorIndex = line.IndexOf(':');

				if (separatorIndex <= 0)
					continue;

				string key = line[..separatorIndex].Trim();
				string value = line[(separatorIndex + 1)..].Trim();

				headers[key] = value;
			}

			return headers;
		}

		private async Task<byte[]> ReadPayloadAsync(int contentLength)
		{
			byte[] payloadBytes = new byte[contentLength];
			int totalBytesRead = 0;

			while (totalBytesRead < contentLength)
			{
				int bytesRead = await _inputStream
					.ReadAsync(payloadBytes.AsMemory(totalBytesRead, contentLength - totalBytesRead))
					.ConfigureAwait(false);

				if (bytesRead == 0)
					return null;

				totalBytesRead += bytesRead;
			}

			return payloadBytes;
		}

		private async Task HandleMessageAsync(JsonElement message)
		{
			if (message.TryGetProperty("id", out JsonElement idElement))
			{
				if (message.TryGetProperty("method", out JsonElement methodElement))
				{
					JsonElement parameters = message.TryGetProperty("params", out JsonElement paramsElement)
						? paramsElement.Clone()
						: default;

					await HandleServerRequestAsync(idElement.Clone(), methodElement.GetString(), parameters).ConfigureAwait(false);
				}
				else
				{
					HandleServerResponse(idElement, message);
				}
			}
			else if (message.TryGetProperty("method", out JsonElement notificationMethodElement))
			{
				JsonElement parameters = message.TryGetProperty("params", out JsonElement paramsElement)
					? paramsElement.Clone()
					: default;

				HandleServerNotification(notificationMethodElement.GetString(), parameters);
			}
		}

		private void HandleServerResponse(JsonElement idElement, JsonElement message)
		{
			if (!idElement.TryGetInt64(out long requestId) || !_pendingRequests.TryGetValue(requestId, out TaskCompletionSource<JsonElement> responseSource))
				return;

			if (message.TryGetProperty("error", out JsonElement errorElement))
			{
				string messageText = errorElement.TryGetProperty("message", out JsonElement errorMessageElement)
					? errorMessageElement.GetString()
					: "Lua language server request failed.";

				responseSource.TrySetException(new InvalidOperationException(messageText));
				return;
			}

			if (message.TryGetProperty("result", out JsonElement resultElement))
				responseSource.TrySetResult(resultElement.Clone());
			else
				responseSource.TrySetResult(default);
		}

		private async Task HandleServerRequestAsync(JsonElement idElement, string method, JsonElement parameters)
		{
			object result = method switch
			{
				"workspace/configuration" => BuildConfigurationResponse(parameters),
				"workspace/workspaceFolders" => BuildWorkspaceFolderResponse(),
				"client/registerCapability" => null,
				"window/workDoneProgress/create" => null,
				_ => null
			};

			await WriteMessageAsync(new { jsonrpc = "2.0", id = idElement, result }, CancellationToken.None).ConfigureAwait(false);
		}

		private object[] BuildConfigurationResponse(JsonElement parameters)
		{
			if (!parameters.TryGetProperty("items", out JsonElement itemsElement) || itemsElement.ValueKind != JsonValueKind.Array)
				return Array.Empty<object>();

			JsonElement settingsElement = JsonSerializer.SerializeToElement(_settingsProvider());
			JsonElement luaElement = settingsElement.GetProperty("Lua");

			var results = new List<object>();

			foreach (JsonElement item in itemsElement.EnumerateArray())
			{
				string section = item.TryGetProperty("section", out JsonElement sectionElement)
					? sectionElement.GetString()
					: null;

				results.Add(GetConfigurationSection(settingsElement, luaElement, section));
			}

			return results.ToArray();
		}

		private static object GetConfigurationSection(JsonElement settingsElement, JsonElement luaElement, string section)
		{
			if (string.IsNullOrWhiteSpace(section))
				return settingsElement.Clone();

			if (section.Equals("Lua", StringComparison.OrdinalIgnoreCase))
				return luaElement.Clone();

			if (section.StartsWith("Lua.", StringComparison.OrdinalIgnoreCase))
			{
				JsonElement nestedSection = luaElement;
				string[] parts = section[4..].Split('.');

				foreach (string part in parts)
				{
					if (!nestedSection.TryGetProperty(part, out JsonElement nextSection))
						return new { };

					nestedSection = nextSection;
				}

				return nestedSection.Clone();
			}

			return new { };
		}

		private object[] BuildWorkspaceFolderResponse()
			=> new object[]
			{
				new
				{
					uri = CreateFileUri(_workspaceRootDirectoryPath),
					name = Path.GetFileName(_workspaceRootDirectoryPath)
				}
			};

		private void HandleServerNotification(string method, JsonElement parameters)
		{
			switch (method)
			{
				case "textDocument/publishDiagnostics":
					DiagnosticsPublished?.Invoke(parameters);
					return;
				case "window/logMessage":
				case "window/showMessage":
				case "telemetry/event":
				case "$/progress":
					return;
			}
		}

		private async Task WriteMessageAsync(object payload, CancellationToken cancellationToken)
		{
			ThrowIfDisposed();

			byte[] payloadBytes = JsonSerializer.SerializeToUtf8Bytes(payload);
			byte[] headerBytes = Encoding.ASCII.GetBytes($"Content-Length: {payloadBytes.Length}\r\n\r\n");

			await _writeLock.WaitAsync(cancellationToken).ConfigureAwait(false);

			try
			{
				await _outputStream.WriteAsync(headerBytes.AsMemory(0, headerBytes.Length), cancellationToken).ConfigureAwait(false);
				await _outputStream.WriteAsync(payloadBytes.AsMemory(0, payloadBytes.Length), cancellationToken).ConfigureAwait(false);
				await _outputStream.FlushAsync(cancellationToken).ConfigureAwait(false);
			}
			finally
			{
				_writeLock.Release();
			}
		}

		private void Process_Exited(object sender, EventArgs e)
		{
			int? exitCode = null;

			try
			{
				if (_process is not null && _process.HasExited)
					exitCode = _process.ExitCode;
			}
			catch
			{
				// Ignore exit-code access failures.
			}

			IsReady = false;
			Debug.WriteLine($"[LuaLS] Process exited unexpectedly{(exitCode is not null ? $" with code {exitCode.Value}" : string.Empty)}.");
			FailPendingRequests(new IOException("The Lua language server process exited unexpectedly."));
		}

		private void FailPendingRequests(Exception exception)
		{
			foreach (KeyValuePair<long, TaskCompletionSource<JsonElement>> pendingRequest in _pendingRequests)
				pendingRequest.Value.TrySetException(exception);
		}

		private async Task DisposeProcessAsync()
		{
			try
			{
				if (_process is not null && !_process.HasExited)
				{
					using var shutdownTimeout = new CancellationTokenSource(TimeSpan.FromSeconds(1));

					try
					{
						await SendRequestAsync("shutdown", new { }, shutdownTimeout.Token).ConfigureAwait(false);
					}
					catch
					{
						// Ignore shutdown failures.
					}

					try
					{
						await SendNotificationAsync("exit", new { }, shutdownTimeout.Token).ConfigureAwait(false);
					}
					catch
					{
						// Ignore exit notification failures.
					}

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
			_readLoopTask = null;
			_stderrLoopTask = null;
		}

		private void ThrowIfDisposed()
		{
			if (_isDisposed)
				throw new ObjectDisposedException(nameof(LuaLanguageServerClient));
		}

		private static string CreateFileUri(string path)
			=> new Uri(Path.GetFullPath(path)).AbsoluteUri;

		public void Dispose()
		{
			if (_isDisposed)
				return;

			_isDisposed = true;
			FailPendingRequests(new ObjectDisposedException(nameof(LuaLanguageServerClient)));

			DisposeProcessAsync().GetAwaiter().GetResult();

			_startLock.Dispose();
			_writeLock.Dispose();
		}
	}
}