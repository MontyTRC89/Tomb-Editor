namespace TombLib.LanguageServer.Core;

public abstract partial class TrackedDocumentStore<TTrackedDocumentState>
	where TTrackedDocumentState : TrackedDocumentState
{
	/// <summary>
	/// Synchronizes the tracked state for a document and returns the LSP action required to mirror it to the server.
	/// </summary>
	/// <param name="filePath">The normalized file path.</param>
	/// <param name="content">The latest document content.</param>
	/// <param name="acquireOpenReference">Whether an additional open-editor reference should be recorded.</param>
	/// <param name="acquireRequestReference">Whether a temporary request-driven reference should be recorded.</param>
	/// <returns>A synchronization request when the server copy must be updated; otherwise, <see langword="null"/>.</returns>
	public DocumentSynchronizationRequest? Synchronize(
		string filePath,
		string? content,
		bool acquireOpenReference = false,
		bool acquireRequestReference = false)
	{
		string normalizedFilePath = NormalizeTrackedFilePath(filePath);
		string safeContent = content ?? string.Empty;

		lock (_syncRoot)
		{
			if (!_documents.TryGetValue(normalizedFilePath, out TTrackedDocumentState? state))
			{
				state = CreateTrackedDocumentState(
					normalizedFilePath,
					LanguageServerPathHelper.CreateFileUri(normalizedFilePath),
					safeContent,
					version: 1,
					isOpen: true,
					openReferenceCount: acquireOpenReference ? 1 : 0,
					requestReferenceCount: acquireRequestReference ? 1 : 0,
					lastAccessStamp: GetNextAccessStamp());

				_documents[normalizedFilePath] = state;
				return new DocumentSynchronizationRequest(DocumentSynchronizationKind.Open, state.CreateSnapshot());
			}

			if (acquireOpenReference)
				state.References.AcquireOpen();

			if (acquireRequestReference)
				state.References.AcquireRequest();

			TouchTrackedDocumentState(state, GetNextAccessStamp());

			if (!state.IsOpen)
			{
				ReopenTrackedDocumentState(state, safeContent);
				return new DocumentSynchronizationRequest(DocumentSynchronizationKind.Open, state.CreateSnapshot());
			}

			if (!string.Equals(state.Content, safeContent, StringComparison.Ordinal))
			{
				string previousContent = ReplaceTrackedDocumentContent(state, safeContent);
				DocumentLineOffsets previousOffsets = DocumentLineOffsets.Build(previousContent);
				DocumentChangeRange changeRange = DocumentIncrementalEditCalculator.Compute(previousContent, safeContent, previousOffsets);

				return new DocumentSynchronizationRequest(DocumentSynchronizationKind.Change, state.CreateSnapshot(), changeRange);
			}

			return null;
		}
	}
}
