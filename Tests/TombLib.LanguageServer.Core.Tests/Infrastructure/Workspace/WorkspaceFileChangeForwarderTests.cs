namespace Nickelony.LanguageServer.Core.Tests;

[TestClass]
public class WorkspaceFileChangeForwarderTests
{
	[TestMethod]
	public async Task DispatchAsync_Success_ForwardsImmediatelyWithoutBuffering()
	{
		bool ensureStartedCalled = false;
		int markTransportUnavailableCallCount = 0;
		IReadOnlyList<WorkspaceFileChange>? forwardedChanges = null;

		var forwarder = new WorkspaceFileChangeForwarder(
			canForwardAccessor: () => true,
			isDisposedAccessor: () => false,
			ensureStartedAsync: _ =>
			{
				ensureStartedCalled = true;
				return Task.FromResult(true);
			},
			markTransportUnavailable: () => markTransportUnavailableCallCount++);

		WorkspaceFileChange[] changes = [new(@"C:\Workspace\Scripts\test.lua", FileChangeKind.Changed)];

		await forwarder.DispatchAsync(changes,
			(items, _) =>
			{
				forwardedChanges = [.. items];
				return Task.CompletedTask;
			},
			CancellationToken.None).ConfigureAwait(false);

		Assert.IsTrue(ensureStartedCalled);
		Assert.IsNotNull(forwardedChanges);
		Assert.AreEqual(1, forwardedChanges.Count);
		Assert.AreEqual(0, markTransportUnavailableCallCount);

		await forwarder.ReplayDeferredAsync((_, _) => throw new AssertFailedException("No deferred changes should remain."), CancellationToken.None)
			.ConfigureAwait(false);
	}

	[TestMethod]
	public async Task DispatchAsync_WhenForwardingNotCurrentlyAllowed_UsesExplicitDropModeWithoutBuffering()
	{
		bool ensureStartedCalled = false;
		bool forwardCalled = false;

		var forwarder = new WorkspaceFileChangeForwarder(
			canForwardAccessor: () => false,
			isDisposedAccessor: () => false,
			ensureStartedAsync: _ =>
			{
				ensureStartedCalled = true;
				return Task.FromResult(true);
			},
			markTransportUnavailable: static () => { },
			bufferChangesWhileForwardingDisabled: false);

		WorkspaceFileChange[] changes = [new(@"C:\Workspace\Scripts\test.lua", FileChangeKind.Changed)];

		await forwarder.DispatchAsync(changes,
			(_, _) =>
			{
				forwardCalled = true;
				return Task.CompletedTask;
			},
			CancellationToken.None).ConfigureAwait(false);

		await forwarder.ReplayDeferredAsync(
			(_, _) => throw new AssertFailedException("Ignored changes should not be retained for replay."),
			CancellationToken.None).ConfigureAwait(false);

		Assert.IsFalse(ensureStartedCalled);
		Assert.IsFalse(forwardCalled);
	}

	[TestMethod]
	public async Task DispatchAsync_WhenForwardingNotCurrentlyAllowed_BuffersChangesByDefault()
	{
		bool canForward = false;
		IReadOnlyList<WorkspaceFileChange>? replayedChanges = null;

		var forwarder = new WorkspaceFileChangeForwarder(
			canForwardAccessor: () => canForward,
			isDisposedAccessor: () => false,
			ensureStartedAsync: _ => Task.FromResult(true),
			markTransportUnavailable: static () => { });

		WorkspaceFileChange[] changes = [new(@"C:\Workspace\Scripts\test.lua", FileChangeKind.Changed)];

		await forwarder.DispatchAsync(changes, (_, _) => Task.CompletedTask, CancellationToken.None).ConfigureAwait(false);
		canForward = true;

		await forwarder.ReplayDeferredAsync(
			(items, _) =>
			{
				replayedChanges = [.. items];
				return Task.CompletedTask;
			},
			CancellationToken.None).ConfigureAwait(false);

		Assert.IsNotNull(replayedChanges);
		Assert.AreEqual(1, replayedChanges.Count);
		Assert.AreEqual(changes[0].Path, replayedChanges[0].Path);
		Assert.AreEqual(changes[0].Kind, replayedChanges[0].Kind);
	}

