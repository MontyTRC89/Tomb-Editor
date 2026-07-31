namespace Nickelony.LanguageServer.Core.Tests;

[TestClass]
public class DocumentOperationSchedulerTests
{
	[TestMethod]
	public async Task EnqueueGlobalAsync_CanceledWhileWaiting_DoesNotInvokeDelegate()
	{
		var scheduler = new DocumentOperationScheduler();
		var firstStarted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		var allowFirstToFinish = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		using var cancellationTokenSource = new CancellationTokenSource();

		int canceledOperationCallCount = 0;

		Task<bool> firstTask = scheduler.EnqueueGlobalAsync(async _ =>
		{
			firstStarted.TrySetResult(true);
			await allowFirstToFinish.Task.ConfigureAwait(false);
			return true;
		}, CancellationToken.None);

		await firstStarted.Task.ConfigureAwait(false);

		Task<bool> canceledTask = scheduler.EnqueueGlobalAsync(_ =>
		{
			Interlocked.Increment(ref canceledOperationCallCount);
			return Task.FromResult(true);
		}, cancellationTokenSource.Token);

		cancellationTokenSource.Cancel();
		allowFirstToFinish.TrySetResult(true);

		await firstTask.ConfigureAwait(false);
		await Assert.ThrowsExceptionAsync<TaskCanceledException>(() => canceledTask).ConfigureAwait(false);

		Assert.AreEqual(0, canceledOperationCallCount);
	}

	[TestMethod]
	public async Task EnqueuePerDocumentAsync_CanceledWhileWaiting_DoesNotInvokeDelegate()
	{
		var scheduler = new DocumentOperationScheduler();
		var firstStarted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		var allowFirstToFinish = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		using var cancellationTokenSource = new CancellationTokenSource();

		int canceledOperationCallCount = 0;

		Task<bool> firstTask = scheduler.EnqueuePerDocumentAsync("test.lua", async _ =>
		{
			firstStarted.TrySetResult(true);
			await allowFirstToFinish.Task.ConfigureAwait(false);
			return true;
		}, CancellationToken.None);

		await firstStarted.Task.ConfigureAwait(false);

		Task<bool> canceledTask = scheduler.EnqueuePerDocumentAsync("test.lua", _ =>
		{
			Interlocked.Increment(ref canceledOperationCallCount);
			return Task.FromResult(true);
		}, cancellationTokenSource.Token);

		cancellationTokenSource.Cancel();
		allowFirstToFinish.TrySetResult(true);

		await firstTask.ConfigureAwait(false);
		await Assert.ThrowsExceptionAsync<TaskCanceledException>(() => canceledTask).ConfigureAwait(false);

		Assert.AreEqual(0, canceledOperationCallCount);
	}

	[TestMethod]
	public async Task EnqueuePerDocumentAsync_LaterWorkStillRunsAfterCanceledPredecessor()
	{
		var scheduler = new DocumentOperationScheduler();
		var firstStarted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		var allowFirstToFinish = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		using var cancellationTokenSource = new CancellationTokenSource();

		int canceledOperationCallCount = 0;
		int laterOperationCallCount = 0;

		Task<bool> firstTask = scheduler.EnqueuePerDocumentAsync("test.lua", async _ =>
		{
			firstStarted.TrySetResult(true);
			await allowFirstToFinish.Task.ConfigureAwait(false);
			return true;
		}, CancellationToken.None);

		await firstStarted.Task.ConfigureAwait(false);

		Task<bool> canceledTask = scheduler.EnqueuePerDocumentAsync("test.lua", _ =>
		{
			Interlocked.Increment(ref canceledOperationCallCount);
			return Task.FromResult(true);
		}, cancellationTokenSource.Token);

		Task<bool> laterTask = scheduler.EnqueuePerDocumentAsync("test.lua", _ =>
		{
			Interlocked.Increment(ref laterOperationCallCount);
			return Task.FromResult(true);
		}, CancellationToken.None);

		cancellationTokenSource.Cancel();
		allowFirstToFinish.TrySetResult(true);

		await firstTask.ConfigureAwait(false);
		await Assert.ThrowsExceptionAsync<TaskCanceledException>(() => canceledTask).ConfigureAwait(false);
		Assert.IsTrue(await laterTask.ConfigureAwait(false));

		Assert.AreEqual(0, canceledOperationCallCount);
		Assert.AreEqual(1, laterOperationCallCount);
	}

