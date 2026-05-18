namespace TombLib.LanguageServer.Lua.Tests;

[TestClass]
public class LuaDocumentStoreTests
{
	[TestMethod]
	public void Rename_ReturnsNullAndPreservesTrackedDocuments_WhenDestinationIsAlreadyTracked()
	{
		var manager = new LuaDocumentStore();
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
		var manager = new LuaDocumentStore();
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
		var manager = new LuaDocumentStore();
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
		var manager = new LuaDocumentStore();
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
		var manager = new LuaDocumentStore();
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
}
