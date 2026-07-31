using System.Collections.Concurrent;
using System.Threading.Channels;

namespace TombLib.LanguageServer.Core;

public sealed partial class LanguageServerClient
{
	/// <summary>
	/// Stores one queued diagnostics payload together with the transport generation that produced it.
	/// </summary>
	/// <param name="TransportGeneration">The transport generation that published the diagnostics.</param>
	/// <param name="Parameters">The diagnostics payload.</param>
	private readonly record struct QueuedDiagnostics(long TransportGeneration, PublishDiagnosticsParams Parameters);

	/// <summary>
	/// Identifies one coalesced diagnostics queue slot.
	/// </summary>
	/// <param name="TransportGeneration">The transport generation that produced the diagnostics.</param>
	/// <param name="DocumentKey">The per-document coalescing key.</param>
	private readonly record struct DiagnosticsQueueKey(long TransportGeneration, string DocumentKey);

	// Queued diagnostics state.
	private readonly ConcurrentDictionary<DiagnosticsQueueKey, QueuedDiagnostics> _pendingDiagnostics = [];
	private readonly ConcurrentDictionary<string, QueuedDiagnostics> _pendingCallbackDiagnostics = [];

	// Diagnostics arrive on the LSP read loop and are stored as the latest payload per file URI.
	// A bounded single-slot channel acts only as a wake signal for the pump, so bursty notifications
	// for the same file collapse to one queued wake-up instead of building an unbounded backlog.
	private readonly Channel<bool> _diagnosticsSignal = Channel.CreateBounded<bool>(
		new BoundedChannelOptions(1)
		{
			SingleReader = true,
			SingleWriter = false,
			AllowSynchronousContinuations = false,
			FullMode = BoundedChannelFullMode.DropWrite
		});

	// Background callback pump state.
	private Task _diagnosticsPumpTask = Task.CompletedTask;
	private int _pendingSemanticTokensRefresh;

	private readonly Channel<bool> _callbackSignal = Channel.CreateBounded<bool>(
		new BoundedChannelOptions(1)
		{
			SingleReader = true,
			SingleWriter = false,
			AllowSynchronousContinuations = false,
			FullMode = BoundedChannelFullMode.DropWrite
		});

	private Task _callbackPumpTask = Task.CompletedTask;
	private long _diagnosticsFallbackSequence;

	private readonly SerializedDiagnosticsSubscriberSet<Action<PublishDiagnosticsParams>> _diagnosticsPublishedSubscribers;
	private readonly SerializedSignalSubscriberSet<Action> _semanticTokensRefreshSubscribers;

	/// <summary>
	/// Occurs when the server publishes diagnostics for a tracked document.
	/// Each subscribed handler is queued independently on the thread pool. Reentrant notifications for the same handler
	/// are serialized, and repeated pending diagnostics for the same document may coalesce to the latest payload while a handler is still busy.
	/// Different handlers may run concurrently and must marshal to a UI thread when required.
	/// </summary>
	public event Action<PublishDiagnosticsParams>? DiagnosticsPublished
	{
		add => _diagnosticsPublishedSubscribers.Add(value);
		remove => _diagnosticsPublishedSubscribers.Remove(value);
	}

	/// <summary>
	/// Occurs when the server requests that semantic tokens be refreshed.
	/// Each subscribed handler is queued independently on the thread pool. Reentrant notifications for the same handler
	/// are serialized, and repeated pending refresh requests may coalesce while a handler is still busy.
	/// Different handlers may run concurrently and must marshal to a UI thread when required.
	/// </summary>
	public event Action? SemanticTokensRefreshRequested
	{
		add => _semanticTokensRefreshSubscribers.Add(value);
		remove => _semanticTokensRefreshSubscribers.Remove(value);
	}

	/// <summary>
	/// Queues a diagnostics payload for later publication on the diagnostics pump.
	/// </summary>
	/// <param name="transportGeneration">The transport generation that published the diagnostics.</param>
	/// <param name="parameters">The diagnostics payload.</param>
	private void RaiseDiagnosticsPublished(long transportGeneration, PublishDiagnosticsParams parameters)
	{
		// Keep only the newest diagnostics payload per file within one transport generation and wake the pump if it is idle.
		// Store a detached snapshot so queued callbacks never share a caller-owned diagnostics array instance.
		_pendingDiagnostics[GetDiagnosticsQueueKey(transportGeneration, parameters)] = new QueuedDiagnostics(transportGeneration, parameters.CreateSnapshot());
		_diagnosticsSignal.Writer.TryWrite(true);
	}

