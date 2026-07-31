using System.Diagnostics;

namespace Nickelony.LanguageServer.Core;

public sealed partial class LanguageServerClient
{
	private async Task DisposeFailedSessionAsync(LanguageServerTransportSession session, string reason)
	{
		try
		{
			await DisposeSessionAsync(session).ConfigureAwait(false);
		}
		catch (Exception exception)
		{
			_logger.LogDebug(exception,
				"Failed to dispose detached language server transport generation {Generation} after {Reason}.",
				session.Generation,
				reason);
		}
	}

	private async Task WaitForQueuedFailedSessionDisposalsAsync(Stopwatch disposeStopwatch)
	{
		while (true)
		{
			Task pendingDisposal;

			lock (_failedSessionDisposalSyncRoot)
				pendingDisposal = _queuedFailedSessionDisposal;

			await WaitWithDisposeBudgetAsync(
				pendingDisposal,
				disposeStopwatch,
				"Detached language server session cleanup did not complete within {TimeoutMs} ms during disposal.",
				"Detached language server session cleanup raised exceptions.").ConfigureAwait(false);

			lock (_failedSessionDisposalSyncRoot)
			{
				if (ReferenceEquals(pendingDisposal, _queuedFailedSessionDisposal))
					return;
			}
		}
	}

	/// <summary>
	/// Disposes one transport session and all of its associated resources.
	/// </summary>
	/// <param name="session">The session to dispose.</param>
	private async Task DisposeSessionAsync(LanguageServerTransportSession session)
	{
		Task? rpcCompletionTask = session.RpcCompletionTask;
		Task? stderrLoopTask = session.StderrLoopTask;

		try
		{
			if (session.Process is not null && session.ProcessExitedHandler is not null)
				session.Process.Exited -= session.ProcessExitedHandler;
		}
		catch
		{
			// Ignore event detach failures.
		}

		try
		{
			if (session.Process is not null && !session.Process.HasExited)
			{
				_logger.LogInformation("Attempting graceful shutdown for language server transport generation {Generation} in workspace '{Workspace}'.",
					session.Generation,
					_workspaceRootDirectoryPath);

				await TrySendShutdownAsync(session).ConfigureAwait(false);
				await TrySendExitNotificationAsync(session).ConfigureAwait(false);

				if (!session.Process.HasExited)
				{
					_logger.LogWarning("Language server transport generation {Generation} in workspace '{Workspace}' did not exit after graceful shutdown; forcing process termination.",
						session.Generation,
						_workspaceRootDirectoryPath);

					LogRecentStandardErrorContext(session, "Forced process termination");

					session.Process.Kill(true);
				}
			}
		}
		catch (Exception exception)
		{
			_logger.LogWarning(exception, "Disposing language server transport generation {Generation} raised exceptions while stopping the server process.", session.Generation);
		}
		finally
		{
			try
			{
				session.JsonRpc?.Dispose();
			}
			catch
			{
				// Ignore JSON-RPC disposal failures.
			}

			try
			{
				if (session.MessageHandler is not null)
					await session.MessageHandler.DisposeAsync().ConfigureAwait(false);
			}
			catch
			{
				// Ignore message-handler disposal failures.
			}

			CleanupSessionResources(session);
		}

		await WaitForBackgroundLoopsAsync(rpcCompletionTask, stderrLoopTask).ConfigureAwait(false);
	}

	/// <summary>
	/// Disposes startup resources that failed before the session became active.
	/// </summary>
	/// <param name="session">The startup session, when transport construction completed.</param>
	/// <param name="process">The spawned process, when startup reached process launch.</param>
	/// <param name="isExpectedCancellation">Whether startup is being cleaned up due to expected cancellation rather than a startup failure.</param>
	private async Task DisposeStartupSessionResourcesAsync(LanguageServerTransportSession? session, Process? process, bool isExpectedCancellation)
	{
		if (session is not null)
		{
			await DisposeSessionAsync(session).ConfigureAwait(false);
			return;
		}

		if (process is null)
			return;

		try
		{
			if (!process.HasExited)
			{
				if (isExpectedCancellation)
					_logger.LogDebug("Startup cleanup is terminating the language server process after cancellation before session activation completed.");
				else
					_logger.LogWarning("Startup cleanup is forcing language server process termination before session activation completed.");

				process.Kill(true);
			}
		}
		catch (Exception exception)
		{
			if (isExpectedCancellation)
				_logger.LogDebug(exception, "Startup cleanup failed while terminating the language server process after cancellation before session activation completed.");
			else
				_logger.LogWarning(exception, "Startup cleanup failed while terminating the language server process after startup did not complete.");
		}
		finally
		{
			try
			{
				process.Dispose();
			}
			catch
			{
				// Ignore startup cleanup failures.
			}
		}
	}

