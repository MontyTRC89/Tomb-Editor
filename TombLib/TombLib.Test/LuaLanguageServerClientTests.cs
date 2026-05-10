using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json;
using TombIDE.ScriptingStudio.Services.LuaIntellisense;

namespace TombLib.Test;

[TestClass]
public class LuaLanguageServerClientTests
{
	[TestMethod]
	public void CaptureServerCapabilities_UsesFullTextSyncWhenServerAdvertisesFullSync()
	{
		using var client = new LuaLanguageServerClient(@"C:\Workspace", "lua-language-server.exe", static () => new { });

		InvokePrivateMethod(client, "CaptureServerCapabilities", JsonSerializer.SerializeToElement(new
		{
			capabilities = new
			{
				textDocumentSync = new
				{
					change = 1
				}
			}
		}));

		Assert.AreEqual(LuaTextDocumentSyncKind.Full, client.TextDocumentSyncKind);
	}

	[TestMethod]
	public void CaptureServerCapabilities_RejectsMissingDocumentChangeSupport()
	{
		using var client = new LuaLanguageServerClient(@"C:\Workspace", "lua-language-server.exe", static () => new { });

		TargetInvocationException exception = Assert.ThrowsException<TargetInvocationException>(() =>
			InvokePrivateMethod(client, "CaptureServerCapabilities", JsonSerializer.SerializeToElement(new
			{
				capabilities = new
				{
					textDocumentSync = 0
				}
			})));

		Assert.IsInstanceOfType(exception.InnerException, typeof(NotSupportedException));
	}

	[TestMethod]
	public void CaptureServerCapabilities_RecognizesReferenceRenameAndFormattingProviders()
	{
		using var client = new LuaLanguageServerClient(@"C:\Workspace", "lua-language-server.exe", static () => new { });

		InvokePrivateMethod(client, "CaptureServerCapabilities", JsonSerializer.SerializeToElement(new
		{
			capabilities = new
			{
				referencesProvider = new { },
				renameProvider = new { prepareProvider = true },
				documentFormattingProvider = true
			}
		}));

		Assert.IsTrue(client.SupportsReferences);
		Assert.IsTrue(client.SupportsRename);
		Assert.IsTrue(client.SupportsFormatting);
	}

	[TestMethod]
	public void Dispose_WritesGracefulShutdownMessages()
	{
		using Process process = StartDisposableProcess();
		using var outputStream = new RecordingStream();
		using var client = new LuaLanguageServerClient(@"C:\Workspace", process.StartInfo.FileName, static () => new { });
		object session = CreateTransportSession(1, process, Stream.Null, outputStream);

		SetActiveSession(client, session);

		client.Dispose();

		string writtenPayload = outputStream.GetWrittenText();

		Assert.IsTrue(writtenPayload.Contains("\"method\":\"shutdown\"", StringComparison.Ordinal), writtenPayload);
		Assert.IsTrue(writtenPayload.Contains("\"method\":\"exit\"", StringComparison.Ordinal), writtenPayload);
	}

	[TestMethod]
	public async Task HandleServerRequestAsync_SemanticTokensRefresh_AcknowledgesAndRaisesEvent()
	{
		await using var outputStream = new RecordingStream();
		using var client = new LuaLanguageServerClient(@"C:\Workspace", "lua-language-server.exe", static () => new { });
		object session = CreateTransportSession(1, process: null, Stream.Null, outputStream);
		bool refreshRequested = false;

		client.SemanticTokensRefreshRequested += () => refreshRequested = true;

		await InvokePrivateTaskAsync(client, "HandleServerRequestAsync",
			session,
			JsonSerializer.SerializeToElement(7L),
			"workspace/semanticTokens/refresh",
			default(JsonElement)).ConfigureAwait(false);

		string writtenPayload = outputStream.GetWrittenText();

		Assert.IsTrue(refreshRequested);
		Assert.IsTrue(writtenPayload.Contains("\"id\":7", StringComparison.Ordinal), writtenPayload);
		Assert.IsTrue(writtenPayload.Contains("\"result\":null", StringComparison.Ordinal), writtenPayload);
	}

	[TestMethod]
	public async Task PumpDiagnosticsAsync_CoalescesQueuedDiagnosticsByFile()
	{
		using var client = new LuaLanguageServerClient(@"C:\Workspace", "lua-language-server.exe", static () => new { });
		int publishedCount = 0;
		string? lastMessage = null;

		client.DiagnosticsPublished += parameters =>
		{
			publishedCount++;
			lastMessage = parameters.GetProperty("diagnostics")[0].GetProperty("message").GetString();
			CancelLifetime(client);
		};

		InvokePrivateMethod(client, "RaiseDiagnosticsPublished", CreateDiagnosticsParameters("file:///C:/Workspace/test.lua", "Stale warning."));
		InvokePrivateMethod(client, "RaiseDiagnosticsPublished", CreateDiagnosticsParameters("file:///C:/Workspace/test.lua", "Current warning."));

		await InvokePrivateTaskAsync(client, "PumpDiagnosticsAsync").ConfigureAwait(false);

		Assert.AreEqual(1, publishedCount);
		Assert.AreEqual("Current warning.", lastMessage);
	}

