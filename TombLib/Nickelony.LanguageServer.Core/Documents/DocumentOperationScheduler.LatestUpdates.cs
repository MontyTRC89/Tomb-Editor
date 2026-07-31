namespace Nickelony.LanguageServer.Core;

public sealed partial class DocumentOperationScheduler
{
	/// <summary>
	/// Replaces any queued latest-only update for the specified document path so only the newest pending update remains active.
	/// A newly queued update still waits for any already-running update on the same document to finish.
	/// </summary>
	/// <param name="filePath">The document path whose latest update should be replaced.</param>
	/// <param name="operation">The latest-only update delegate to execute.</param>
	/// <returns>A task that represents the active update.</returns>
	public Task QueueLatestUpdateAsync(string filePath, Func<CancellationToken, Task> operation)
	{
		string normalizedFilePath = NormalizeDocumentPath(filePath);
		var replacementRegistration = new QueuedUpdateRegistration(new CancellationTokenSource());

		QueuedUpdateRegistration? previousRegistration = null;
		Task scheduledOperation;
		Task barrierOperation;

		lock (_syncRoot)
		{
			Task previousOperation = _queuedLatestUpdateOperations.TryGetValue(normalizedFilePath, out Task? queuedOperation)
				? queuedOperation
				: Task.CompletedTask;

			barrierOperation = GetQueuedPerDocumentBarrierUnderLock(normalizedFilePath);

			if (_queuedDocumentUpdates.TryGetValue(normalizedFilePath, out QueuedUpdateRegistration? existingRegistration))
				previousRegistration = existingRegistration;

			_queuedDocumentUpdates[normalizedFilePath] = replacementRegistration;

			scheduledOperation = ExecuteQueuedUpdateAsync(normalizedFilePath, previousOperation, barrierOperation, replacementRegistration, operation);
			_queuedLatestUpdateOperations[normalizedFilePath] = scheduledOperation;
		}

		CancelSupersededQueuedUpdate(previousRegistration);

		scheduledOperation.ContinueWith(
			_ => ClearQueuedLatestUpdateOperation(normalizedFilePath, scheduledOperation),
			CancellationToken.None,
			TaskContinuationOptions.ExecuteSynchronously,
			TaskScheduler.Default);

		return scheduledOperation;
	}

	/// <summary>
	/// Cancels the currently active latest-only update for the specified document path.
	/// </summary>
	/// <param name="filePath">The document path whose queued update should be canceled.</param>
	public void CancelQueuedUpdate(string filePath)
	{
		string normalizedFilePath = NormalizeDocumentPath(filePath);
		QueuedUpdateRegistration? registration;

		lock (_syncRoot)
		{
			if (!_queuedDocumentUpdates.TryGetValue(normalizedFilePath, out registration))
				return;

			_queuedDocumentUpdates.Remove(normalizedFilePath);
		}

		CancelQueuedUpdate(registration);
	}

	/// <summary>
	/// Cancels all currently active latest-only updates.
	/// </summary>
	public void CancelAllQueuedUpdates()
	{
		QueuedUpdateRegistration[] registrations;

		lock (_syncRoot)
		{
			if (_queuedDocumentUpdates.Count == 0)
				return;

			registrations = [.. _queuedDocumentUpdates.Values];
			_queuedDocumentUpdates.Clear();
		}

		for (int i = 0; i < registrations.Length; i++)
			CancelQueuedUpdate(registrations[i]);
	}

	/// <summary>
	/// Executes a latest-only queued update and clears its registration when complete.
	/// </summary>
	/// <param name="filePath">The document path associated with the update.</param>
	/// <param name="previousOperation">The previously queued latest-only update for this document.</param>
	/// <param name="barrierOperation">The exclusion barrier that must complete before the update can run.</param>
	/// <param name="replacementRegistration">The active queued-update registration.</param>
	/// <param name="operation">The update delegate to execute.</param>
	/// <returns>A task that completes when the active update finishes.</returns>
	private async Task ExecuteQueuedUpdateAsync(
		string filePath,
		Task previousOperation,
		Task barrierOperation,
		QueuedUpdateRegistration replacementRegistration,
		Func<CancellationToken, Task> operation)
	{
		try
		{
			await WaitForQueuedOperationsAsync(previousOperation, barrierOperation).ConfigureAwait(false);

			if (!TryMarkQueuedUpdateStarted(replacementRegistration))
				return;

			await operation(replacementRegistration.Source.Token).ConfigureAwait(false);
		}
		finally
		{
			ClearQueuedUpdate(filePath, replacementRegistration);
		}
	}

