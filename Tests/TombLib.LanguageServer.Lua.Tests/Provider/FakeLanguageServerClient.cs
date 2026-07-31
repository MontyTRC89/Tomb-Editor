using System.Text.Json;

namespace Nickelony.LanguageServer.Lua.Tests;

internal sealed class FakeLanguageServerClient : ILanguageServerClient
{
	private readonly object _syncRoot = new();
	private readonly List<(string Method, JsonElement Parameters)> _sentNotifications = [];
	private readonly List<(string Method, JsonElement Parameters)> _sentRequests = [];
	private readonly List<string> _sentMethodNames = [];
	private readonly Queue<JsonElement> _semanticTokensDeltaResponses = [];
	private readonly Queue<JsonElement> _semanticTokensFullResponses = [];
	private TaskCompletionSource<bool>? _hoverRequestGate;
	private TaskCompletionSource<bool>? _openNotificationGate;
	private TaskCompletionSource<bool>? _startGate;
	private TaskCompletionSource<bool>? _changeNotificationGate;
	private TaskCompletionSource<bool>? _semanticTokensFullRequestGate;
	private TaskCompletionSource<bool>? _watchedFilesNotificationGate;
	private readonly TaskCompletionSource<bool> _changeNotificationObserved = new(TaskCreationOptions.RunContinuationsAsynchronously);
	private readonly TaskCompletionSource<bool> _closeNotificationObserved = new(TaskCreationOptions.RunContinuationsAsynchronously);

	public bool IsReady { get; set; } = true;
	public long TransportGeneration { get; private set; }
	public bool StartResult { get; set; } = true;
	public JsonElement CompletionResponse { get; set; }
	public JsonElement CompletionResolveResponse { get; set; }
	public JsonElement DefinitionResponse { get; set; }
	public JsonElement FormattingResponse { get; set; }
	public JsonElement HoverResponse { get; set; }
	public JsonElement ReferencesResponse { get; set; }
	public JsonElement RenameResponse { get; set; }
	public JsonElement SignatureHelpResponse { get; set; }
	public TextDocumentSyncKind TextDocumentSyncKind { get; set; } = TextDocumentSyncKind.Incremental;
	public IReadOnlyList<string> SemanticTokenTypes { get; set; } = [];
	public IReadOnlyList<string> SemanticTokenModifiers { get; set; } = [];
	public bool SupportsCompletionResolve { get; set; }
	public bool SupportsReferences { get; set; } = true;
	public bool SupportsRename { get; set; } = true;
	public bool SupportsFormatting { get; set; } = true;
	public bool SupportsSemanticTokensFull { get; set; } = true;
	public bool SupportsSemanticTokensDelta { get; set; }
	public bool FailStartWhenCancellationRequested { get; set; }
	public bool CancelNextHoverRequestWithoutTimeout { get; set; }
	public int StartCallCount { get; private set; }
	public int MarkTransportUnhealthyCallCount { get; private set; }
	public int DisposeCallCount { get; private set; }
	public int TimedOutHoverRequestsRemaining { get; set; }
	public int TransportChangedRequestFailuresRemaining { get; set; }
	public string? ThrowIOExceptionOnNextRequestMethod { get; set; }
	public string? ThrowInvalidOperationOnNextRequestMethod { get; set; }
	public bool ThrowIOExceptionOnNextDidChange { get; set; }
	public bool ThrowInvalidOperationOnNextWatchedFilesNotification { get; set; }
	public bool ThrowIOExceptionOnNextWatchedFilesNotification { get; set; }
	public bool ThrowIOExceptionAfterWatchedFilesNotificationGateRelease { get; set; }
	public List<bool> StartCancellationTokenCanBeCanceled { get; } = [];

	public event Action<PublishDiagnosticsParams>? DiagnosticsPublished;

	public event Action? SemanticTokensRefreshRequested;

	public async Task<bool> StartAsync(CancellationToken cancellationToken)
	{
		StartCallCount++;
		StartCancellationTokenCanBeCanceled.Add(cancellationToken.CanBeCanceled);

		TaskCompletionSource<bool>? startGate = _startGate;

		if (startGate is not null)
			await startGate.Task.WaitAsync(cancellationToken).ConfigureAwait(false);

		if (FailStartWhenCancellationRequested && cancellationToken.IsCancellationRequested)
			throw new OperationCanceledException(cancellationToken);

		IsReady = StartResult;

		if (StartResult)
			TransportGeneration++;

		return StartResult;
	}

	public void MarkTransportUnhealthy()
	{
		MarkTransportUnhealthyCallCount++;
		IsReady = false;
	}