	[TestMethod]
	public async Task ReadHeadersAndPayloadAsync_ReadsChunkedFrameAcrossBoundaries()
	{
		using var client = new LuaLanguageServerClient(@"C:\Workspace", "lua-language-server.exe", static () => new { });
		string payloadJson = "{\"jsonrpc\":\"2.0\",\"id\":1,\"result\":{\"value\":42}}";
		byte[] frame = CreateLspFrame(payloadJson);
		object session = CreateTransportSession(1, process: null, new ChunkedReadStream(SplitBytes(frame, 5, 7, 11, 3)), Stream.Null);

		int? contentLength = await InvokePrivateTaskAsync<int?>(client, "ReadHeadersAsync", session).ConfigureAwait(false);
		byte[]? payload = await InvokePrivateTaskAsync<byte[]?>(client, "ReadPayloadAsync", session, contentLength!.Value).ConfigureAwait(false);

		Assert.AreEqual(Encoding.UTF8.GetByteCount(payloadJson), contentLength);
		Assert.IsNotNull(payload);
		Assert.AreEqual(payloadJson, Encoding.UTF8.GetString(payload));
	}

	[TestMethod]
	public async Task ReadLoopAsync_HandlesChunkedSemanticTokensRefreshRequest()
	{
		await using var outputStream = new RecordingStream();
		using var client = new LuaLanguageServerClient(@"C:\Workspace", "lua-language-server.exe", static () => new { });
		bool refreshRequested = false;
		byte[] frame = CreateLspFrame("{\"jsonrpc\":\"2.0\",\"id\":9,\"method\":\"workspace/semanticTokens/refresh\"}");
		object session = CreateTransportSession(1, process: null, new ChunkedReadStream(SplitBytes(frame, 4, 9, 6, 13)), outputStream);

		client.SemanticTokensRefreshRequested += () => refreshRequested = true;

		await InvokePrivateTaskAsync(client, "ReadLoopAsync", session).ConfigureAwait(false);

		string writtenPayload = outputStream.GetWrittenText();

		Assert.IsTrue(refreshRequested);
		Assert.IsTrue(writtenPayload.Contains("\"id\":9", StringComparison.Ordinal), writtenPayload);
		Assert.IsTrue(writtenPayload.Contains("\"result\":null", StringComparison.Ordinal), writtenPayload);
	}

	[TestMethod]
	public async Task ReadLoopAsync_FailsPendingRequestsWhenConnectionCloses()
	{
		using var client = new LuaLanguageServerClient(@"C:\Workspace", "lua-language-server.exe", static () => new { });
		var pendingRequest = new TaskCompletionSource<JsonElement>(TaskCreationOptions.RunContinuationsAsynchronously);
		object session = CreateTransportSession(1, process: null, new ChunkedReadStream(), Stream.Null);

		GetPendingRequests(session).TryAdd(42, pendingRequest);

		await InvokePrivateTaskAsync(client, "ReadLoopAsync", session).ConfigureAwait(false);

		await Assert.ThrowsExceptionAsync<IOException>(() => pendingRequest.Task).ConfigureAwait(false);
	}

	[TestMethod]
	public async Task ReadLoopAsync_CompletesPendingRequestFromChunkedResponse()
	{
		using var client = new LuaLanguageServerClient(@"C:\Workspace", "lua-language-server.exe", static () => new { });
		var pendingRequest = new TaskCompletionSource<JsonElement>(TaskCreationOptions.RunContinuationsAsynchronously);
		byte[] frame = CreateLspFrame("{\"jsonrpc\":\"2.0\",\"id\":42,\"result\":{\"label\":\"ok\"}}");
		object session = CreateTransportSession(1, process: null, new ChunkedReadStream(SplitBytes(frame, 6, 8, 5, 9)), Stream.Null);

		GetPendingRequests(session).TryAdd(42, pendingRequest);

		await InvokePrivateTaskAsync(client, "ReadLoopAsync", session).ConfigureAwait(false);

		JsonElement result = await pendingRequest.Task.ConfigureAwait(false);

		Assert.AreEqual("ok", result.GetProperty("label").GetString());
	}

