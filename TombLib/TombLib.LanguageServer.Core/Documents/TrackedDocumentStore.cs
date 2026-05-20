namespace TombLib.LanguageServer.Core;

/// <summary>
/// Owns generic tracked-document synchronization and lifecycle mechanics for a language-server integration.
/// Derived stores keep language-specific caches and policies outside this core abstraction.
/// </summary>
/// <typeparam name="TTrackedDocumentState">The tracked document state type owned by the store.</typeparam>
public abstract partial class TrackedDocumentStore<TTrackedDocumentState>
	where TTrackedDocumentState : TrackedDocumentState
{
	// Store state is keyed by normalized file path. Access stamps provide an LRU-style ordering for
	// request-only document trimming without pushing that policy into derived language-specific stores.
	private readonly object _syncRoot = new();
	private readonly Dictionary<string, TTrackedDocumentState> _documents = new(LanguageServerPathHelper.LocalPathComparer);
	private long _nextAccessStamp;

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
	{ }

	private static string NormalizeTrackedFilePath(string filePath)
		=> LanguageServerPathHelper.NormalizeLocalPath(filePath);

	private long GetNextAccessStamp() => ++_nextAccessStamp;
}