	/// <summary>
	/// Publishes queued diagnostics in transport-generation order while coalescing repeated updates.
	/// </summary>
	private async Task PumpDiagnosticsAsync()
	{
		ChannelReader<bool> reader = _diagnosticsSignal.Reader;

		try
		{
			while (await reader.WaitToReadAsync(_lifetimeCts.Token).ConfigureAwait(false))
			{
				while (reader.TryRead(out _))
				{ }

				while (!_pendingDiagnostics.IsEmpty)
				{
					KeyValuePair<DiagnosticsQueueKey, QueuedDiagnostics>[] pendingDiagnostics = [.. _pendingDiagnostics];

					for (int i = 0; i < pendingDiagnostics.Length; i++)
					{
						if (!_pendingDiagnostics.TryRemove(pendingDiagnostics[i].Key, out QueuedDiagnostics queuedDiagnostics)
							|| queuedDiagnostics.TransportGeneration != TransportGeneration)
						{
							continue;
						}

						QueueDiagnosticsCallback(pendingDiagnostics[i].Key.DocumentKey, queuedDiagnostics);
					}
				}
			}
		}
		catch (OperationCanceledException)
		{
			// Expected on dispose.
		}
	}

	/// <summary>
	/// Builds the queue key used to coalesce diagnostics payloads.
	/// </summary>
	/// <param name="transportGeneration">The transport generation that published the diagnostics.</param>
	/// <param name="parameters">The diagnostics payload.</param>
	/// <returns>The queue key for the payload.</returns>
	private DiagnosticsQueueKey GetDiagnosticsQueueKey(long transportGeneration, PublishDiagnosticsParams parameters)
	{
		if (!string.IsNullOrWhiteSpace(parameters.Uri))
			return new DiagnosticsQueueKey(transportGeneration, NormalizeDiagnosticsDocumentKey(parameters.Uri));

		return new DiagnosticsQueueKey(transportGeneration,
			"diagnostics:" + Interlocked.Increment(ref _diagnosticsFallbackSequence));
	}

	private static string NormalizeDiagnosticsDocumentKey(string uri)
	{
		if (!LanguageServerPathHelper.TryGetFilePath(uri, out string filePath))
			return uri;

		return LanguageServerPathHelper.GetPathKeyFromNormalizedPath(filePath);
	}

	/// <summary>
	/// Queues a semantic tokens refresh callback for background subscriber dispatch.
	/// </summary>
	private void QueueSemanticTokensRefreshCallback()
	{
		Interlocked.Exchange(ref _pendingSemanticTokensRefresh, 1);
		_callbackSignal.Writer.TryWrite(true);
	}

	/// <summary>
	/// Queues a diagnostics callback for background subscriber dispatch.
	/// </summary>
	/// <param name="documentKey">The document key used to coalesce the callback payload.</param>
	/// <param name="parameters">The diagnostics payload to publish.</param>
	private void QueueDiagnosticsCallback(string documentKey, QueuedDiagnostics parameters)
	{
		_pendingCallbackDiagnostics[documentKey] = parameters;
		_callbackSignal.Writer.TryWrite(true);
	}

	/// <summary>
	/// Dispatches queued client callbacks on the background callback pump.
	/// </summary>
	private async Task PumpCallbacksAsync()
	{
		ChannelReader<bool> reader = _callbackSignal.Reader;

		try
		{
			while (await reader.WaitToReadAsync(_lifetimeCts.Token).ConfigureAwait(false))
			{
				while (reader.TryRead(out _))
				{ }

				while (true)
				{
					bool dispatchedCallbacks = false;

					if (Interlocked.Exchange(ref _pendingSemanticTokensRefresh, 0) != 0)
					{
						InvokeSemanticTokensRefreshRequested();
						dispatchedCallbacks = true;
					}

					if (!_pendingCallbackDiagnostics.IsEmpty)
					{
						KeyValuePair<string, QueuedDiagnostics>[] pendingDiagnostics = [.. _pendingCallbackDiagnostics];

						for (int i = 0; i < pendingDiagnostics.Length; i++)
						{
							if (!_pendingCallbackDiagnostics.TryRemove(pendingDiagnostics[i].Key, out QueuedDiagnostics queuedDiagnostics)
								|| queuedDiagnostics.TransportGeneration != TransportGeneration)
							{
								continue;
							}

							InvokeDiagnosticsPublished(pendingDiagnostics[i].Key, queuedDiagnostics.Parameters);
							dispatchedCallbacks = true;
						}
					}

					if (!dispatchedCallbacks
						&& Volatile.Read(ref _pendingSemanticTokensRefresh) == 0
						&& _pendingCallbackDiagnostics.IsEmpty)
					{
						break;
					}
				}
			}
		}
		catch (OperationCanceledException)
		{
			// Expected on dispose.
		}
	}

	private void InvokeSemanticTokensRefreshRequested()
		=> _semanticTokensRefreshSubscribers.Dispatch();

	private void InvokeDiagnosticsPublished(string documentKey, PublishDiagnosticsParams parameters)
		=> _diagnosticsPublishedSubscribers.Dispatch(documentKey, parameters);
}
