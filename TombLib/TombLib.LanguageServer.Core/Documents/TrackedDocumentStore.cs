namespace TombLib.LanguageServer.Core;

/// <summary>
/// Owns generic tracked-document synchronization and lifecycle mechanics for a language-server integration.
/// Derived stores keep language-specific caches and policies outside this core abstraction.
/// </summary>
/// <typeparam name="TTrackedDocumentState">The tracked document state type owned by the store.</typeparam>
public abstract class TrackedDocumentStore<TTrackedDocumentState>
	where TTrackedDocumentState : TrackedDocumentState
{
	private readonly object _syncRoot = new();
	private readonly Dictionary<string, TTrackedDocumentState> _documents = new(StringComparer.OrdinalIgnoreCase);
	private long _nextAccessStamp;

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
		string safeContent = content ?? string.Empty;

		lock (_syncRoot)
		{
			if (!_documents.TryGetValue(filePath, out TTrackedDocumentState? state))
			{
				state = CreateTrackedDocumentState(
					filePath,
					LanguageServerPathHelper.CreateFileUri(filePath),
					safeContent,
					version: 1,
					isOpen: true,
					openReferenceCount: acquireOpenReference ? 1 : 0,
					requestReferenceCount: acquireRequestReference ? 1 : 0,
					lastAccessStamp: GetNextAccessStamp());

				_documents[filePath] = state;
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

	/// <summary>
	/// Releases one temporary request-driven reference for <paramref name="filePath"/> without
	/// immediately evicting the cached request-only document.
	/// </summary>
	public void ReleaseRequest(string filePath)
	{
		lock (_syncRoot)
		{
			if (!_documents.TryGetValue(filePath, out TTrackedDocumentState? state))
				return;

			state.References.ReleaseRequest();
			TouchTrackedDocumentState(state, GetNextAccessStamp());
		}
	}

	/// <summary>
	/// Releases one temporary request-driven reference for <paramref name="filePath"/>.
	/// </summary>
	/// <param name="filePath">The normalized file path.</param>
	/// <param name="document">When this method returns, contains the closing snapshot if the server copy is still open.</param>
	/// <returns><see langword="true"/> when the document was removed locally; otherwise, <see langword="false"/>.</returns>
	public bool TryReleaseRequest(string filePath, out DocumentSnapshot? document)
	{
		lock (_syncRoot)
		{
			if (!_documents.TryGetValue(filePath, out TTrackedDocumentState? state))
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
			_documents.Remove(filePath);
			return true;
		}
	}

	/// <summary>
	/// Evicts the oldest fully idle request-only documents until at most <paramref name="maxCount"/>
	/// remain tracked.
	/// </summary>
	public IReadOnlyList<DocumentSnapshot> TrimRequestOnlyDocuments(int maxCount)
	{
		if (maxCount < 0)
			throw new ArgumentOutOfRangeException(nameof(maxCount));

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
	/// Rekeys a tracked document to a new normalized file path.
	/// </summary>
	/// <param name="oldFilePath">The current normalized file path.</param>
	/// <param name="newFilePath">The replacement normalized file path.</param>
	/// <param name="content">The latest editor content.</param>
	/// <returns>The rename request that should be mirrored to the server, or <see langword="null"/> when no document was tracked.</returns>
	public DocumentRenameRequest? Rename(string oldFilePath, string newFilePath, string? content = null)
	{
		if (string.Equals(oldFilePath, newFilePath, StringComparison.OrdinalIgnoreCase))
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
	/// Releases one open reference for <paramref name="filePath"/>.
	/// </summary>
	/// <param name="filePath">The normalized file path.</param>
	/// <param name="document">When this method returns, contains the closing snapshot if the server copy is still open.</param>
	/// <returns><see langword="true"/> when the document was removed locally; otherwise, <see langword="false"/>.</returns>
	public bool TryClose(string filePath, out DocumentSnapshot? document)
	{
		lock (_syncRoot)
		{
			if (!_documents.TryGetValue(filePath, out TTrackedDocumentState? state))
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
			_documents.Remove(filePath);
			return true;
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

	/// <summary>
	/// Gets the current snapshot for a tracked document.
	/// </summary>
	/// <param name="filePath">The normalized file path.</param>
	/// <returns>The current snapshot, or <see langword="null"/> when the document is not tracked.</returns>
	public DocumentSnapshot? GetDocumentSnapshot(string filePath)
		=> WithTrackedDocument(filePath, static state => state.CreateSnapshot(), default(DocumentSnapshot));

	/// <summary>
	/// Gets snapshots for all documents that are currently considered open.
	/// </summary>
	/// <returns>The open-document snapshots.</returns>
	public IReadOnlyList<DocumentSnapshot> GetOpenDocuments()
	{
		lock (_syncRoot)
		{
			var documents = new List<DocumentSnapshot>();

			foreach (TTrackedDocumentState state in _documents.Values)
			{
				if (state.IsOpen)
					documents.Add(state.CreateSnapshot());
			}

			return documents;
		}
	}

	/// <summary>
	/// Gets the current tracked document count.
	/// </summary>
	protected int TrackedDocumentCountCore
	{
		get
		{
			lock (_syncRoot)
				return _documents.Count;
		}
	}

	/// <summary>
	/// Executes a callback against a tracked document while holding the store lock.
	/// </summary>
	/// <typeparam name="TResult">The callback result type.</typeparam>
	/// <param name="filePath">The normalized tracked file path.</param>
	/// <param name="accessTrackedDocument">The callback to execute when the document exists.</param>
	/// <param name="defaultValue">The result to return when the document is not tracked.</param>
	/// <returns>The callback result, or <paramref name="defaultValue"/> when no document is tracked.</returns>
	protected TResult WithTrackedDocument<TResult>(string filePath, Func<TTrackedDocumentState, TResult> accessTrackedDocument, TResult defaultValue)
	{
		lock (_syncRoot)
		{
			return _documents.TryGetValue(filePath, out TTrackedDocumentState? state)
				? accessTrackedDocument(state)
				: defaultValue;
		}
	}

	/// <summary>
	/// Executes a callback against a tracked document while holding the store lock.
	/// </summary>
	/// <param name="filePath">The normalized tracked file path.</param>
	/// <param name="mutateTrackedDocument">The callback to execute when the document exists.</param>
	protected void WithTrackedDocument(string filePath, Action<TTrackedDocumentState> mutateTrackedDocument)
	{
		lock (_syncRoot)
		{
			if (_documents.TryGetValue(filePath, out TTrackedDocumentState? state))
				mutateTrackedDocument(state);
		}
	}

	/// <summary>
	/// Creates a tracked document state for a newly seen document.
	/// </summary>
	protected abstract TTrackedDocumentState CreateTrackedDocumentState(
		string filePath,
		string uri,
		string content,
		int version,
		bool isOpen,
		int openReferenceCount,
		int requestReferenceCount,
		long lastAccessStamp);

	/// <summary>
	/// Reads the current access stamp for a tracked document.
	/// </summary>
	protected abstract long GetLastAccessStamp(TTrackedDocumentState state);

	/// <summary>
	/// Updates the access stamp for a tracked document.
	/// </summary>
	protected abstract void TouchTrackedDocumentState(TTrackedDocumentState state, long lastAccessStamp);

	/// <summary>
	/// Reopens a previously closed tracked document.
	/// </summary>
	protected abstract void ReopenTrackedDocumentState(TTrackedDocumentState state, string content);

	/// <summary>
	/// Replaces the tracked document content and returns the previous content.
	/// </summary>
	protected abstract string ReplaceTrackedDocumentContent(TTrackedDocumentState state, string content);

	/// <summary>
	/// Renames a tracked document to a new path and URI.
	/// </summary>
	protected abstract void RenameTrackedDocumentState(TTrackedDocumentState state, string filePath, string uri);

	/// <summary>
	/// Marks a tracked document as closed on the server.
	/// </summary>
	protected abstract void MarkTrackedDocumentClosed(TTrackedDocumentState state);

	/// <summary>
	/// Allows derived stores to react after a tracked document rename has completed.
	/// </summary>
	protected virtual void OnTrackedDocumentRenamed(TTrackedDocumentState state, bool contentChanged)
	{
	}

	private long GetNextAccessStamp() => ++_nextAccessStamp;
}