	[TestMethod]
	public async Task DispatchAsync_WhenStartupFails_BuffersAndReplayDispatchesChanges()
	{
		bool startResult = false;
		IReadOnlyList<WorkspaceFileChange>? replayedChanges = null;

		var forwarder = new WorkspaceFileChangeForwarder(
			canForwardAccessor: () => true,
			isDisposedAccessor: () => false,
			ensureStartedAsync: _ => Task.FromResult(startResult),
			markTransportUnavailable: static () => { });

		WorkspaceFileChange[] changes = [new(@"C:\Workspace\Scripts\test.lua", FileChangeKind.Changed)];

		await forwarder.DispatchAsync(changes, (_, _) => throw new AssertFailedException("Dispatch should be buffered while startup fails."), CancellationToken.None)
			.ConfigureAwait(false);

		startResult = true;

		await forwarder.ReplayDeferredAsync(
			(items, _) =>
			{
				replayedChanges = [.. items];
				return Task.CompletedTask;
			},
			CancellationToken.None).ConfigureAwait(false);

		Assert.IsNotNull(replayedChanges);
		Assert.AreEqual(1, replayedChanges.Count);
		Assert.AreEqual(changes[0].Path, replayedChanges[0].Path);
		Assert.AreEqual(changes[0].Kind, replayedChanges[0].Kind);
	}

	[TestMethod]
	public async Task DispatchAsync_WhenForwardingThrowsIOException_BuffersMarksTransportUnavailableAndReplays()
	{
		int markTransportUnavailableCallCount = 0;
		int logForwardingFailureCallCount = 0;
		WorkspaceFileForwardingFailure? loggedFailure = null;
		IReadOnlyList<WorkspaceFileChange>? replayedChanges = null;

		var forwarder = new WorkspaceFileChangeForwarder(
			canForwardAccessor: () => true,
			isDisposedAccessor: () => false,
			ensureStartedAsync: _ => Task.FromResult(true),
			markTransportUnavailable: () => markTransportUnavailableCallCount++,
			logForwardingFailure: failure =>
			{
				logForwardingFailureCallCount++;
				loggedFailure = failure;
			});

		WorkspaceFileChange[] changes = [new(@"C:\Workspace\Scripts\test.lua", FileChangeKind.Changed)];

		await forwarder.DispatchAsync(changes, (_, _) => throw new IOException("Simulated forwarding failure."), CancellationToken.None)
			.ConfigureAwait(false);

		await forwarder.ReplayDeferredAsync(
			(items, _) =>
			{
				replayedChanges = [.. items];
				return Task.CompletedTask;
			},
			CancellationToken.None).ConfigureAwait(false);

		Assert.AreEqual(1, markTransportUnavailableCallCount);
		Assert.AreEqual(1, logForwardingFailureCallCount);
		Assert.IsNotNull(loggedFailure);
		Assert.IsInstanceOfType(loggedFailure.Value.Exception, typeof(IOException));
		Assert.AreEqual(1, loggedFailure.Value.BatchCount);
		Assert.AreEqual(changes[0].Path, loggedFailure.Value.FirstPath);
		Assert.IsFalse(loggedFailure.Value.WasDropped);
		Assert.IsNotNull(replayedChanges);
		Assert.AreEqual(1, replayedChanges.Count);
	}

	[TestMethod]
	public async Task DispatchAsync_WhenEnsureStartedReplaysDeferredChanges_CompletesCurrentDispatchWithoutDeadlock()
	{
		bool startupSucceeds = false;
		var forwardedBatches = new List<IReadOnlyList<WorkspaceFileChange>>();
		WorkspaceFileChangeForwarder? forwarder = null;

		forwarder = new WorkspaceFileChangeForwarder(
			canForwardAccessor: () => true,
			isDisposedAccessor: () => false,
			ensureStartedAsync: async cancellationToken =>
			{
				if (!startupSucceeds)
					return false;

				await forwarder!.ReplayDeferredAsync(
					(items, _) =>
					{
						forwardedBatches.Add([.. items]);
						return Task.CompletedTask;
					},
					cancellationToken).ConfigureAwait(false);

				return true;
			},
			markTransportUnavailable: static () => { });

		WorkspaceFileChange[] deferredChanges = [new(@"C:\Workspace\Scripts\deferred.lua", FileChangeKind.Changed)];
		WorkspaceFileChange[] currentChanges = [new(@"C:\Workspace\Scripts\current.lua", FileChangeKind.Changed)];

		await forwarder.DispatchAsync(
			deferredChanges,
			(_, _) => throw new AssertFailedException("Deferred changes should be buffered while startup fails."),
			CancellationToken.None).ConfigureAwait(false);

		startupSucceeds = true;

		await forwarder.DispatchAsync(
			currentChanges,
			(items, _) =>
			{
				forwardedBatches.Add([.. items]);
				return Task.CompletedTask;
			},
			CancellationToken.None).WaitAsync(TimeSpan.FromSeconds(1)).ConfigureAwait(false);

		Assert.AreEqual(2, forwardedBatches.Count);
		Assert.AreEqual(deferredChanges[0].Path, forwardedBatches[0][0].Path);
		Assert.AreEqual(deferredChanges[0].Kind, forwardedBatches[0][0].Kind);
		Assert.AreEqual(currentChanges[0].Path, forwardedBatches[1][0].Path);
		Assert.AreEqual(currentChanges[0].Kind, forwardedBatches[1][0].Kind);
	}