	/// <summary>
	/// Releases the unmanaged and managed resources owned by a transport session.
	/// </summary>
	/// <param name="session">The session whose resources should be cleared.</param>
	private static void CleanupSessionResources(LanguageServerTransportSession session)
	{
		try
		{
			session.ServerInputStream.Dispose();
			session.ServerOutputStream.Dispose();
			session.Process?.Dispose();
		}
		catch
		{
			// Ignore stream disposal failures.
		}

		session.RpcCompletionTask = null;
		session.StderrLoopTask = null;
		session.JsonRpc = null;
		session.MessageHandler = null;
		session.RpcTarget = null;
	}

	/// <summary>
	/// Attempts to send a graceful shutdown request to the server.
	/// </summary>
	/// <param name="session">The session being shut down.</param>
	private async Task TrySendShutdownAsync(LanguageServerTransportSession session)
	{
		using var shutdownTimeout = new CancellationTokenSource(_shutdownRequestTimeout);

		Task<object?>? shutdownTask = null;

		try
		{
			shutdownTask = SendRequestCoreAsync<object?>(session, "shutdown", new EmptyParams(), shutdownTimeout.Token, allowDisposed: true);
			await shutdownTask.WaitAsync(_shutdownRequestTimeout).ConfigureAwait(false);
		}
		catch (TimeoutException)
		{
			shutdownTimeout.Cancel();

			if (shutdownTask is not null)
				ObserveLateShutdownTask(shutdownTask);

			_logger.LogWarning("Language server transport generation {Generation} in workspace '{Workspace}' did not acknowledge shutdown within {TimeoutMs} ms; continuing teardown.",
				session.Generation,
				_workspaceRootDirectoryPath,
				(int)_shutdownRequestTimeout.TotalMilliseconds);

			LogRecentStandardErrorContext(session, "Shutdown timeout");
		}
		catch (OperationCanceledException) when (shutdownTimeout.IsCancellationRequested)
		{
			if (shutdownTask is not null)
				ObserveLateShutdownTask(shutdownTask);

			_logger.LogWarning("Language server transport generation {Generation} in workspace '{Workspace}' did not acknowledge shutdown within {TimeoutMs} ms; continuing teardown.",
				session.Generation,
				_workspaceRootDirectoryPath,
				(int)_shutdownRequestTimeout.TotalMilliseconds);

			LogRecentStandardErrorContext(session, "Shutdown timeout");
		}
		catch (Exception exception)
		{
			_logger.LogWarning(exception,
				"Sending the language server shutdown request during disposal failed for transport generation {Generation} in workspace '{Workspace}'; continuing teardown.",
				session.Generation,
				_workspaceRootDirectoryPath);

			LogRecentStandardErrorContext(session, "Shutdown failure");
		}
	}

	private static void ObserveLateShutdownTask(Task task)
	{
		task.ContinueWith(
			static completedTask =>
			{
				if (completedTask.Exception is not null)
					_ = completedTask.Exception;
			},
			CancellationToken.None,
			TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.NotOnRanToCompletion,
			TaskScheduler.Default);
	}

	/// <summary>
	/// Attempts to queue the exit notification to the server as a best-effort local dispatch.
	/// </summary>
	/// <param name="session">The session being shut down.</param>
	private async Task TrySendExitNotificationAsync(LanguageServerTransportSession session)
	{
		try
		{
			await SendNotificationCoreAsync(session, "exit", new { }, CancellationToken.None, allowDisposed: true).ConfigureAwait(false);
		}
		catch (Exception exception)
		{
			_logger.LogDebug(exception, "Sending the language server exit notification during disposal failed for transport generation {Generation}; continuing teardown.", session.Generation);
		}
	}

