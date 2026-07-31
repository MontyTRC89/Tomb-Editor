namespace Nickelony.LanguageServer.Core;

public sealed partial class DocumentOperationScheduler
{
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
	/// Runs an operation after a previously scheduled operation has completed.
	/// </summary>
	/// <typeparam name="TResult">The operation result type.</typeparam>
	/// <param name="previousOperation">The operation that must complete first.</param>
	/// <param name="barrierOperation">The exclusion barrier that must also complete first.</param>
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
}