	[TestMethod]
	public async Task ReadLoopAsync_OldTransportGenerationDoesNotFailActiveSessionRequests()
	{
		using var client = new LuaLanguageServerClient(@"C:\Workspace", "lua-language-server.exe", static () => new { });
		object oldSession = CreateTransportSession(1, process: null, new ChunkedReadStream(), Stream.Null);
		object newSession = CreateTransportSession(2, process: null, new ChunkedReadStream(), Stream.Null);
		var oldPendingRequest = new TaskCompletionSource<JsonElement>(TaskCreationOptions.RunContinuationsAsynchronously);
		var newPendingRequest = new TaskCompletionSource<JsonElement>(TaskCreationOptions.RunContinuationsAsynchronously);

		GetPendingRequests(oldSession).TryAdd(41, oldPendingRequest);
		GetPendingRequests(newSession).TryAdd(42, newPendingRequest);
		SetActiveSession(client, newSession);

		await InvokePrivateTaskAsync(client, "ReadLoopAsync", oldSession).ConfigureAwait(false);

		await Assert.ThrowsExceptionAsync<IOException>(() => oldPendingRequest.Task).ConfigureAwait(false);
		Assert.IsFalse(newPendingRequest.Task.IsCompleted);
	}

	private static Process StartDisposableProcess()
	{
		var startInfo = new ProcessStartInfo
		{
			FileName = Path.Combine(Environment.SystemDirectory, "cmd.exe"),
			Arguments = "/c ping 127.0.0.1 -n 10 > nul",
			UseShellExecute = false,
			CreateNoWindow = true,
			RedirectStandardInput = true,
			RedirectStandardOutput = true,
			RedirectStandardError = true
		};

		return Process.Start(startInfo)
			?? throw new InvalidOperationException("Unable to start the disposable test process.");
	}

	private static void SetPrivateField(object instance, string fieldName, object? value)
	{
		FieldInfo field = instance.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
			?? throw new InvalidOperationException($"Private field '{fieldName}' was not found.");

		field.SetValue(instance, value);
	}

	private static object CreateTransportSession(long generation, Process? process, Stream inputStream, Stream outputStream)
	{
		Type sessionType = typeof(LuaLanguageServerClient).GetNestedType("LuaLanguageServerTransportSession", BindingFlags.NonPublic)
			?? throw new InvalidOperationException("Nested type 'LuaLanguageServerTransportSession' was not found.");

		ConstructorInfo constructor = sessionType.GetConstructor(
			BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
			binder: null,
			[typeof(long), typeof(Process), typeof(Stream), typeof(Stream)],
			modifiers: null)
			?? throw new InvalidOperationException("Lua transport session constructor was not found.");

		return constructor.Invoke([generation, process, inputStream, outputStream]);
	}

	private static void SetActiveSession(LuaLanguageServerClient client, object session)
	{
		SetPrivateField(client, "_activeSession", session);
		SetPrivateField(client, "_activeTransportGeneration", GetTransportGeneration(session));
	}

	private static long GetTransportGeneration(object session)
	{
		PropertyInfo property = session.GetType().GetProperty("Generation", BindingFlags.Instance | BindingFlags.Public)
			?? throw new InvalidOperationException("Transport session property 'Generation' was not found.");

		return (long)(property.GetValue(session)
			?? throw new InvalidOperationException("Transport session generation value was null."));
	}

	private static void InvokePrivateMethod(object instance, string methodName, params object?[] parameters)
	{
		MethodInfo method = instance.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)
			?? throw new InvalidOperationException($"Private method '{methodName}' was not found.");

