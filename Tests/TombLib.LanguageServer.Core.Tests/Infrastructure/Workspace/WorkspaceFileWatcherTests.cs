using Microsoft.Extensions.Logging;
using Nickelony.LanguageServer.Testing;

namespace Nickelony.LanguageServer.Core.Tests;

[TestClass]
public class WorkspaceFileWatcherTests
{
	[TestMethod]
	public async Task DispatchPendingChangesForTestAsync_PreservesDeleteThenCreatePairForSamePath()
	{
		using var workspace = new TemporaryWorkspaceRoot("LuaWatcherCoalesce_");
		string workspaceRoot = workspace.DirectoryPath;
		WorkspaceWatchSpecification[] watchSpecifications = [new("*.lua", IncludeSubdirectories: true)];
		FileChangeBatch? dispatchedBatch = null;

		await using var watcher = new WorkspaceFileWatcher(workspaceRoot, (batch, _) =>
		{
			dispatchedBatch = batch;
			return Task.CompletedTask;
		}, watchSpecifications);

		string filePath = Path.Combine(workspaceRoot, "test.lua");

		QueueChangeForTest(watcher, filePath, FileChangeKind.Deleted);
		QueueChangeForTest(watcher, filePath, FileChangeKind.Created);
		await DispatchPendingChangesForTestAsync(watcher).ConfigureAwait(false);

		Assert.IsNotNull(dispatchedBatch);
		Assert.AreEqual(2, dispatchedBatch.Count);
		Assert.AreEqual(filePath, dispatchedBatch.Entries[0].Path);
		Assert.AreEqual(FileChangeKind.Deleted, dispatchedBatch.Entries[0].Kind);
		Assert.AreEqual(filePath, dispatchedBatch.Entries[1].Path);
		Assert.AreEqual(FileChangeKind.Created, dispatchedBatch.Entries[1].Kind);
	}

	[TestMethod]
	public async Task DispatchPendingChangesForTestAsync_NormalizesEquivalentPathFormsBeforeCoalescing()
	{
		using var workspace = new TemporaryWorkspaceRoot("LuaWatcherNormalize_");
		string workspaceRoot = workspace.DirectoryPath;
		WorkspaceWatchSpecification[] watchSpecifications = [new("*.lua", IncludeSubdirectories: true)];
		FileChangeBatch? dispatchedBatch = null;

		Directory.CreateDirectory(Path.Combine(workspaceRoot, "Scripts"));

		await using var watcher = new WorkspaceFileWatcher(workspaceRoot, (batch, _) =>
		{
			dispatchedBatch = batch;
			return Task.CompletedTask;
		}, watchSpecifications);

		string normalizedPath = Path.Combine(workspaceRoot, "Scripts", "test.lua");
		string alternatePath = normalizedPath.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

		QueueChangeForTest(watcher, normalizedPath, FileChangeKind.Changed);
		QueueChangeForTest(watcher, alternatePath, FileChangeKind.Changed);
		await DispatchPendingChangesForTestAsync(watcher).ConfigureAwait(false);

		Assert.IsNotNull(dispatchedBatch);
		Assert.AreEqual(1, dispatchedBatch.Count);
		Assert.AreEqual(LanguageServerPathHelper.NormalizeLocalPath(normalizedPath), dispatchedBatch.Entries[0].Path);
		Assert.AreEqual(FileChangeKind.Changed, dispatchedBatch.Entries[0].Kind);
	}

	[TestMethod]
	public async Task DispatchPendingChangesForTestAsync_WhenDeleteCreateRetryIsNeeded_PreservesBothEntries()
	{
		using var workspace = new TemporaryWorkspaceRoot("LuaWatcherRetryPair_");
		string workspaceRoot = workspace.DirectoryPath;
		WorkspaceWatchSpecification[] watchSpecifications = [new("*.lua", IncludeSubdirectories: true)];
		FileChangeBatch? dispatchedBatch = null;
		int dispatchAttemptCount = 0;

		await using var watcher = new WorkspaceFileWatcher(workspaceRoot, (batch, _) =>
		{
			dispatchAttemptCount++;

			if (dispatchAttemptCount == 1)
				throw new IOException("Simulated dispatch failure.");

			dispatchedBatch = batch;
			return Task.CompletedTask;
		}, watchSpecifications);

		string filePath = Path.Combine(workspaceRoot, "test.lua");

		QueueChangeForTest(watcher, filePath, FileChangeKind.Deleted);
		QueueChangeForTest(watcher, filePath, FileChangeKind.Created);
		await DispatchPendingChangesForTestAsync(watcher).ConfigureAwait(false);
		await DispatchPendingChangesForTestAsync(watcher).ConfigureAwait(false);

		Assert.AreEqual(2, dispatchAttemptCount);
		Assert.IsNotNull(dispatchedBatch);
		Assert.AreEqual(2, dispatchedBatch.Count);
		Assert.AreEqual(FileChangeKind.Deleted, dispatchedBatch.Entries[0].Kind);
		Assert.AreEqual(FileChangeKind.Created, dispatchedBatch.Entries[1].Kind);
	}

