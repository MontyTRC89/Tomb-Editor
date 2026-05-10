using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json;
using StreamJsonRpc;
using TombIDE.ScriptingStudio.Services.LuaIntellisense;

namespace TombLib.Test;

[TestClass]
public class LuaLanguageServerClientTests
{
	[TestMethod]
	public void CaptureServerCapabilities_UsesFullTextSyncWhenServerAdvertisesFullSync()
	{
		using var client = new LuaLanguageServerClient(@"C:\Workspace", "lua-language-server.exe", static () => new { });

		InvokePrivateMethod(client, "CaptureServerCapabilities", DeserializeInitializeResponse(
			"""
			{
			  "capabilities": {
			    "textDocumentSync": {
			      "change": 1
			    }
			  }
			}
			"""));

		Assert.AreEqual(LuaTextDocumentSyncKind.Full, client.TextDocumentSyncKind);
	}

	[TestMethod]
	public void CaptureServerCapabilities_RejectsMissingDocumentChangeSupport()
	{
		using var client = new LuaLanguageServerClient(@"C:\Workspace", "lua-language-server.exe", static () => new { });

		TargetInvocationException exception = Assert.ThrowsException<TargetInvocationException>(() =>
			InvokePrivateMethod(client, "CaptureServerCapabilities", DeserializeInitializeResponse(
				"""
				{
				  "capabilities": {
				    "textDocumentSync": {}
				  }
				}
				""")));

		Assert.IsInstanceOfType(exception.InnerException, typeof(NotSupportedException));
	}

	[TestMethod]
	public void CaptureServerCapabilities_RecognizesReferenceRenameAndFormattingProviders()
	{
		using var client = new LuaLanguageServerClient(@"C:\Workspace", "lua-language-server.exe", static () => new { });

		InvokePrivateMethod(client, "CaptureServerCapabilities", DeserializeInitializeResponse(
			"""
			{
			  "capabilities": {
			    "referencesProvider": {},
			    "renameProvider": { "prepareProvider": true },
			    "documentFormattingProvider": true
			  }
			}
			"""));

		Assert.IsTrue(client.SupportsReferences);
		Assert.IsTrue(client.SupportsRename);
		Assert.IsTrue(client.SupportsFormatting);
	}

	[TestMethod]
	public void CaptureServerCapabilities_RecognizesSemanticTokensLegendAndDeltaSupport()
	{
		using var client = new LuaLanguageServerClient(@"C:\Workspace", "lua-language-server.exe", static () => new { });

		InvokePrivateMethod(client, "CaptureServerCapabilities", DeserializeInitializeResponse(
			"""
			{
			  "capabilities": {
			    "semanticTokensProvider": {
			      "full": {
			        "delta": true
			      },
			      "legend": {
			        "tokenTypes": ["function", "variable"],
			        "tokenModifiers": ["declaration"]
			      }
			    }
			  }
			}
			"""));

		Assert.IsTrue(client.SupportsSemanticTokensDelta);
		CollectionAssert.AreEqual(new[] { "function", "variable" }, client.SemanticTokenTypes.ToArray());
		CollectionAssert.AreEqual(new[] { "declaration" }, client.SemanticTokenModifiers.ToArray());
	}

	[TestMethod]
	public void Dispose_WritesGracefulShutdownMessages()
	{
		using Process process = StartDisposableProcess();
		using var inputStream = new PendingReadStream();
		using var outputStream = new RecordingStream();
		using var client = new LuaLanguageServerClient(@"C:\Workspace", process.StartInfo.FileName, static () => new { });
		object session = CreateTransportSession(client, 1, process, inputStream, outputStream, startListening: true);

		SetActiveSession(client, session);

		client.Dispose();

		string writtenPayload = outputStream.GetWrittenText();

		Assert.IsTrue(writtenPayload.Contains("\"method\":\"shutdown\"", StringComparison.Ordinal), writtenPayload);
		Assert.IsTrue(writtenPayload.Contains("\"method\":\"exit\"", StringComparison.Ordinal), writtenPayload);
	}

	[TestMethod]
	public async Task HandleSemanticTokensRefreshRequestAsync_RaisesEventAndReturnsNull()
	{
		using var client = new LuaLanguageServerClient(@"C:\Workspace", "lua-language-server.exe", static () => new { });
		bool refreshRequested = false;

		object rpcTarget = CreateRpcTarget(client);

		client.SemanticTokensRefreshRequested += () => refreshRequested = true;

		object? result = await InvokePrivateTaskAsync<object?>(rpcTarget, "RefreshSemanticTokensAsync").ConfigureAwait(false);

		Assert.IsTrue(refreshRequested);
		Assert.IsNull(result);
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
			lastMessage = parameters.Diagnostics?[0].Message;
			CancelLifetime(client);
		};