	public bool TryMarkTransportUnhealthy(long transportGeneration)
	{
		if (transportGeneration != TransportGeneration)
			return false;

		MarkTransportUnhealthy();
		return true;
	}

	public Task SendNotificationAsync(string method, object parameters, CancellationToken cancellationToken)
	{
		if (cancellationToken.IsCancellationRequested)
			return Task.FromCanceled(cancellationToken);

		lock (_syncRoot)
		{
			_sentMethodNames.Add(method);
			_sentNotifications.Add((method, JsonSerializer.SerializeToElement(parameters)));
		}

		if (method == "textDocument/didChange")
		{
			_changeNotificationObserved.TrySetResult(true);

			if (ThrowIOExceptionOnNextDidChange)
			{
				ThrowIOExceptionOnNextDidChange = false;
				throw new IOException("Simulated didChange transport failure.");
			}

			TaskCompletionSource<bool>? changeNotificationGate = _changeNotificationGate;

			if (changeNotificationGate is not null)
				return changeNotificationGate.Task;
		}

		if (method == "workspace/didChangeWatchedFiles" && ThrowIOExceptionOnNextWatchedFilesNotification)
		{
			ThrowIOExceptionOnNextWatchedFilesNotification = false;
			throw new IOException("Simulated workspace watcher transport failure.");
		}

		if (method == "workspace/didChangeWatchedFiles" && ThrowInvalidOperationOnNextWatchedFilesNotification)
		{
			ThrowInvalidOperationOnNextWatchedFilesNotification = false;
			throw new InvalidOperationException("Simulated unexpected workspace watcher transport failure.");
		}

		if (method == "workspace/didChangeWatchedFiles" && _watchedFilesNotificationGate is not null)
			return WaitForWatchedFilesNotificationGateAsync();

		if (method == "textDocument/didClose")
			_closeNotificationObserved.TrySetResult(true);

		if (method == "textDocument/didOpen" && _openNotificationGate is not null)
			return _openNotificationGate.Task;

		return Task.CompletedTask;
	}

	public Task<TResult> SendRequestAsync<TResult>(string method, object parameters, CancellationToken cancellationToken)
	{
		RecordRequest(method, parameters);

		if (string.Equals(ThrowIOExceptionOnNextRequestMethod, method, StringComparison.Ordinal))
		{
			ThrowIOExceptionOnNextRequestMethod = null;
			IsReady = false;
			throw new LanguageServerTransportUnavailableException($"Simulated {method} transport failure.");
		}

		if (string.Equals(ThrowInvalidOperationOnNextRequestMethod, method, StringComparison.Ordinal))
		{
			ThrowInvalidOperationOnNextRequestMethod = null;
			throw new InvalidOperationException($"Simulated {method} request failure.");
		}

		if (TransportChangedRequestFailuresRemaining > 0)
		{
			TransportChangedRequestFailuresRemaining--;
			IsReady = false;
			throw new LanguageServerTransportChangedException();
		}

		if (method == "textDocument/hover")
		{
			if (_hoverRequestGate is not null)
				return WaitForHoverRequestGateAsync<TResult>(cancellationToken);

			if (CancelNextHoverRequestWithoutTimeout)
			{
				CancelNextHoverRequestWithoutTimeout = false;
				throw new OperationCanceledException("Simulated internal hover cancellation.");
			}

			if (TimedOutHoverRequestsRemaining > 0)
			{
				TimedOutHoverRequestsRemaining--;
				return WaitForCancellationAsync<TResult>(cancellationToken);
			}

			if (HoverResponse.ValueKind != JsonValueKind.Undefined)
				return DeserializeResponseAsync<TResult>(HoverResponse);
		}

		if (method == "textDocument/completion" && CompletionResponse.ValueKind != JsonValueKind.Undefined)
			return DeserializeResponseAsync<TResult>(CompletionResponse);

		if (method == "completionItem/resolve" && CompletionResolveResponse.ValueKind != JsonValueKind.Undefined)
			return DeserializeResponseAsync<TResult>(CompletionResolveResponse);

		if (method == "textDocument/definition" && DefinitionResponse.ValueKind != JsonValueKind.Undefined)
			return DeserializeResponseAsync<TResult>(DefinitionResponse);

		if (method == "textDocument/references" && ReferencesResponse.ValueKind != JsonValueKind.Undefined)
			return DeserializeResponseAsync<TResult>(ReferencesResponse);

		if (method == "textDocument/rename" && RenameResponse.ValueKind != JsonValueKind.Undefined)
			return DeserializeResponseAsync<TResult>(RenameResponse);

		if (method == "textDocument/formatting" && FormattingResponse.ValueKind != JsonValueKind.Undefined)
			return DeserializeResponseAsync<TResult>(FormattingResponse);

		if (method == "textDocument/semanticTokens/full/delta")
		{
			if (_semanticTokensDeltaResponses.Count > 0)
				return DeserializeResponseAsync<TResult>(_semanticTokensDeltaResponses.Dequeue());

			return DeserializeResponseAsync<TResult>(JsonSerializer.SerializeToElement(new
			{
				edits = Array.Empty<object>(),
				resultId = "tokens-delta"
			}));
		}

		if (method == "textDocument/semanticTokens/full")
		{
			if (_semanticTokensFullRequestGate is not null)
				return WaitForSemanticTokensFullRequestGateAsync<TResult>();

			if (_semanticTokensFullResponses.Count > 0)
				return DeserializeResponseAsync<TResult>(_semanticTokensFullResponses.Dequeue());

			return DeserializeResponseAsync<TResult>(JsonSerializer.SerializeToElement(new
			{
				data = new[] { 0, 6, 5, 0, 0 },
				resultId = "tokens-1"
			}));
		}

		if (method == "textDocument/signatureHelp" && SignatureHelpResponse.ValueKind != JsonValueKind.Undefined)
			return DeserializeResponseAsync<TResult>(SignatureHelpResponse);

		if (typeof(TResult) == typeof(JsonElement))
			return Task.FromResult((TResult)(object)JsonSerializer.SerializeToElement(new { }));

		return Task.FromResult<TResult>(CreateDefaultResponse<TResult>());
	}