	[TestMethod]
	public void Start_MissingWorkspaceRoot_ReturnsWorkspaceRootMissingWithoutException()
	{
		string workspaceRoot = Path.Combine(Path.GetTempPath(), "WorkspaceWatcherMissing_" + Guid.NewGuid().ToString("N"));

		using var watcher = new WorkspaceFileWatcher(
			workspaceRoot,
			(_, _) => Task.CompletedTask,
			[new WorkspaceWatchSpecification("*.lua", IncludeSubdirectories: true)]);

		WorkspaceWatcherStartStatus startStatus = watcher.Start(out Exception? startupException);

		Assert.AreEqual(WorkspaceWatcherStartStatus.WorkspaceRootMissing, startStatus);
		Assert.IsNull(startupException);
		Assert.IsFalse(watcher.HasActiveWatchers);
	}

	[TestMethod]
	public void Start_FileSystemWatcherFactoryThrows_ReturnsStartupFailedAndException()
	{
		using var workspace = new TemporaryWorkspaceRoot("WorkspaceWatcherFactoryThrow_");
		string workspaceRoot = workspace.DirectoryPath;

		using var watcher = new WorkspaceFileWatcher(
			workspaceRoot,
			(_, _) => Task.CompletedTask,
			[new WorkspaceWatchSpecification("*.lua", IncludeSubdirectories: false)],
			fileSystemWatcherFactory: static (_, _) => throw new InvalidOperationException("Simulated watcher creation failure."));

		WorkspaceWatcherStartStatus startStatus = watcher.Start(out Exception? startupException);

		Assert.AreEqual(WorkspaceWatcherStartStatus.StartupFailed, startStatus);
		Assert.IsNotNull(startupException);
		Assert.IsTrue(watcher.IsDisposed);
	}

	[TestMethod]
	public void Start_AfterStartupFailureOnSameInstance_ReturnsDisposed()
	{
		using var workspace = new TemporaryWorkspaceRoot("WorkspaceWatcherRetryAfterFailure_");
		string workspaceRoot = workspace.DirectoryPath;

		using var watcher = new WorkspaceFileWatcher(
			workspaceRoot,
			(_, _) => Task.CompletedTask,
			[new WorkspaceWatchSpecification("*.lua", IncludeSubdirectories: false)],
			fileSystemWatcherFactory: static (_, _) => throw new InvalidOperationException("Simulated watcher creation failure."));

		WorkspaceWatcherStartStatus firstStartStatus = watcher.Start(out Exception? startupException);
		WorkspaceWatcherStartStatus retryStatus = watcher.Start(out Exception? retryException);

		Assert.AreEqual(WorkspaceWatcherStartStatus.StartupFailed, firstStartStatus);
		Assert.IsNotNull(startupException);
		Assert.AreEqual(WorkspaceWatcherStartStatus.Disposed, retryStatus);
		Assert.IsNull(retryException);
		Assert.IsTrue(watcher.IsDisposed);
	}