		method.Invoke(instance, parameters);
	}

	private static async Task InvokePrivateTaskAsync(object instance, string methodName, params object?[] parameters)
	{
		MethodInfo method = instance.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)
			?? throw new InvalidOperationException($"Private method '{methodName}' was not found.");

		Task task = (Task)(method.Invoke(instance, parameters)
			?? throw new InvalidOperationException($"Private method '{methodName}' returned null instead of a Task."));

		await task.ConfigureAwait(false);
	}

	private static async Task<T> InvokePrivateTaskAsync<T>(object instance, string methodName, params object?[] parameters)
	{
		MethodInfo method = instance.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)
			?? throw new InvalidOperationException($"Private method '{methodName}' was not found.");

		Task<T> task = (Task<T>)(method.Invoke(instance, parameters)
			?? throw new InvalidOperationException($"Private method '{methodName}' returned null instead of a Task."));

		return await task.ConfigureAwait(false);
	}

	private static JsonElement CreateDiagnosticsParameters(string uri, string message)
		=> JsonSerializer.SerializeToElement(new
		{
			uri,
			diagnostics = new[]
			{
				new
				{
					message,
					range = new
					{
						start = new { line = 0, character = 0 },
						end = new { line = 0, character = 1 }
					}
				}
			}
		});

	private static void CancelLifetime(LuaLanguageServerClient client)
	{
		FieldInfo field = typeof(LuaLanguageServerClient).GetField("_lifetimeCts", BindingFlags.Instance | BindingFlags.NonPublic)
			?? throw new InvalidOperationException("Private field '_lifetimeCts' was not found.");

		((CancellationTokenSource)field.GetValue(client)!).Cancel();
	}

	private static ConcurrentDictionary<long, TaskCompletionSource<JsonElement>> GetPendingRequests(object session)
	{
		PropertyInfo property = session.GetType().GetProperty("PendingRequests", BindingFlags.Instance | BindingFlags.Public)
			?? throw new InvalidOperationException("Transport session property 'PendingRequests' was not found.");

		return (ConcurrentDictionary<long, TaskCompletionSource<JsonElement>>)(property.GetValue(session)
			?? throw new InvalidOperationException("Transport session pending request map was null."));
	}

	private static byte[] CreateLspFrame(string json)
	{
		byte[] payload = Encoding.UTF8.GetBytes(json);
		byte[] header = Encoding.ASCII.GetBytes($"Content-Length: {payload.Length}\r\n\r\n");
		byte[] frame = new byte[header.Length + payload.Length];

		Buffer.BlockCopy(header, 0, frame, 0, header.Length);
		Buffer.BlockCopy(payload, 0, frame, header.Length, payload.Length);
		return frame;
	}

	private static byte[][] SplitBytes(byte[] source, params int[] chunkLengths)
	{
		var chunks = new List<byte[]>();
		int offset = 0;

		for (int i = 0; i < chunkLengths.Length && offset < source.Length; i++)
		{
			int length = Math.Min(chunkLengths[i], source.Length - offset);

			if (length <= 0)
				continue;

			byte[] chunk = new byte[length];
			Buffer.BlockCopy(source, offset, chunk, 0, length);
			chunks.Add(chunk);
			offset += length;
		}

		if (offset < source.Length)
		{
			byte[] remainder = new byte[source.Length - offset];
			Buffer.BlockCopy(source, offset, remainder, 0, remainder.Length);
			chunks.Add(remainder);
		}

		return [.. chunks];
	}

	private sealed class RecordingStream : Stream
	{
		private readonly MemoryStream _innerStream = new();

		public override bool CanRead => false;
		public override bool CanSeek => false;
		public override bool CanWrite => true;
		public override long Length => _innerStream.Length;

		public override long Position
		{
			get => _innerStream.Position;
			set => _innerStream.Position = value;
		}

		public string GetWrittenText()
			=> Encoding.UTF8.GetString(_innerStream.ToArray());

		public override void Flush()
			=> _innerStream.Flush();

		public override int Read(byte[] buffer, int offset, int count)
			=> throw new NotSupportedException();

		public override long Seek(long offset, SeekOrigin origin)
			=> throw new NotSupportedException();

		public override void SetLength(long value)
			=> _innerStream.SetLength(value);

		public override void Write(byte[] buffer, int offset, int count)
			=> _innerStream.Write(buffer, offset, count);

		public override void Write(ReadOnlySpan<byte> buffer)
			=> _innerStream.Write(buffer);

		public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
			=> _innerStream.WriteAsync(buffer, cancellationToken);

		protected override void Dispose(bool disposing)
		{
			if (disposing)
				_innerStream.Flush();

			base.Dispose(disposing);
		}
	}

	private sealed class ChunkedReadStream(params byte[][] chunks) : Stream
	{
		private readonly IReadOnlyList<byte[]> _chunks = chunks;
		private int _chunkIndex;
		private int _chunkOffset;

		public override bool CanRead => true;
		public override bool CanSeek => false;
		public override bool CanWrite => false;
		public override long Length => throw new NotSupportedException();
		public override long Position
		{
			get => throw new NotSupportedException();
			set => throw new NotSupportedException();
		}

		public override int Read(byte[] buffer, int offset, int count)
			=> ReadAsync(buffer.AsMemory(offset, count)).GetAwaiter().GetResult();

		public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
		{
			cancellationToken.ThrowIfCancellationRequested();

			if (_chunkIndex >= _chunks.Count)
				return ValueTask.FromResult(0);

			byte[] chunk = _chunks[_chunkIndex];
			int bytesAvailable = chunk.Length - _chunkOffset;
			int bytesToCopy = Math.Min(buffer.Length, bytesAvailable);

			chunk.AsMemory(_chunkOffset, bytesToCopy).CopyTo(buffer);
			_chunkOffset += bytesToCopy;

			if (_chunkOffset >= chunk.Length)
			{
				_chunkIndex++;
				_chunkOffset = 0;
			}

			return ValueTask.FromResult(bytesToCopy);
		}

		public override void Flush()
		{ }

		public override long Seek(long offset, SeekOrigin origin)
			=> throw new NotSupportedException();

		public override void SetLength(long value)
			=> throw new NotSupportedException();

		public override void Write(byte[] buffer, int offset, int count)
			=> throw new NotSupportedException();
	}
}