	public string[] GetSentMethodNames()
	{
		lock (_syncRoot)
			return [.. _sentMethodNames];
	}

	public JsonElement GetLastNotificationParameters(string method)
	{
		lock (_syncRoot)
		{
			for (int i = _sentNotifications.Count - 1; i >= 0; i--)
			{
				if (string.Equals(_sentNotifications[i].Method, method, StringComparison.Ordinal))
					return _sentNotifications[i].Parameters;
			}
		}

		throw new InvalidOperationException($"Notification '{method}' was not observed.");
	}

	public JsonElement GetLastRequestParameters(string method)
	{
		lock (_syncRoot)
		{
			for (int i = _sentRequests.Count - 1; i >= 0; i--)
			{
				if (string.Equals(_sentRequests[i].Method, method, StringComparison.Ordinal))
					return _sentRequests[i].Parameters;
			}
		}

		throw new InvalidOperationException($"Request '{method}' was not observed.");
	}

	public void BlockNextOpenNotification()
		=> _openNotificationGate = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

	public void BlockNextStartAsync()
		=> _startGate = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

	public void BlockNextHoverRequest()
		=> _hoverRequestGate = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

	public void ReleaseOpenNotification()
		=> _openNotificationGate?.TrySetResult(true);

	public void ReleaseStartAsync()
	{
		TaskCompletionSource<bool>? startGate = _startGate;
		_startGate = null;
		startGate?.TrySetResult(true);
	}

	public void ReleaseHoverRequest()
	{
		TaskCompletionSource<bool>? hoverRequestGate = _hoverRequestGate;
		_hoverRequestGate = null;
		hoverRequestGate?.TrySetResult(true);
	}

	public void BlockNextChangeNotification()
		=> _changeNotificationGate = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

	public void BlockNextWatchedFilesNotification()
		=> _watchedFilesNotificationGate = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

	public void BlockNextSemanticTokensFullRequest()
		=> _semanticTokensFullRequestGate = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

	public void ReleaseChangeNotification()
	{
		TaskCompletionSource<bool>? changeNotificationGate = _changeNotificationGate;
		_changeNotificationGate = null;
		changeNotificationGate?.TrySetResult(true);
	}

	public void ReleaseWatchedFilesNotification()
	{
		TaskCompletionSource<bool>? watchedFilesNotificationGate = _watchedFilesNotificationGate;
		_watchedFilesNotificationGate = null;
		watchedFilesNotificationGate?.TrySetResult(true);
	}

	public void ReleaseSemanticTokensFullRequest()
	{
		TaskCompletionSource<bool>? semanticTokensFullRequestGate = _semanticTokensFullRequestGate;
		_semanticTokensFullRequestGate = null;
		semanticTokensFullRequestGate?.TrySetResult(true);
	}

	public async Task<bool> WaitForNotificationAsync(string method, TimeSpan timeout)
	{
		Task observedNotification = method switch
		{
			"textDocument/didChange" => _changeNotificationObserved.Task,
			"textDocument/didClose" => _closeNotificationObserved.Task,
			_ => Task.CompletedTask
		};

		Task completedTask = await Task.WhenAny(observedNotification, Task.Delay(timeout)).ConfigureAwait(false);
		return ReferenceEquals(completedTask, observedNotification);
	}