	[TestMethod]
	public async Task DispatchAsync_WhenForwardingThrowsObjectDisposedExceptionWhileOwnerAlive_BuffersMarksTransportUnavailableAndReplays()
	{
		int markTransportUnavailableCallCount = 0;
		IReadOnlyList<WorkspaceFileChange>? replayedChanges = null;

		var forwarder = new WorkspaceFileChangeForwarder(
			canForwardAccessor: () => true,
			isDisposedAccessor: () => false,
			ensureStartedAsync: _ => Task.FromResult(true),
			markTransportUnavailable: () => markTransportUnavailableCallCount++);

		WorkspaceFileChange[] changes = [new(@"C:\Workspace\Scripts\test.lua", FileChangeKind.Changed)];

		await forwarder.DispatchAsync(changes,
			(_, _) => throw new ObjectDisposedException("transport"),
			CancellationToken.None).ConfigureAwait(false);

		await forwarder.ReplayDeferredAsync(
			(items, _) =>
			{
				replayedChanges = [.. items];
				return Task.CompletedTask;
			},
			CancellationToken.None).ConfigureAwait(false);

		Assert.AreEqual(1, markTransportUnavailableCallCount);
		Assert.IsNotNull(replayedChanges);
		Assert.AreEqual(1, replayedChanges.Count);
	}

	[TestMethod]
	public async Task DispatchAsync_WhenForwardingThrowsUnexpectedException_LogsAndIntentionallyDropsChangesWithoutReplay()
	{
		int markTransportUnavailableCallCount = 0;
		int logForwardingFailureCallCount = 0;
		WorkspaceFileForwardingFailure? loggedFailure = null;

		var forwarder = new WorkspaceFileChangeForwarder(
			canForwardAccessor: () => true,
			isDisposedAccessor: () => false,
			ensureStartedAsync: _ => Task.FromResult(true),
			markTransportUnavailable: () => markTransportUnavailableCallCount++,
			logForwardingFailure: failure =>
			{
				logForwardingFailureCallCount++;
				loggedFailure = failure;
			});

		WorkspaceFileChange[] changes = [new(@"C:\Workspace\Scripts\test.lua", FileChangeKind.Changed)];

		await forwarder.DispatchAsync(changes,
			(_, _) => throw new InvalidOperationException("Simulated unexpected forwarding failure."),
			CancellationToken.None).ConfigureAwait(false);

		await forwarder.ReplayDeferredAsync(
			(_, _) => throw new AssertFailedException("Unexpected forwarding failures should not be replayed."),
			CancellationToken.None).ConfigureAwait(false);

		Assert.AreEqual(0, markTransportUnavailableCallCount);
		Assert.AreEqual(1, logForwardingFailureCallCount);
		Assert.IsNotNull(loggedFailure);
		Assert.IsInstanceOfType(loggedFailure.Value.Exception, typeof(InvalidOperationException));
		Assert.AreEqual(1, loggedFailure.Value.BatchCount);
		Assert.AreEqual(changes[0].Path, loggedFailure.Value.FirstPath);
		Assert.IsTrue(loggedFailure.Value.WasDropped);
	}