	[TestMethod]
	public async Task DispatchPendingChangesForTestAsync_WhenDispatchFails_RetainsBatchForRetry()
	{
		using var workspace = new TemporaryWorkspaceRoot("LuaWatcherRetry_");
		string workspaceRoot = workspace.DirectoryPath;
		WorkspaceWatchSpecification[] watchSpecifications = [new("*.lua", IncludeSubdirectories: true)];
		FileChangeBatch? dispatchedBatch = null;
		int dispatchAttemptCount = 0;

		using var logScope = new TestLoggerScope(LogLevel.Debug);

		await using var watcher = new WorkspaceFileWatcher(workspaceRoot, (batch, _) =>
		{
			dispatchAttemptCount++;

			if (dispatchAttemptCount == 1)
				throw new IOException("Simulated dispatch failure.");

			dispatchedBatch = batch;
			return Task.CompletedTask;
		}, watchSpecifications, logger: logScope.CreateLogger<WorkspaceFileWatcher>());

		string filePath = Path.Combine(workspaceRoot, "test.lua");

		QueueChangeForTest(watcher, filePath, FileChangeKind.Changed);
		await DispatchPendingChangesForTestAsync(watcher).ConfigureAwait(false);
		await DispatchPendingChangesForTestAsync(watcher).ConfigureAwait(false);

		Assert.AreEqual(2, dispatchAttemptCount);
		Assert.IsNotNull(dispatchedBatch);
		Assert.AreEqual(1, dispatchedBatch.Count);
		Assert.AreEqual(filePath, dispatchedBatch.Entries[0].Path);
		Assert.AreEqual(FileChangeKind.Changed, dispatchedBatch.Entries[0].Kind);

		Assert.IsTrue(logScope.Logs.Any(log => log.Contains("Workspace file watcher dispatch failed", StringComparison.OrdinalIgnoreCase)
			&& log.Contains("Simulated dispatch failure.", StringComparison.Ordinal)
			&& log.Contains(workspaceRoot, StringComparison.OrdinalIgnoreCase)
			&& log.Contains("1 queued change", StringComparison.OrdinalIgnoreCase)),
			string.Join(Environment.NewLine, logScope.Logs));
	}

	[TestMethod]
	public async Task DispatchPendingChangesForTestAsync_WhenDispatchKeepsFailing_EscalatesLogLevelAndBackoff()
	{
		using var workspace = new TemporaryWorkspaceRoot("LuaWatcherBackoff_");
		string workspaceRoot = workspace.DirectoryPath;
		WorkspaceWatchSpecification[] watchSpecifications = [new("*.lua", IncludeSubdirectories: true)];

		using var logScope = new TestLoggerScope(LogLevel.Debug);

		await using var watcher = new WorkspaceFileWatcher(workspaceRoot, (_, _) => throw new IOException("Persistent dispatch failure."), watchSpecifications, logger: logScope.CreateLogger<WorkspaceFileWatcher>());

		string filePath = Path.Combine(workspaceRoot, "test.lua");

		QueueChangeForTest(watcher, filePath, FileChangeKind.Changed);
		await DispatchPendingChangesForTestAsync(watcher).ConfigureAwait(false);
		await DispatchPendingChangesForTestAsync(watcher).ConfigureAwait(false);
		await DispatchPendingChangesForTestAsync(watcher).ConfigureAwait(false);

		Assert.IsTrue(logScope.Logs.Any(log => log.StartsWith("Debug|", StringComparison.Ordinal)
			&& log.Contains("retrying in 250 ms", StringComparison.OrdinalIgnoreCase)),
			string.Join(Environment.NewLine, logScope.Logs));

		Assert.IsTrue(logScope.Logs.Any(log => log.StartsWith("Debug|", StringComparison.Ordinal)
			&& log.Contains("retrying in 500 ms", StringComparison.OrdinalIgnoreCase)),
			string.Join(Environment.NewLine, logScope.Logs));

		Assert.IsTrue(logScope.Logs.Any(log => log.StartsWith("Warn|", StringComparison.Ordinal)
			&& log.Contains("3 times in a row", StringComparison.OrdinalIgnoreCase)
			&& log.Contains("retrying in 1000 ms", StringComparison.OrdinalIgnoreCase)
			&& log.Contains("backoff", StringComparison.OrdinalIgnoreCase)),
			string.Join(Environment.NewLine, logScope.Logs));
	}

	[TestMethod]
	public async Task DispatchPendingChangesForTestAsync_WhenDispatchKeepsFailing_ReportsWatcherFailureAfterBoundedRetries()
	{
		using var workspace = new TemporaryWorkspaceRoot("LuaWatcherEscalate_");
		string workspaceRoot = workspace.DirectoryPath;
		WorkspaceWatchSpecification[] watchSpecifications = [new("*.lua", IncludeSubdirectories: true)];
		int watcherFailedCallCount = 0;
		Exception? reportedException = null;

		await using var watcher = new WorkspaceFileWatcher(
			workspaceRoot,
			(_, _) => throw new IOException("Persistent dispatch failure."),
			watchSpecifications,
			(_, exception) =>
			{
				watcherFailedCallCount++;
				reportedException = exception;
			});

		Assert.IsTrue(watcher.Start());

		string filePath = Path.Combine(workspaceRoot, "test.lua");

		QueueChangeForTest(watcher, filePath, FileChangeKind.Changed);

		for (int i = 0; i < 5; i++)
			await DispatchPendingChangesForTestAsync(watcher).ConfigureAwait(false);

		Assert.AreEqual(1, watcherFailedCallCount);
		Assert.IsInstanceOfType(reportedException, typeof(IOException));
		Assert.IsFalse(watcher.HasActiveWatchers);
	}

