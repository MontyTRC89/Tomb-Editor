using NLog;
using StreamJsonRpc;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace TombLib.LanguageServer.Core.Tests;

[TestClass]
public partial class LanguageServerClientTests
{
	private static readonly LanguageServerClientOptions DefaultClientOptions = new(static () => new { });

	[TestMethod]
	public async Task SendNotificationAsync_CompletesAfterLocalDispatchEvenWhenTransportWriteRemainsBlocked()
	{
		using var blockingWriteStream = new BlockingWriteStream();
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", DefaultClientOptions);
		object session = CreateTransportSession(client, 1, process: null, blockingWriteStream, Stream.Null);
		using var cancellationSource = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));

		SetActiveSession(client, session);
		SetReadyState(client, true);

		Task notificationTask = client.SendNotificationAsync(
			"workspace/didChangeConfiguration",
			new { settings = new { } },
			cancellationSource.Token);

		try
		{
			Task completedTask = await Task.WhenAny(notificationTask, Task.Delay(TimeSpan.FromSeconds(1))).ConfigureAwait(false);
			Assert.AreSame(notificationTask, completedTask);
			await notificationTask.ConfigureAwait(false);
		}
		finally
		{
			blockingWriteStream.Release();
		}
	}

	[TestMethod]
	public void CapabilityRegistrationParams_Deserialize_BindsRegistrationsWireProperty()
	{
		CapabilityRegistrationParams parameters = JsonSerializer.Deserialize<CapabilityRegistrationParams>(
			"""
			{
			  "registrations": [
			    { "id": "1", "method": "textDocument/rename" }
			  ]
			}
			""");

		Assert.IsNotNull(parameters.Registrations);
		Assert.AreEqual(1, parameters.Registrations.Length);
		Assert.AreEqual("1", parameters.Registrations[0].Id);
		Assert.AreEqual("textDocument/rename", parameters.Registrations[0].Method);
	}

	[TestMethod]
	public void CapabilityUnregistrationParams_Deserialize_BindsHistoricalWireProperty()
	{
		CapabilityUnregistrationParams parameters = JsonSerializer.Deserialize<CapabilityUnregistrationParams>(
			"""
			{
			  "unregisterations": [
			    { "id": "1", "method": "textDocument/rename" }
			  ]
			}
			""");

		Assert.IsNotNull(parameters.Unregistrations);
		Assert.AreEqual(1, parameters.Unregistrations.Length);
		Assert.AreEqual("1", parameters.Unregistrations[0].Id);
		Assert.AreEqual("textDocument/rename", parameters.Unregistrations[0].Method);
	}

	[TestMethod]
	public void CapabilityUnregistrationParams_Deserialize_BindsCorrectedPropertyName()
	{
		CapabilityUnregistrationParams parameters = JsonSerializer.Deserialize<CapabilityUnregistrationParams>(
			"""
			{
			  "unregistrations": [
			    { "id": "1", "method": "textDocument/rename" }
			  ]
			}
			""");

		Assert.IsNotNull(parameters.Unregistrations);
		Assert.AreEqual(1, parameters.Unregistrations.Length);
		Assert.AreEqual("1", parameters.Unregistrations[0].Id);
		Assert.AreEqual("textDocument/rename", parameters.Unregistrations[0].Method);
	}

	[TestMethod]
	public void CapabilityUnregistrationParams_Serialize_WritesCurrentSpecPropertyName()
	{
		string json = JsonSerializer.Serialize(
			new CapabilityUnregistrationParams(
			[
				new CapabilityUnregistrationPayload("1", "textDocument/rename")
			]));

		using JsonDocument document = JsonDocument.Parse(json);
		JsonElement root = document.RootElement;

		Assert.IsTrue(root.TryGetProperty("unregisterations", out JsonElement unregistrations));
		Assert.IsFalse(root.TryGetProperty("unregistrations", out _));
		Assert.AreEqual(JsonValueKind.Array, unregistrations.ValueKind);
		Assert.AreEqual(1, unregistrations.GetArrayLength());
	}

	[TestMethod]
	public void GetRequiredReadySession_WhenClientIsNotReady_ThrowsIOException()
	{
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", DefaultClientOptions);
		object session = CreateTransportSession(client, 1, process: null, Stream.Null, Stream.Null);

		SetActiveSession(client, session);

		Assert.IsFalse(client.IsReady);

		TargetInvocationException exception = Assert.ThrowsException<TargetInvocationException>(() =>
			InvokePrivateMethodWithReturn(client, "GetRequiredReadySession", false));

		Assert.IsInstanceOfType(exception.InnerException, typeof(IOException));
	}

	[TestMethod]
	public void GetRequiredReadySession_WhenActiveSessionGenerationDoesNotMatchPublishedReadyGeneration_ThrowsIOException()
	{
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", DefaultClientOptions);
		object readySession = CreateTransportSession(client, 1, process: null, Stream.Null, Stream.Null);
		object replacementSession = CreateTransportSession(client, 2, process: null, Stream.Null, Stream.Null);

		SetActiveSession(client, readySession);
		SetReadyState(client, true);
		SetPrivateField(client, "_activeSession", replacementSession);

		TargetInvocationException exception = Assert.ThrowsException<TargetInvocationException>(() =>
			InvokePrivateMethodWithReturn(client, "GetRequiredReadySession", false));

		Assert.IsInstanceOfType(exception.InnerException, typeof(IOException));
	}

	[TestMethod]
	public async Task SendNotificationAsync_WhenClientIsNotReady_ThrowsIOException()
	{
		using var blockingWriteStream = new BlockingWriteStream();
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", DefaultClientOptions);
		object session = CreateTransportSession(client, 1, process: null, blockingWriteStream, Stream.Null);

		SetActiveSession(client, session);

		Assert.IsFalse(client.IsReady);

		await Assert.ThrowsExceptionAsync<IOException>(async () =>
			await client.SendNotificationAsync(
				"workspace/didChangeConfiguration",
				new { settings = new { } },
				CancellationToken.None).ConfigureAwait(false)).ConfigureAwait(false);
	}

	[TestMethod]
	public async Task SendNotificationAsync_WhenPayloadSerializationFails_DoesNotInvalidateTransport()
	{
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", DefaultClientOptions);
		object session = CreateTransportSession(client, 10, process: null, Stream.Null, Stream.Null);
		var cyclicPayload = new Dictionary<string, object>();

		cyclicPayload["self"] = cyclicPayload;

		SetActiveSession(client, session);
		SetReadyState(client, true);

		Exception? observedException = null;

		try
		{
			await client.SendNotificationAsync("workspace/didChangeWatchedFiles", cyclicPayload, CancellationToken.None).ConfigureAwait(false);
			Assert.Fail("Expected the notification serialization to fail.");
		}
		catch (Exception exception)
		{
			observedException = exception;
		}

		Assert.IsNotNull(observedException);
		Assert.IsFalse(observedException is LanguageServerTransportUnavailableException);
		Assert.IsTrue(client.IsReady);
		Assert.AreEqual(10L, client.TransportGeneration);
	}

	[TestMethod]
	public async Task SendRequestAsync_WhenClientIsNotReady_ThrowsIOException()
	{
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", DefaultClientOptions);
		object session = CreateTransportSession(client, 1, process: null, Stream.Null, Stream.Null);

		SetActiveSession(client, session);

		Assert.IsFalse(client.IsReady);

		await Assert.ThrowsExceptionAsync<IOException>(async () =>
			await client.SendRequestAsync<JsonElement>(
				"workspace/configuration",
				new WorkspaceConfigurationParams([]),
				CancellationToken.None).ConfigureAwait(false)).ConfigureAwait(false);
	}

	[TestMethod]
	public async Task MarkTransportUnhealthy_DuringInFlightRequest_DoesNotCancelOwnedSessionButBlocksFutureRequests()
	{
		using var serverOutputStream = new PendingReadStream();
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", DefaultClientOptions);
		object session = CreateTransportSession(client, 1, process: null, serverOutputStream, Stream.Null, startListening: true);

		SetActiveSession(client, session);
		SetReadyState(client, true);

		Task<JsonElement> requestTask = client.SendRequestAsync<JsonElement>(
			"workspace/configuration",
			new WorkspaceConfigurationParams([]),
			CancellationToken.None);

		Task completedTask = await Task.WhenAny(requestTask, Task.Delay(TimeSpan.FromMilliseconds(100))).ConfigureAwait(false);
		Assert.AreNotSame(requestTask, completedTask);

		client.MarkTransportUnhealthy();

		Assert.IsFalse(client.IsReady);

		await Assert.ThrowsExceptionAsync<IOException>(async () =>
			await client.SendRequestAsync<JsonElement>(
				"workspace/configuration",
				new WorkspaceConfigurationParams([]),
				CancellationToken.None).ConfigureAwait(false)).ConfigureAwait(false);

		completedTask = await Task.WhenAny(requestTask, Task.Delay(TimeSpan.FromMilliseconds(100))).ConfigureAwait(false);
		Assert.AreNotSame(requestTask, completedTask);

		((JsonRpc)GetPropertyValue(session, "JsonRpc")).Dispose();

		await AssertFaultedOrCanceledAsync(requestTask).ConfigureAwait(false);
	}

	[TestMethod]
	public async Task ActiveSessionDisconnect_WhileRequestIsInFlight_CompletesPendingRequestAndMarksClientNotReady()
	{
		using var serverOutputStream = new PendingReadStream();
		await using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", DefaultClientOptions);
		object session = CreateTransportSession(client, 1, process: null, serverOutputStream, Stream.Null, startListening: true);

		SetActiveSession(client, session);
		SetReadyState(client, true);

		Task<JsonElement> requestTask = client.SendRequestAsync<JsonElement>(
			"workspace/configuration",
			new WorkspaceConfigurationParams([]),
			CancellationToken.None);

		Task completedTask = await Task.WhenAny(requestTask, Task.Delay(TimeSpan.FromMilliseconds(100))).ConfigureAwait(false);
		Assert.AreNotSame(requestTask, completedTask);

		((JsonRpc)GetPropertyValue(session, "JsonRpc")).Dispose();

		completedTask = await Task.WhenAny(requestTask, Task.Delay(TimeSpan.FromSeconds(1))).ConfigureAwait(false);
		Assert.AreSame(requestTask, completedTask);
		Assert.IsFalse(client.IsReady);

		await Assert.ThrowsExceptionAsync<IOException>(async () =>
			await client.SendRequestAsync<JsonElement>(
				"workspace/configuration",
				new WorkspaceConfigurationParams([]),
				CancellationToken.None).ConfigureAwait(false)).ConfigureAwait(false);

		await AssertFaultedOrCanceledAsync(requestTask).ConfigureAwait(false);
	}

	[TestMethod]
	public async Task SendRequestAsync_WhenActiveTransportFails_LogsRequestFailureWithGeneration()
	{
		using var logScope = new NLogMemoryScope(LogLevel.Debug);
		using var serverOutputStream = new PendingReadStream();
		await using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", DefaultClientOptions);
		object session = CreateTransportSession(client, 7, process: null, serverOutputStream, Stream.Null, startListening: true);

		SetActiveSession(client, session);
		SetReadyState(client, true);

		Task<JsonElement> requestTask = client.SendRequestAsync<JsonElement>(
			"workspace/configuration",
			new WorkspaceConfigurationParams([]),
			CancellationToken.None);

		Task completedTask = await Task.WhenAny(requestTask, Task.Delay(TimeSpan.FromMilliseconds(100))).ConfigureAwait(false);
		Assert.AreNotSame(requestTask, completedTask);

		((JsonRpc)GetPropertyValue(session, "JsonRpc")).Dispose();

		await AssertFaultedOrCanceledAsync(requestTask).ConfigureAwait(false);
		Assert.IsFalse(client.IsReady);

		Assert.IsTrue(logScope.Logs.Any(log => log.Contains("request", StringComparison.OrdinalIgnoreCase)
			&& log.Contains("workspace/configuration", StringComparison.OrdinalIgnoreCase)
			&& log.Contains("failed", StringComparison.OrdinalIgnoreCase)
			&& log.Contains("generation 7", StringComparison.OrdinalIgnoreCase)),
			string.Join(Environment.NewLine, logScope.Logs));
	}

	[TestMethod]
	public async Task SendRequestAsync_WhenServerReturnsJsonRpcError_DoesNotInvalidateTransport()
	{
		using var deferredServerOutputStream = new DeferredPersistentJsonRpcResponseStream();
		using var serverInputStream = new RecordingStream();
		await using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", DefaultClientOptions);
		object session = CreateTransportSession(client, 9, process: null, deferredServerOutputStream, serverInputStream, startListening: true);

		SetActiveSession(client, session);
		SetReadyState(client, true);

		Task<JsonElement> requestTask = client.SendRequestAsync<JsonElement>(
			"workspace/configuration",
			new WorkspaceConfigurationParams([]),
			CancellationToken.None);

		int requestId = await WaitForRequestIdAsync(serverInputStream).ConfigureAwait(false);
		deferredServerOutputStream.SetPayload(CreateJsonRpcErrorMessage(requestId, -32000, "Simulated request failure."));

		Exception? observedException = null;

		try
		{
			await requestTask.ConfigureAwait(false);
			Assert.Fail("Expected the JSON-RPC request to fail.");
		}
		catch (Exception exception)
		{
			observedException = exception;
		}

		Assert.IsNotNull(observedException);
		Assert.IsFalse(observedException is LanguageServerTransportUnavailableException);
		Assert.IsTrue(client.IsReady);
		Assert.AreEqual(9L, client.TransportGeneration);
	}

	[TestMethod]
	public void JsonRpc_Disconnected_LocallyDisposedActiveTransport_LogsExpectedShutdownAtInfo()
	{
		using var logScope = new NLogMemoryScope(LogLevel.Debug);
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", DefaultClientOptions);
		object session = CreateTransportSession(client, 3, process: null, Stream.Null, Stream.Null);

		SetActiveSession(client, session);
		SetReadyState(client, true);

		InvokePrivateMethod(client, "JsonRpc_Disconnected",
			session,
			new JsonRpcDisconnectedEventArgs("active transport closed", DisconnectedReason.LocallyDisposed));

		Assert.IsTrue(logScope.Logs.Any(log => log.StartsWith("Info|", StringComparison.Ordinal)
			&& log.Contains("expected local shutdown", StringComparison.OrdinalIgnoreCase)
			&& log.Contains("generation 3", StringComparison.OrdinalIgnoreCase)),
			string.Join(Environment.NewLine, logScope.Logs));
	}

	[TestMethod]
	public async Task JsonRpc_Disconnected_DuringClientDisposal_LogsDebugWithoutWarning()
	{
		using var logScope = new NLogMemoryScope(LogLevel.Debug);
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", DefaultClientOptions);
		object session = CreateTransportSession(client, 4, process: null, Stream.Null, Stream.Null);

		SetActiveSession(client, session);
		SetReadyState(client, true);

		bool disposeStarted = (bool)InvokePrivateMethodWithReturn(client, "TryBeginDispose");

		Assert.IsTrue(disposeStarted);

		InvokePrivateMethod(client, "JsonRpc_Disconnected",
			session,
			new JsonRpcDisconnectedEventArgs("disposing transport closed", DisconnectedReason.StreamError));

		Assert.IsFalse(client.IsReady);

		Assert.IsTrue(logScope.Logs.Any(log => log.StartsWith("Debug|", StringComparison.Ordinal)
			&& log.Contains("during client disposal", StringComparison.OrdinalIgnoreCase)
			&& log.Contains("generation 4", StringComparison.OrdinalIgnoreCase)),
			string.Join(Environment.NewLine, logScope.Logs));

		Assert.IsFalse(logScope.Logs.Any(log => log.StartsWith("Warn|", StringComparison.Ordinal)
			&& log.Contains("disconnected unexpectedly", StringComparison.OrdinalIgnoreCase)),
			string.Join(Environment.NewLine, logScope.Logs));

		await InvokePrivateTaskAsync(client, "DisposeCoreAsync").ConfigureAwait(false);
	}

	[TestMethod]
	public void MarkTransportUnhealthy_WhenReady_LogsRestartBoundary()
	{
		using var logScope = new NLogMemoryScope(LogLevel.Debug);
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", DefaultClientOptions);
		object session = CreateTransportSession(client, 5, process: null, Stream.Null, Stream.Null);

		SetActiveSession(client, session);

		InvokePrivateMethod(client, "CaptureServerCapabilities", DeserializeInitializeResponse(
			"""
			{
			  "capabilities": {
			    "textDocumentSync": {
			      "change": 1
			    },
			    "renameProvider": true,
			    "semanticTokensProvider": {
			      "full": {
			        "delta": true
			      },
			      "legend": {
			        "tokenTypes": ["function"],
			        "tokenModifiers": ["declaration"]
			      }
			    }
			  }
			}
			"""));

		SetReadyState(client, true);

		client.MarkTransportUnhealthy();

		Assert.IsFalse(client.IsReady);
		Assert.AreEqual(5L, client.TransportGeneration);
		Assert.AreEqual(TextDocumentSyncKind.None, client.TextDocumentSyncKind);
		Assert.AreEqual(0, client.SemanticTokenTypes.Count);
		Assert.AreEqual(0, client.SemanticTokenModifiers.Count);
		Assert.IsFalse(client.SupportsCompletionResolve);
		Assert.IsFalse(client.SupportsReferences);
		Assert.IsFalse(client.SupportsRename);
		Assert.IsFalse(client.SupportsFormatting);
		Assert.IsFalse(client.SupportsSemanticTokensDelta);

		Assert.IsTrue(logScope.Logs.Any(log => log.StartsWith("Warn|", StringComparison.Ordinal)
			&& log.Contains("generation 5", StringComparison.OrdinalIgnoreCase)
			&& log.Contains("restart", StringComparison.OrdinalIgnoreCase)),
			string.Join(Environment.NewLine, logScope.Logs));
	}

	[TestMethod]
	public void TryMarkTransportUnhealthy_StaleGenerationDoesNotOverwriteActiveSnapshot()
	{
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", DefaultClientOptions);
		object oldSession = CreateTransportSession(client, 5, process: null, Stream.Null, Stream.Null);
		object newSession = CreateTransportSession(client, 6, process: null, Stream.Null, Stream.Null);

		SetActiveSession(client, newSession);

		InvokePrivateMethod(client, "CaptureServerCapabilities", DeserializeInitializeResponse(
			"""
			{
			  "capabilities": {
			    "textDocumentSync": {
			      "change": 1
			    },
			    "renameProvider": true
			  }
			}
			"""));

		SetReadyState(client, true);

		bool markedUnhealthy = client.TryMarkTransportUnhealthy(GetTransportGeneration(oldSession));

		Assert.IsFalse(markedUnhealthy);

		Assert.AreEqual(6L, client.TransportGeneration);
		Assert.IsTrue(client.IsReady);
		Assert.AreEqual(TextDocumentSyncKind.Full, client.TextDocumentSyncKind);
		Assert.IsTrue(client.SupportsRename);
	}

	[TestMethod]
	public void DetachActiveSession_ResetsPublishedCapabilitySnapshot()
	{
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", DefaultClientOptions);
		object session = CreateTransportSession(client, 9, process: null, Stream.Null, Stream.Null);

		SetActiveSession(client, session);

		InvokePrivateMethod(client, "CaptureServerCapabilities", DeserializeInitializeResponse(
			"""
			{
			  "capabilities": {
			    "textDocumentSync": {
			      "change": 1
			    },
			    "referencesProvider": true,
			    "renameProvider": true,
			    "documentFormattingProvider": true,
			    "semanticTokensProvider": {
			      "full": {
			        "delta": true
			      },
			      "legend": {
			        "tokenTypes": ["function"],
			        "tokenModifiers": ["declaration"]
			      }
			    }
			  }
			}
			"""));

		SetReadyState(client, true);

		object detachedSession = InvokePrivateMethodWithReturn(client, "DetachActiveSession");

		Assert.AreSame(session, detachedSession);
		Assert.AreEqual(0L, client.TransportGeneration);
		Assert.IsFalse(client.IsReady);
		Assert.AreEqual(TextDocumentSyncKind.None, client.TextDocumentSyncKind);
		Assert.AreEqual(0, client.SemanticTokenTypes.Count);
		Assert.AreEqual(0, client.SemanticTokenModifiers.Count);
		Assert.IsFalse(client.SupportsCompletionResolve);
		Assert.IsFalse(client.SupportsReferences);
		Assert.IsFalse(client.SupportsRename);
		Assert.IsFalse(client.SupportsFormatting);
		Assert.IsFalse(client.SupportsSemanticTokensDelta);
	}

	[TestMethod]
	public async Task SendNotificationAsync_WhenStartupHandshakeIsInProgress_ThrowsIOException()
	{
		var sessionActivated = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		using var startupCancellation = new CancellationTokenSource();

		await using var client = new LanguageServerClient(
			@"C:\Workspace",
			Path.Combine(Environment.SystemDirectory, "cmd.exe"),
			DefaultClientOptions,
			processStartedTestHook: null,
			sessionActivatedTestHook: cancellationToken => WaitForStartupCancellationAsync(sessionActivated, cancellationToken));

		Task<bool> startTask = client.StartAsync(startupCancellation.Token);

		await sessionActivated.Task.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);

		await Assert.ThrowsExceptionAsync<IOException>(async () =>
			await client.SendNotificationAsync(
				"workspace/didChangeConfiguration",
				new { settings = new { } },
				CancellationToken.None).ConfigureAwait(false)).ConfigureAwait(false);

		startupCancellation.Cancel();

		await Assert.ThrowsExceptionAsync<TaskCanceledException>(async () => await startTask.ConfigureAwait(false)).ConfigureAwait(false);
	}

	[TestMethod]
	public async Task SendRequestAsync_WhenStartupHandshakeIsInProgress_ThrowsIOException()
	{
		var sessionActivated = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		using var startupCancellation = new CancellationTokenSource();

		await using var client = new LanguageServerClient(
			@"C:\Workspace",
			Path.Combine(Environment.SystemDirectory, "cmd.exe"),
			DefaultClientOptions,
			processStartedTestHook: null,
			sessionActivatedTestHook: cancellationToken => WaitForStartupCancellationAsync(sessionActivated, cancellationToken));

		Task<bool> startTask = client.StartAsync(startupCancellation.Token);

		await sessionActivated.Task.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);

		await Assert.ThrowsExceptionAsync<IOException>(async () =>
			await client.SendRequestAsync<JsonElement>(
				"workspace/configuration",
				new WorkspaceConfigurationParams([]),
				CancellationToken.None).ConfigureAwait(false)).ConfigureAwait(false);

		startupCancellation.Cancel();

		await Assert.ThrowsExceptionAsync<TaskCanceledException>(async () => await startTask.ConfigureAwait(false)).ConfigureAwait(false);
	}

	[TestMethod]
	public void RegisterCapability_IgnoresDynamicRegistrationAndLogsWarning()
	{
		using var logScope = new NLogMemoryScope(LogLevel.Debug);
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", DefaultClientOptions);
		object session = CreateTransportSession(client, 1, process: null, Stream.Null, Stream.Null);

		SetActiveSession(client, session);

		object rpcTarget = CreateRpcTarget(client, GetTransportGeneration(session));
		object? result = InvokePrivateMethodAllowingNull(rpcTarget, "RegisterCapability",
			new CapabilityRegistrationParams(
			[
				new CapabilityRegistrationPayload("1", "textDocument/rename")
			]));

		Assert.IsNull(result);

		Assert.IsTrue(logScope.Logs.Any(log => log.StartsWith("Warn|", StringComparison.Ordinal)
			&& log.Contains("client/registerCapability", StringComparison.Ordinal)
			&& log.Contains("generation 1", StringComparison.OrdinalIgnoreCase)
			&& log.Contains("dynamicRegistration = false", StringComparison.Ordinal)
			&& log.Contains("textDocument/rename", StringComparison.Ordinal)),
			string.Join(Environment.NewLine, logScope.Logs));
	}

	[TestMethod]
	public void UnregisterCapability_IgnoresDynamicUnregistrationAndLogsWarning()
	{
		using var logScope = new NLogMemoryScope(LogLevel.Debug);
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", DefaultClientOptions);
		object session = CreateTransportSession(client, 2, process: null, Stream.Null, Stream.Null);

		SetActiveSession(client, session);

		object rpcTarget = CreateRpcTarget(client, GetTransportGeneration(session));
		object? result = InvokePrivateMethodAllowingNull(rpcTarget, "UnregisterCapability",
			new CapabilityUnregistrationParams(
			[
				new CapabilityUnregistrationPayload("1", "textDocument/rename")
			]));

		Assert.IsNull(result);

		Assert.IsTrue(logScope.Logs.Any(log => log.StartsWith("Warn|", StringComparison.Ordinal)
			&& log.Contains("client/unregisterCapability", StringComparison.Ordinal)
			&& log.Contains("generation 2", StringComparison.OrdinalIgnoreCase)
			&& log.Contains("dynamicRegistration = false", StringComparison.Ordinal)
			&& log.Contains("textDocument/rename", StringComparison.Ordinal)),
			string.Join(Environment.NewLine, logScope.Logs));
	}

	[TestMethod]
	public void RegisterCapability_StaleTransportGeneration_LogsDebugWithoutWarning()
	{
		using var logScope = new NLogMemoryScope(LogLevel.Debug);
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", DefaultClientOptions);
		object oldSession = CreateTransportSession(client, 3, process: null, Stream.Null, Stream.Null);
		object newSession = CreateTransportSession(client, 4, process: null, Stream.Null, Stream.Null);

		SetActiveSession(client, newSession);

		object rpcTarget = CreateRpcTarget(client, GetTransportGeneration(oldSession));
		object? result = InvokePrivateMethodAllowingNull(rpcTarget, "RegisterCapability",
			new CapabilityRegistrationParams(
			[
				new CapabilityRegistrationPayload("1", "textDocument/rename")
			]));

		Assert.IsNull(result);

		Assert.IsTrue(logScope.Logs.Any(log => log.StartsWith("Debug|", StringComparison.Ordinal)
			&& log.Contains("client/registerCapability", StringComparison.Ordinal)
			&& log.Contains("generation 3", StringComparison.OrdinalIgnoreCase)
			&& log.Contains("stale", StringComparison.OrdinalIgnoreCase)),
			string.Join(Environment.NewLine, logScope.Logs));

		Assert.IsFalse(logScope.Logs.Any(log => log.StartsWith("Warn|", StringComparison.Ordinal)
			&& log.Contains("client/registerCapability", StringComparison.Ordinal)),
			string.Join(Environment.NewLine, logScope.Logs));
	}

	[TestMethod]
	public void Hello_IgnoresArrayPayloadWithoutThrowing()
	{
		using var logScope = new NLogMemoryScope(LogLevel.Debug);
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", DefaultClientOptions);
		object session = CreateTransportSession(client, 5, process: null, Stream.Null, Stream.Null);

		SetActiveSession(client, session);
		SetReadyState(client, true);

		object rpcTarget = CreateRpcTarget(client, GetTransportGeneration(session));

		InvokePrivateMethod(rpcTarget, "Hello", JsonSerializer.SerializeToElement(new[] { "world" }));

		Assert.IsTrue(logScope.Logs.Any(log => log.StartsWith("Debug|", StringComparison.Ordinal)
			&& log.Contains("$/hello", StringComparison.Ordinal)
			&& log.Contains("generation 5", StringComparison.OrdinalIgnoreCase)),
			string.Join(Environment.NewLine, logScope.Logs));
	}

	[TestMethod]
	public async Task SendRequestAsync_WhenTransportGenerationIsReplacedBeforeSuccessfulResponse_CompletesWithTransportChangedException()
	{
		using var deferredServerOutputStream = new DeferredJsonRpcResponseStream();
		using var serverInputStream = new RecordingStream();
		await using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", DefaultClientOptions);
		object originalSession = CreateTransportSession(client, 1, process: null, deferredServerOutputStream, serverInputStream, startListening: true);
		object replacementSession = CreateTransportSession(client, 2, process: null, Stream.Null, Stream.Null);

		SetActiveSession(client, originalSession);
		SetReadyState(client, true);

		Task<JsonElement> requestTask = client.SendRequestAsync<JsonElement>(
			"workspace/configuration",
			new WorkspaceConfigurationParams([]),
			CancellationToken.None);

		int requestId = await WaitForRequestIdAsync(serverInputStream).ConfigureAwait(false);

		SetActiveSession(client, replacementSession);
		SetReadyState(client, true);
		deferredServerOutputStream.SetPayload(CreateJsonRpcResultMessage(requestId, "{\"value\":1}"));

		await Assert.ThrowsExceptionAsync<LanguageServerTransportChangedException>(async () => await requestTask.ConfigureAwait(false)).ConfigureAwait(false);
	}

	[TestMethod]
	public void JsonRpc_Disconnected_OldTransportGenerationDoesNotAffectActiveSession()
	{
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", DefaultClientOptions);
		object oldSession = CreateTransportSession(client, 1, process: null, Stream.Null, Stream.Null);
		object newSession = CreateTransportSession(client, 2, process: null, Stream.Null, Stream.Null);

		SetActiveSession(client, newSession);
		SetReadyState(client, true);

		InvokePrivateMethod(client, "JsonRpc_Disconnected",
			oldSession,
			new JsonRpcDisconnectedEventArgs("old transport closed", DisconnectedReason.LocallyDisposed));

		Assert.IsTrue(client.IsReady);

		InvokePrivateMethod(client, "JsonRpc_Disconnected",
			newSession,
			new JsonRpcDisconnectedEventArgs("active transport closed", DisconnectedReason.LocallyDisposed));

		Assert.IsFalse(client.IsReady);
		Assert.IsNull(GetPrivateFieldAllowingNull(client, "_activeSession"));
	}

	[TestMethod]
	public void Process_Exited_SupersededTransportGenerationDoesNotAffectActiveSessionOrWarn()
	{
		using var logScope = new NLogMemoryScope(LogLevel.Debug);
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", DefaultClientOptions);
		object oldSession = CreateTransportSession(client, 1, process: null, Stream.Null, Stream.Null);
		object newSession = CreateTransportSession(client, 2, process: null, Stream.Null, Stream.Null);

		SetActiveSession(client, newSession);
		SetReadyState(client, true);

		InvokePrivateMethod(client, "Process_Exited", oldSession);

		Assert.AreEqual(2L, client.TransportGeneration);
		Assert.IsTrue(client.IsReady);

		Assert.IsFalse(logScope.Logs.Any(log => log.StartsWith("Warn|", StringComparison.Ordinal)
			&& log.Contains("generation 1", StringComparison.OrdinalIgnoreCase)
			&& log.Contains("exited unexpectedly", StringComparison.OrdinalIgnoreCase)),
			string.Join(Environment.NewLine, logScope.Logs));
	}

	[TestMethod]
	public void Process_Exited_ActiveTransport_DetachesActiveSessionImmediately()
	{
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", DefaultClientOptions);
		object session = CreateTransportSession(client, 2, process: null, Stream.Null, Stream.Null);

		SetActiveSession(client, session);
		SetReadyState(client, true);

		InvokePrivateMethod(client, "Process_Exited", session);

		Assert.IsFalse(client.IsReady);
		Assert.IsNull(GetPrivateFieldAllowingNull(client, "_activeSession"));
	}

	[TestMethod]
	public async Task DisposeAsync_WaitsForDetachedFailedSessionCleanup()
	{
		var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", DefaultClientOptions);
		var queuedCleanup = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

		SetPrivateField(client, "_queuedFailedSessionDisposal", queuedCleanup.Task);

		Task disposeTask = client.DisposeAsync().AsTask();
		Task completedTask = await Task.WhenAny(disposeTask, Task.Delay(TimeSpan.FromMilliseconds(200))).ConfigureAwait(false);

		Assert.AreNotSame(disposeTask, completedTask);

		queuedCleanup.TrySetResult(true);
		await disposeTask.ConfigureAwait(false);
	}

	[TestMethod]
	public void SetCapabilityReadinessForGeneration_StaleGenerationDoesNotClearActiveSnapshot()
	{
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", DefaultClientOptions);
		object oldSession = CreateTransportSession(client, 1, process: null, Stream.Null, Stream.Null);
		object newSession = CreateTransportSession(client, 2, process: null, Stream.Null, Stream.Null);

		SetActiveSession(client, newSession);

		InvokePrivateMethod(client, "CaptureServerCapabilities", DeserializeInitializeResponse(
			"""
			{
			  "capabilities": {
			    "textDocumentSync": {
			      "change": 1
			    },
			    "documentFormattingProvider": true
			  }
			}
			"""));

		SetReadyState(client, true);

		InvokePrivateMethod(client, "SetCapabilityReadinessForGeneration", GetTransportGeneration(oldSession), false);

		Assert.AreEqual(2L, client.TransportGeneration);
		Assert.IsTrue(client.IsReady);
		Assert.AreEqual(TextDocumentSyncKind.Full, client.TextDocumentSyncKind);
		Assert.IsTrue(client.SupportsFormatting);
	}

	[TestMethod]
	public void CaptureServerCapabilitiesForGeneration_StaleGenerationDoesNotOverwriteActiveCapabilities()
	{
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", DefaultClientOptions);
		object oldSession = CreateTransportSession(client, 3, process: null, Stream.Null, Stream.Null);
		object newSession = CreateTransportSession(client, 4, process: null, Stream.Null, Stream.Null);

		SetActiveSession(client, newSession);

		InvokePrivateMethod(client, "CaptureServerCapabilitiesForGeneration",
			GetTransportGeneration(newSession),
			DeserializeInitializeResponse(
			"""
			{
			  "capabilities": {
			    "textDocumentSync": {
			      "change": 1
			    },
			    "semanticTokensProvider": {
			      "full": {
			        "delta": true
			      },
			      "legend": {
			        "tokenTypes": ["function"],
			        "tokenModifiers": ["declaration"]
			      }
			    }
			  }
			}
			"""));

		InvokePrivateMethod(client, "CaptureServerCapabilitiesForGeneration",
			GetTransportGeneration(oldSession),
			DeserializeInitializeResponse(
			"""
			{
			  "capabilities": {
			    "textDocumentSync": {
			      "change": 2
			    },
			    "renameProvider": true
			  }
			}
			"""));

		Assert.AreEqual(4L, client.TransportGeneration);
		Assert.AreEqual(TextDocumentSyncKind.Full, client.TextDocumentSyncKind);
		Assert.IsTrue(client.SupportsSemanticTokensDelta);
		CollectionAssert.AreEqual(new[] { "function" }, client.SemanticTokenTypes.ToArray());
		CollectionAssert.AreEqual(new[] { "declaration" }, client.SemanticTokenModifiers.ToArray());
		Assert.IsFalse(client.SupportsRename);
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

	private static Process StartShortLivedProcess()
	{
		var startInfo = new ProcessStartInfo
		{
			FileName = Path.Combine(Environment.SystemDirectory, "cmd.exe"),
			Arguments = "/c ping 127.0.0.1 -n 1 -w 200 > nul",
			UseShellExecute = false,
			CreateNoWindow = true,
			RedirectStandardInput = true,
			RedirectStandardOutput = true,
			RedirectStandardError = true
		};

		return Process.Start(startInfo)
			?? throw new InvalidOperationException("Unable to start the short-lived disposable test process.");
	}

	private static string CreateJsonRpcResultMessage(int id, string resultJson)
	{
		string payload = "{\"jsonrpc\":\"2.0\",\"id\":" + id + ",\"result\":" + resultJson + "}";
		int payloadLength = Encoding.UTF8.GetByteCount(payload);
		return "Content-Length: " + payloadLength + "\r\n\r\n" + payload;
	}

	private static string CreateJsonRpcErrorMessage(int id, int code, string message)
	{
		string payload = "{\"jsonrpc\":\"2.0\",\"id\":" + id + ",\"error\":{\"code\":" + code + ",\"message\":" + JsonSerializer.Serialize(message) + "}}";
		int payloadLength = Encoding.UTF8.GetByteCount(payload);
		return "Content-Length: " + payloadLength + "\r\n\r\n" + payload;
	}

	private static SemaphoreSlim GetStartLock(LanguageServerClient client)
	{
		FieldInfo field = typeof(LanguageServerClient).GetField("_startLock", BindingFlags.Instance | BindingFlags.NonPublic)
			?? throw new InvalidOperationException("Private field '_startLock' was not found.");

		return (SemaphoreSlim)(field.GetValue(client)
			?? throw new InvalidOperationException("Client start lock was null."));
	}

	private static void SetPrivateField(object instance, string fieldName, object? value)
	{
		FieldInfo field = instance.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
			?? throw new InvalidOperationException($"Private field '{fieldName}' was not found.");

		field.SetValue(instance, value);
	}

	private static object CreateTransportSession(LanguageServerClient client, long generation, Process? process, Stream serverOutputStream, Stream serverInputStream, bool startListening = false)
	{
		Type sessionType = typeof(LanguageServerClient).GetNestedType("LanguageServerTransportSession", BindingFlags.NonPublic)
			?? throw new InvalidOperationException("Nested type 'LanguageServerTransportSession' was not found.");

		ConstructorInfo constructor = sessionType.GetConstructor(
			BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
			binder: null,
			[typeof(long), typeof(Process), typeof(Stream), typeof(Stream)],
			modifiers: null)
			?? throw new InvalidOperationException("Lua transport session constructor was not found.");

		object session = constructor.Invoke([generation, process, serverOutputStream, serverInputStream]);
		object messageHandler = InvokePrivateStaticMethodWithReturn(typeof(LanguageServerClient), "CreateMessageHandler", serverInputStream, serverOutputStream);
		object rpcTarget = CreateRpcTarget(client, generation);

		SetSessionProperty(session, "MessageHandler", messageHandler);
		SetSessionProperty(session, "RpcTarget", rpcTarget);

		object jsonRpc = InvokePrivateMethodWithReturn(client, "CreateJsonRpc", session);

		SetSessionProperty(session, "JsonRpc", jsonRpc);
		SetSessionProperty(session, "RpcCompletionTask", GetPropertyValue(jsonRpc, "Completion"));

		if (startListening)
			((JsonRpc)jsonRpc).StartListening();

		return session;
	}

	private static object CreateRpcTarget(LanguageServerClient client, long generation = 0)
	{
		Type targetType = typeof(LanguageServerClient).GetNestedType("LanguageServerClientRpcTarget", BindingFlags.NonPublic)
			?? throw new InvalidOperationException("Nested type 'LanguageServerClientRpcTarget' was not found.");

		ConstructorInfo constructor = targetType.GetConstructor(
			BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
			binder: null,
			[typeof(LanguageServerClient), typeof(long)],
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

	private static object GetPrivateField(object instance, string fieldName)
	{
		FieldInfo field = instance.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
			?? throw new InvalidOperationException($"Private field '{fieldName}' was not found.");

		return field.GetValue(instance)
			?? throw new InvalidOperationException($"Private field '{fieldName}' returned null.");
	}

	private static object? GetPrivateFieldAllowingNull(object instance, string fieldName)
	{
		FieldInfo field = instance.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
			?? throw new InvalidOperationException($"Private field '{fieldName}' was not found.");

		return field.GetValue(instance);
	}

	private static void SetSessionProperty(object session, string propertyName, object? value)
	{
		PropertyInfo property = session.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
			?? throw new InvalidOperationException($"Transport session property '{propertyName}' was not found.");

		property.SetValue(session, value);
	}

	private static void SetActiveSession(LanguageServerClient client, object session)
	{
		InvokePrivateMethod(client, "SetActiveSession", session);
	}

	private static void SetReadyState(LanguageServerClient client, bool isReady)
	{
		InvokePrivateMethod(client, "SetCapabilityReadiness", isReady);
	}

	private static long GetTransportGeneration(object session)
	{
		PropertyInfo property = session.GetType().GetProperty("Generation", BindingFlags.Instance | BindingFlags.Public)
			?? throw new InvalidOperationException("Transport session property 'Generation' was not found.");

		return (long)(property.GetValue(session)
			?? throw new InvalidOperationException("Transport session generation value was null."));
	}

	private static void RecordStandardErrorLine(object session, string line)
	{
		MethodInfo method = session.GetType().GetMethod("RecordStandardErrorLine", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
			?? throw new InvalidOperationException("Transport session method 'RecordStandardErrorLine' was not found.");

		method.Invoke(session, [line]);
	}

	private static void InvokePrivateMethod(object instance, string methodName, params object?[] parameters)
	{
		MethodInfo method = instance.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
			?? throw new InvalidOperationException($"Private method '{methodName}' was not found.");

		method.Invoke(instance, parameters);
	}

	private static object InvokePrivateMethodWithReturn(object instance, string methodName, params object?[] parameters)
	{
		MethodInfo method = instance.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
			?? throw new InvalidOperationException($"Private method '{methodName}' was not found.");

		return method.Invoke(instance, parameters)
			?? throw new InvalidOperationException($"Private method '{methodName}' returned null.");
	}

	private static object? InvokePrivateMethodAllowingNull(object instance, string methodName, params object?[] parameters)
	{
		MethodInfo method = instance.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
			?? throw new InvalidOperationException($"Private method '{methodName}' was not found.");

		return method.Invoke(instance, parameters);
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

	private static InitializeResponse DeserializeInitializeResponse(string json)
		=> JsonSerializer.Deserialize<InitializeResponse>(json)
			?? throw new InvalidOperationException("Failed to deserialize the Lua initialize response test payload.");

	private static PublishDiagnosticsParams CreateDiagnosticsParameters(string uri, string message) => new(
		uri,
		Version: null,
		Diagnostics:
		[
			new DiagnosticPayload(
				new ProtocolRangePayload(
					new ProtocolNullablePosition(0, 0),
					new ProtocolNullablePosition(0, 1)),
				Severity: null,
				Message: message,
				Source: null,
				Code: null)
		]);

	private static void CancelLifetime(LanguageServerClient client)
	{
		FieldInfo field = typeof(LanguageServerClient).GetField("_lifetimeCts", BindingFlags.Instance | BindingFlags.NonPublic)
			?? throw new InvalidOperationException("Private field '_lifetimeCts' was not found.");

		((CancellationTokenSource)field.GetValue(client)!).Cancel();
	}

	private static async Task WaitForStartupCancellationAsync(TaskCompletionSource<bool> sessionActivated, CancellationToken cancellationToken)
	{
		sessionActivated.TrySetResult(true);
		await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken).ConfigureAwait(false);
	}

	private static async Task AssertFaultedOrCanceledAsync(Task task)
	{
		try
		{
			await task.ConfigureAwait(false);
			Assert.Fail("Expected the task to fault or be canceled.");
		}
		catch (OperationCanceledException)
		{ }
		catch (Exception)
		{ }
	}

	private static async Task<bool> WaitForProcessExitAsync(int processId)
	{
		for (int attempt = 0; attempt < 20; attempt++)
		{
			try
			{
				using Process process = Process.GetProcessById(processId);

				if (process.HasExited)
					return true;
			}
			catch (ArgumentException)
			{
				return true;
			}

			await Task.Delay(50).ConfigureAwait(false);
		}

		return false;
	}

	private static async Task<int> WaitForRequestIdAsync(RecordingStream stream)
	{
		for (int attempt = 0; attempt < 20; attempt++)
		{
			byte[] writtenPayload = stream.GetWrittenBytes();

			if (TryExtractJsonRpcRequestId(writtenPayload, out int requestId))
				return requestId;

			await Task.Delay(25).ConfigureAwait(false);
		}

		throw new AssertFailedException("Timed out waiting for the JSON-RPC request payload to be written.");
	}

	private static bool TryExtractJsonRpcRequestId(byte[] writtenPayload, out int requestId)
	{
		requestId = 0;

		if (writtenPayload.Length == 0)
			return false;

		string payloadText = Encoding.UTF8.GetString(writtenPayload);
		int bodySeparatorIndex = payloadText.IndexOf("\r\n\r\n", StringComparison.Ordinal);

		if (bodySeparatorIndex < 0)
			return false;

		string headerText = payloadText[..bodySeparatorIndex];
		const string contentLengthPrefix = "Content-Length:";
		int contentLengthLineIndex = headerText.IndexOf(contentLengthPrefix, StringComparison.OrdinalIgnoreCase);

		if (contentLengthLineIndex < 0)
			throw new AssertFailedException("The JSON-RPC request payload did not contain a Content-Length header.");

		int contentLengthValueStart = contentLengthLineIndex + contentLengthPrefix.Length;
		int contentLengthValueEnd = headerText.IndexOf("\r\n", contentLengthValueStart, StringComparison.Ordinal);
		string contentLengthText = (contentLengthValueEnd >= 0
			? headerText[contentLengthValueStart..contentLengthValueEnd]
			: headerText[contentLengthValueStart..]).Trim();

		if (!int.TryParse(contentLengthText, out int contentLength) || contentLength < 0)
			throw new AssertFailedException("The JSON-RPC request payload contained an invalid Content-Length header.");

		int bodyStartIndex = bodySeparatorIndex + 4;

		if (writtenPayload.Length < bodyStartIndex + contentLength)
			return false;

		string jsonPayload = Encoding.UTF8.GetString(writtenPayload, bodyStartIndex, contentLength);
		using JsonDocument document = JsonDocument.Parse(jsonPayload);

		if (!document.RootElement.TryGetProperty("id", out JsonElement idElement)
			|| !idElement.TryGetInt32(out requestId))
		{
			throw new AssertFailedException("The JSON-RPC request payload did not contain an integer request id.");
		}

		return true;
	}

	private sealed class RecordingStream : Stream
	{
		private readonly object _syncRoot = new();
		private readonly MemoryStream _innerStream = new();

		public override bool CanRead => false;
		public override bool CanSeek => false;
		public override bool CanWrite => true;
		public override long Length => _innerStream.Length;

		public override long Position
		{
			get
			{
				lock (_syncRoot)
					return _innerStream.Position;
			}
			set
			{
				lock (_syncRoot)
					_innerStream.Position = value;
			}
		}

		public byte[] GetWrittenBytes()
		{
			lock (_syncRoot)
				return _innerStream.ToArray();
		}

		public string GetWrittenText()
			=> Encoding.UTF8.GetString(GetWrittenBytes());

		public override void Flush()
		{
			lock (_syncRoot)
				_innerStream.Flush();
		}

		public override int Read(byte[] buffer, int offset, int count)
			=> throw new NotSupportedException();

		public override long Seek(long offset, SeekOrigin origin)
			=> throw new NotSupportedException();

		public override void SetLength(long value)
		{
			lock (_syncRoot)
				_innerStream.SetLength(value);
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			lock (_syncRoot)
				_innerStream.Write(buffer, offset, count);
		}

		public override void Write(ReadOnlySpan<byte> buffer)
		{
			lock (_syncRoot)
				_innerStream.Write(buffer);
		}

		public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
		{
			lock (_syncRoot)
			{
				_innerStream.Write(buffer.Span);
				return ValueTask.CompletedTask;
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				lock (_syncRoot)
					_innerStream.Flush();
			}

			base.Dispose(disposing);
		}
	}

	private sealed class BlockingWriteStream : Stream
	{
		private readonly TaskCompletionSource<bool> _release = new(TaskCreationOptions.RunContinuationsAsynchronously);

		public override bool CanRead => false;
		public override bool CanSeek => false;
		public override bool CanWrite => true;
		public override long Length => throw new NotSupportedException();

		public override long Position
		{
			get => throw new NotSupportedException();
			set => throw new NotSupportedException();
		}

		public void Release()
			=> _release.TrySetResult(true);

		public override int Read(byte[] buffer, int offset, int count)
			=> throw new NotSupportedException();

		public override long Seek(long offset, SeekOrigin origin)
			=> throw new NotSupportedException();

		public override void SetLength(long value)
			=> throw new NotSupportedException();

		public override void Write(byte[] buffer, int offset, int count)
			=> throw new NotSupportedException();

		public override void Write(ReadOnlySpan<byte> buffer)
			=> throw new NotSupportedException();

		public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
			=> new(WaitForReleaseAsync(cancellationToken));

		public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
			=> WaitForReleaseAsync(cancellationToken);

		public override void Flush()
		{ }

		public override Task FlushAsync(CancellationToken cancellationToken)
			=> Task.CompletedTask;

		private async Task WaitForReleaseAsync(CancellationToken cancellationToken)
		{
			if (_release.Task.IsCompleted)
				return;

			if (!cancellationToken.CanBeCanceled)
			{
				await _release.Task.ConfigureAwait(false);
				return;
			}

			await _release.Task.WaitAsync(cancellationToken).ConfigureAwait(false);
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

	private sealed class DelayedJsonRpcResponseStream : Stream
	{
		private readonly byte[] _payloadBytes;
		private readonly TimeSpan _delay;
		private int _position;
		private int _delayApplied;

		public DelayedJsonRpcResponseStream(string payload, TimeSpan delay)
		{
			_payloadBytes = Encoding.UTF8.GetBytes(payload);
			_delay = delay;
		}

		public override bool CanRead => true;
		public override bool CanSeek => false;
		public override bool CanWrite => false;
		public override long Length => _payloadBytes.Length;

		public override long Position
		{
			get => _position;
			set => throw new NotSupportedException();
		}

		public override int Read(byte[] buffer, int offset, int count)
			=> throw new NotSupportedException();

		public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
		{
			if (Interlocked.Exchange(ref _delayApplied, 1) == 0 && _delay > TimeSpan.Zero)
				await Task.Delay(_delay, cancellationToken).ConfigureAwait(false);

			if (_position >= _payloadBytes.Length)
				return 0;

			int bytesToCopy = Math.Min(buffer.Length, _payloadBytes.Length - _position);
			_payloadBytes.AsMemory(_position, bytesToCopy).CopyTo(buffer);
			_position += bytesToCopy;
			return bytesToCopy;
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

	private sealed class DeferredJsonRpcResponseStream : Stream
	{
		private readonly TaskCompletionSource<byte[]> _payloadSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
		private byte[]? _payloadBytes;
		private int _position;

		public override bool CanRead => true;
		public override bool CanSeek => false;
		public override bool CanWrite => false;
		public override long Length => _payloadBytes?.Length ?? 0;

		public override long Position
		{
			get => _position;
			set => throw new NotSupportedException();
		}

		public void SetPayload(string payload)
			=> _payloadSource.TrySetResult(Encoding.UTF8.GetBytes(payload));

		public override int Read(byte[] buffer, int offset, int count)
			=> throw new NotSupportedException();

		public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
		{
			_payloadBytes ??= await _payloadSource.Task.WaitAsync(cancellationToken).ConfigureAwait(false);

			if (_position >= _payloadBytes.Length)
				return 0;

			int bytesToCopy = Math.Min(buffer.Length, _payloadBytes.Length - _position);
			_payloadBytes.AsMemory(_position, bytesToCopy).CopyTo(buffer);
			_position += bytesToCopy;
			return bytesToCopy;
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

	private sealed class DeferredPersistentJsonRpcResponseStream : Stream
	{
		private readonly TaskCompletionSource<byte[]> _payloadSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
		private readonly TaskCompletionSource<bool> _completionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
		private byte[]? _payloadBytes;
		private int _position;

		public override bool CanRead => true;
		public override bool CanSeek => false;
		public override bool CanWrite => false;
		public override long Length => _payloadBytes?.Length ?? 0;

		public override long Position
		{
			get => _position;
			set => throw new NotSupportedException();
		}

		public void SetPayload(string payload)
			=> _payloadSource.TrySetResult(Encoding.UTF8.GetBytes(payload));

		public override int Read(byte[] buffer, int offset, int count)
			=> throw new NotSupportedException();

		public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
		{
			_payloadBytes ??= await _payloadSource.Task.WaitAsync(cancellationToken).ConfigureAwait(false);

			if (_position < _payloadBytes.Length)
			{
				int bytesToCopy = Math.Min(buffer.Length, _payloadBytes.Length - _position);
				_payloadBytes.AsMemory(_position, bytesToCopy).CopyTo(buffer);
				_position += bytesToCopy;
				return bytesToCopy;
			}

			await _completionSource.Task.WaitAsync(cancellationToken).ConfigureAwait(false);
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

	private sealed class TestConfigurationRoot
	{
		public TestLuaConfiguration? Lua { get; init; }
	}

	private sealed class TestLuaConfiguration
	{
		public TestLuaRuntimeConfiguration? Runtime { get; init; }
	}

	private sealed class TestLuaRuntimeConfiguration
	{
		public string? Version { get; init; }
	}
}
