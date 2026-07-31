namespace TombLib.LanguageServer.Core;

public abstract partial class TrackedDocumentStore<TTrackedDocumentState>
	where TTrackedDocumentState : TrackedDocumentState
{
	/// <summary>
	/// Releases one temporary request-driven reference for <paramref name="filePath"/> without
	/// immediately evicting the cached request-only document.
	/// </summary>
	/// <param name="filePath">The local file path of the document.</param>
	public void ReleaseRequest(string filePath)
	{
		string normalizedFilePath = NormalizeTrackedFilePath(filePath);

		lock (_syncRoot)
		{
			if (!_documents.TryGetValue(normalizedFilePath, out TTrackedDocumentState? state))
				return;

			state.References.ReleaseRequest();
			TouchTrackedDocumentState(state, GetNextAccessStamp());
		}
	}

	/// <summary>
	/// Releases one temporary request-driven reference for <paramref name="filePath"/> and
	/// removes the document from local tracking when no references remain.
	/// </summary>
	/// <param name="filePath">The local file path of the document.</param>
	/// <param name="document">When this method returns, contains the closing snapshot if the server copy is still open.</param>
	/// <returns><see langword="true"/> when the document was removed locally; otherwise, <see langword="false"/>.</returns>
	public bool TryReleaseRequest(string filePath, out DocumentSnapshot? document)
	{
		string normalizedFilePath = NormalizeTrackedFilePath(filePath);

		lock (_syncRoot)
		{
			if (!_documents.TryGetValue(normalizedFilePath, out TTrackedDocumentState? state))
			{
				document = null;
				return false;
			}

			state.References.ReleaseRequest();
			TouchTrackedDocumentState(state, GetNextAccessStamp());

			if (!state.References.IsIdle)
			{
				document = null;
				return false;
			}

			document = state.IsOpen ? state.CreateSnapshot() : null;
			_documents.Remove(normalizedFilePath);
			return true;
		}
	}

	/// <summary>
	/// Evicts the oldest fully idle request-only documents until at most <paramref name="maxCount"/>
	/// idle request-only documents remain tracked.
	/// </summary>
	/// <param name="maxCount">The maximum number of idle request-only documents to keep tracked.</param>
	public IReadOnlyList<DocumentSnapshot> TrimRequestOnlyDocuments(int maxCount)
	{
		ArgumentOutOfRangeException.ThrowIfNegative(maxCount);

		lock (_syncRoot)
		{
			var candidates = new List<TTrackedDocumentState>();

			foreach (TTrackedDocumentState state in _documents.Values)
			{
				if (state.References.IsIdle)
					candidates.Add(state);
			}

			if (candidates.Count <= maxCount)
				return [];

			candidates.Sort((left, right) => GetLastAccessStamp(left).CompareTo(GetLastAccessStamp(right)));

			int removeCount = candidates.Count - maxCount;
			var documentsToClose = new List<DocumentSnapshot>(removeCount);

			for (int i = 0; i < removeCount; i++)
			{
				TTrackedDocumentState state = candidates[i];
				_documents.Remove(state.FilePath);

				if (state.IsOpen)
					documentsToClose.Add(state.CreateSnapshot());
			}

			return documentsToClose;
		}
	}

	/// <summary>
	/// Releases one open reference for <paramref name="filePath"/>.
	/// </summary>
	/// <param name="filePath">The local file path of the document.</param>
	/// <param name="document">When this method returns, contains the closing snapshot if the server copy is still open.</param>
	/// <returns><see langword="true"/> when the document was removed locally; otherwise, <see langword="false"/>.</returns>
	public bool TryClose(string filePath, out DocumentSnapshot? document)
	{
		string normalizedFilePath = NormalizeTrackedFilePath(filePath);

		lock (_syncRoot)
		{
			if (!_documents.TryGetValue(normalizedFilePath, out TTrackedDocumentState? state))
			{
				document = null;
				return false;
			}

			state.References.ReleaseOpen();

			if (state.References.HasOpenReferences)
			{
				document = null;
				return false;
			}

			if (!state.References.IsIdle)
			{
				TouchTrackedDocumentState(state, GetNextAccessStamp());
				document = null;
				return false;
			}

			document = state.IsOpen ? state.CreateSnapshot() : null;
			_documents.Remove(normalizedFilePath);
			return true;
		}
	}
}