	[TestMethod]
	public async Task DispatchPendingChangesForTestAsync_WhenDispatchEscalates_PreservesFinalBatchForRecoveryDispatch()
	{
		using var workspace = new TemporaryWorkspaceRoot("LuaWatcherRecoveryBatch_");
		string workspaceRoot = workspace.DirectoryPath;
		WorkspaceWatchSpecification[] watchSpecifications = [new("*.lua", IncludeSubdirectories: true)];
		var recoveredBatch = new TaskCompletionSource<FileChangeBatch>(TaskCreationOptions.RunContinuationsAsynchronously);
		int watcherFailedCallCount = 0;
		int dispatchAttemptCount = 0;
		int allowRecoveryDispatch = 0;

		await using var watcher = new WorkspaceFileWatcher(
			workspaceRoot,
			(batch, _) =>
			{
				Interlocked.Increment(ref dispatchAttemptCount);

				if (Volatile.Read(ref allowRecoveryDispatch) == 0)
					throw new IOException("Persistent dispatch failure.");

				recoveredBatch.TrySetResult(batch);
				return Task.CompletedTask;
			},
			watchSpecifications,
			(_, _) =>
			{
				Interlocked.Increment(ref watcherFailedCallCount);
				Interlocked.Exchange(ref allowRecoveryDispatch, 1);
			});

		Assert.IsTrue(watcher.Start());

		string filePath = Path.Combine(workspaceRoot, "test.lua");

		QueueChangeForTest(watcher, filePath, FileChangeKind.Changed);

		for (int i = 0; i < 5; i++)
			await DispatchPendingChangesForTestAsync(watcher).ConfigureAwait(false);

		FileChangeBatch dispatchedBatch = await recoveredBatch.Task.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);