	[TestMethod]
	public async Task DispatchAsync_WhenOwnerAlreadyDisposed_DoesNotBufferObjectDisposedFailure()
	{
		int markTransportUnavailableCallCount = 0;

		var forwarder = new WorkspaceFileChangeForwarder(
			canForwardAccessor: () => true,
			isDisposedAccessor: () => true,
			ensureStartedAsync: _ => Task.FromResult(true),
			markTransportUnavailable: () => markTransportUnavailableCallCount++);

		WorkspaceFileChange[] changes = [new(@"C:\Workspace\Scripts\test.lua", FileChangeKind.Changed)];

		await forwarder.DispatchAsync(changes,
			(_, _) => throw new ObjectDisposedException("transport"),
			CancellationToken.None).ConfigureAwait(false);

		await forwarder.ReplayDeferredAsync(
			(_, _) => throw new AssertFailedException("Disposed owners should not retain deferred changes."),
			CancellationToken.None).ConfigureAwait(false);

		Assert.AreEqual(0, markTransportUnavailableCallCount);
	}

	[TestMethod]
	public async Task ReplayDeferredAsync_WhenCallerCancels_RetainsDeferredChangesForLaterReplay()
	{
		bool startResult = false;
		int markTransportUnavailableCallCount = 0;
		IReadOnlyList<WorkspaceFileChange>? replayedChanges = null;

		var forwarder = new WorkspaceFileChangeForwarder(
			canForwardAccessor: () => true,
			isDisposedAccessor: () => false,
			ensureStartedAsync: _ => Task.FromResult(startResult),
			markTransportUnavailable: () => markTransportUnavailableCallCount++);

		WorkspaceFileChange[] changes = [new(@"C:\Workspace\Scripts\test.lua", FileChangeKind.Changed)];

		await forwarder.DispatchAsync(changes, (_, _) => Task.CompletedTask, CancellationToken.None).ConfigureAwait(false);

		startResult = true;

		using (var cancellationTokenSource = new CancellationTokenSource())
		{
			cancellationTokenSource.Cancel();

			await forwarder.ReplayDeferredAsync(
				(_, token) => Task.FromCanceled(token),
				cancellationTokenSource.Token).ConfigureAwait(false);
		}

		await forwarder.ReplayDeferredAsync(
			(items, _) =>
			{
				replayedChanges = [.. items];
				return Task.CompletedTask;
			},
			CancellationToken.None).ConfigureAwait(false);

		Assert.AreEqual(0, markTransportUnavailableCallCount);
		Assert.IsNotNull(replayedChanges);
		Assert.AreEqual(1, replayedChanges.Count);
	}

	[TestMethod]
	public async Task ReplayDeferredAsync_WhenForwardingNotCurrentlyAllowed_RetainsDeferredChangesForLaterReplay()
	{
		bool canForward = true;
		bool startResult = false;
		IReadOnlyList<WorkspaceFileChange>? replayedChanges = null;

		var forwarder = new WorkspaceFileChangeForwarder(
			canForwardAccessor: () => canForward,
			isDisposedAccessor: () => false,
			ensureStartedAsync: _ => Task.FromResult(startResult),
			markTransportUnavailable: static () => { });

		WorkspaceFileChange[] changes = [new(@"C:\Workspace\Scripts\test.lua", FileChangeKind.Changed)];

		await forwarder.DispatchAsync(changes, (_, _) => Task.CompletedTask, CancellationToken.None).ConfigureAwait(false);

		canForward = false;
		startResult = true;

		await forwarder.ReplayDeferredAsync(
			(_, _) => throw new AssertFailedException("Deferred changes should remain buffered while forwarding is not allowed."),
			CancellationToken.None).ConfigureAwait(false);

		canForward = true;

		await forwarder.ReplayDeferredAsync(
			(items, _) =>
			{
				replayedChanges = [.. items];
				return Task.CompletedTask;
			},
			CancellationToken.None).ConfigureAwait(false);

		Assert.IsNotNull(replayedChanges);
		Assert.AreEqual(1, replayedChanges.Count);
		Assert.AreEqual(changes[0].Path, replayedChanges[0].Path);
		Assert.AreEqual(changes[0].Kind, replayedChanges[0].Kind);
	}

