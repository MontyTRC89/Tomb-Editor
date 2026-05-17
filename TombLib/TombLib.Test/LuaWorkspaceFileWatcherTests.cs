using TombIDE.ScriptingStudio.Services.LuaIntellisense;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace TombLib.Test;

[TestClass]
public class WorkspaceFileWatcherTests
{
	[TestMethod]
	public async Task DispatchPendingChangesForTestAsync_PreservesDeleteThenCreatePairForSamePath()
	{
		string workspaceRoot = Path.Combine(Path.GetTempPath(), "LuaWatcherCoalesce_" + Guid.NewGuid().ToString("N"));
		WorkspaceWatchSpecification[] watchSpecifications = [new("*.lua", IncludeSubdirectories: true)];
		FileChangeBatch? dispatchedBatch = null;

		try
		{
			Directory.CreateDirectory(workspaceRoot);

			using var watcher = new WorkspaceFileWatcher(workspaceRoot, (batch, _) =>
			{
				dispatchedBatch = batch;
				return Task.CompletedTask;
			}, watchSpecifications);

			string filePath = Path.Combine(workspaceRoot, "test.lua");

			#pragma warning disable CS0618
			watcher.QueueChangeForTest(filePath, FileChangeKind.Deleted);
			watcher.QueueChangeForTest(filePath, FileChangeKind.Created);
			await watcher.DispatchPendingChangesForTestAsync().ConfigureAwait(false);
			#pragma warning restore CS0618

			Assert.IsNotNull(dispatchedBatch);
			Assert.AreEqual(2, dispatchedBatch.Count);
			Assert.AreEqual(filePath, dispatchedBatch.Entries[0].Path);
			Assert.AreEqual(FileChangeKind.Deleted, dispatchedBatch.Entries[0].Kind);
			Assert.AreEqual(filePath, dispatchedBatch.Entries[1].Path);
			Assert.AreEqual(FileChangeKind.Created, dispatchedBatch.Entries[1].Kind);
		}
		finally
		{
			if (Directory.Exists(workspaceRoot))
				Directory.Delete(workspaceRoot, recursive: true);
		}
	}

	[TestMethod]
	public async Task DispatchPendingChangesForTestAsync_NormalizesEquivalentPathFormsBeforeCoalescing()
	{
		string workspaceRoot = Path.Combine(Path.GetTempPath(), "LuaWatcherNormalize_" + Guid.NewGuid().ToString("N"));
		WorkspaceWatchSpecification[] watchSpecifications = [new("*.lua", IncludeSubdirectories: true)];
		FileChangeBatch? dispatchedBatch = null;

		try
		{
			Directory.CreateDirectory(workspaceRoot);
			Directory.CreateDirectory(Path.Combine(workspaceRoot, "Scripts"));

			using var watcher = new WorkspaceFileWatcher(workspaceRoot, (batch, _) =>
			{
				dispatchedBatch = batch;
				return Task.CompletedTask;
			}, watchSpecifications);

			string normalizedPath = Path.Combine(workspaceRoot, "Scripts", "test.lua");
			string alternatePath = normalizedPath.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

			#pragma warning disable CS0618
			watcher.QueueChangeForTest(normalizedPath, FileChangeKind.Changed);
			watcher.QueueChangeForTest(alternatePath, FileChangeKind.Changed);
			await watcher.DispatchPendingChangesForTestAsync().ConfigureAwait(false);
			#pragma warning restore CS0618

			Assert.IsNotNull(dispatchedBatch);
			Assert.AreEqual(1, dispatchedBatch.Count);
			Assert.AreEqual(LanguageServerPathHelper.NormalizeLocalPath(normalizedPath), dispatchedBatch.Entries[0].Path);
			Assert.AreEqual(FileChangeKind.Changed, dispatchedBatch.Entries[0].Kind);
		}
		finally
		{
			if (Directory.Exists(workspaceRoot))
				Directory.Delete(workspaceRoot, recursive: true);
		}
	}