	/// <summary>
	/// Waits for the background read and stderr loops to finish within the dispose timeout.
	/// </summary>
	/// <param name="readLoopTask">The main JSON-RPC completion task.</param>
	/// <param name="stderrLoopTask">The standard-error read loop task.</param>
	private async Task WaitForBackgroundLoopsAsync(Task? readLoopTask, Task? stderrLoopTask)
	{
		Task combined = Task.WhenAll(
			readLoopTask ?? Task.CompletedTask,
			stderrLoopTask ?? Task.CompletedTask);

		try
		{
			await combined.WaitAsync(_disposeWaitTimeout).ConfigureAwait(false);
		}
		catch (TimeoutException)
		{
			_logger.LogWarning("Language server background loops did not complete within {TimeoutMs} ms during disposal.",
				(int)_disposeWaitTimeout.TotalMilliseconds);
		}
		catch (Exception exception)
		{
			bool loggedSpecificLoop = false;
			loggedSpecificLoop |= TryLogCanceledBackgroundLoop(readLoopTask, "JSON-RPC completion");
			loggedSpecificLoop |= TryLogCanceledBackgroundLoop(stderrLoopTask, "stderr read");
			loggedSpecificLoop |= TryLogFaultedBackgroundLoop(readLoopTask, "JSON-RPC completion");
			loggedSpecificLoop |= TryLogFaultedBackgroundLoop(stderrLoopTask, "stderr read");

			if (!loggedSpecificLoop)
				_logger.LogWarning(exception, "Language server background loop failed during disposal.");
		}
	}

	/// <summary>
	/// Logs cancellation for one background loop when disposal canceled it intentionally.
	/// </summary>
	/// <param name="task">The loop task to inspect.</param>
	/// <param name="loopName">The logical loop name for diagnostics.</param>
	/// <returns><see langword="true"/> when cancellation was logged; otherwise, <see langword="false"/>.</returns>
	private bool TryLogCanceledBackgroundLoop(Task? task, string loopName)
	{
		if (task?.IsCanceled != true)
			return false;

		_logger.LogDebug("Language server background loop '{LoopName}' was canceled during disposal.", loopName);
		return true;
	}

	/// <summary>
	/// Logs the failure for one background loop when it faulted during disposal.
	/// </summary>
	/// <param name="task">The loop task to inspect.</param>
	/// <param name="loopName">The logical loop name for diagnostics.</param>
	/// <returns><see langword="true"/> when a fault was logged; otherwise, <see langword="false"/>.</returns>
	private bool TryLogFaultedBackgroundLoop(Task? task, string loopName)
	{
		if (task?.IsFaulted != true || task.Exception is not { } aggregateException)
			return false;

		Exception loggedException = aggregateException.Flatten().InnerExceptions.Count == 1
			? aggregateException.Flatten().InnerExceptions[0]
			: aggregateException.Flatten();

		_logger.LogWarning(loggedException, "Language server background loop '{LoopName}' failed during disposal.", loopName);
		return true;
	}

	/// <summary>
	/// Begins disposal for the client if it has not already started.
	/// </summary>
	/// <returns><see langword="true"/> when the current caller should continue disposal; otherwise, <see langword="false"/>.</returns>
	private bool TryBeginDispose()
	{
		if (Interlocked.Exchange(ref _disposeStarted, 1) != 0)
			return false;

		_isDisposed = true;
		return true;
	}

	/// <summary>
	/// Stops the language-server process, completes pending requests, and releases transport resources.
	/// </summary>
	/// <returns>A task that completes when disposal finishes.</returns>
	private async Task DisposeCoreAsync()
	{
		var disposeStopwatch = Stopwatch.StartNew();

		_callbackSignal.Writer.TryComplete();
		_diagnosticsSignal.Writer.TryComplete();

		try
		{
			_lifetimeCts.Cancel();
		}
		catch (ObjectDisposedException)
		{ }

		await WaitWithDisposeBudgetAsync(
			DisposeActiveSessionAsync(),
			disposeStopwatch,
			"Disposing language server timed out after {TimeoutMs} ms; abandoning background tasks.",
			"Disposing language server raised exceptions.").ConfigureAwait(false);

		await WaitForQueuedFailedSessionDisposalsAsync(disposeStopwatch).ConfigureAwait(false);

		if (!ReferenceEquals(_diagnosticsPumpTask, Task.CompletedTask))
		{
			await WaitWithDisposeBudgetAsync(
				_diagnosticsPumpTask,
				disposeStopwatch,
				"Language server diagnostics pump did not complete within {TimeoutMs} ms during disposal.",
				"Disposing the language server diagnostics pump raised exceptions.").ConfigureAwait(false);
		}

		await WaitWithDisposeBudgetAsync(
			_callbackPumpTask,
			disposeStopwatch,
			"Language server callback dispatcher did not complete within {TimeoutMs} ms during disposal.",
			"Disposing the language server callback dispatcher raised exceptions.").ConfigureAwait(false);

		await DisposeStartLockAsync(GetRemainingDisposeBudget(disposeStopwatch)).ConfigureAwait(false);
		_lifetimeCts.Dispose();
	}