	[TestMethod]
	public async Task ReplayDeferredAsync_WhenReplayIsInFlight_DoesNotLetNewDispatchPassIt()
	{
		bool startResult = false;
		var forwardedBatches = new List<IReadOnlyList<WorkspaceFileChange>>();
		var replayEntered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		var allowReplayToFinish = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		var dispatchObserved = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

		var forwarder = new WorkspaceFileChangeForwarder(
			canForwardAccessor: () => true,
			isDisposedAccessor: () => false,
			ensureStartedAsync: _ => Task.FromResult(startResult),
			markTransportUnavailable: static () => { });

		WorkspaceFileChange[] deferredChanges = [new(@"C:\Workspace\Scripts\deferred.lua", FileChangeKind.Changed)];
		WorkspaceFileChange[] liveChanges = [new(@"C:\Workspace\Scripts\live.lua", FileChangeKind.Created)];

		await forwarder.DispatchAsync(
			deferredChanges,
			(_, _) => throw new AssertFailedException("Deferred changes should be buffered while startup is unavailable."),
			CancellationToken.None).ConfigureAwait(false);

		startResult = true;

		Task replayTask = forwarder.ReplayDeferredAsync(
			async (changes, _) =>
			{
				forwardedBatches.Add([.. changes]);
				replayEntered.TrySetResult(true);
				await allowReplayToFinish.Task.ConfigureAwait(false);
			},
			CancellationToken.None);

		await replayEntered.Task.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);

		Task dispatchTask = forwarder.DispatchAsync(
			liveChanges,
			(items, _) =>
			{
				forwardedBatches.Add([.. items]);
				dispatchObserved.TrySetResult(true);
				return Task.CompletedTask;
			},
			CancellationToken.None);

		Task completedTask = await Task.WhenAny(dispatchObserved.Task, Task.Delay(TimeSpan.FromMilliseconds(150))).ConfigureAwait(false);

		Assert.AreNotSame(dispatchObserved.Task, completedTask,
			"A live dispatch should not overtake an older deferred replay while the replay is still in flight.");

		allowReplayToFinish.TrySetResult(true);

		await replayTask.ConfigureAwait(false);
		await dispatchTask.ConfigureAwait(false);

