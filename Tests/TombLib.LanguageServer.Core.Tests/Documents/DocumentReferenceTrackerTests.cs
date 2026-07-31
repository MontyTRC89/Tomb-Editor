namespace Nickelony.LanguageServer.Core.Tests;

[TestClass]
public class DocumentReferenceTrackerTests
{
	[TestMethod]
	public void AcquireAndRelease_OpenAndRequestReferences_RemainAccurateUnderConcurrency()
	{
		const int operationCount = 2000;
		var tracker = new DocumentReferenceTracker();

		Parallel.For(0, operationCount, _ => tracker.AcquireOpen());
		Parallel.For(0, operationCount, _ => tracker.AcquireRequest());

		Assert.AreEqual(operationCount, tracker.OpenReferenceCount);
		Assert.AreEqual(operationCount, tracker.RequestReferenceCount);
		Assert.IsTrue(tracker.HasOpenReferences);
		Assert.IsFalse(tracker.IsIdle);

		Parallel.For(0, operationCount, _ => tracker.ReleaseOpen());
		Parallel.For(0, operationCount, _ => tracker.ReleaseRequest());

		Assert.AreEqual(0, tracker.OpenReferenceCount);
		Assert.AreEqual(0, tracker.RequestReferenceCount);
		Assert.IsFalse(tracker.HasOpenReferences);
		Assert.IsTrue(tracker.IsIdle);
	}

	[TestMethod]
	public void ReleaseMethods_DoNotDriveCountsNegativeUnderConcurrency()
	{
		var tracker = new DocumentReferenceTracker();

		Parallel.For(0, 2000, _ =>
		{
			tracker.ReleaseOpen();
			tracker.ReleaseRequest();
		});

		Assert.AreEqual(0, tracker.OpenReferenceCount);
		Assert.AreEqual(0, tracker.RequestReferenceCount);
		Assert.IsTrue(tracker.IsIdle);
	}
}