		InvokePrivateMethod(client, "RaiseDiagnosticsPublished", 0L, CreateDiagnosticsParameters("file:///C:/Workspace/test.lua", "Stale warning."));
		InvokePrivateMethod(client, "RaiseDiagnosticsPublished", 0L, CreateDiagnosticsParameters("file:///C:/Workspace/test.lua", "Current warning."));

		await InvokePrivateTaskAsync(client, "PumpDiagnosticsAsync").ConfigureAwait(false);

		Assert.AreEqual(1, publishedCount);
		Assert.AreEqual("Current warning.", lastMessage);
	}

	[TestMethod]
	public async Task PumpDiagnosticsAsync_DropsQueuedDiagnosticsFromInactiveTransportGeneration()
	{
		using var client = new LuaLanguageServerClient(@"C:\Workspace", "lua-language-server.exe", static () => new { });
		object oldSession = CreateTransportSession(client, 1, process: null, Stream.Null, Stream.Null);
		object newSession = CreateTransportSession(client, 2, process: null, Stream.Null, Stream.Null);
		int publishedCount = 0;
		string? lastMessage = null;

		SetActiveSession(client, newSession);

		client.DiagnosticsPublished += parameters =>
		{
			publishedCount++;
			lastMessage = parameters.Diagnostics?[0].Message;
			CancelLifetime(client);
		};

		InvokePrivateMethod(client, "RaiseDiagnosticsPublished", GetTransportGeneration(oldSession),
			CreateDiagnosticsParameters("file:///C:/Workspace/stale.lua", "Stale warning."));
		InvokePrivateMethod(client, "RaiseDiagnosticsPublished", GetTransportGeneration(newSession),
			CreateDiagnosticsParameters("file:///C:/Workspace/current.lua", "Current warning."));

		await InvokePrivateTaskAsync(client, "PumpDiagnosticsAsync").ConfigureAwait(false);

		Assert.AreEqual(1, publishedCount);
		Assert.AreEqual("Current warning.", lastMessage);
	}

	[TestMethod]
	public void BuildConfigurationResponse_ReturnsRequestedLuaSections()
	{
		using var client = new LuaLanguageServerClient(@"C:\Workspace", "lua-language-server.exe", static () => new
		{
			Lua = new
			{
				runtime = new
				{
					version = "Lua 5.4"
				}
			}
		});

		object[] response = (object[])InvokePrivateMethodWithReturn(client, "BuildConfigurationResponse",
			new LuaWorkspaceConfigurationParams(
			[
				new LuaWorkspaceConfigurationItem("Lua"),
				new LuaWorkspaceConfigurationItem("Lua.runtime")
			]));

		Assert.AreEqual(2, response.Length);
		Assert.AreEqual("Lua 5.4", JsonSerializer.SerializeToElement(response[1]).GetProperty("version").GetString());
	}

	[TestMethod]
	public void JsonRpc_Disconnected_OldTransportGenerationDoesNotAffectActiveSession()
	{
		using var client = new LuaLanguageServerClient(@"C:\Workspace", "lua-language-server.exe", static () => new { });
		object oldSession = CreateTransportSession(client, 1, process: null, Stream.Null, Stream.Null);
		object newSession = CreateTransportSession(client, 2, process: null, Stream.Null, Stream.Null);

		SetActiveSession(client, newSession);
		SetPrivateField(client, "_isReady", true);

		InvokePrivateMethod(client, "JsonRpc_Disconnected",
			oldSession,
			new JsonRpcDisconnectedEventArgs("old transport closed", DisconnectedReason.LocallyDisposed));

		Assert.IsTrue(client.IsReady);

		InvokePrivateMethod(client, "JsonRpc_Disconnected",
			newSession,
			new JsonRpcDisconnectedEventArgs("active transport closed", DisconnectedReason.LocallyDisposed));

		Assert.IsFalse(client.IsReady);
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

	private static object CreateTransportSession(LuaLanguageServerClient client, long generation, Process? process, Stream inputStream, Stream outputStream, bool startListening = false)
	{
		Type sessionType = typeof(LuaLanguageServerClient).GetNestedType("LuaLanguageServerTransportSession", BindingFlags.NonPublic)
			?? throw new InvalidOperationException("Nested type 'LuaLanguageServerTransportSession' was not found.");

		ConstructorInfo constructor = sessionType.GetConstructor(
			BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
			binder: null,
			[typeof(long), typeof(Process), typeof(Stream), typeof(Stream)],
			modifiers: null)
			?? throw new InvalidOperationException("Lua transport session constructor was not found.");

		object session = constructor.Invoke([generation, process, inputStream, outputStream]);
		object messageHandler = InvokePrivateStaticMethodWithReturn(typeof(LuaLanguageServerClient), "CreateMessageHandler", outputStream, inputStream);
		object rpcTarget = CreateRpcTarget(client);

		SetSessionProperty(session, "MessageHandler", messageHandler);
		SetSessionProperty(session, "RpcTarget", rpcTarget);
		object jsonRpc = InvokePrivateMethodWithReturn(client, "CreateJsonRpc", session);
		SetSessionProperty(session, "JsonRpc", jsonRpc);
		SetSessionProperty(session, "RpcCompletionTask", GetPropertyValue(jsonRpc, "Completion"));

		if (startListening)
			((JsonRpc)jsonRpc).StartListening();

		return session;
	}

	private static object CreateRpcTarget(LuaLanguageServerClient client, long generation = 0)
	{
		Type targetType = typeof(LuaLanguageServerClient).GetNestedType("LuaLanguageServerClientRpcTarget", BindingFlags.NonPublic)
			?? throw new InvalidOperationException("Nested type 'LuaLanguageServerClientRpcTarget' was not found.");

		ConstructorInfo constructor = targetType.GetConstructor(
			BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
			binder: null,
			[typeof(LuaLanguageServerClient), typeof(long)],
			modifiers: null)
			?? throw new InvalidOperationException("Lua RPC target constructor was not found.");

		return constructor.Invoke([client, generation]);
	}

	private static object GetPropertyValue(object instance, string propertyName)
	{
		PropertyInfo property = instance.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
			?? throw new InvalidOperationException($"Property '{propertyName}' was not found.");

		return property.GetValue(instance)
			?? throw new InvalidOperationException($"Property '{propertyName}' returned null.");
	}

	private static void SetSessionProperty(object session, string propertyName, object? value)
	{
		PropertyInfo property = session.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
			?? throw new InvalidOperationException($"Transport session property '{propertyName}' was not found.");

		property.SetValue(session, value);
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

	private static object InvokePrivateMethodWithReturn(object instance, string methodName, params object?[] parameters)
	{
		MethodInfo method = instance.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)
			?? throw new InvalidOperationException($"Private method '{methodName}' was not found.");

		return method.Invoke(instance, parameters)
			?? throw new InvalidOperationException($"Private method '{methodName}' returned null.");
	}

	private static object InvokePrivateStaticMethodWithReturn(Type type, string methodName, params object?[] parameters)
	{
		MethodInfo method = type.GetMethod(methodName, BindingFlags.Static | BindingFlags.NonPublic)
			?? throw new InvalidOperationException($"Private static method '{methodName}' was not found.");

		return method.Invoke(obj: null, parameters)
			?? throw new InvalidOperationException($"Private static method '{methodName}' returned null.");
	}

	private static async Task InvokePrivateTaskAsync(object instance, string methodName, params object?[] parameters)
	{
		MethodInfo method = instance.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
			?? throw new InvalidOperationException($"Method '{methodName}' was not found.");

		Task task = (Task)(method.Invoke(instance, parameters)
			?? throw new InvalidOperationException($"Method '{methodName}' returned null instead of a Task."));

		await task.ConfigureAwait(false);
	}

	private static async Task<T> InvokePrivateTaskAsync<T>(object instance, string methodName, params object?[] parameters)
	{
		MethodInfo method = instance.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
			?? throw new InvalidOperationException($"Method '{methodName}' was not found.");

		Task<T> task = (Task<T>)(method.Invoke(instance, parameters)
			?? throw new InvalidOperationException($"Method '{methodName}' returned null instead of a Task."));

		return await task.ConfigureAwait(false);
	}

	private static LuaInitializeResponse DeserializeInitializeResponse(string json)
		=> JsonSerializer.Deserialize<LuaInitializeResponse>(json)
			?? throw new InvalidOperationException("Failed to deserialize the Lua initialize response test payload.");

	private static LuaPublishDiagnosticsParams CreateDiagnosticsParameters(string uri, string message)
		=> new(
			uri,
			Version: null,
			Diagnostics:
			[
				new LuaDiagnosticPayload(
					new LuaProtocolRangePayload(
						new LuaProtocolNullablePosition(0, 0),
						new LuaProtocolNullablePosition(0, 1)),
					Severity: null,
					Message: message,
					Source: null,
					Code: null)
			]);

	private static void CancelLifetime(LuaLanguageServerClient client)
	{
		FieldInfo field = typeof(LuaLanguageServerClient).GetField("_lifetimeCts", BindingFlags.Instance | BindingFlags.NonPublic)
			?? throw new InvalidOperationException("Private field '_lifetimeCts' was not found.");

		((CancellationTokenSource)field.GetValue(client)!).Cancel();
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

	private sealed class PendingReadStream : Stream
	{
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
			=> throw new NotSupportedException();

		public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
		{
			try
			{
				await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken).ConfigureAwait(false);
			}
			catch (OperationCanceledException)
			{ }

			return 0;
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
