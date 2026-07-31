namespace Nickelony.LanguageServer.Core;

/// <summary>
/// Serializes one signal-style callback stream per subscriber while allowing different subscribers to run concurrently.
/// </summary>
internal sealed class SerializedSignalSubscriberSet<THandler>
	where THandler : Delegate
{
	private readonly object _syncRoot = new();
	private readonly Action<THandler> _invokeHandler;
	private readonly Action<Exception> _logHandlerFailure;
	private readonly List<Subscription> _subscriptions = [];

	/// <summary>
	/// Initializes a new instance of the <see cref="SerializedSignalSubscriberSet{THandler}"/> class.
	/// </summary>
	/// <param name="invokeHandler">Invokes one subscribed handler.</param>
	/// <param name="logHandlerFailure">Logs one handler exception without interrupting later subscribers.</param>
	public SerializedSignalSubscriberSet(Action<THandler> invokeHandler, Action<Exception> logHandlerFailure)
	{
		_invokeHandler = invokeHandler;
		_logHandlerFailure = logHandlerFailure;
	}

	/// <summary>
	/// Adds one subscriber to the serialized signal-dispatch set.
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
	/// Removes one subscriber from the serialized signal-dispatch set.
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
	/// Queues one signal notification for each current subscriber.
	/// </summary>
	public void Dispatch()
	{
		Subscription[] subscriptions;

		lock (_syncRoot)
		{
			if (_subscriptions.Count == 0)
				return;

			subscriptions = [.. _subscriptions];
		}

		for (int i = 0; i < subscriptions.Length; i++)
			subscriptions[i].Enqueue();
	}

	/// <summary>
	/// Collapses repeated signal notifications for one subscriber into a serialized background drain.
	/// </summary>
	private sealed class Subscription
	{
		private readonly Action<THandler> _invokeHandler;
		private readonly Action<Exception> _logHandlerFailure;

		private int _pendingSignal;
		private int _drainScheduled;
		private int _isDisposed;

		public Subscription(THandler handler, Action<THandler> invokeHandler, Action<Exception> logHandlerFailure)
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

			ThreadPool.QueueUserWorkItem(static state => ((Subscription)state!).Drain(), this, preferLocal: false);
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
}
