using TombIDE.ScriptingStudio.Services.LuaIntellisense;
using System.Reflection;

namespace TombLib.Test;

[TestClass]
public class LuaIntellisenseDocumentManagerTests
{
	[TestMethod]
	public void TrackedDocumentState_DoesNotExposePublicWritableProperties()
	{
		PropertyInfo[] properties = typeof(TrackedDocumentState).GetProperties(BindingFlags.Instance | BindingFlags.Public);

		foreach (PropertyInfo property in properties)
		{
			MethodInfo? setter = property.SetMethod;

			if (setter is null)
				continue;

			Assert.IsFalse(setter.IsPublic, $"Property '{property.Name}' should not expose a public setter.");
		}
	}

	[TestMethod]
	public void TrackedDocumentState_IsAbstractAndExposesOwnerControlledMutationHelpersOnly()
	{
		Assert.IsTrue(typeof(TrackedDocumentState).IsAbstract);

		MethodInfo[] protectedMethods = typeof(TrackedDocumentState).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);

		CollectionAssert.AreEquivalent(
			new[]
			{
				"SetLastAccessStamp",
				"ReopenDocument",
				"ReplaceContent",
				"RenameDocument",
				"MarkDocumentClosed"
			},
			protectedMethods
				.Where(method => method.IsFamily)
				.Select(method => method.Name)
				.ToArray());
	}

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

	[TestMethod]
	public void Rename_ReturnsNullAndPreservesTrackedDocuments_WhenDestinationIsAlreadyTracked()
	{
		var manager = new LuaIntellisenseDocumentManager();
		const string oldFilePath = @"C:\Workspace\Scripts\source.lua";
		const string newFilePath = @"C:\Workspace\Scripts\target.lua";

		manager.Synchronize(oldFilePath, "return 1", acquireOpenReference: true);
		manager.Synchronize(newFilePath, "return 2", acquireOpenReference: true);

		DocumentRenameRequest? renameRequest = manager.Rename(oldFilePath, newFilePath, "return 1");

		Assert.IsNull(renameRequest);
		Assert.IsNotNull(manager.GetDocumentSnapshot(oldFilePath));

		DocumentSnapshot? destinationDocument = manager.GetDocumentSnapshot(newFilePath);

		Assert.IsNotNull(destinationDocument);
		Assert.AreEqual("return 2", destinationDocument.Content);
		Assert.AreEqual(1, destinationDocument.Version);
		Assert.IsTrue(manager.TryClose(oldFilePath, out _));
		Assert.IsTrue(manager.TryClose(newFilePath, out DocumentSnapshot? closedDestinationDocument));
		Assert.IsNotNull(closedDestinationDocument);
		Assert.AreEqual("return 2", closedDestinationDocument.Content);
	}

	[TestMethod]
	public void TryClose_RemovesTrackedDocumentWhileRestartReplayIsPending()
	{
		var manager = new LuaIntellisenseDocumentManager();
		const string filePath = @"C:\Workspace\Scripts\pending.lua";

		manager.Synchronize(filePath, "return 1", acquireOpenReference: true);
		IReadOnlyList<DocumentSnapshot> documentsToReopen = manager.PrepareForRestart();

		Assert.AreEqual(1, documentsToReopen.Count);
		Assert.IsTrue(manager.TryClose(filePath, out DocumentSnapshot? closingDocument));
		Assert.IsNull(closingDocument);
		Assert.IsNull(manager.GetDocumentSnapshot(filePath));
	}

	[TestMethod]
	public void TryReleaseRequest_RemovesRequestOnlyTrackedDocument()
	{
		var manager = new LuaIntellisenseDocumentManager();
		const string filePath = @"C:\Workspace\Scripts\hover.lua";

		manager.Synchronize(filePath, "return 1", acquireRequestReference: true);

		Assert.IsTrue(manager.TryReleaseRequest(filePath, out DocumentSnapshot? closingDocument));
		Assert.IsNotNull(closingDocument);
		Assert.AreEqual(filePath, closingDocument.FilePath);
		Assert.IsNull(manager.GetDocumentSnapshot(filePath));
	}

	[TestMethod]
	public void TryReleaseRequest_PreservesEditorOwnedTrackedDocument()
	{
		var manager = new LuaIntellisenseDocumentManager();
		const string filePath = @"C:\Workspace\Scripts\open.lua";

		manager.Synchronize(filePath, "return 1", acquireOpenReference: true);
		manager.Synchronize(filePath, "return 1", acquireRequestReference: true);

		Assert.IsFalse(manager.TryReleaseRequest(filePath, out DocumentSnapshot? closingDocument));
		Assert.IsNull(closingDocument);
		Assert.IsNotNull(manager.GetDocumentSnapshot(filePath));
		Assert.IsTrue(manager.TryClose(filePath, out DocumentSnapshot? closedDocument));
		Assert.IsNotNull(closedDocument);
	}

	[TestMethod]
	public void TryClose_PreservesRequestOwnedTrackedDocumentUntilRequestRelease()
	{
		var manager = new LuaIntellisenseDocumentManager();
		const string filePath = @"C:\Workspace\Scripts\request-owned.lua";

		manager.Synchronize(filePath, "return 1", acquireOpenReference: true);
		manager.Synchronize(filePath, "return 1", acquireRequestReference: true);

		Assert.IsFalse(manager.TryClose(filePath, out DocumentSnapshot? closingDocument));
		Assert.IsNull(closingDocument);
		Assert.IsNotNull(manager.GetDocumentSnapshot(filePath));
		Assert.IsTrue(manager.TryReleaseRequest(filePath, out DocumentSnapshot? closedDocument));
		Assert.IsNotNull(closedDocument);
		Assert.IsNull(manager.GetDocumentSnapshot(filePath));
	}

	private sealed class TestTrackedDocumentState : TrackedDocumentState
	{
		public TestTrackedDocumentState(string filePath, string uri, string content, int version, bool isOpen,
			int openReferenceCount, int requestReferenceCount, long lastAccessStamp)
			: base(filePath, uri, content, version, isOpen, openReferenceCount, requestReferenceCount, lastAccessStamp)
		{
		}

		public void Rename(string filePath, string uri)
			=> RenameDocument(filePath, uri);

		public string Update(string content)
			=> ReplaceContent(content);

		public void Close()
			=> MarkDocumentClosed();
	}
}