	/// <summary>
	/// Removes the latest-only queue tail when the completed task is still the current tail.
	/// </summary>
	/// <param name="filePath">The document path whose latest-only queue should be cleared.</param>
	/// <param name="scheduledOperation">The completed scheduled latest-only update.</param>
	private void ClearQueuedLatestUpdateOperation(string filePath, Task scheduledOperation)
	{
		lock (_syncRoot)
		{
			if (_queuedLatestUpdateOperations.TryGetValue(filePath, out Task? queuedOperation)
				&& ReferenceEquals(queuedOperation, scheduledOperation))
			{
				_queuedLatestUpdateOperations.Remove(filePath);
			}
		}
	}

	/// <summary>
	/// Gets the current queued latest-update operation for the supplied path.
	/// The caller must hold <see cref="_syncRoot"/>.
	/// </summary>
	private Task GetQueuedLatestUpdateOperationUnderLock(string filePath)
	{
		return _queuedLatestUpdateOperations.TryGetValue(filePath, out Task? queuedOperation)
			? queuedOperation
			: Task.CompletedTask;
	}

	/// <summary>
	/// Removes the latest-only queued update when the supplied cancellation source is still active.
	/// </summary>
	/// <param name="filePath">The document path whose update should be cleared.</param>
	/// <param name="replacementRegistration">The active queued-update registration.</param>
	private void ClearQueuedUpdate(string filePath, QueuedUpdateRegistration replacementRegistration)
	{
		lock (_syncRoot)
		{
			if (_queuedDocumentUpdates.TryGetValue(filePath, out QueuedUpdateRegistration? queuedRegistration)
				&& ReferenceEquals(queuedRegistration, replacementRegistration))
			{
				_queuedDocumentUpdates.Remove(filePath);
			}
		}

		replacementRegistration.Source.Dispose();
	}

	/// <summary>
	/// Marks a queued update as running so later replacements stop treating it as cancelable pending work.
	/// </summary>
	/// <param name="registration">The queued-update registration to mark as running.</param>
	private bool TryMarkQueuedUpdateStarted(QueuedUpdateRegistration registration)
	{
		lock (_syncRoot)
		{
			if (registration.Source.IsCancellationRequested)
				return false;

			registration.MarkStarted();
			return true;
		}
	}

	/// <summary>
	/// Cancels a superseded queued update only while it is still pending.
	/// </summary>
	/// <param name="registration">The queued-update registration to cancel, or <see langword="null"/>.</param>
	private static void CancelSupersededQueuedUpdate(QueuedUpdateRegistration? registration)
	{
		if (registration is null || registration.HasStarted)
			return;

		CancelQueuedUpdate(registration);
	}

	/// <summary>
	/// Cancels a queued update source without disposing it so the owning delegate controls source lifetime.
	/// </summary>
	/// <param name="registration">The queued-update registration to cancel, or <see langword="null"/>.</param>
	private static void CancelQueuedUpdate(QueuedUpdateRegistration? registration)
	{
		if (registration is null)
			return;

		try
		{
			registration.Source.Cancel();
		}
		catch (ObjectDisposedException)
		{ }
	}

	private sealed class QueuedUpdateRegistration(CancellationTokenSource source)
	{
		private int _hasStarted;

		public CancellationTokenSource Source { get; } = source;

		public bool HasStarted => Volatile.Read(ref _hasStarted) != 0;

		public void MarkStarted()
			=> Interlocked.Exchange(ref _hasStarted, 1);
	}
}