		Assert.AreEqual(1, watcherFailedCallCount);
		Assert.AreEqual(6, dispatchAttemptCount);
		Assert.IsFalse(watcher.HasActiveWatchers);
		Assert.AreEqual(1, dispatchedBatch.Count);
		Assert.AreEqual(filePath, dispatchedBatch.Entries[0].Path);
		Assert.AreEqual(FileChangeKind.Changed, dispatchedBatch.Entries[0].Kind);
	}

	[TestMethod]
	public async Task Dispose_DuringActiveDispatch_DoesNotFaultDispatch()
	{
		using var workspace = new TemporaryWorkspaceRoot("LuaWatcherDispose_");
		string workspaceRoot = workspace.DirectoryPath;
		WorkspaceWatchSpecification[] watchSpecifications = [new("*.lua", IncludeSubdirectories: true)];
		var dispatchStarted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		var allowDispatchToFinish = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

		await using var watcher = new WorkspaceFileWatcher(workspaceRoot, async (_, _) =>
		{
			dispatchStarted.TrySetResult(true);
			await allowDispatchToFinish.Task.ConfigureAwait(false);
		}, watchSpecifications);

		QueueChangeForTest(watcher, Path.Combine(workspaceRoot, "test.lua"), FileChangeKind.Changed);
		Task dispatchTask = DispatchPendingChangesForTestAsync(watcher);

		Task completedTask = await Task.WhenAny(dispatchStarted.Task, Task.Delay(TimeSpan.FromSeconds(1))).ConfigureAwait(false);
		Assert.AreSame(dispatchStarted.Task, completedTask);

		Task disposeTask = Task.Run(watcher.Dispose);
		Assert.IsFalse(disposeTask.IsCompleted);

		allowDispatchToFinish.TrySetResult(true);

		await Task.WhenAll(dispatchTask, disposeTask).ConfigureAwait(false);
	}

	[TestMethod]
	public async Task DisposeWithoutFinalFlush_DuringActiveDispatch_DropsRequeuedBatch()
	{
		using var workspace = new TemporaryWorkspaceRoot("LuaWatcherDisposeNoFlush_");
		string workspaceRoot = workspace.DirectoryPath;
		WorkspaceWatchSpecification[] watchSpecifications = [new("*.lua", IncludeSubdirectories: true)];
		var dispatchStarted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		var allowFirstDispatchToFinish = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		FileChangeBatch? dispatchedBatch = null;
		int dispatchAttemptCount = 0;

		await using var watcher = new WorkspaceFileWatcher(workspaceRoot, async (batch, _) =>
		{
			dispatchAttemptCount++;

			if (dispatchAttemptCount == 1)
			{
				dispatchStarted.TrySetResult(true);
				await allowFirstDispatchToFinish.Task.ConfigureAwait(false);
				throw new IOException("Simulated dispatch failure during no-flush disposal.");
			}

			dispatchedBatch = batch;
		}, watchSpecifications);

		string filePath = Path.Combine(workspaceRoot, "test.lua");

		QueueChangeForTest(watcher, filePath, FileChangeKind.Changed);
		Task dispatchTask = DispatchPendingChangesForTestAsync(watcher);

		await dispatchStarted.Task.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);

		Task disposeTask = Task.Run(watcher.DisposeWithoutFinalFlush);
		Assert.IsFalse(disposeTask.IsCompleted);

		allowFirstDispatchToFinish.TrySetResult(true);

		await Task.WhenAll(dispatchTask, disposeTask).ConfigureAwait(false);

		Assert.AreEqual(1, dispatchAttemptCount);
		Assert.IsNull(dispatchedBatch);
	}

	[TestMethod]
	public async Task DisposeAsync_DuringActiveDispatch_PreservesRequeuedBatchForFinalFlush()
	{
		using var workspace = new TemporaryWorkspaceRoot("LuaWatcherDisposeRetry_");
		string workspaceRoot = workspace.DirectoryPath;
		WorkspaceWatchSpecification[] watchSpecifications = [new("*.lua", IncludeSubdirectories: true)];
		var dispatchStarted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		var allowFirstDispatchToFinish = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		FileChangeBatch? dispatchedBatch = null;
		int dispatchAttemptCount = 0;

		await using var watcher = new WorkspaceFileWatcher(workspaceRoot, async (batch, _) =>
		{
			dispatchAttemptCount++;

			if (dispatchAttemptCount == 1)
			{
				dispatchStarted.TrySetResult(true);
				await allowFirstDispatchToFinish.Task.ConfigureAwait(false);
				throw new IOException("Simulated dispatch failure during disposal.");
			}

			dispatchedBatch = batch;
		}, watchSpecifications);

		string filePath = Path.Combine(workspaceRoot, "test.lua");

		QueueChangeForTest(watcher, filePath, FileChangeKind.Changed);
		Task dispatchTask = DispatchPendingChangesForTestAsync(watcher);

		await dispatchStarted.Task.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);

		Task disposeTask = watcher.DisposeAsync().AsTask();

		Assert.IsFalse(disposeTask.IsCompleted);

		allowFirstDispatchToFinish.TrySetResult(true);

		await Task.WhenAll(dispatchTask, disposeTask).ConfigureAwait(false);

		Assert.AreEqual(2, dispatchAttemptCount);
		Assert.IsNotNull(dispatchedBatch);
		Assert.AreEqual(1, dispatchedBatch.Count);
		Assert.AreEqual(filePath, dispatchedBatch.Entries[0].Path);
		Assert.AreEqual(FileChangeKind.Changed, dispatchedBatch.Entries[0].Kind);
	}

	[TestMethod]
	public async Task DisposeAsync_WhenFinalFlushStalls_CompletesWithoutWaitingIndefinitely()
	{
		using var workspace = new TemporaryWorkspaceRoot("LuaWatcherDisposeTimedFlush_");
		string workspaceRoot = workspace.DirectoryPath;
		WorkspaceWatchSpecification[] watchSpecifications = [new("*.lua", IncludeSubdirectories: true)];
		var dispatchStarted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		var allowFirstDispatchToFinish = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		var finalFlushStarted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		var finalFlushExited = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		int finalFlushCancellationObserved = 0;

		await using var watcher = new WorkspaceFileWatcher(workspaceRoot, async (_, cancellationToken) =>
		{
			if (!dispatchStarted.Task.IsCompleted)
			{
				dispatchStarted.TrySetResult(true);
				await allowFirstDispatchToFinish.Task.ConfigureAwait(false);
				throw new IOException("Simulated dispatch failure during disposal.");
			}

			finalFlushStarted.TrySetResult(true);

			using CancellationTokenRegistration cancellationRegistration = cancellationToken.Register(
				() => Interlocked.Exchange(ref finalFlushCancellationObserved, 1));

			try
			{
				await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken).ConfigureAwait(false);
			}
			catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
			{
				Interlocked.Exchange(ref finalFlushCancellationObserved, 1);
			}
			finally
			{
				finalFlushExited.TrySetResult(true);
			}
		}, watchSpecifications);

		string filePath = Path.Combine(workspaceRoot, "test.lua");

		QueueChangeForTest(watcher, filePath, FileChangeKind.Changed);
		Task dispatchTask = DispatchPendingChangesForTestAsync(watcher);

		await dispatchStarted.Task.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);

		Task disposeTask = watcher.DisposeAsync().AsTask();

		allowFirstDispatchToFinish.TrySetResult(true);

		await finalFlushStarted.Task.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
		await Task.WhenAll(dispatchTask, disposeTask.WaitAsync(TimeSpan.FromSeconds(5))).ConfigureAwait(false);

		Assert.IsTrue(finalFlushExited.Task.IsCompleted);
		Assert.AreEqual(1, Volatile.Read(ref finalFlushCancellationObserved));
	}

	[TestMethod]
	public async Task ReportErrorForTest_WithPendingChanges_WaitsForFailureHandlerBeforeRecoveryDispatch()
	{
		using var workspace = new TemporaryWorkspaceRoot("WorkspaceWatcherRecoveryOrdering_");
		string workspaceRoot = workspace.DirectoryPath;
		WorkspaceWatchSpecification[] watchSpecifications = [new("*.lua", IncludeSubdirectories: true)];
		var failureHandlerEntered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		var allowFailureHandlerToFinish = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		var recoveryDispatchStarted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		int dispatchObservedBeforeFailureHandlerFinished = 0;

		await using var watcher = new WorkspaceFileWatcher(
			workspaceRoot,
			(_, _) =>
			{
				if (!allowFailureHandlerToFinish.Task.IsCompleted)
					Interlocked.Exchange(ref dispatchObservedBeforeFailureHandlerFinished, 1);

				recoveryDispatchStarted.TrySetResult(true);
				return Task.CompletedTask;
			},
			watchSpecifications,
			(_, _) =>
			{
				failureHandlerEntered.TrySetResult(true);
				allowFailureHandlerToFinish.Task.GetAwaiter().GetResult();
			});

		Assert.IsTrue(watcher.Start());

		QueueChangeForTest(watcher, Path.Combine(workspaceRoot, "test.lua"), FileChangeKind.Changed);
		Task errorTask = Task.Run(() => ReportErrorForTest(watcher, new IOException("Simulated watcher failure.")));

		await failureHandlerEntered.Task.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);

		Task completedTask = await Task.WhenAny(recoveryDispatchStarted.Task, Task.Delay(TimeSpan.FromMilliseconds(200))).ConfigureAwait(false);
		Assert.AreNotSame(recoveryDispatchStarted.Task, completedTask);
		Assert.AreEqual(0, Volatile.Read(ref dispatchObservedBeforeFailureHandlerFinished));

		allowFailureHandlerToFinish.TrySetResult(true);

		await errorTask.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
		await recoveryDispatchStarted.Task.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
		Assert.AreEqual(0, Volatile.Read(ref dispatchObservedBeforeFailureHandlerFinished));
	}

	[TestMethod]
	public void Dispose_WhenPendingChangesExistAndNoDispatchIsActive_DropsBufferedBatch()
	{
		using var workspace = new TemporaryWorkspaceRoot("LuaWatcherDisposeFlush_");
		string workspaceRoot = workspace.DirectoryPath;
		WorkspaceWatchSpecification[] watchSpecifications = [new("*.lua", IncludeSubdirectories: true)];
		FileChangeBatch? dispatchedBatch = null;

		using var watcher = new WorkspaceFileWatcher(workspaceRoot, (batch, _) =>
		{
			dispatchedBatch = batch;
			return Task.CompletedTask;
		}, watchSpecifications);

		QueueChangeForTest(watcher, Path.Combine(workspaceRoot, "test.lua"), FileChangeKind.Changed);

		watcher.Dispose();

		Assert.IsNull(dispatchedBatch);
	}

	[TestMethod]
	public void DisposeWithoutFinalFlush_WhenPendingChangesExistAndNoDispatchIsActive_DropsBufferedBatch()
	{
		using var workspace = new TemporaryWorkspaceRoot("LuaWatcherDisposeNoFlushPending_");
		string workspaceRoot = workspace.DirectoryPath;
		WorkspaceWatchSpecification[] watchSpecifications = [new("*.lua", IncludeSubdirectories: true)];
		FileChangeBatch? dispatchedBatch = null;

		using var watcher = new WorkspaceFileWatcher(workspaceRoot, (batch, _) =>
		{
			dispatchedBatch = batch;
			return Task.CompletedTask;
		}, watchSpecifications);

		QueueChangeForTest(watcher, Path.Combine(workspaceRoot, "test.lua"), FileChangeKind.Changed);

		watcher.DisposeWithoutFinalFlush();

		Assert.IsNull(dispatchedBatch);
	}

	[TestMethod]
	public void Dispose_WhenBufferedChangesExist_DoesNotDeadlockCallerContext()
	{
		using var workspace = new TemporaryWorkspaceRoot("LuaWatcherDisposeContext_");
		string workspaceRoot = workspace.DirectoryPath;
		WorkspaceWatchSpecification[] watchSpecifications = [new("*.lua", IncludeSubdirectories: true)];
		var disposeCompleted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		Exception? failure = null;

		var thread = new Thread(() =>
		{
			SynchronizationContext.SetSynchronizationContext(new NonPumpingSynchronizationContext());

			try
			{
				using var watcher = new WorkspaceFileWatcher(
					workspaceRoot,
					async (_, _) => await Task.Yield(),
					watchSpecifications);

				QueueChangeForTest(watcher, Path.Combine(workspaceRoot, "test.lua"), FileChangeKind.Changed);

				watcher.Dispose();
				disposeCompleted.TrySetResult(true);
			}
			catch (Exception exception)
			{
				failure = exception;
				disposeCompleted.TrySetException(exception);
			}
			finally
			{
				SynchronizationContext.SetSynchronizationContext(null);
			}
		})
		{
			IsBackground = true
		};

		thread.Start();

		Assert.IsTrue(disposeCompleted.Task.Wait(TimeSpan.FromSeconds(5)));
		Assert.IsTrue(thread.Join(TimeSpan.FromSeconds(1)));
		Assert.IsNull(failure);
	}

	[TestMethod]
	public void Start_UsesConfiguredWatchSpecifications()
	{
		using var workspace = new TemporaryWorkspaceRoot("WorkspaceWatcherSpecs_");
		string workspaceRoot = workspace.DirectoryPath;

		using var watcher = new WorkspaceFileWatcher(
			workspaceRoot,
			(_, _) => Task.CompletedTask,
			watchSpecifications:
			[
				new WorkspaceWatchSpecification("*.lua", IncludeSubdirectories: true),
				new WorkspaceWatchSpecification(".luarc.*", IncludeSubdirectories: false)
			]);

		Assert.IsTrue(watcher.Start());
		Assert.AreEqual(2, watcher.ActiveWatcherCount);
		Assert.IsTrue(watcher.HasActiveWatchers);
	}

	[TestMethod]
	public async Task ReportErrorForTest_ConcurrentWithDispose_LeavesNoActiveWatchers()
	{
		using var workspace = new TemporaryWorkspaceRoot("WorkspaceWatcherErrorDispose_");
		string workspaceRoot = workspace.DirectoryPath;

		for (int i = 0; i < 50; i++)
		{
			await using var watcher = new WorkspaceFileWatcher(
				workspaceRoot,
				(_, _) => Task.CompletedTask,
				[new WorkspaceWatchSpecification("*.lua", IncludeSubdirectories: true)]);

			Assert.IsTrue(watcher.Start());

			Task errorTask = Task.Run(() => ReportErrorForTest(watcher, new IOException("Simulated watcher failure.")));
			Task disposeTask = Task.Run(watcher.Dispose);

			await Task.WhenAll(errorTask, disposeTask).ConfigureAwait(false);

			Assert.IsTrue(watcher.IsDisposed);
			Assert.AreEqual(0, watcher.ActiveWatcherCount);
			Assert.IsFalse(watcher.HasActiveWatchers);
		}
	}

	[TestMethod]
	public void ReportErrorForTest_WhenFailureHandlerThrows_LogsWarningAndStopsWatching()
	{
		using var workspace = new TemporaryWorkspaceRoot("WorkspaceWatcherFailureCallback_");
		string workspaceRoot = workspace.DirectoryPath;
		using var logScope = new TestLoggerScope(LogLevel.Warning);

		using var watcher = new WorkspaceFileWatcher(
			workspaceRoot,
			(_, _) => Task.CompletedTask,
			[new WorkspaceWatchSpecification("*.lua", IncludeSubdirectories: true)],
			(_, _) => throw new InvalidOperationException("Simulated watcher failure callback exception."),
			logger: logScope.CreateLogger<WorkspaceFileWatcher>());

		Assert.IsTrue(watcher.Start());

		ReportErrorForTest(watcher, new IOException("Simulated watcher failure."));

		Assert.IsFalse(watcher.HasActiveWatchers);
		Assert.AreEqual(0, watcher.ActiveWatcherCount);
		Assert.IsTrue(logScope.Logs.Any(log => log.Contains("Workspace watcher failure handler threw.", StringComparison.OrdinalIgnoreCase)
			&& log.Contains("Simulated watcher failure callback exception.", StringComparison.Ordinal)),
			string.Join(Environment.NewLine, logScope.Logs));
	}

	[TestMethod]
	public void Start_AfterFailure_ResetsFailureReportingForNextFailureSequence()
	{
		using var workspace = new TemporaryWorkspaceRoot("WorkspaceWatcherRestart_");
		string workspaceRoot = workspace.DirectoryPath;
		int failureCount = 0;

		using var watcher = new WorkspaceFileWatcher(
			workspaceRoot,
			(_, _) => Task.CompletedTask,
			[new WorkspaceWatchSpecification("*.lua", IncludeSubdirectories: true)],
			(_, _) => failureCount++);

		Assert.IsTrue(watcher.Start());

		ReportErrorForTest(watcher, new IOException("Simulated watcher failure 1."));

		Assert.AreEqual(1, failureCount);
		Assert.IsFalse(watcher.HasActiveWatchers);

		Assert.IsTrue(watcher.Start());

		ReportErrorForTest(watcher, new IOException("Simulated watcher failure 2."));

		Assert.AreEqual(2, failureCount);
		Assert.IsFalse(watcher.HasActiveWatchers);
	}

	[TestMethod]
	public async Task Start_ConcurrentWithDispose_DoesNotLeaveOwnedWatchersBehind()
	{
		using var workspace = new TemporaryWorkspaceRoot("WorkspaceWatcherStartDispose_");
		string workspaceRoot = workspace.DirectoryPath;

		for (int i = 0; i < 50; i++)
		{
			var watcher = new WorkspaceFileWatcher(
				workspaceRoot,
				(_, _) => Task.CompletedTask,
				[new WorkspaceWatchSpecification("*.lua", IncludeSubdirectories: true)]);

			Task<bool> startTask = Task.Run(watcher.Start);
			Task disposeTask = Task.Run(watcher.Dispose);

			await Task.WhenAll(startTask, disposeTask).ConfigureAwait(false);

			Assert.IsTrue(watcher.IsDisposed);
			Assert.AreEqual(0, watcher.ActiveWatcherCount);
			Assert.IsFalse(watcher.HasActiveWatchers);

			watcher.Dispose();
		}
	}

	private static void QueueChangeForTest(WorkspaceFileWatcher watcher, string filePath, FileChangeKind changeKind)
	{
#pragma warning disable CS0618
		watcher.QueueChangeForTest(filePath, changeKind);
#pragma warning restore CS0618
	}

	private static Task DispatchPendingChangesForTestAsync(WorkspaceFileWatcher watcher)
	{
#pragma warning disable CS0618
		return watcher.DispatchPendingChangesForTestAsync();
#pragma warning restore CS0618
	}

	private static void ReportErrorForTest(WorkspaceFileWatcher watcher, Exception exception)
	{
#pragma warning disable CS0618
		watcher.ReportErrorForTest(exception);
#pragma warning restore CS0618
	}

	private sealed class TemporaryWorkspaceRoot : IDisposable
	{
		public TemporaryWorkspaceRoot(string namePrefix)
		{
			DirectoryPath = Path.Combine(Path.GetTempPath(), namePrefix + Guid.NewGuid().ToString("N"));
			Directory.CreateDirectory(DirectoryPath);
		}

		public string DirectoryPath { get; }

		public void Dispose()
		{
			if (Directory.Exists(DirectoryPath))
				Directory.Delete(DirectoryPath, recursive: true);
		}
	}

	private sealed class NonPumpingSynchronizationContext : SynchronizationContext
	{
		public override void Post(SendOrPostCallback d, object? state)
		{
			// Intentionally never pumps posted continuations.
		}

		public override void Send(SendOrPostCallback d, object? state)
			=> d(state);
	}
}
