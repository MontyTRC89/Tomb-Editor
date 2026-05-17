using System.Collections.Concurrent;

namespace TombLib.LanguageServer.Core;

/// <summary>
/// Coordinates globally ordered operations, per-document ordered operations, and document updates where
/// only the most recently queued update per document remains active while older pending updates are canceled.
/// </summary>
public sealed class DocumentOperationScheduler
{
	private readonly object _syncRoot = new();
	private readonly Dictionary<string, Task> _queuedPerDocumentOperations = new(StringComparer.OrdinalIgnoreCase);
	private readonly Dictionary<string, Task> _queuedLatestUpdateOperations = new(StringComparer.OrdinalIgnoreCase);
	private readonly Dictionary<string, QueuedUpdateRegistration> _queuedDocumentUpdates = new(StringComparer.OrdinalIgnoreCase);
	private readonly Dictionary<string, Task> _queuedPerDocumentBarriers = new(StringComparer.OrdinalIgnoreCase);
	private Task _queuedGlobalOperation = Task.CompletedTask;

	/// <summary>
	/// Enqueues an operation behind the current global operation chain.
	/// </summary>
	/// <typeparam name="TResult">The operation result type.</typeparam>
	/// <param name="operation">The operation to enqueue.</param>
	/// <param name="cancellationToken">Cancels the queued operation.</param>
	/// <returns>A task that completes with the queued operation result.</returns>
	public Task<TResult> EnqueueGlobalAsync<TResult>(Func<CancellationToken, Task<TResult>> operation, CancellationToken cancellationToken)
	{
		var completionSource = new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);

		lock (_syncRoot)
		{
			Task previousOperation = _queuedGlobalOperation;
			_queuedGlobalOperation = RunQueuedOperationAsync(
				previousOperation,
				Task.CompletedTask,
				operation,
				completionSource,
				cancellationToken);
		}

