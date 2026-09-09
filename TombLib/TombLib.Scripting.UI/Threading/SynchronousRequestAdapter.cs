using System;
using System.Threading;
using System.Threading.Tasks;

namespace TombLib.Scripting.UI.Threading;

/// <summary>
/// Adapts an intrinsically synchronous provider request to the asynchronous controller callback
/// shape used by the shared UI controllers. Use this only where no asynchronous provider work is
/// possible.
/// </summary>
public static class SynchronousRequestAdapter
{
	/// <summary>
	/// Runs the synchronous request on the calling thread and returns its result in a completed task.
	/// </summary>
	/// <param name="request">The synchronous provider request to run.</param>
	/// <param name="cancellationToken">The controller's cancellation token.</param>
	/// <returns>A completed task carrying the provider result.</returns>
	/// <remarks>
	/// Cancellation is best effort: the token is honored before the request starts, and the calling
	/// controller re-validates it after the call completes and drops stale results. A synchronous
	/// provider cannot be interrupted mid-call, so the token is not observed while the request runs.
	/// </remarks>
	public static Task<TResult> Adapt<TResult>(Func<TResult> request, CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(request);
		cancellationToken.ThrowIfCancellationRequested();

		return Task.FromResult(request());
	}
}
