using System.Collections.Concurrent;

namespace Nickelony.LanguageServer.Core;

/// <summary>
/// Serializes diagnostics callback delivery per subscriber while coalescing repeated updates by document key.
/// </summary>
internal sealed class SerializedDiagnosticsSubscriberSet<THandler>
	where THandler : Delegate
{
	private readonly object _syncRoot = new();
	private readonly Action<THandler, PublishDiagnosticsParams> _invokeHandler;
	private readonly Action<Exception> _logHandlerFailure;
	private readonly List<Subscription> _subscriptions = [];

	/// <summary>
	/// Initializes a new instance of the <see cref="SerializedDiagnosticsSubscriberSet{THandler}"/> class.
	/// </summary>
	/// <param name="invokeHandler">Invokes one subscribed handler with one diagnostics payload.</param>
	/// <param name="logHandlerFailure">Logs one handler exception without interrupting later subscribers.</param>
	public SerializedDiagnosticsSubscriberSet(Action<THandler, PublishDiagnosticsParams> invokeHandler, Action<Exception> logHandlerFailure)
	{
		_invokeHandler = invokeHandler;
		_logHandlerFailure = logHandlerFailure;
	}

	/// <summary>
	/// Adds one subscriber to the serialized diagnostics-dispatch set.
	/// </summary>
	/// <param name="handler">The subscriber to add.</param>
	public void Add(THandler? handler)
	{
		if (handler is null)
			return;

		lock (_syncRoot)
			_subscriptions.Add(new Subscription(handler, _invokeHandler, _logHandlerFailure));
	}

	/// <summary>
	/// Removes one subscriber from the serialized diagnostics-dispatch set.
	/// </summary>
	/// <param name="handler">The subscriber to remove.</param>
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

	/// <summary>
	/// Queues one diagnostics payload for each current subscriber.
	/// </summary>
	/// <param name="documentKey">The document key used to coalesce repeated payloads per subscriber.</param>
	/// <param name="parameters">The diagnostics payload to dispatch.</param>
	public void Dispatch(string documentKey, PublishDiagnosticsParams parameters)
	{
		Subscription[] subscriptions;

		lock (_syncRoot)
		{
			if (_subscriptions.Count == 0)
				return;

			subscriptions = [.. _subscriptions];
		}

		for (int i = 0; i < subscriptions.Length; i++)
			subscriptions[i].Enqueue(documentKey, parameters.CreateSnapshot());
	}

	/// <summary>
	/// Keeps only the newest diagnostics payload per document key for one subscriber and drains them in enqueue order.
	/// </summary>
	private sealed class Subscription
	{
		private readonly Action<THandler, PublishDiagnosticsParams> _invokeHandler;
		private readonly Action<Exception> _logHandlerFailure;
		private readonly ConcurrentDictionary<string, PendingDiagnosticsPayload> _pendingPayloads = new(StringComparer.Ordinal);

		private int _drainScheduled;
		private int _isDisposed;
		private long _nextSequence;

		private readonly record struct PendingDiagnosticsPayload(long Sequence, PublishDiagnosticsParams Parameters);
		private readonly record struct DrainedDiagnosticsPayload(long Sequence, PublishDiagnosticsParams Parameters);

		public Subscription(THandler handler, Action<THandler, PublishDiagnosticsParams> invokeHandler, Action<Exception> logHandlerFailure)
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

			ThreadPool.QueueUserWorkItem(static state => state.Drain(), this, preferLocal: false);
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
