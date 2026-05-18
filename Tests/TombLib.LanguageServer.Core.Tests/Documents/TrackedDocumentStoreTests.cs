namespace TombLib.LanguageServer.Core.Tests;

[TestClass]
public class TrackedDocumentStoreTests
{
	[TestMethod]
	public void Rename_ReturnsNullAndPreservesTrackedDocuments_WhenDestinationIsAlreadyTracked()
	{
		var store = new TestTrackedDocumentStore();
		const string oldFilePath = @"C:\Workspace\Scripts\source.lua";
		const string newFilePath = @"C:\Workspace\Scripts\target.lua";

		store.Synchronize(oldFilePath, "return 1", acquireOpenReference: true);
		store.Synchronize(newFilePath, "return 2", acquireOpenReference: true);

		DocumentRenameRequest? renameRequest = store.Rename(oldFilePath, newFilePath, "return 1");

		Assert.IsNull(renameRequest);
		Assert.IsNotNull(store.GetDocumentSnapshot(oldFilePath));

		DocumentSnapshot? destinationDocument = store.GetDocumentSnapshot(newFilePath);

		Assert.IsNotNull(destinationDocument);
		Assert.AreEqual("return 2", destinationDocument.Content);
		Assert.AreEqual(1, destinationDocument.Version);
		Assert.IsTrue(store.TryClose(oldFilePath, out _));
		Assert.IsTrue(store.TryClose(newFilePath, out DocumentSnapshot? closedDestinationDocument));
		Assert.IsNotNull(closedDestinationDocument);
		Assert.AreEqual("return 2", closedDestinationDocument.Content);
	}

