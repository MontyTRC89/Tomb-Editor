namespace TombLib.LanguageServer.Core;

public sealed partial class DocumentOperationScheduler
{
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
		string normalizedFilePath = NormalizeDocumentPath(filePath);
		var completionSource = new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);

		Task scheduledOperation;
		Task barrierOperation;

		lock (_syncRoot)
		{
			Task previousOperation = _queuedPerDocumentOperations.TryGetValue(normalizedFilePath, out Task? queuedOperation)
				? queuedOperation
				: Task.CompletedTask;

			barrierOperation = GetQueuedPerDocumentBarrierUnderLock(normalizedFilePath);

			scheduledOperation = RunQueuedOperationAsync(previousOperation, barrierOperation, operation, completionSource, cancellationToken);
			_queuedPerDocumentOperations[normalizedFilePath] = scheduledOperation;
		}

		scheduledOperation.ContinueWith(
			_ => ClearQueuedPerDocumentOperation(normalizedFilePath, scheduledOperation),
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
		string normalizedFirstFilePath = NormalizeDocumentPath(firstFilePath);
		string normalizedSecondFilePath = NormalizeDocumentPath(secondFilePath);

		var completionSource = new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);
		var barrierSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

		Task scheduledOperation;

		lock (_syncRoot)
		{
			Task previousGlobalOperation = _queuedGlobalOperation;
			Task[] queuedOperations = GetQueuedOperationsSnapshotUnderLock(normalizedFirstFilePath, normalizedSecondFilePath);

			SetQueuedPerDocumentBarrierUnderLock(normalizedFirstFilePath, barrierSource.Task);
			SetQueuedPerDocumentBarrierUnderLock(normalizedSecondFilePath, barrierSource.Task);

			scheduledOperation = RunExclusivePerDocumentOperationAsync(
				normalizedFirstFilePath,
				normalizedSecondFilePath,
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
		string normalizedFirstFilePath = NormalizeDocumentPath(firstFilePath);
		string normalizedSecondFilePath = NormalizeDocumentPath(secondFilePath);

		Task[] queuedOperations = GetQueuedOperationsSnapshot(normalizedFirstFilePath, normalizedSecondFilePath, includeBarriers: true);

		for (int i = 0; i < queuedOperations.Length; i++)
			await WaitForQueuedOperationAsync(queuedOperations[i]).ConfigureAwait(false);
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
	/// Captures the current queued per-document and latest-update tails for the requested document paths.
	/// </summary>
	/// <param name="firstFilePath">The first document path.</param>
	/// <param name="secondFilePath">The second document path.</param>
	/// <returns>The distinct queued operation tails that were current at snapshot time.</returns>
	private Task[] GetQueuedOperationsSnapshot(string firstFilePath, string secondFilePath, bool includeBarriers = false)
	{
		lock (_syncRoot)
			return GetQueuedOperationsSnapshotUnderLock(firstFilePath, secondFilePath, includeBarriers);
	}

	/// <summary>
	/// Captures the current queued per-document and latest-update tails for the requested document paths.
	/// The caller must hold <see cref="_syncRoot"/>.
	/// </summary>
	private Task[] GetQueuedOperationsSnapshotUnderLock(string firstFilePath, string secondFilePath, bool includeBarriers = false)
	{
		Task firstPerDocumentOperation = GetQueuedPerDocumentOperationUnderLock(firstFilePath);
		Task firstLatestUpdateOperation = GetQueuedLatestUpdateOperationUnderLock(firstFilePath);
		bool samePath = LanguageServerPathHelper.AreLocalPathsEqual(firstFilePath, secondFilePath);

		Task firstBarrierOperation = includeBarriers
			? GetQueuedPerDocumentBarrierUnderLock(firstFilePath)
			: Task.CompletedTask;

		Task secondPerDocumentOperation = samePath
			? Task.CompletedTask
			: GetQueuedPerDocumentOperationUnderLock(secondFilePath);

		Task secondLatestUpdateOperation = samePath
			? Task.CompletedTask
			: GetQueuedLatestUpdateOperationUnderLock(secondFilePath);

		Task secondBarrierOperation = includeBarriers || samePath
			? GetQueuedPerDocumentBarrierUnderLock(secondFilePath)
			: Task.CompletedTask;

		var queuedOperations = new List<Task>(6);
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
	private Task GetQueuedPerDocumentOperationUnderLock(string filePath)
	{
		return _queuedPerDocumentOperations.TryGetValue(filePath, out Task? queuedOperation)
			? queuedOperation
			: Task.CompletedTask;
	}

	/// <summary>
	/// Gets the current exclusion barrier for the supplied path.
	/// The caller must hold <see cref="_syncRoot"/>.
	/// </summary>
	private Task GetQueuedPerDocumentBarrierUnderLock(string filePath)
	{
		return _queuedPerDocumentBarriers.TryGetValue(filePath, out Task? barrierOperation)
			? barrierOperation
			: Task.CompletedTask;
	}

	/// <summary>
	/// Records an exclusion barrier for the supplied document path.
	/// The caller must hold <see cref="_syncRoot"/>.
	/// </summary>
	private void SetQueuedPerDocumentBarrierUnderLock(string filePath, Task barrierOperation)
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
}