	[TestMethod]
	public async Task EnqueuePerDocumentAsync_NormalizesEquivalentPathsIntoSameQueue()
	{
		var scheduler = new DocumentOperationScheduler();
		string canonicalFilePath = Path.Combine(Path.GetTempPath(), "DocumentOperationSchedulerTests", "test.lua");
		string aliasedFilePath = Path.Combine(Path.GetDirectoryName(canonicalFilePath)!, ".", Path.GetFileName(canonicalFilePath));
		var firstStarted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		var allowFirstToFinish = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		int secondOperationCallCount = 0;

		Task<bool> firstTask = scheduler.EnqueuePerDocumentAsync(canonicalFilePath, async _ =>
		{
			firstStarted.TrySetResult(true);
			await allowFirstToFinish.Task.ConfigureAwait(false);
			return true;
		}, CancellationToken.None);

		await firstStarted.Task.ConfigureAwait(false);

		Task<bool> secondTask = scheduler.EnqueuePerDocumentAsync(aliasedFilePath, _ =>
		{
			Interlocked.Increment(ref secondOperationCallCount);
			return Task.FromResult(true);
		}, CancellationToken.None);

		await Task.Delay(50).ConfigureAwait(false);

		Assert.AreEqual(0, secondOperationCallCount);

		allowFirstToFinish.TrySetResult(true);

		Assert.IsTrue(await firstTask.ConfigureAwait(false));
		Assert.IsTrue(await secondTask.ConfigureAwait(false));
		Assert.AreEqual(1, secondOperationCallCount);
	}

	[TestMethod]
	public async Task QueueLatestUpdateAsync_SerializesRunningWorkAndSkipsSupersededPendingUpdates()
	{
		var scheduler = new DocumentOperationScheduler();
		var firstStarted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		var allowFirstToFinish = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

		int secondStarted = 0;
		int thirdStarted = 0;

		Task firstTask = scheduler.QueueLatestUpdateAsync("test.lua", async _ =>
		{
			firstStarted.TrySetResult(true);
			await allowFirstToFinish.Task.ConfigureAwait(false);
		});

		await firstStarted.Task.ConfigureAwait(false);

		Task secondTask = scheduler.QueueLatestUpdateAsync("test.lua", _ =>
		{
			Interlocked.Increment(ref secondStarted);
			return Task.CompletedTask;
		});

		Task thirdTask = scheduler.QueueLatestUpdateAsync("test.lua", _ =>
		{
			Interlocked.Increment(ref thirdStarted);
			return Task.CompletedTask;
		});

		await Task.Delay(50).ConfigureAwait(false);

		Assert.AreEqual(0, secondStarted);
		Assert.AreEqual(0, thirdStarted);

		allowFirstToFinish.TrySetResult(true);

		await Task.WhenAll(firstTask, secondTask, thirdTask).ConfigureAwait(false);

		Assert.AreEqual(0, secondStarted);
		Assert.AreEqual(1, thirdStarted);
	}

	[TestMethod]
	public async Task QueueLatestUpdateAsync_SupersedingPendingWork_DoesNotCancelRunningUpdate()
	{
		var scheduler = new DocumentOperationScheduler();
		var firstStarted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		var allowFirstToFinish = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		var secondQueued = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		int thirdStarted = 0;

		Task firstTask = scheduler.QueueLatestUpdateAsync("test.lua", async token =>
		{
			firstStarted.TrySetResult(true);
			await secondQueued.Task.ConfigureAwait(false);

			Assert.IsFalse(token.IsCancellationRequested, "A newer queued update should not cancel the already-running update.");
			Assert.IsNotNull(token.WaitHandle, "The running update should keep owning its token source until it finishes.");

			await allowFirstToFinish.Task.ConfigureAwait(false);
		});

		await firstStarted.Task.ConfigureAwait(false);

		Task secondTask = scheduler.QueueLatestUpdateAsync("test.lua", _ => Task.CompletedTask);
		Task thirdTask = scheduler.QueueLatestUpdateAsync("test.lua", _ =>
		{
			Interlocked.Increment(ref thirdStarted);
			return Task.CompletedTask;
		});

		secondQueued.TrySetResult(true);
		await Task.Delay(50).ConfigureAwait(false);

		Assert.AreEqual(0, thirdStarted);

		allowFirstToFinish.TrySetResult(true);

		await Task.WhenAll(firstTask, secondTask, thirdTask).ConfigureAwait(false);

		Assert.AreEqual(1, thirdStarted);
	}

