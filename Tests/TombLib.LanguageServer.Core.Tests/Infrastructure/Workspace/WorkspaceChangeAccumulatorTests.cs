namespace Nickelony.LanguageServer.Core.Tests;

[TestClass]
public class WorkspaceChangeAccumulatorTests
{
	[TestMethod]
	public void Add_DeleteThenCreateForSamePath_PreservesDeleteThenCreatePair()
	{
		var accumulator = new WorkspaceChangeAccumulator();

		accumulator.Add(@"C:\Workspace\Scripts\test.lua", FileChangeKind.Deleted);
		accumulator.Add(@"C:\Workspace\Scripts\test.lua", FileChangeKind.Created);

		List<WorkspaceFileChange> drainedChanges = accumulator.DrainChanges();

		Assert.AreEqual(2, drainedChanges.Count);
		Assert.AreEqual(@"C:\Workspace\Scripts\test.lua", drainedChanges[0].Path);
		Assert.AreEqual(FileChangeKind.Deleted, drainedChanges[0].Kind);
		Assert.AreEqual(@"C:\Workspace\Scripts\test.lua", drainedChanges[1].Path);
		Assert.AreEqual(FileChangeKind.Created, drainedChanges[1].Kind);
		Assert.IsTrue(accumulator.IsEmpty);
	}

	[TestMethod]
	public void Add_CreatedThenChangedForSamePath_PreservesCreated()
	{
		var accumulator = new WorkspaceChangeAccumulator();

		accumulator.Add(@"C:\Workspace\Scripts\test.lua", FileChangeKind.Created);
		accumulator.Add(@"C:\Workspace\Scripts\test.lua", FileChangeKind.Changed);

		List<WorkspaceFileChange> drainedChanges = accumulator.DrainChanges();

		Assert.AreEqual(1, drainedChanges.Count);
		Assert.AreEqual(FileChangeKind.Created, drainedChanges[0].Kind);
	}

	[TestMethod]
	public void Add_CreatedThenDeletedForSamePath_RemovesBufferedEntry()
	{
		var accumulator = new WorkspaceChangeAccumulator();

		accumulator.Add(@"C:\Workspace\Scripts\test.lua", FileChangeKind.Created);
		accumulator.Add(@"C:\Workspace\Scripts\test.lua", FileChangeKind.Deleted);

		List<WorkspaceFileChange> drainedChanges = accumulator.DrainChanges();

		Assert.AreEqual(0, drainedChanges.Count);
		Assert.IsTrue(accumulator.IsEmpty);
	}

	[TestMethod]
	public void Add_CreatedChangedThenDeletedForSamePath_RemovesBufferedEntry()
	{
		var accumulator = new WorkspaceChangeAccumulator();

		accumulator.Add(@"C:\Workspace\Scripts\test.lua", FileChangeKind.Created);
		accumulator.Add(@"C:\Workspace\Scripts\test.lua", FileChangeKind.Changed);
		accumulator.Add(@"C:\Workspace\Scripts\test.lua", FileChangeKind.Deleted);

		List<WorkspaceFileChange> drainedChanges = accumulator.DrainChanges();

		Assert.AreEqual(0, drainedChanges.Count);
		Assert.IsTrue(accumulator.IsEmpty);
	}

	[TestMethod]
	public void Add_TreatsPathsCaseInsensitively()
	{
		var accumulator = new WorkspaceChangeAccumulator();

		accumulator.Add(@"C:\Workspace\Scripts\Test.lua", FileChangeKind.Deleted);
		accumulator.Add(@"c:\workspace\scripts\test.lua", FileChangeKind.Created);

		List<WorkspaceFileChange> drainedChanges = accumulator.DrainChanges();

		Assert.AreEqual(2, drainedChanges.Count);
		Assert.AreEqual(FileChangeKind.Deleted, drainedChanges[0].Kind);
		Assert.AreEqual(FileChangeKind.Created, drainedChanges[1].Kind);
	}

	[TestMethod]
	public void DrainBatch_ClearsBufferedEntries()
	{
		var accumulator = new WorkspaceChangeAccumulator();

		accumulator.Add(@"C:\Workspace\Scripts\first.lua", FileChangeKind.Changed);
		accumulator.Add(@"C:\Workspace\Scripts\second.lua", FileChangeKind.Deleted);

		FileChangeBatch drainedBatch = accumulator.DrainBatch();
		FileChangeBatch drainedAgain = accumulator.DrainBatch();

		Assert.AreEqual(2, drainedBatch.Count);
		Assert.AreEqual(0, drainedAgain.Count);
		Assert.IsTrue(accumulator.IsEmpty);
	}

	[TestMethod]
	public void DrainBatch_EntriesAreExposedAsReadOnlyView()
	{
		var accumulator = new WorkspaceChangeAccumulator();

		accumulator.Add(@"C:\Workspace\Scripts\first.lua", FileChangeKind.Changed);

		FileChangeBatch drainedBatch = accumulator.DrainBatch();

		Assert.ThrowsException<NotSupportedException>(() => ((IList<WorkspaceFileChange>)drainedBatch.Entries)[0] =
			new WorkspaceFileChange(@"C:\Workspace\Scripts\second.lua", FileChangeKind.Deleted));
	}

	[TestMethod]
	public void DrainChanges_MultiplePaths_PreservesFirstPendingOccurrenceOrder()
	{
		var accumulator = new WorkspaceChangeAccumulator();

		accumulator.Add(@"C:\Workspace\Scripts\first.lua", FileChangeKind.Created);
		accumulator.Add(@"C:\Workspace\Scripts\second.lua", FileChangeKind.Changed);
		accumulator.Add(@"C:\Workspace\Scripts\first.lua", FileChangeKind.Changed);
		accumulator.Add(@"C:\Workspace\Scripts\third.lua", FileChangeKind.Deleted);

		List<WorkspaceFileChange> drainedChanges = accumulator.DrainChanges();

		CollectionAssert.AreEqual(
			new[]
			{
				@"C:\Workspace\Scripts\first.lua",
				@"C:\Workspace\Scripts\second.lua",
				@"C:\Workspace\Scripts\third.lua"
			},
			drainedChanges.Select(change => change.Path).ToArray());

		CollectionAssert.AreEqual(
			new[]
			{
				FileChangeKind.Created,
				FileChangeKind.Changed,
				FileChangeKind.Deleted
			},
			drainedChanges.Select(change => change.Kind).ToArray());
	}

	[TestMethod]
	public void DrainChanges_PathRemovedAndReadded_GetsNewPendingOrder()
	{
		var accumulator = new WorkspaceChangeAccumulator();

		accumulator.Add(@"C:\Workspace\Scripts\first.lua", FileChangeKind.Created);
		accumulator.Add(@"C:\Workspace\Scripts\first.lua", FileChangeKind.Deleted);
		accumulator.Add(@"C:\Workspace\Scripts\second.lua", FileChangeKind.Changed);
		accumulator.Add(@"C:\Workspace\Scripts\first.lua", FileChangeKind.Created);

		List<WorkspaceFileChange> drainedChanges = accumulator.DrainChanges();

		CollectionAssert.AreEqual(
			new[]
			{
				@"C:\Workspace\Scripts\second.lua",
				@"C:\Workspace\Scripts\first.lua"
			},
			drainedChanges.Select(change => change.Path).ToArray());
	}
}