	[TestMethod]
	public void Rename_PathCaseOnlyDifference_FollowsPlatformPathSensitivity()
	{
		var store = new TestTrackedDocumentStore();
		string directoryPath = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "TrackedDocumentStoreTests"));
		string originalFilePath = LanguageServerPathHelper.NormalizeLocalPath(Path.Combine(directoryPath, "test.lua"));
		string renamedFilePath = LanguageServerPathHelper.NormalizeLocalPath(Path.Combine(directoryPath, "TEST.lua"));

		store.Synchronize(originalFilePath, "return 1", acquireOpenReference: true);

		DocumentRenameRequest? renameRequest = store.Rename(originalFilePath, renamedFilePath, "return 1");

		if (LanguageServerPathHelper.UsesCaseSensitiveLocalPaths)
		{
			Assert.IsNotNull(renameRequest);
			Assert.IsNull(store.GetDocumentSnapshot(originalFilePath));
			Assert.IsNotNull(store.GetDocumentSnapshot(renamedFilePath));
		}
		else
		{
			Assert.IsNull(renameRequest);
			Assert.IsNotNull(store.GetDocumentSnapshot(originalFilePath));

			DocumentSnapshot? aliasedDocument = store.GetDocumentSnapshot(renamedFilePath);

			Assert.IsNotNull(aliasedDocument);
			Assert.AreEqual(originalFilePath, aliasedDocument.FilePath);
		}
	}

	[TestMethod]
	public void TryClose_RemovesTrackedDocumentWhileRestartReplayIsPending()
	{
		var store = new TestTrackedDocumentStore();
		const string filePath = @"C:\Workspace\Scripts\pending.lua";

		store.Synchronize(filePath, "return 1", acquireOpenReference: true);
		IReadOnlyList<DocumentSnapshot> documentsToReopen = store.PrepareForRestart();

		Assert.AreEqual(1, documentsToReopen.Count);
		Assert.IsTrue(store.TryClose(filePath, out DocumentSnapshot? closingDocument));
		Assert.IsNull(closingDocument);
		Assert.IsNull(store.GetDocumentSnapshot(filePath));
	}

	[TestMethod]
	public void TryReleaseRequest_RemovesRequestOnlyTrackedDocument()
	{
		var store = new TestTrackedDocumentStore();
		const string filePath = @"C:\Workspace\Scripts\hover.lua";

		store.Synchronize(filePath, "return 1", acquireRequestReference: true);

		Assert.IsTrue(store.TryReleaseRequest(filePath, out DocumentSnapshot? closingDocument));
		Assert.IsNotNull(closingDocument);
		Assert.AreEqual(filePath, closingDocument.FilePath);
		Assert.IsNull(store.GetDocumentSnapshot(filePath));
	}

	[TestMethod]
	public void TryReleaseRequest_PreservesEditorOwnedTrackedDocument()
	{
		var store = new TestTrackedDocumentStore();
		const string filePath = @"C:\Workspace\Scripts\open.lua";

		store.Synchronize(filePath, "return 1", acquireOpenReference: true);
		store.Synchronize(filePath, "return 1", acquireRequestReference: true);

		Assert.IsFalse(store.TryReleaseRequest(filePath, out DocumentSnapshot? closingDocument));
		Assert.IsNull(closingDocument);
		Assert.IsNotNull(store.GetDocumentSnapshot(filePath));
		Assert.IsTrue(store.TryClose(filePath, out DocumentSnapshot? closedDocument));
		Assert.IsNotNull(closedDocument);
	}

	[TestMethod]
	public void TryClose_PreservesRequestOwnedTrackedDocumentUntilRequestRelease()
	{
		var store = new TestTrackedDocumentStore();
		const string filePath = @"C:\Workspace\Scripts\request-owned.lua";

		store.Synchronize(filePath, "return 1", acquireOpenReference: true);
		store.Synchronize(filePath, "return 1", acquireRequestReference: true);

		Assert.IsFalse(store.TryClose(filePath, out DocumentSnapshot? closingDocument));
		Assert.IsNull(closingDocument);
		Assert.IsNotNull(store.GetDocumentSnapshot(filePath));
		Assert.IsTrue(store.TryReleaseRequest(filePath, out DocumentSnapshot? closedDocument));
		Assert.IsNotNull(closedDocument);
		Assert.IsNull(store.GetDocumentSnapshot(filePath));
	}

	[TestMethod]
	public void Synchronize_ReopensTrackedDocumentAfterRestartPreparation()
	{
		var store = new TestTrackedDocumentStore();
		const string filePath = @"C:\Workspace\Scripts\reopen.lua";

		DocumentSynchronizationRequest? initialRequest = store.Synchronize(filePath, "return 1", acquireOpenReference: true);

		Assert.IsNotNull(initialRequest);
		Assert.AreEqual(DocumentSynchronizationKind.Open, initialRequest.Value.Kind);

		IReadOnlyList<DocumentSnapshot> documentsToReopen = store.PrepareForRestart();
		DocumentSynchronizationRequest? reopenRequest = store.Synchronize(filePath, "return 2", acquireOpenReference: true);

		Assert.AreEqual(1, documentsToReopen.Count);
		Assert.AreEqual(filePath, documentsToReopen[0].FilePath);
		Assert.IsNotNull(reopenRequest);
		Assert.AreEqual(DocumentSynchronizationKind.Open, reopenRequest.Value.Kind);
		Assert.AreEqual(filePath, reopenRequest.Value.Document.FilePath);
		Assert.AreEqual("return 2", reopenRequest.Value.Document.Content);
		Assert.AreEqual(2, reopenRequest.Value.Document.Version);

		DocumentSnapshot? reopenedDocument = store.GetDocumentSnapshot(filePath);

		Assert.IsNotNull(reopenedDocument);
		Assert.AreEqual("return 2", reopenedDocument.Content);
		Assert.AreEqual(2, reopenedDocument.Version);
		Assert.IsFalse(store.TryClose(filePath, out _));
		Assert.IsTrue(store.TryClose(filePath, out DocumentSnapshot? finalClosedDocument));
		Assert.IsNotNull(finalClosedDocument);
	}

	[TestMethod]
	public void TrimRequestOnlyDocuments_RemovesOldestIdleRequestOnlyDocuments()
	{
		var store = new TestTrackedDocumentStore();
		const string firstFilePath = @"C:\Workspace\Scripts\first.lua";
		const string secondFilePath = @"C:\Workspace\Scripts\second.lua";
		const string thirdFilePath = @"C:\Workspace\Scripts\third.lua";

		store.Synchronize(firstFilePath, "return 1", acquireRequestReference: true);
		store.ReleaseRequest(firstFilePath);

		store.Synchronize(secondFilePath, "return 2", acquireRequestReference: true);
		store.ReleaseRequest(secondFilePath);

		store.Synchronize(thirdFilePath, "return 3", acquireRequestReference: true);
		store.ReleaseRequest(thirdFilePath);

		IReadOnlyList<DocumentSnapshot> trimmedDocuments = store.TrimRequestOnlyDocuments(1);

		Assert.AreEqual(2, trimmedDocuments.Count);

		CollectionAssert.AreEquivalent(
			new[] { firstFilePath, secondFilePath },
			new[] { trimmedDocuments[0].FilePath, trimmedDocuments[1].FilePath });

		Assert.IsNull(store.GetDocumentSnapshot(firstFilePath));
		Assert.IsNull(store.GetDocumentSnapshot(secondFilePath));
		Assert.IsNotNull(store.GetDocumentSnapshot(thirdFilePath));
		Assert.AreEqual(0, store.TrimRequestOnlyDocuments(1).Count);
	}

	[TestMethod]
	public void TrimRequestOnlyDocuments_NegativeMaxCount_ThrowsArgumentOutOfRangeException()
	{
		var store = new TestTrackedDocumentStore();
		Assert.ThrowsException<ArgumentOutOfRangeException>(() => store.TrimRequestOnlyDocuments(-1));
	}

	private sealed class TestTrackedDocumentStore : TrackedDocumentStore<TestTrackedDocumentState>
	{
		protected override TestTrackedDocumentState CreateTrackedDocumentState(
			string filePath,
			string uri,
			string content,
			int version,
			bool isOpen,
			int openReferenceCount,
			int requestReferenceCount,
			long lastAccessStamp)
			=> new(filePath, uri, content, version, isOpen, openReferenceCount, requestReferenceCount, lastAccessStamp);

		protected override long GetLastAccessStamp(TestTrackedDocumentState state)
			=> state.LastAccessStamp;

		protected override void TouchTrackedDocumentState(TestTrackedDocumentState state, long lastAccessStamp)
			=> state.Touch(lastAccessStamp);

		protected override void ReopenTrackedDocumentState(TestTrackedDocumentState state, string content)
			=> state.Reopen(content);

		protected override string ReplaceTrackedDocumentContent(TestTrackedDocumentState state, string content)
			=> state.Update(content);

		protected override void RenameTrackedDocumentState(TestTrackedDocumentState state, string filePath, string uri)
			=> state.Rename(filePath, uri);

		protected override void MarkTrackedDocumentClosed(TestTrackedDocumentState state)
			=> state.Close();
	}

	private sealed class TestTrackedDocumentState : TrackedDocumentState
	{
		public TestTrackedDocumentState(string filePath, string uri, string content, int version, bool isOpen,
			int openReferenceCount, int requestReferenceCount, long lastAccessStamp)
			: base(filePath, uri, content, version, isOpen, openReferenceCount, requestReferenceCount, lastAccessStamp)
		{ }

		public void Touch(long lastAccessStamp)
			=> SetLastAccessStamp(lastAccessStamp);

		public void Reopen(string content)
			=> ReopenDocument(content);

		public string Update(string content)
			=> ReplaceContent(content);

		public void Rename(string filePath, string uri)
			=> RenameDocument(filePath, uri);

		public void Close()
			=> MarkDocumentClosed();
	}
}
