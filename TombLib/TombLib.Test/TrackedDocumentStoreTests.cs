using TombLib.LanguageServer.Core;

namespace TombLib.Test;

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
		{
		}

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