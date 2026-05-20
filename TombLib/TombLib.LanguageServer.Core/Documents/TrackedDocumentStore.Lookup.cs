namespace TombLib.LanguageServer.Core;

public abstract partial class TrackedDocumentStore<TTrackedDocumentState>
	where TTrackedDocumentState : TrackedDocumentState
{
	/// <summary>
	/// Gets the current snapshot for a tracked document.
	/// </summary>
	/// <param name="filePath">The normalized file path.</param>
	/// <returns>The current snapshot, or <see langword="null"/> when the document is not tracked.</returns>
	public DocumentSnapshot? GetDocumentSnapshot(string filePath)
		=> WithTrackedDocument(filePath, static state => state.CreateSnapshot(), default);

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
		string normalizedFilePath = NormalizeTrackedFilePath(filePath);

		lock (_syncRoot)
		{
			return _documents.TryGetValue(normalizedFilePath, out TTrackedDocumentState? state)
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
		string normalizedFilePath = NormalizeTrackedFilePath(filePath);

		lock (_syncRoot)
		{
			if (_documents.TryGetValue(normalizedFilePath, out TTrackedDocumentState? state))
				mutateTrackedDocument(state);
		}
	}
}
