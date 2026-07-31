using Nickelony.LanguageServer.Core.Diagnostics;

namespace Nickelony.LanguageServer.Lua.Tests;

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

	[TestMethod]
	public void DiagnosticsCache_StoresReadOnlyCopyDetachedFromSourceCollection()
	{
		var manager = new LuaDocumentStore();
		const string filePath = @"C:\Workspace\Scripts\diagnostics.lua";

		manager.Synchronize(filePath, "return 1", acquireOpenReference: true);
		DocumentSnapshot? trackedDocument = manager.GetDocumentSnapshot(filePath);

		var originalDiagnostic = new TextEditorDiagnostic(TextEditorDiagnosticSeverity.Warning, "Original", 0, 1);
		var replacementDiagnostic = new TextEditorDiagnostic(TextEditorDiagnosticSeverity.Warning, "Replacement", 1, 2);
		TextEditorDiagnostic[] sourceDiagnostics = [originalDiagnostic];

		Assert.IsNotNull(trackedDocument);

		Assert.IsTrue(manager.TryStoreDiagnostics(
			new LuaPublishedDiagnostics(filePath, sourceDiagnostics, version: trackedDocument.Version),
			expectedDocumentVersion: trackedDocument.Version));

		sourceDiagnostics[0] = replacementDiagnostic;

		IReadOnlyList<TextEditorDiagnostic> storedDiagnostics = manager.GetDiagnostics(filePath);

		Assert.AreEqual(1, storedDiagnostics.Count);
		Assert.AreSame(originalDiagnostic, storedDiagnostics[0]);
		Assert.ThrowsException<NotSupportedException>(() => ((IList<TextEditorDiagnostic>)storedDiagnostics)[0] = replacementDiagnostic);
	}

	[TestMethod]
	public void DiagnosticsCache_DoesNotStorePayloadWhenTrackedDocumentVersionAdvanced()
	{
		var manager = new LuaDocumentStore();
		const string filePath = @"C:\Workspace\Scripts\diagnostics.lua";

		manager.Synchronize(filePath, "return 1", acquireOpenReference: true);
		DocumentSnapshot? staleDocument = manager.GetDocumentSnapshot(filePath);
		manager.Synchronize(filePath, "return 2");

		Assert.IsNotNull(staleDocument);

		bool stored = manager.TryStoreDiagnostics(
			new LuaPublishedDiagnostics(
				filePath,
				[new TextEditorDiagnostic(TextEditorDiagnosticSeverity.Warning, "Stale", 0, 1)],
				version: staleDocument.Version),
			expectedDocumentVersion: staleDocument.Version);

		Assert.IsFalse(stored);
		Assert.AreEqual(0, manager.GetDiagnostics(filePath).Count);
	}

	[TestMethod]
	public void SemanticTokensCache_ClonesStoredCollectionsAndDeltaState()
	{
		var manager = new LuaDocumentStore();
		const string filePath = @"C:\Workspace\Scripts\semantic.lua";

		manager.Synchronize(filePath, "return 1", acquireOpenReference: true);

		var originalToken = new LuaSemanticToken(0, 0, 6, "variable", []);
		var replacementToken = new LuaSemanticToken(1, 0, 6, "function", []);
		LuaSemanticToken[] sourceTokens = [originalToken];
		int[] sourceDeltaData = [1, 2, 3];

		Assert.IsTrue(manager.TryStoreSemanticTokens(filePath, version: 1, sourceTokens));
		manager.StoreSemanticTokensDeltaState(filePath, "tokens-1", sourceDeltaData);

		sourceTokens[0] = replacementToken;
		sourceDeltaData[0] = 99;

		IReadOnlyList<LuaSemanticToken> storedTokens = manager.GetSemanticTokens(filePath);
		SemanticTokensDeltaState deltaState = manager.GetSemanticTokensDeltaState(filePath);

		Assert.AreEqual(1, storedTokens.Count);
		Assert.AreSame(originalToken, storedTokens[0]);
		Assert.ThrowsException<NotSupportedException>(() => ((IList<LuaSemanticToken>)storedTokens)[0] = replacementToken);
		Assert.IsNotNull(deltaState.PreviousData);
		Assert.AreEqual(1, deltaState.PreviousData[0]);

		deltaState.PreviousData[1] = 77;

		SemanticTokensDeltaState reloadedDeltaState = manager.GetSemanticTokensDeltaState(filePath);

		Assert.IsNotNull(reloadedDeltaState.PreviousData);
		Assert.AreEqual(2, reloadedDeltaState.PreviousData[1]);
	}

	[TestMethod]
	public void SemanticTokensCache_DoesNotStoreTokensWhenTrackedDocumentVersionAdvanced()
	{
		var manager = new LuaDocumentStore();
		const string filePath = @"C:\Workspace\Scripts\semantic.lua";

		manager.Synchronize(filePath, "return 1", acquireOpenReference: true);
		DocumentSnapshot? staleDocument = manager.GetDocumentSnapshot(filePath);
		manager.Synchronize(filePath, "return 2");

		Assert.IsNotNull(staleDocument);

		bool stored = manager.TryStoreSemanticTokens(
			filePath,
			version: staleDocument.Version,
			[new LuaSemanticToken(0, 0, 6, "variable", [])]);

		Assert.IsFalse(stored);
		Assert.AreEqual(0, manager.GetSemanticTokens(filePath).Count);
	}
}
