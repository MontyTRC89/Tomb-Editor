namespace TombLib.LanguageServer.Core;

public abstract partial class TrackedDocumentStore<TTrackedDocumentState>
	where TTrackedDocumentState : TrackedDocumentState
{
	/// <summary>
	/// Rekeys a tracked document to a new normalized file path.
	/// </summary>
	/// <param name="oldFilePath">The current normalized file path.</param>
	/// <param name="newFilePath">The replacement normalized file path.</param>
	/// <param name="content">The latest editor content.</param>
	/// <returns>The rename request that should be mirrored to the server, or <see langword="null"/> when no document was tracked.</returns>
	public DocumentRenameRequest? Rename(string oldFilePath, string newFilePath, string? content = null)
	{
		if (LanguageServerPathHelper.AreLocalPathsEqual(oldFilePath, newFilePath))
			return null;

		lock (_syncRoot)
		{
			if (!_documents.TryGetValue(oldFilePath, out TTrackedDocumentState? state))
				return null;

			if (_documents.ContainsKey(newFilePath))
				return null;

			string safeContent = content ?? state.Content;
			bool contentChanged = !string.Equals(state.Content, safeContent, StringComparison.Ordinal);
			DocumentSnapshot? previousDocument = state.IsOpen ? state.CreateSnapshot() : null;

			_documents.Remove(oldFilePath);
			RenameTrackedDocumentState(state, newFilePath, LanguageServerPathHelper.CreateFileUri(newFilePath));

			if (contentChanged)
				ReplaceTrackedDocumentContent(state, safeContent);

			OnTrackedDocumentRenamed(state, contentChanged);

			_documents[newFilePath] = state;
			return new DocumentRenameRequest(previousDocument, state.CreateSnapshot(), previousDocument is not null);
		}
	}

	/// <summary>
	/// Marks every tracked document as closed on the server after a language-server restart.
	/// </summary>
	/// <returns>The snapshots that should be reopened on the next successful start.</returns>
	public IReadOnlyList<DocumentSnapshot> PrepareForRestart()
	{
		lock (_syncRoot)
		{
			var documentsToReopen = new List<DocumentSnapshot>();

			foreach (TTrackedDocumentState state in _documents.Values)
			{
				MarkTrackedDocumentClosed(state);

				if (state.References.HasOpenReferences)
					documentsToReopen.Add(state.CreateSnapshot());
			}

			return documentsToReopen;
		}
	}
}
