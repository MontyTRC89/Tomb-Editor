namespace Nickelony.LanguageServer.Core.Tests;

[TestClass]
public class TrackedDocumentStateTests
{
	[TestMethod]
	public void TrackedDocumentState_CreateSnapshot_CapturesCurrentCoreState()
	{
		var state = new TestTrackedDocumentState(
			@"C:\Workspace\Scripts\start.lua",
			"file:///C:/Workspace/Scripts/start.lua",
			"return 1",
			version: 4,
			isOpen: true,
			openReferenceCount: 1,
			requestReferenceCount: 2,
			lastAccessStamp: 3);

		DocumentSnapshot initialSnapshot = state.CreateSnapshot();
		state.Rename(@"C:\Workspace\Scripts\renamed.lua", "file:///C:/Workspace/Scripts/renamed.lua");
		string previousContent = state.Update("return 2");
		state.Close();

		DocumentSnapshot updatedSnapshot = state.CreateSnapshot();

		Assert.AreEqual(@"C:\Workspace\Scripts\start.lua", initialSnapshot.FilePath);
		Assert.AreEqual("file:///C:/Workspace/Scripts/start.lua", initialSnapshot.Uri);
		Assert.AreEqual("return 1", initialSnapshot.Content);
		Assert.AreEqual(4, initialSnapshot.Version);
		Assert.AreEqual("return 1", previousContent);
		Assert.AreEqual(@"C:\Workspace\Scripts\renamed.lua", updatedSnapshot.FilePath);
		Assert.AreEqual("file:///C:/Workspace/Scripts/renamed.lua", updatedSnapshot.Uri);
		Assert.AreEqual("return 2", updatedSnapshot.Content);
		Assert.AreEqual(5, updatedSnapshot.Version);
		Assert.IsFalse(state.IsOpen);
	}

	[TestMethod]
	public async Task TrackedDocumentState_CreateSnapshot_DoesNotObserveMismatchedRenamePairsUnderConcurrency()
	{
		var state = new TestTrackedDocumentState(
			@"C:\Workspace\Scripts\a.lua",
			"file:///C:/Workspace/Scripts/a.lua",
			"return 'a'",
			version: 1,
			isOpen: true,
			openReferenceCount: 0,
			requestReferenceCount: 0,
			lastAccessStamp: 0);

		var mismatchMessages = new List<string>();
		using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMilliseconds(150));

		Task writerTask = Task.Run(() =>
		{
			while (!cancellationTokenSource.IsCancellationRequested)
			{
				state.Rename(@"C:\Workspace\Scripts\a.lua", "file:///C:/Workspace/Scripts/a.lua");
				state.Update("return 'a'");
				state.Rename(@"C:\Workspace\Scripts\b.lua", "file:///C:/Workspace/Scripts/b.lua");
				state.Update("return 'b'");
			}
		}, cancellationTokenSource.Token);

		Task readerTask = Task.Run(() =>
		{
			while (!cancellationTokenSource.IsCancellationRequested)
			{
				DocumentSnapshot snapshot = state.CreateSnapshot();

				bool isA = string.Equals(snapshot.FilePath, @"C:\Workspace\Scripts\a.lua", StringComparison.Ordinal)
					&& string.Equals(snapshot.Uri, "file:///C:/Workspace/Scripts/a.lua", StringComparison.Ordinal);

				bool isB = string.Equals(snapshot.FilePath, @"C:\Workspace\Scripts\b.lua", StringComparison.Ordinal)
					&& string.Equals(snapshot.Uri, "file:///C:/Workspace/Scripts/b.lua", StringComparison.Ordinal);

				if (!isA && !isB)
				{
					lock (mismatchMessages)
						mismatchMessages.Add(snapshot.FilePath + " | " + snapshot.Uri);
				}
			}
		}, cancellationTokenSource.Token);

		await Task.WhenAll(writerTask, readerTask).ConfigureAwait(false);

		Assert.AreEqual(0, mismatchMessages.Count,
			"Snapshots should not observe mixed file-path/URI rename pairs: " + string.Join(", ", mismatchMessages));
	}
}