	[TestMethod]
	public async Task DispatchPendingChangesForTestAsync_WhenDeleteCreateRetryIsNeeded_PreservesBothEntries()
	{
		string workspaceRoot = Path.Combine(Path.GetTempPath(), "LuaWatcherRetryPair_" + Guid.NewGuid().ToString("N"));
		WorkspaceWatchSpecification[] watchSpecifications = [new("*.lua", IncludeSubdirectories: true)];
		FileChangeBatch? dispatchedBatch = null;
		int dispatchAttemptCount = 0;

		try
		{
			Directory.CreateDirectory(workspaceRoot);

			using var watcher = new WorkspaceFileWatcher(workspaceRoot, (batch, _) =>
			{
				dispatchAttemptCount++;

				if (dispatchAttemptCount == 1)
					throw new IOException("Simulated dispatch failure.");

				dispatchedBatch = batch;
				return Task.CompletedTask;
			}, watchSpecifications);

			string filePath = Path.Combine(workspaceRoot, "test.lua");

			#pragma warning disable CS0618
			watcher.QueueChangeForTest(filePath, FileChangeKind.Deleted);
			watcher.QueueChangeForTest(filePath, FileChangeKind.Created);
			await watcher.DispatchPendingChangesForTestAsync().ConfigureAwait(false);
			await watcher.DispatchPendingChangesForTestAsync().ConfigureAwait(false);
			#pragma warning restore CS0618

			Assert.AreEqual(2, dispatchAttemptCount);
			Assert.IsNotNull(dispatchedBatch);
			Assert.AreEqual(2, dispatchedBatch.Count);
			Assert.AreEqual(FileChangeKind.Deleted, dispatchedBatch.Entries[0].Kind);
			Assert.AreEqual(FileChangeKind.Created, dispatchedBatch.Entries[1].Kind);
		}
		finally
		{
			if (Directory.Exists(workspaceRoot))
				Directory.Delete(workspaceRoot, recursive: true);
		}
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
		string workspaceRoot = Path.Combine(Path.GetTempPath(), "WorkspaceWatcherFactoryThrow_" + Guid.NewGuid().ToString("N"));

		try
		{
			Directory.CreateDirectory(workspaceRoot);

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
		finally
		{
			if (Directory.Exists(workspaceRoot))
				Directory.Delete(workspaceRoot, recursive: true);
		}
	}

	[TestMethod]
	public void Start_AfterStartupFailureOnSameInstance_ReturnsDisposed()
	{
		string workspaceRoot = Path.Combine(Path.GetTempPath(), "WorkspaceWatcherRetryAfterFailure_" + Guid.NewGuid().ToString("N"));

		try
		{
			Directory.CreateDirectory(workspaceRoot);

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
		finally
		{
			if (Directory.Exists(workspaceRoot))
				Directory.Delete(workspaceRoot, recursive: true);
		}
	}

	[TestMethod]
	public async Task DispatchPendingChangesForTestAsync_WhenDispatchFails_RetainsBatchForRetry()
	{
		string workspaceRoot = Path.Combine(Path.GetTempPath(), "LuaWatcherRetry_" + Guid.NewGuid().ToString("N"));
		WorkspaceWatchSpecification[] watchSpecifications = [new("*.lua", IncludeSubdirectories: true)];
		FileChangeBatch? dispatchedBatch = null;
		int dispatchAttemptCount = 0;
		using var logScope = new NLogMemoryScope(LogLevel.Debug);

		try
		{
			Directory.CreateDirectory(workspaceRoot);

			using var watcher = new WorkspaceFileWatcher(workspaceRoot, (batch, _) =>
			{
				dispatchAttemptCount++;

				if (dispatchAttemptCount == 1)
					throw new IOException("Simulated dispatch failure.");

				dispatchedBatch = batch;
				return Task.CompletedTask;
			}, watchSpecifications);

			string filePath = Path.Combine(workspaceRoot, "test.lua");

			#pragma warning disable CS0618
			watcher.QueueChangeForTest(filePath, FileChangeKind.Changed);
			await watcher.DispatchPendingChangesForTestAsync().ConfigureAwait(false);
			await watcher.DispatchPendingChangesForTestAsync().ConfigureAwait(false);
			#pragma warning restore CS0618

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
		finally
		{
			if (Directory.Exists(workspaceRoot))
				Directory.Delete(workspaceRoot, recursive: true);
		}
	}

	[TestMethod]
	public async Task DispatchPendingChangesForTestAsync_WhenDispatchKeepsFailing_EscalatesLogLevelAndBackoff()
	{
		string workspaceRoot = Path.Combine(Path.GetTempPath(), "LuaWatcherBackoff_" + Guid.NewGuid().ToString("N"));
		WorkspaceWatchSpecification[] watchSpecifications = [new("*.lua", IncludeSubdirectories: true)];
		using var logScope = new NLogMemoryScope(LogLevel.Debug);

		try
		{
			Directory.CreateDirectory(workspaceRoot);

			using var watcher = new WorkspaceFileWatcher(workspaceRoot, (_, _) => throw new IOException("Persistent dispatch failure."), watchSpecifications);

			string filePath = Path.Combine(workspaceRoot, "test.lua");

			#pragma warning disable CS0618
			watcher.QueueChangeForTest(filePath, FileChangeKind.Changed);
			await watcher.DispatchPendingChangesForTestAsync().ConfigureAwait(false);
			await watcher.DispatchPendingChangesForTestAsync().ConfigureAwait(false);
			await watcher.DispatchPendingChangesForTestAsync().ConfigureAwait(false);
			#pragma warning restore CS0618

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
		finally
		{
			if (Directory.Exists(workspaceRoot))
				Directory.Delete(workspaceRoot, recursive: true);
		}
	}

	[TestMethod]
	public async Task Dispose_DuringActiveDispatch_DoesNotFaultDispatch()
	{
		string workspaceRoot = Path.Combine(Path.GetTempPath(), "LuaWatcherDispose_" + Guid.NewGuid().ToString("N"));
		WorkspaceWatchSpecification[] watchSpecifications = [new("*.lua", IncludeSubdirectories: true)];
		var dispatchStarted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		var allowDispatchToFinish = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

		try
		{
			Directory.CreateDirectory(workspaceRoot);

			using var watcher = new WorkspaceFileWatcher(workspaceRoot, async (_, _) =>
			{
				dispatchStarted.TrySetResult(true);
				await allowDispatchToFinish.Task.ConfigureAwait(false);
			}, watchSpecifications);

			#pragma warning disable CS0618
			watcher.QueueChangeForTest(Path.Combine(workspaceRoot, "test.lua"), FileChangeKind.Changed);
			Task dispatchTask = watcher.DispatchPendingChangesForTestAsync();
			#pragma warning restore CS0618

			Task completedTask = await Task.WhenAny(dispatchStarted.Task, Task.Delay(TimeSpan.FromSeconds(1))).ConfigureAwait(false);
			Assert.AreSame(dispatchStarted.Task, completedTask);

			Task disposeTask = Task.Run(watcher.Dispose);
			Assert.IsFalse(disposeTask.IsCompleted);

			allowDispatchToFinish.TrySetResult(true);

			await Task.WhenAll(dispatchTask, disposeTask).ConfigureAwait(false);
		}
		finally
		{
			if (Directory.Exists(workspaceRoot))
				Directory.Delete(workspaceRoot, recursive: true);
		}
	}

	[TestMethod]
	public async Task DisposeWithoutFinalFlush_DuringActiveDispatch_DropsRequeuedBatch()
	{
		string workspaceRoot = Path.Combine(Path.GetTempPath(), "LuaWatcherDisposeNoFlush_" + Guid.NewGuid().ToString("N"));
		WorkspaceWatchSpecification[] watchSpecifications = [new("*.lua", IncludeSubdirectories: true)];
		var dispatchStarted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		var allowFirstDispatchToFinish = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		FileChangeBatch? dispatchedBatch = null;
		int dispatchAttemptCount = 0;

		try
		{
			Directory.CreateDirectory(workspaceRoot);

			using var watcher = new WorkspaceFileWatcher(workspaceRoot, async (batch, _) =>
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

			#pragma warning disable CS0618
			watcher.QueueChangeForTest(filePath, FileChangeKind.Changed);
			Task dispatchTask = watcher.DispatchPendingChangesForTestAsync();
			#pragma warning restore CS0618

			await dispatchStarted.Task.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);

			Task disposeTask = Task.Run(watcher.DisposeWithoutFinalFlush);
			Assert.IsFalse(disposeTask.IsCompleted);

			allowFirstDispatchToFinish.TrySetResult(true);

			await Task.WhenAll(dispatchTask, disposeTask).ConfigureAwait(false);

			Assert.AreEqual(1, dispatchAttemptCount);
			Assert.IsNull(dispatchedBatch);
		}
		finally
		{
			if (Directory.Exists(workspaceRoot))
				Directory.Delete(workspaceRoot, recursive: true);
		}
	}

	[TestMethod]
	public async Task DisposeAsync_DuringActiveDispatch_PreservesRequeuedBatchForFinalFlush()
	{
		string workspaceRoot = Path.Combine(Path.GetTempPath(), "LuaWatcherDisposeRetry_" + Guid.NewGuid().ToString("N"));
		WorkspaceWatchSpecification[] watchSpecifications = [new("*.lua", IncludeSubdirectories: true)];
		var dispatchStarted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		var allowFirstDispatchToFinish = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		FileChangeBatch? dispatchedBatch = null;
		int dispatchAttemptCount = 0;

		try
		{
			Directory.CreateDirectory(workspaceRoot);

			using var watcher = new WorkspaceFileWatcher(workspaceRoot, async (batch, _) =>
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

			#pragma warning disable CS0618
			watcher.QueueChangeForTest(filePath, FileChangeKind.Changed);
			Task dispatchTask = watcher.DispatchPendingChangesForTestAsync();
			#pragma warning restore CS0618

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
		finally
		{
			if (Directory.Exists(workspaceRoot))
				Directory.Delete(workspaceRoot, recursive: true);
		}
	}

	[TestMethod]
	public void Dispose_WhenPendingChangesExistAndNoDispatchIsActive_DropsBufferedBatch()
	{
		string workspaceRoot = Path.Combine(Path.GetTempPath(), "LuaWatcherDisposeFlush_" + Guid.NewGuid().ToString("N"));
		WorkspaceWatchSpecification[] watchSpecifications = [new("*.lua", IncludeSubdirectories: true)];
		FileChangeBatch? dispatchedBatch = null;

		try
		{
			Directory.CreateDirectory(workspaceRoot);

			using var watcher = new WorkspaceFileWatcher(workspaceRoot, (batch, _) =>
			{
				dispatchedBatch = batch;
				return Task.CompletedTask;
			}, watchSpecifications);

			#pragma warning disable CS0618
			watcher.QueueChangeForTest(Path.Combine(workspaceRoot, "test.lua"), FileChangeKind.Changed);
			#pragma warning restore CS0618

			watcher.Dispose();

			Assert.IsNull(dispatchedBatch);
		}
		finally
		{
			if (Directory.Exists(workspaceRoot))
				Directory.Delete(workspaceRoot, recursive: true);
		}
	}

	[TestMethod]
	public void Dispose_WhenBufferedChangesExist_DoesNotDeadlockCallerContext()
	{
		string workspaceRoot = Path.Combine(Path.GetTempPath(), "LuaWatcherDisposeContext_" + Guid.NewGuid().ToString("N"));
		WorkspaceWatchSpecification[] watchSpecifications = [new("*.lua", IncludeSubdirectories: true)];
		var disposeCompleted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		Exception? failure = null;

		try
		{
			Directory.CreateDirectory(workspaceRoot);

			var thread = new Thread(() =>
			{
				SynchronizationContext.SetSynchronizationContext(new NonPumpingSynchronizationContext());

				try
				{
					using var watcher = new WorkspaceFileWatcher(workspaceRoot, async (_, _) =>
					{
						await Task.Yield();
					}, watchSpecifications);

					#pragma warning disable CS0618
					watcher.QueueChangeForTest(Path.Combine(workspaceRoot, "test.lua"), FileChangeKind.Changed);
					#pragma warning restore CS0618

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
		finally
		{
			if (Directory.Exists(workspaceRoot))
				Directory.Delete(workspaceRoot, recursive: true);
		}
	}

	[TestMethod]
	public void Start_UsesConfiguredWatchSpecifications()
	{
		string workspaceRoot = Path.Combine(Path.GetTempPath(), "WorkspaceWatcherSpecs_" + Guid.NewGuid().ToString("N"));

		try
		{
			Directory.CreateDirectory(workspaceRoot);

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
		finally
		{
			if (Directory.Exists(workspaceRoot))
				Directory.Delete(workspaceRoot, recursive: true);
		}
	}

	[TestMethod]
	public async Task ReportErrorForTest_ConcurrentWithDispose_LeavesNoActiveWatchers()
	{
		string workspaceRoot = Path.Combine(Path.GetTempPath(), "WorkspaceWatcherErrorDispose_" + Guid.NewGuid().ToString("N"));

		try
		{
			Directory.CreateDirectory(workspaceRoot);

			for (int i = 0; i < 50; i++)
			{
				using var watcher = new WorkspaceFileWatcher(
					workspaceRoot,
					(_, _) => Task.CompletedTask,
					[new WorkspaceWatchSpecification("*.lua", IncludeSubdirectories: true)]);

				Assert.IsTrue(watcher.Start());

				#pragma warning disable CS0618
				Task errorTask = Task.Run(() => watcher.ReportErrorForTest(new IOException("Simulated watcher failure.")));
				Task disposeTask = Task.Run(watcher.Dispose);
				#pragma warning restore CS0618

				await Task.WhenAll(errorTask, disposeTask).ConfigureAwait(false);

				Assert.IsTrue(watcher.IsDisposed);
				Assert.AreEqual(0, watcher.ActiveWatcherCount);
				Assert.IsFalse(watcher.HasActiveWatchers);
			}
		}
		finally
		{
			if (Directory.Exists(workspaceRoot))
				Directory.Delete(workspaceRoot, recursive: true);
		}
	}

	[TestMethod]
	public void ReportErrorForTest_WhenFailureHandlerThrows_LogsWarningAndStopsWatching()
	{
		string workspaceRoot = Path.Combine(Path.GetTempPath(), "WorkspaceWatcherFailureCallback_" + Guid.NewGuid().ToString("N"));
		using var logScope = new NLogMemoryScope(LogLevel.Warn);

		try
		{
			Directory.CreateDirectory(workspaceRoot);

			using var watcher = new WorkspaceFileWatcher(
				workspaceRoot,
				(_, _) => Task.CompletedTask,
				[new WorkspaceWatchSpecification("*.lua", IncludeSubdirectories: true)],
				(_, _) => throw new InvalidOperationException("Simulated watcher failure callback exception."));

			Assert.IsTrue(watcher.Start());

			#pragma warning disable CS0618
			watcher.ReportErrorForTest(new IOException("Simulated watcher failure."));
			#pragma warning restore CS0618

			Assert.IsFalse(watcher.HasActiveWatchers);
			Assert.AreEqual(0, watcher.ActiveWatcherCount);
			Assert.IsTrue(logScope.Logs.Any(log => log.Contains("Workspace watcher failure handler threw.", StringComparison.OrdinalIgnoreCase)
				&& log.Contains("Simulated watcher failure callback exception.", StringComparison.Ordinal)),
				string.Join(Environment.NewLine, logScope.Logs));
		}
		finally
		{
			if (Directory.Exists(workspaceRoot))
				Directory.Delete(workspaceRoot, recursive: true);
		}
	}

	[TestMethod]
	public void Start_AfterFailure_ResetsFailureReportingForNextFailureSequence()
	{
		string workspaceRoot = Path.Combine(Path.GetTempPath(), "WorkspaceWatcherRestart_" + Guid.NewGuid().ToString("N"));
		int failureCount = 0;

		try
		{
			Directory.CreateDirectory(workspaceRoot);

			using var watcher = new WorkspaceFileWatcher(
				workspaceRoot,
				(_, _) => Task.CompletedTask,
				[new WorkspaceWatchSpecification("*.lua", IncludeSubdirectories: true)],
				(_, _) => failureCount++);

			Assert.IsTrue(watcher.Start());

			#pragma warning disable CS0618
			watcher.ReportErrorForTest(new IOException("Simulated watcher failure 1."));
			#pragma warning restore CS0618

			Assert.AreEqual(1, failureCount);
			Assert.IsFalse(watcher.HasActiveWatchers);

			Assert.IsTrue(watcher.Start());

			#pragma warning disable CS0618
			watcher.ReportErrorForTest(new IOException("Simulated watcher failure 2."));
			#pragma warning restore CS0618

			Assert.AreEqual(2, failureCount);
			Assert.IsFalse(watcher.HasActiveWatchers);
		}
		finally
		{
			if (Directory.Exists(workspaceRoot))
				Directory.Delete(workspaceRoot, recursive: true);
		}
	}

	[TestMethod]
	public async Task Start_ConcurrentWithDispose_DoesNotLeaveOwnedWatchersBehind()
	{
		string workspaceRoot = Path.Combine(Path.GetTempPath(), "WorkspaceWatcherStartDispose_" + Guid.NewGuid().ToString("N"));

		try
		{
			Directory.CreateDirectory(workspaceRoot);

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
		finally
		{
			if (Directory.Exists(workspaceRoot))
				Directory.Delete(workspaceRoot, recursive: true);
		}
	}

	private sealed class NLogMemoryScope : IDisposable
	{
		private readonly LoggingConfiguration? _previousConfiguration;

		public NLogMemoryScope(LogLevel minLevel)
		{
			_previousConfiguration = LogManager.Configuration;

			var target = new MemoryTarget("WorkspaceWatcherTests")
			{
				Layout = "${level}|${message}|${exception:format=Message}"
			};

			var configuration = new LoggingConfiguration();
			configuration.AddTarget(target);
			configuration.AddRule(minLevel, LogLevel.Fatal, target);

			LogManager.Configuration = configuration;
			LogManager.ReconfigExistingLoggers();

			Target = target;
		}

		public MemoryTarget Target { get; }

		public IList<string> Logs => Target.Logs;

		public void Dispose()
		{
			LogManager.Configuration = _previousConfiguration;
			LogManager.ReconfigExistingLoggers();
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