		Assert.AreEqual(2, forwardedBatches.Count);
		Assert.AreEqual(deferredChanges[0].Path, forwardedBatches[0][0].Path);
		Assert.AreEqual(liveChanges[0].Path, forwardedBatches[1][0].Path);
	}

	[TestMethod]
	public async Task ReplayDeferredAsync_ReplaysBufferedPathsInFirstPendingOccurrenceOrder()
	{
		bool startResult = false;
		IReadOnlyList<WorkspaceFileChange>? replayedChanges = null;

		var forwarder = new WorkspaceFileChangeForwarder(
			canForwardAccessor: () => true,
			isDisposedAccessor: () => false,
			ensureStartedAsync: _ => Task.FromResult(startResult),
			markTransportUnavailable: static () => { });

		await forwarder.DispatchAsync(
			[
				new WorkspaceFileChange(@"C:\Workspace\Scripts\first.lua", FileChangeKind.Created),
				new WorkspaceFileChange(@"C:\Workspace\Scripts\second.lua", FileChangeKind.Changed)
			],
			(_, _) => throw new AssertFailedException("Deferred changes should be buffered while startup is unavailable."),
			CancellationToken.None).ConfigureAwait(false);

		await forwarder.DispatchAsync(
			[new WorkspaceFileChange(@"C:\Workspace\Scripts\first.lua", FileChangeKind.Changed)],
			(_, _) => throw new AssertFailedException("Deferred changes should still be buffered while startup is unavailable."),
			CancellationToken.None).ConfigureAwait(false);

		startResult = true;

		await forwarder.ReplayDeferredAsync(
			(items, _) =>
			{
				replayedChanges = [.. items];
				return Task.CompletedTask;
			},
			CancellationToken.None).ConfigureAwait(false);

		Assert.IsNotNull(replayedChanges);

		CollectionAssert.AreEqual(
			new[]
			{
				@"C:\Workspace\Scripts\first.lua",
				@"C:\Workspace\Scripts\second.lua"
			},
			replayedChanges.Select(change => change.Path).ToArray());

		CollectionAssert.AreEqual(
			new[]
			{
				FileChangeKind.Created,
				FileChangeKind.Changed
			},
			replayedChanges.Select(change => change.Kind).ToArray());
	}

	[TestMethod]
	public async Task Dispose_AfterDisposal_IgnoresDispatchAndReplay()
	{
		bool ensureStartedCalled = false;
		bool forwardCalled = false;

		var forwarder = new WorkspaceFileChangeForwarder(
			canForwardAccessor: () => true,
			isDisposedAccessor: () => false,
			ensureStartedAsync: _ =>
			{
				ensureStartedCalled = true;
				return Task.FromResult(true);
			},
			markTransportUnavailable: static () => { });

		forwarder.Dispose();

		WorkspaceFileChange[] changes = [new(@"C:\Workspace\Scripts\test.lua", FileChangeKind.Changed)];

		await forwarder.DispatchAsync(
			changes,
			(_, _) =>
			{
				forwardCalled = true;
				return Task.CompletedTask;
			},
			CancellationToken.None).ConfigureAwait(false);

		await forwarder.ReplayDeferredAsync(
			(_, _) =>
			{
				forwardCalled = true;
				return Task.CompletedTask;
			},
			CancellationToken.None).ConfigureAwait(false);

		Assert.IsFalse(ensureStartedCalled);
		Assert.IsFalse(forwardCalled);
	}

	[TestMethod]
	public async Task Dispose_WhileReplayIsInFlight_AllowsReplayToFinishAndBlocksNewDispatch()
	{
		bool startResult = false;
		var forwardedBatches = new List<IReadOnlyList<WorkspaceFileChange>>();
		var replayEntered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		var allowReplayToFinish = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

		var forwarder = new WorkspaceFileChangeForwarder(
			canForwardAccessor: () => true,
			isDisposedAccessor: () => false,
			ensureStartedAsync: _ => Task.FromResult(startResult),
			markTransportUnavailable: static () => { });

		WorkspaceFileChange[] deferredChanges = [new(@"C:\Workspace\Scripts\deferred.lua", FileChangeKind.Changed)];
		WorkspaceFileChange[] liveChanges = [new(@"C:\Workspace\Scripts\live.lua", FileChangeKind.Created)];

		await forwarder.DispatchAsync(
			deferredChanges,
			(_, _) => throw new AssertFailedException("Deferred changes should be buffered while startup is unavailable."),
			CancellationToken.None).ConfigureAwait(false);

		startResult = true;

		Task replayTask = forwarder.ReplayDeferredAsync(
			async (changes, _) =>
			{
				forwardedBatches.Add([.. changes]);
				replayEntered.TrySetResult(true);
				await allowReplayToFinish.Task.ConfigureAwait(false);
			},
			CancellationToken.None);

		await replayEntered.Task.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
		forwarder.Dispose();

		await forwarder.DispatchAsync(
			liveChanges,
			(items, _) =>
			{
				forwardedBatches.Add([.. items]);
				return Task.CompletedTask;
			},
			CancellationToken.None).ConfigureAwait(false);

		allowReplayToFinish.TrySetResult(true);

		await replayTask.ConfigureAwait(false);

		Assert.AreEqual(1, forwardedBatches.Count);
		Assert.AreEqual(deferredChanges[0].Path, forwardedBatches[0][0].Path);
	}

	[TestMethod]
	public async Task Dispose_WhileDispatchWaitsForGate_DoesNotStartForwardingAfterGateOpens()
	{
		bool ensureStartedCalled = false;
		bool forwardCalled = false;
		var firstForwardEntered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		var allowFirstForwardToFinish = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

		var forwarder = new WorkspaceFileChangeForwarder(
			canForwardAccessor: () => true,
			isDisposedAccessor: () => false,
			ensureStartedAsync: _ =>
			{
				ensureStartedCalled = true;
				return Task.FromResult(true);
			},
			markTransportUnavailable: static () => { });

		Task firstDispatchTask = forwarder.DispatchAsync(
			[new WorkspaceFileChange(@"C:\Workspace\Scripts\first.lua", FileChangeKind.Changed)],
			async (_, _) =>
			{
				firstForwardEntered.TrySetResult(true);
				await allowFirstForwardToFinish.Task.ConfigureAwait(false);
			},
			CancellationToken.None);

		await firstForwardEntered.Task.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);

		Task blockedDispatchTask = forwarder.DispatchAsync(
			[new WorkspaceFileChange(@"C:\Workspace\Scripts\blocked.lua", FileChangeKind.Created)],
			(_, _) =>
			{
				forwardCalled = true;
				return Task.CompletedTask;
			},
			CancellationToken.None);

		await Task.Delay(100).ConfigureAwait(false);
		forwarder.Dispose();
		allowFirstForwardToFinish.TrySetResult(true);

		await firstDispatchTask.ConfigureAwait(false);
		await blockedDispatchTask.ConfigureAwait(false);

		Assert.IsTrue(ensureStartedCalled);
		Assert.IsFalse(forwardCalled);
	}
}