		return completionSource.Task;
	}

	/// <summary>
	/// Enqueues an operation behind the current chain for the specified document path.
	/// </summary>
	/// <typeparam name="TResult">The operation result type.</typeparam>
	/// <param name="filePath">The document path whose queue should receive the operation.</param>
	/// <param name="operation">The operation to enqueue.</param>
	/// <param name="cancellationToken">Cancels the queued operation.</param>
	/// <returns>A task that completes with the queued operation result.</returns>
	public Task<TResult> EnqueuePerDocumentAsync<TResult>(string filePath, Func<CancellationToken, Task<TResult>> operation, CancellationToken cancellationToken)
	{
		var completionSource = new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);
		Task scheduledOperation;
		Task barrierOperation;

		lock (_syncRoot)
		{
			Task previousOperation = _queuedPerDocumentOperations.TryGetValue(filePath, out Task? queuedOperation)
				? queuedOperation
				: Task.CompletedTask;
			barrierOperation = GetQueuedPerDocumentBarrier(filePath);

			scheduledOperation = RunQueuedOperationAsync(previousOperation, barrierOperation, operation, completionSource, cancellationToken);
			_queuedPerDocumentOperations[filePath] = scheduledOperation;
		}

		_ = scheduledOperation.ContinueWith(
			_ => ClearQueuedPerDocumentOperation(filePath, scheduledOperation),
			CancellationToken.None,
			TaskContinuationOptions.ExecuteSynchronously,
			TaskScheduler.Default);

		return completionSource.Task;
	}

	/// <summary>
	/// Enqueues an exclusive operation for one or two document paths so later work on those paths cannot run until the
	/// exclusive operation has finished.
	/// </summary>
	/// <typeparam name="TResult">The operation result type.</typeparam>
	/// <param name="firstFilePath">The first affected document path.</param>
	/// <param name="secondFilePath">The second affected document path.</param>
	/// <param name="operation">The exclusive operation to enqueue.</param>
	/// <param name="cancellationToken">Cancels the queued operation.</param>
	/// <returns>A task that completes with the queued operation result.</returns>
	public Task<TResult> EnqueueExclusivePerDocumentAsync<TResult>(
		string firstFilePath,
		string secondFilePath,
		Func<CancellationToken, Task<TResult>> operation,
		CancellationToken cancellationToken)
	{
		var completionSource = new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);
		var barrierSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		Task scheduledOperation;

		lock (_syncRoot)
		{
			Task previousGlobalOperation = _queuedGlobalOperation;
			Task[] queuedOperations = GetQueuedOperationsSnapshotCore(firstFilePath, secondFilePath);

			SetQueuedPerDocumentBarrier(firstFilePath, barrierSource.Task);
			SetQueuedPerDocumentBarrier(secondFilePath, barrierSource.Task);

			scheduledOperation = RunExclusivePerDocumentOperationAsync(
				firstFilePath,
				secondFilePath,
				previousGlobalOperation,
				queuedOperations,
				barrierSource,
				operation,
				completionSource,
				cancellationToken);
			_queuedGlobalOperation = scheduledOperation;
		}

		return completionSource.Task;
	}

	/// <summary>
	/// Waits for the currently queued per-document and latest-update operations for one or two document paths to complete.
	/// </summary>
	/// <param name="firstFilePath">The first document path to await.</param>
	/// <param name="secondFilePath">The second document path to await.</param>
	/// <returns>A task that completes when the queued operations have finished.</returns>
	public async Task WaitForPerDocumentOperationsAsync(string firstFilePath, string secondFilePath)
	{
		Task[] queuedOperations = GetQueuedOperationsSnapshot(firstFilePath, secondFilePath, includeBarriers: true);

		for (int i = 0; i < queuedOperations.Length; i++)
			await WaitForQueuedOperationAsync(queuedOperations[i]).ConfigureAwait(false);
	}

	/// <summary>
	/// Replaces any queued latest-only update for the specified document path so only the newest pending update remains active.
	/// A newly queued update still waits for any already-running update on the same document to finish.
	/// </summary>
	/// <param name="filePath">The document path whose latest update should be replaced.</param>
	/// <param name="operation">The latest-only update delegate to execute.</param>
	/// <returns>A task that represents the active update.</returns>
	public Task QueueLatestUpdateAsync(string filePath, Func<CancellationToken, Task> operation)
	{
		var replacementRegistration = new QueuedUpdateRegistration(new CancellationTokenSource());
		QueuedUpdateRegistration? previousRegistration = null;
		Task scheduledOperation;
		Task barrierOperation;

		lock (_syncRoot)
		{
			Task previousOperation = _queuedLatestUpdateOperations.TryGetValue(filePath, out Task? queuedOperation)
				? queuedOperation
				: Task.CompletedTask;
			barrierOperation = GetQueuedPerDocumentBarrier(filePath);

			if (_queuedDocumentUpdates.TryGetValue(filePath, out QueuedUpdateRegistration? existingRegistration))
				previousRegistration = existingRegistration;

			_queuedDocumentUpdates[filePath] = replacementRegistration;

			scheduledOperation = ExecuteQueuedUpdateAsync(filePath, previousOperation, barrierOperation, replacementRegistration, operation);
			_queuedLatestUpdateOperations[filePath] = scheduledOperation;
		}

		CancelSupersededQueuedUpdate(previousRegistration);

		_ = scheduledOperation.ContinueWith(
			_ => ClearQueuedLatestUpdateOperation(filePath, scheduledOperation),
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
		QueuedUpdateRegistration? registration;

		lock (_syncRoot)
		{
			if (!_queuedDocumentUpdates.TryGetValue(filePath, out registration))
				return;

			_queuedDocumentUpdates.Remove(filePath);
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
	/// <param name="replacementSource">The cancellation source for the active update.</param>
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
	/// Runs an operation after a previously scheduled operation has completed.
	/// </summary>
	/// <typeparam name="TResult">The operation result type.</typeparam>
	/// <param name="previousOperation">The operation that must complete first.</param>
	/// <param name="operation">The queued operation to execute.</param>
	/// <param name="completionSource">The completion source exposed to the caller.</param>
	/// <param name="cancellationToken">Cancels the queued operation.</param>
	/// <returns>A task representing the scheduled queue node.</returns>
	private static async Task RunQueuedOperationAsync<TResult>(
		Task previousOperation,
		Task barrierOperation,
		Func<CancellationToken, Task<TResult>> operation,
		TaskCompletionSource<TResult> completionSource,
		CancellationToken cancellationToken)
	{
		try
		{
			await WaitForQueuedOperationsAsync(previousOperation, barrierOperation).ConfigureAwait(false);
			cancellationToken.ThrowIfCancellationRequested();
			TResult result = await operation(cancellationToken).ConfigureAwait(false);
			completionSource.TrySetResult(result);
		}
		catch (OperationCanceledException exception) when (exception.CancellationToken == cancellationToken)
		{
			completionSource.TrySetCanceled(cancellationToken);
		}
		catch (Exception exception)
		{
			completionSource.TrySetException(exception);
		}
	}

	/// <summary>
	/// Runs an exclusive document operation after previously queued global and per-document work has completed.
	/// </summary>
	private async Task RunExclusivePerDocumentOperationAsync<TResult>(
		string firstFilePath,
		string secondFilePath,
		Task previousGlobalOperation,
		Task[] queuedOperations,
		TaskCompletionSource<bool> barrierSource,
		Func<CancellationToken, Task<TResult>> operation,
		TaskCompletionSource<TResult> completionSource,
		CancellationToken cancellationToken)
	{
		try
		{
			await WaitForQueuedOperationAsync(previousGlobalOperation).ConfigureAwait(false);

			for (int i = 0; i < queuedOperations.Length; i++)
				await WaitForQueuedOperationAsync(queuedOperations[i]).ConfigureAwait(false);

			cancellationToken.ThrowIfCancellationRequested();
			TResult result = await operation(cancellationToken).ConfigureAwait(false);
			completionSource.TrySetResult(result);
		}
		catch (OperationCanceledException exception) when (exception.CancellationToken == cancellationToken)
		{
			completionSource.TrySetCanceled(cancellationToken);
		}
		catch (Exception exception)
		{
			completionSource.TrySetException(exception);
		}
		finally
		{
			barrierSource.TrySetResult(true);
			ClearQueuedPerDocumentBarrier(firstFilePath, barrierSource.Task);
			ClearQueuedPerDocumentBarrier(secondFilePath, barrierSource.Task);
		}
	}

	/// <summary>
	/// Awaits a queued operation while suppressing its failure so later operations can continue.
	/// </summary>
	/// <param name="previousOperation">The previously scheduled operation.</param>
	/// <returns>A task that completes after the previous operation settles.</returns>
	private static async Task WaitForQueuedOperationAsync(Task previousOperation)
	{
		try
		{
			await previousOperation.ConfigureAwait(false);
		}
		catch
		{ }
	}

	/// <summary>
	/// Awaits one or two queued operations while suppressing failures so later operations can continue.
	/// </summary>
	private static async Task WaitForQueuedOperationsAsync(Task firstOperation, Task secondOperation)
	{
		await WaitForQueuedOperationAsync(firstOperation).ConfigureAwait(false);

		if (ReferenceEquals(secondOperation, Task.CompletedTask) || ReferenceEquals(secondOperation, firstOperation))
			return;

		await WaitForQueuedOperationAsync(secondOperation).ConfigureAwait(false);
	}

	/// <summary>
	/// Removes the per-document queue tail when the completed task is still the current tail.
	/// </summary>
	/// <param name="filePath">The document path whose queue should be cleared.</param>
	/// <param name="scheduledOperation">The completed scheduled operation.</param>
	private void ClearQueuedPerDocumentOperation(string filePath, Task scheduledOperation)
	{
		lock (_syncRoot)
		{
			if (_queuedPerDocumentOperations.TryGetValue(filePath, out Task? queuedOperation)
				&& ReferenceEquals(queuedOperation, scheduledOperation))
			{
				_queuedPerDocumentOperations.Remove(filePath);
			}
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
	/// Gets the current queued per-document operation for the supplied path.
	/// </summary>
	/// <param name="filePath">The document path.</param>
	/// <returns>The queued operation tail, or <see cref="Task.CompletedTask"/> when none exists.</returns>
	private Task GetQueuedPerDocumentOperation(string filePath)
	{
		lock (_syncRoot)
		{
			return _queuedPerDocumentOperations.TryGetValue(filePath, out Task? queuedOperation)
				? queuedOperation
				: Task.CompletedTask;
		}
	}

	/// <summary>
	/// Gets the current queued latest-update operation for the supplied path.
	/// </summary>
	/// <param name="filePath">The document path.</param>
	/// <returns>The queued latest-update tail, or <see cref="Task.CompletedTask"/> when none exists.</returns>
	private Task GetQueuedLatestUpdateOperation(string filePath)
	{
		lock (_syncRoot)
		{
			return _queuedLatestUpdateOperations.TryGetValue(filePath, out Task? queuedOperation)
				? queuedOperation
				: Task.CompletedTask;
		}
	}

	/// <summary>
	/// Gets the current exclusion barrier for the supplied document path.
	/// </summary>
	private Task GetQueuedPerDocumentBarrier(string filePath)
	{
		return _queuedPerDocumentBarriers.TryGetValue(filePath, out Task? barrierOperation)
			? barrierOperation
			: Task.CompletedTask;
	}

	/// <summary>
	/// Captures the current queued per-document and latest-update tails for the requested document paths.
	/// </summary>
	/// <param name="firstFilePath">The first document path.</param>
	/// <param name="secondFilePath">The second document path.</param>
	/// <returns>The distinct queued operation tails that were current at snapshot time.</returns>
	private Task[] GetQueuedOperationsSnapshot(string firstFilePath, string secondFilePath, bool includeBarriers = false)
	{
		lock (_syncRoot)
			return GetQueuedOperationsSnapshotCore(firstFilePath, secondFilePath, includeBarriers);
	}

	/// <summary>
	/// Captures the current queued per-document and latest-update tails for the requested document paths.
	/// The caller must hold <see cref="_syncRoot"/>.
	/// </summary>
	private Task[] GetQueuedOperationsSnapshotCore(string firstFilePath, string secondFilePath, bool includeBarriers = false)
	{
		Task firstPerDocumentOperation = GetQueuedPerDocumentOperationCore(firstFilePath);
		Task firstLatestUpdateOperation = GetQueuedLatestUpdateOperationCore(firstFilePath);
		Task firstBarrierOperation = includeBarriers
			? GetQueuedPerDocumentBarrierCore(firstFilePath)
			: Task.CompletedTask;
		Task secondPerDocumentOperation = string.Equals(firstFilePath, secondFilePath, StringComparison.OrdinalIgnoreCase)
			? Task.CompletedTask
			: GetQueuedPerDocumentOperationCore(secondFilePath);
		Task secondLatestUpdateOperation = string.Equals(firstFilePath, secondFilePath, StringComparison.OrdinalIgnoreCase)
			? Task.CompletedTask
			: GetQueuedLatestUpdateOperationCore(secondFilePath);
		Task secondBarrierOperation = includeBarriers || string.Equals(firstFilePath, secondFilePath, StringComparison.OrdinalIgnoreCase)
			? GetQueuedPerDocumentBarrierCore(secondFilePath)
			: Task.CompletedTask;

		var queuedOperations = new List<Task>(4);
		AddDistinctQueuedOperation(queuedOperations, firstPerDocumentOperation);
		AddDistinctQueuedOperation(queuedOperations, firstLatestUpdateOperation);
		AddDistinctQueuedOperation(queuedOperations, firstBarrierOperation);
		AddDistinctQueuedOperation(queuedOperations, secondPerDocumentOperation);
		AddDistinctQueuedOperation(queuedOperations, secondLatestUpdateOperation);
		AddDistinctQueuedOperation(queuedOperations, secondBarrierOperation);
		return [.. queuedOperations];
	}

	/// <summary>
	/// Adds one queued operation to the snapshot when it is not the completed-task sentinel and has not already been captured.
	/// </summary>
	/// <param name="queuedOperations">The captured queued-operation tails.</param>
	/// <param name="queuedOperation">The queued operation to capture.</param>
	private static void AddDistinctQueuedOperation(List<Task> queuedOperations, Task queuedOperation)
	{
		if (ReferenceEquals(queuedOperation, Task.CompletedTask))
			return;

		for (int i = 0; i < queuedOperations.Count; i++)
		{
			if (ReferenceEquals(queuedOperations[i], queuedOperation))
				return;
		}

		queuedOperations.Add(queuedOperation);
	}

	/// <summary>
	/// Gets the current queued per-document operation for the supplied path.
	/// The caller must hold <see cref="_syncRoot"/>.
	/// </summary>
	private Task GetQueuedPerDocumentOperationCore(string filePath)
	{
		return _queuedPerDocumentOperations.TryGetValue(filePath, out Task? queuedOperation)
			? queuedOperation
			: Task.CompletedTask;
	}

	/// <summary>
	/// Gets the current queued latest-update operation for the supplied path.
	/// The caller must hold <see cref="_syncRoot"/>.
	/// </summary>
	private Task GetQueuedLatestUpdateOperationCore(string filePath)
	{
		return _queuedLatestUpdateOperations.TryGetValue(filePath, out Task? queuedOperation)
			? queuedOperation
			: Task.CompletedTask;
	}

	/// <summary>
	/// Gets the current exclusion barrier for the supplied path.
	/// The caller must hold <see cref="_syncRoot"/>.
	/// </summary>
	private Task GetQueuedPerDocumentBarrierCore(string filePath)
	{
		return _queuedPerDocumentBarriers.TryGetValue(filePath, out Task? barrierOperation)
			? barrierOperation
			: Task.CompletedTask;
	}

	/// <summary>
	/// Records an exclusion barrier for the supplied document path.
	/// The caller must hold <see cref="_syncRoot"/>.
	/// </summary>
	private void SetQueuedPerDocumentBarrier(string filePath, Task barrierOperation)
	{
		if (!_queuedPerDocumentBarriers.TryGetValue(filePath, out Task? existingBarrier)
			|| !ReferenceEquals(existingBarrier, barrierOperation))
		{
			_queuedPerDocumentBarriers[filePath] = barrierOperation;
		}
	}

	/// <summary>
	/// Removes a document barrier when it still matches the completed exclusive operation.
	/// </summary>
	private void ClearQueuedPerDocumentBarrier(string filePath, Task barrierOperation)
	{
		lock (_syncRoot)
		{
			if (_queuedPerDocumentBarriers.TryGetValue(filePath, out Task? queuedBarrier)
				&& ReferenceEquals(queuedBarrier, barrierOperation))
			{
				_queuedPerDocumentBarriers.Remove(filePath);
			}
		}
	}

	/// <summary>
	/// Removes the latest-only queued update when the supplied cancellation source is still active.
	/// </summary>
	/// <param name="filePath">The document path whose update should be cleared.</param>
	/// <param name="replacementSource">The active cancellation source for the queued update.</param>
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
	private static void CancelSupersededQueuedUpdate(QueuedUpdateRegistration? registration)
	{
		if (registration is null || registration.HasStarted)
			return;

		CancelQueuedUpdate(registration);
	}

	/// <summary>
	/// Cancels a queued update source without disposing it so the owning delegate controls source lifetime.
	/// </summary>
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
		{
			Interlocked.Exchange(ref _hasStarted, 1);
		}
	}
}