	[TestMethod]
	public async Task WaitForPerDocumentOperationsAsync_WaitsForQueuedLatestUpdatesForSamePath()
	{
		var scheduler = new DocumentOperationScheduler();
		var firstStarted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		var allowFirstToFinish = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		int secondStarted = 0;

		Task firstTask = scheduler.QueueLatestUpdateAsync("test.lua", async _ =>
		{
			firstStarted.TrySetResult(true);
			await allowFirstToFinish.Task.ConfigureAwait(false);
		});

		await firstStarted.Task.ConfigureAwait(false);

		Task secondTask = scheduler.QueueLatestUpdateAsync("test.lua", _ =>
		{
			Interlocked.Increment(ref secondStarted);
			return Task.CompletedTask;
		});

		Task waitTask = scheduler.WaitForPerDocumentOperationsAsync("test.lua", "test.lua");

		await Task.Delay(50).ConfigureAwait(false);

		Assert.IsFalse(waitTask.IsCompleted, "Waiting for document operations should include queued latest-update work for the same path.");

		allowFirstToFinish.TrySetResult(true);

		await Task.WhenAll(firstTask, secondTask, waitTask).ConfigureAwait(false);

		Assert.AreEqual(1, secondStarted);
	}

	[TestMethod]
	public async Task EnqueueExclusivePerDocumentAsync_BlocksLaterPerDocumentWorkOnAffectedPath()
	{
		var scheduler = new DocumentOperationScheduler();
		var exclusiveStarted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		var allowExclusiveToFinish = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		int laterOperationCallCount = 0;

		Task<bool> exclusiveTask = scheduler.EnqueueExclusivePerDocumentAsync(
			"old.lua",
			"new.lua",
			async _ =>
			{
				exclusiveStarted.TrySetResult(true);
				await allowExclusiveToFinish.Task.ConfigureAwait(false);
				return true;
			},
			CancellationToken.None);

		await exclusiveStarted.Task.ConfigureAwait(false);

		Task<bool> laterTask = scheduler.EnqueuePerDocumentAsync("new.lua", _ =>
		{
			Interlocked.Increment(ref laterOperationCallCount);
			return Task.FromResult(true);
		}, CancellationToken.None);

		await Task.Delay(50).ConfigureAwait(false);

		Assert.AreEqual(0, laterOperationCallCount);

		allowExclusiveToFinish.TrySetResult(true);

		Assert.IsTrue(await exclusiveTask.ConfigureAwait(false));
		Assert.IsTrue(await laterTask.ConfigureAwait(false));

		Assert.AreEqual(1, laterOperationCallCount);
	}

	[TestMethod]
	public async Task WaitForPerDocumentOperationsAsync_WaitsForActiveExclusiveBarrier()
	{
		var scheduler = new DocumentOperationScheduler();
		var exclusiveStarted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		var allowExclusiveToFinish = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

		Task<bool> exclusiveTask = scheduler.EnqueueExclusivePerDocumentAsync(
			"old.lua",
			"new.lua",
			async _ =>
			{
				exclusiveStarted.TrySetResult(true);
				await allowExclusiveToFinish.Task.ConfigureAwait(false);
				return true;
			},
			CancellationToken.None);

		await exclusiveStarted.Task.ConfigureAwait(false);

		Task waitTask = scheduler.WaitForPerDocumentOperationsAsync("old.lua", "new.lua");

		await Task.Delay(50).ConfigureAwait(false);

		Assert.IsFalse(waitTask.IsCompleted, "Waiting for document operations should include active exclusive barriers for the affected paths.");

		allowExclusiveToFinish.TrySetResult(true);

		Assert.IsTrue(await exclusiveTask.ConfigureAwait(false));
		await waitTask.ConfigureAwait(false);
	}
}