	/// <summary>
	/// Waits for one teardown task while spending from the caller's remaining disposal budget.
	/// </summary>
	/// <param name="task">The teardown task to await.</param>
	/// <param name="disposeStopwatch">Tracks the elapsed disposal time.</param>
	/// <param name="timeoutMessage">The warning logged when the remaining disposal budget is exhausted.</param>
	/// <param name="exceptionMessage">The warning logged when the teardown task faults.</param>
	private async Task WaitWithDisposeBudgetAsync(Task task, Stopwatch disposeStopwatch, string timeoutMessage, string exceptionMessage)
	{
		TimeSpan remainingDisposeBudget = GetRemainingDisposeBudget(disposeStopwatch);

		if (remainingDisposeBudget <= TimeSpan.Zero)
		{
			if (!task.IsCompleted)
				_logger.LogWarning(timeoutMessage, (int)_disposeWaitTimeout.TotalMilliseconds);

			return;
		}

		try
		{
			await task.WaitAsync(remainingDisposeBudget).ConfigureAwait(false);
		}
		catch (TimeoutException)
		{
			_logger.LogWarning(timeoutMessage, (int)_disposeWaitTimeout.TotalMilliseconds);
		}
		catch (Exception exception)
		{
			if (WasObservedBackgroundLoopTermination(task))
				return;

			_logger.LogWarning(exception, exceptionMessage);
		}
	}

	/// <summary>
	/// Gets the remaining disposal budget shared by the teardown stages.
	/// </summary>
	/// <param name="disposeStopwatch">Tracks the elapsed disposal time.</param>
	/// <returns>The remaining shared disposal budget.</returns>
	private TimeSpan GetRemainingDisposeBudget(Stopwatch disposeStopwatch)
	{
		TimeSpan remainingDisposeBudget = _disposeWaitTimeout - disposeStopwatch.Elapsed;
		return remainingDisposeBudget > TimeSpan.Zero ? remainingDisposeBudget : TimeSpan.Zero;
	}

	/// <summary>
	/// Waits for the startup gate to become quiescent and then disposes it.
	/// </summary>
	/// <returns>A task that completes when the startup gate has been disposed or when cleanup timed out.</returns>
	private async Task DisposeStartLockAsync(TimeSpan waitTimeout)
	{
		bool startLockHeld = false;

		if (waitTimeout <= TimeSpan.Zero)
		{
			_logger.LogWarning("Language server startup gate did not become available within {TimeoutMs} ms during disposal.",
				(int)_disposeWaitTimeout.TotalMilliseconds);

			return;
		}

		try
		{
			if (!await _startLock.WaitAsync(waitTimeout).ConfigureAwait(false))
			{
				_logger.LogWarning("Language server startup gate did not become available within {TimeoutMs} ms during disposal.",
					(int)_disposeWaitTimeout.TotalMilliseconds);

				return;
			}

			startLockHeld = true;
		}
		catch (ObjectDisposedException)
		{
			return;
		}
		finally
		{
			if (startLockHeld)
			{
				try
				{
					_startLock.Release();
				}
				catch (ObjectDisposedException)
				{ }
			}
		}

		try
		{
			_startLock.Dispose();
		}
		catch (ObjectDisposedException)
		{ }
	}

	/// <summary>
	/// Throws when the client has been disposed and disposed access is not allowed.
	/// </summary>
	/// <param name="allowDisposed">Whether disposed access should be allowed.</param>
	private void ThrowIfDisposed(bool allowDisposed)
	{
		if (!allowDisposed)
			ObjectDisposedException.ThrowIf(_isDisposed, nameof(LanguageServerClient));
	}

	/// <summary>
	/// Stops the language-server process, completes pending requests, and releases transport resources.
	/// </summary>
	public void Dispose()
	{
		if (!TryBeginDispose())
			return;

		try
		{
			DisposeCoreAsync().GetAwaiter().GetResult();
		}
		finally
		{
			GC.SuppressFinalize(this);
		}
	}

	/// <summary>
	/// Stops the language-server process, completes pending requests, and releases transport resources asynchronously.
	/// </summary>
	/// <returns>A task that completes when disposal finishes.</returns>
	public async ValueTask DisposeAsync()
	{
		if (!TryBeginDispose())
			return;

		try
		{
			await DisposeCoreAsync().ConfigureAwait(false);
		}
		finally
		{
			GC.SuppressFinalize(this);
		}
	}
}
