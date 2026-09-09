using System;
using System.Threading;
using System.Threading.Tasks;
using TombLib.Scripting.UI.Threading;

namespace TombLib.Tests;

[TestClass]
public class SynchronousRequestAdapterTests
{
	[TestMethod]
	public void Adapt_ReturnsCompletedTaskCarryingProviderResult()
	{
		bool requestRan = false;

		Task<int> task = SynchronousRequestAdapter.Adapt(
			() =>
			{
				requestRan = true;
				return 42;
			},
			CancellationToken.None);

		Assert.IsTrue(requestRan);
		Assert.IsTrue(task.IsCompleted);
		Assert.AreEqual(42, task.Result);
	}

	[TestMethod]
	public void Adapt_RunsRequestOnCallingThread()
	{
		int callingThreadId = Environment.CurrentManagedThreadId;
		int? requestThreadId = null;

		Task<bool> task = SynchronousRequestAdapter.Adapt(
			() =>
			{
				requestThreadId = Environment.CurrentManagedThreadId;
				return true;
			},
			CancellationToken.None);

		Assert.AreEqual(callingThreadId, requestThreadId);
		Assert.IsTrue(task.Result);
	}

	[TestMethod]
	public void Adapt_PreCancelledToken_ThrowsWithoutRunningRequest()
	{
		using var cancellation = new CancellationTokenSource();
		cancellation.Cancel();
		bool requestRan = false;

		Assert.ThrowsException<OperationCanceledException>(() =>
		{
			_ = SynchronousRequestAdapter.Adapt(
				() =>
				{
					requestRan = true;
					return 1;
				},
				cancellation.Token);
		});

		Assert.IsFalse(requestRan);
	}
}