	public async Task<bool> WaitForMethodCountAsync(string method, int expectedCount, TimeSpan timeout)
	{
		DateTime deadline = DateTime.UtcNow + timeout;

		while (DateTime.UtcNow < deadline)
		{
			if (GetSentMethodCount(method) >= expectedCount)
				return true;

			await Task.Delay(10).ConfigureAwait(false);
		}

		return GetSentMethodCount(method) >= expectedCount;
	}

	public void PublishDiagnostics(PublishDiagnosticsParams parameters)
		=> DiagnosticsPublished?.Invoke(parameters);

	public void PublishSemanticTokensRefreshRequested()
		=> SemanticTokensRefreshRequested?.Invoke();

	public void EnqueueSemanticTokensFullResponse(JsonElement response)
		=> _semanticTokensFullResponses.Enqueue(response);

	public void EnqueueSemanticTokensDeltaResponse(JsonElement response)
		=> _semanticTokensDeltaResponses.Enqueue(response);

	private int GetSentMethodCount(string method)
	{
		int count = 0;

		lock (_syncRoot)
		{
			for (int i = 0; i < _sentMethodNames.Count; i++)
			{
				if (string.Equals(_sentMethodNames[i], method, StringComparison.Ordinal))
					count++;
			}
		}

		return count;
	}

	private void RecordRequest(string method, object parameters)
	{
		lock (_syncRoot)
		{
			_sentMethodNames.Add(method);
			_sentRequests.Add((method, JsonSerializer.SerializeToElement(parameters)));
		}
	}

	private static Task<TResult> DeserializeResponseAsync<TResult>(JsonElement response)
	{
		if (typeof(TResult) == typeof(JsonElement))
			return Task.FromResult((TResult)(object)response);

		TResult result = DeserializeResponse<TResult>(response);
		return Task.FromResult(result);
	}

	private static TResult DeserializeResponse<TResult>(JsonElement response)
	{
		TResult? result = JsonSerializer.Deserialize<TResult>(response.GetRawText());

		if (result is null)
			throw new InvalidOperationException("Expected a non-null JSON response.");

		return result;
	}

	private static TResult CreateDefaultResponse<TResult>()
		=> default!;

	private async Task WaitForWatchedFilesNotificationGateAsync()
	{
		TaskCompletionSource<bool>? watchedFilesNotificationGate = _watchedFilesNotificationGate;

		if (watchedFilesNotificationGate is not null)
			await watchedFilesNotificationGate.Task.ConfigureAwait(false);

		if (ThrowIOExceptionAfterWatchedFilesNotificationGateRelease)
		{
			ThrowIOExceptionAfterWatchedFilesNotificationGateRelease = false;
			throw new IOException("Simulated delayed workspace watcher transport failure.");
		}
	}

	private static async Task<TResult> WaitForCancellationAsync<TResult>(CancellationToken cancellationToken)
	{
		await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken).ConfigureAwait(false);
		return CreateDefaultResponse<TResult>();
	}

	private async Task<TResult> WaitForSemanticTokensFullRequestGateAsync<TResult>()
	{
		TaskCompletionSource<bool>? semanticTokensFullRequestGate = _semanticTokensFullRequestGate;

		if (semanticTokensFullRequestGate is not null)
			await semanticTokensFullRequestGate.Task.ConfigureAwait(false);

		if (_semanticTokensFullResponses.Count > 0)
			return DeserializeResponse<TResult>(_semanticTokensFullResponses.Dequeue());

		return DeserializeResponse<TResult>(JsonSerializer.SerializeToElement(new
		{
			data = new[] { 0, 6, 5, 0, 0 },
			resultId = "tokens-1"
		}));
	}

	private async Task<TResult> WaitForHoverRequestGateAsync<TResult>(CancellationToken cancellationToken)
	{
		TaskCompletionSource<bool>? hoverRequestGate = _hoverRequestGate;

		if (hoverRequestGate is not null)
			await hoverRequestGate.Task.WaitAsync(cancellationToken).ConfigureAwait(false);

		if (HoverResponse.ValueKind != JsonValueKind.Undefined)
			return DeserializeResponse<TResult>(HoverResponse);

		return CreateDefaultResponse<TResult>();
	}

	public void Dispose()
		=> DisposeCallCount++;

	public ValueTask DisposeAsync()
		=> ValueTask.CompletedTask;
}
