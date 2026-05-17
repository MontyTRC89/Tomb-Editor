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
	private readonly SerializedDiagnosticsSubscriberSet<Action<PublishDiagnosticsParams>> _diagnosticsPublishedSubscribers =
		new(static (handler, parameters) => handler(parameters),
			exception => Log.Warn(exception, "Diagnostics handler threw; later subscribers will still be notified."));
	private readonly SerializedSignalSubscriberSet<Action> _semanticTokensRefreshSubscribers =
		new(static handler => handler(),
			exception => Log.Warn(exception, "Semantic-tokens refresh request handler threw; later subscribers will still be notified."));

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
		// Parameters were already cloned by the dispatcher in HandleMessageAsync.
		_pendingDiagnostics[GetDiagnosticsQueueKey(transportGeneration, parameters)] = new QueuedDiagnostics(transportGeneration, parameters);
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

		return OperatingSystem.IsWindows()
			? filePath.ToUpperInvariant()
			: filePath;
	}

	/// <summary>
	/// Queues a semantic-tokens refresh callback for background subscriber dispatch.
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
	/// Dispatches queued client callbacks on a dedicated background thread.
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

	private sealed class SerializedSignalSubscriberSet<THandler>
		where THandler : Delegate
	{
		private readonly object _syncRoot = new();
		private readonly Action<THandler> _invokeHandler;
		private readonly Action<Exception> _logHandlerFailure;
		private readonly List<SerializedSignalSubscription<THandler>> _subscriptions = [];

		public SerializedSignalSubscriberSet(Action<THandler> invokeHandler, Action<Exception> logHandlerFailure)
		{
			_invokeHandler = invokeHandler;
			_logHandlerFailure = logHandlerFailure;
		}

		public void Add(THandler? handler)
		{
			if (handler is null)
				return;

			lock (_syncRoot)
				_subscriptions.Add(new SerializedSignalSubscription<THandler>(handler, _invokeHandler, _logHandlerFailure));
		}

		public void Remove(THandler? handler)
		{
			if (handler is null)
				return;

			lock (_syncRoot)
			{
				for (int i = _subscriptions.Count - 1; i >= 0; i--)
				{
					if (!Equals(_subscriptions[i].Handler, handler))
						continue;

					_subscriptions[i].Dispose();
					_subscriptions.RemoveAt(i);
					break;
				}
			}
		}

		public void Dispatch()
		{
			SerializedSignalSubscription<THandler>[] subscriptions;

			lock (_syncRoot)
			{
				if (_subscriptions.Count == 0)
					return;

				subscriptions = [.. _subscriptions];
			}

			for (int i = 0; i < subscriptions.Length; i++)
				subscriptions[i].Enqueue();
		}
	}

	private sealed class SerializedSignalSubscription<THandler>
		where THandler : Delegate
	{
		private readonly Action<THandler> _invokeHandler;
		private readonly Action<Exception> _logHandlerFailure;
		private int _pendingSignal;
		private int _drainScheduled;
		private int _isDisposed;

		public SerializedSignalSubscription(THandler handler, Action<THandler> invokeHandler, Action<Exception> logHandlerFailure)
		{
			Handler = handler;
			_invokeHandler = invokeHandler;
			_logHandlerFailure = logHandlerFailure;
		}

		public THandler Handler { get; }

		public void Enqueue()
		{
			if (Volatile.Read(ref _isDisposed) != 0)
				return;

			Interlocked.Exchange(ref _pendingSignal, 1);
			TryScheduleDrain();
		}

		public void Dispose()
		{
			if (Interlocked.Exchange(ref _isDisposed, 1) != 0)
				return;

			Interlocked.Exchange(ref _pendingSignal, 0);
		}

		private void TryScheduleDrain()
		{
			if (Interlocked.CompareExchange(ref _drainScheduled, 1, 0) != 0)
				return;

			ThreadPool.QueueUserWorkItem(static state => ((SerializedSignalSubscription<THandler>)state!).Drain(), this, preferLocal: false);
		}

		private void Drain()
		{
			try
			{
				while (Interlocked.Exchange(ref _pendingSignal, 0) != 0)
				{
					if (Volatile.Read(ref _isDisposed) != 0)
						return;

					try
					{
						_invokeHandler(Handler);
					}
					catch (Exception exception)
					{
						_logHandlerFailure(exception);
					}
				}
			}
			finally
			{
				Volatile.Write(ref _drainScheduled, 0);

				if (Volatile.Read(ref _isDisposed) == 0 && Volatile.Read(ref _pendingSignal) != 0)
					TryScheduleDrain();
			}
		}
	}

	private sealed class SerializedDiagnosticsSubscriberSet<THandler>
		where THandler : Delegate
	{
		private readonly object _syncRoot = new();
		private readonly Action<THandler, PublishDiagnosticsParams> _invokeHandler;
		private readonly Action<Exception> _logHandlerFailure;
		private readonly List<SerializedDiagnosticsSubscription<THandler>> _subscriptions = [];

		public SerializedDiagnosticsSubscriberSet(Action<THandler, PublishDiagnosticsParams> invokeHandler, Action<Exception> logHandlerFailure)
		{
			_invokeHandler = invokeHandler;
			_logHandlerFailure = logHandlerFailure;
		}

		public void Add(THandler? handler)
		{
			if (handler is null)
				return;

			lock (_syncRoot)
				_subscriptions.Add(new SerializedDiagnosticsSubscription<THandler>(handler, _invokeHandler, _logHandlerFailure));
		}

		public void Remove(THandler? handler)
		{
			if (handler is null)
				return;

			lock (_syncRoot)
			{
				for (int i = _subscriptions.Count - 1; i >= 0; i--)
				{
					if (!Equals(_subscriptions[i].Handler, handler))
						continue;

					_subscriptions[i].Dispose();
					_subscriptions.RemoveAt(i);
					break;
				}
			}
		}

		public void Dispatch(string documentKey, PublishDiagnosticsParams parameters)
		{
			SerializedDiagnosticsSubscription<THandler>[] subscriptions;

			lock (_syncRoot)
			{
				if (_subscriptions.Count == 0)
					return;

				subscriptions = [.. _subscriptions];
			}

			for (int i = 0; i < subscriptions.Length; i++)
				subscriptions[i].Enqueue(documentKey, parameters);
		}
	}

	private sealed class SerializedDiagnosticsSubscription<THandler>
		where THandler : Delegate
	{
		private readonly Action<THandler, PublishDiagnosticsParams> _invokeHandler;
		private readonly Action<Exception> _logHandlerFailure;
		private readonly ConcurrentDictionary<string, PendingDiagnosticsPayload> _pendingPayloads = new(StringComparer.Ordinal);
		private int _drainScheduled;
		private int _isDisposed;
		private long _nextSequence;

		private readonly record struct PendingDiagnosticsPayload(long Sequence, PublishDiagnosticsParams Parameters);

		private readonly record struct DrainedDiagnosticsPayload(long Sequence, PublishDiagnosticsParams Parameters);

		public SerializedDiagnosticsSubscription(THandler handler, Action<THandler, PublishDiagnosticsParams> invokeHandler, Action<Exception> logHandlerFailure)
		{
			Handler = handler;
			_invokeHandler = invokeHandler;
			_logHandlerFailure = logHandlerFailure;
		}

		public THandler Handler { get; }

		public void Enqueue(string documentKey, PublishDiagnosticsParams parameters)
		{
			if (Volatile.Read(ref _isDisposed) != 0)
				return;

			while (true)
			{
				if (!_pendingPayloads.TryGetValue(documentKey, out PendingDiagnosticsPayload existingPayload))
				{
					long sequence = Interlocked.Increment(ref _nextSequence);

					if (_pendingPayloads.TryAdd(documentKey, new PendingDiagnosticsPayload(sequence, parameters)))
						break;

					continue;
				}

				if (_pendingPayloads.TryUpdate(documentKey,
					existingPayload with { Parameters = parameters },
					existingPayload))
				{
					break;
				}
			}

			TryScheduleDrain();
		}

		public void Dispose()
		{
			if (Interlocked.Exchange(ref _isDisposed, 1) != 0)
				return;

			_pendingPayloads.Clear();
		}

		private void TryScheduleDrain()
		{
			if (Interlocked.CompareExchange(ref _drainScheduled, 1, 0) != 0)
				return;

			ThreadPool.QueueUserWorkItem(static state => ((SerializedDiagnosticsSubscription<THandler>)state!).Drain(), this, preferLocal: false);
		}

		private void Drain()
		{
			try
			{
				while (!_pendingPayloads.IsEmpty)
				{
					if (Volatile.Read(ref _isDisposed) != 0)
						return;

					var drainedPayloads = new List<DrainedDiagnosticsPayload>();

					foreach (KeyValuePair<string, PendingDiagnosticsPayload> entry in _pendingPayloads)
					{
						if (_pendingPayloads.TryRemove(entry.Key, out PendingDiagnosticsPayload payload))
							drainedPayloads.Add(new DrainedDiagnosticsPayload(payload.Sequence, payload.Parameters));
					}

					drainedPayloads.Sort(static (left, right) => left.Sequence.CompareTo(right.Sequence));

					for (int i = 0; i < drainedPayloads.Count; i++)
					{
						if (Volatile.Read(ref _isDisposed) != 0)
							return;

						try
						{
							_invokeHandler(Handler, drainedPayloads[i].Parameters);
						}
						catch (Exception exception)
						{
							_logHandlerFailure(exception);
						}
					}
				}
			}
			finally
			{
				Volatile.Write(ref _drainScheduled, 0);

				if (Volatile.Read(ref _isDisposed) == 0 && !_pendingPayloads.IsEmpty)
					TryScheduleDrain();
			}
		}
	}
}
