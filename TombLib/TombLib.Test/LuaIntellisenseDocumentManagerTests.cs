using TombIDE.ScriptingStudio.Services.LuaIntellisense;

namespace TombLib.Test;

[TestClass]
public class LuaIntellisenseDocumentManagerTests
{
	[TestMethod]
	public void Rename_ReturnsNullAndPreservesTrackedDocuments_WhenDestinationIsAlreadyTracked()
	{
		var manager = new LuaIntellisenseDocumentManager();
		const string oldFilePath = @"C:\Workspace\Scripts\source.lua";
		const string newFilePath = @"C:\Workspace\Scripts\target.lua";

		manager.Synchronize(oldFilePath, "return 1", acquireOpenReference: true);
		manager.Synchronize(newFilePath, "return 2", acquireOpenReference: true);

		LuaDocumentRenameRequest? renameRequest = manager.Rename(oldFilePath, newFilePath, "return 1");

		Assert.IsNull(renameRequest);
		Assert.IsNotNull(manager.GetDocumentSnapshot(oldFilePath));

		LuaDocumentSnapshot? destinationDocument = manager.GetDocumentSnapshot(newFilePath);

		Assert.IsNotNull(destinationDocument);
		Assert.AreEqual("return 2", destinationDocument.Content);
		Assert.AreEqual(1, destinationDocument.Version);
		Assert.IsTrue(manager.TryClose(oldFilePath, out _));
		Assert.IsTrue(manager.TryClose(newFilePath, out LuaDocumentSnapshot? closedDestinationDocument));
		Assert.IsNotNull(closedDestinationDocument);
		Assert.AreEqual("return 2", closedDestinationDocument.Content);
	}

	[TestMethod]
	public void TryClose_RemovesTrackedDocumentWhileRestartReplayIsPending()
	{
		var manager = new LuaIntellisenseDocumentManager();
		const string filePath = @"C:\Workspace\Scripts\pending.lua";

		manager.Synchronize(filePath, "return 1", acquireOpenReference: true);
		IReadOnlyList<LuaDocumentSnapshot> documentsToReopen = manager.PrepareForRestart();

		Assert.AreEqual(1, documentsToReopen.Count);
		Assert.IsTrue(manager.TryClose(filePath, out LuaDocumentSnapshot? closingDocument));
		Assert.IsNull(closingDocument);
		Assert.IsNull(manager.GetDocumentSnapshot(filePath));
	}
}