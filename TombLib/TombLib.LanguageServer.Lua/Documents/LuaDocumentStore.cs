using TombLib.Scripting.Diagnostics;
using TombLib.Scripting.Core.Lua;

namespace TombLib.LanguageServer.Lua;

/// <summary>
/// Stores the local document state mirrored to LuaLS, including versions, diagnostics, and semantic-token caches.
/// </summary>
internal sealed class LuaDocumentStore : TrackedDocumentStore<LuaDocumentState>
{
	/// <summary>
	/// Gets the cached diagnostics for the specified normalized file path.
	/// </summary>
	/// <param name="filePath">The normalized file path.</param>
	/// <returns>The cached diagnostics, or an empty list when none are stored.</returns>
	internal IReadOnlyList<TextEditorDiagnostic> GetDiagnostics(string filePath)
		=> WithTrackedDocument(filePath, static state => state.DiagnosticsCache.Diagnostics, defaultValue: []);

	/// <summary>
	/// Gets the cached semantic tokens for the specified normalized file path.
	/// </summary>
	/// <param name="filePath">The normalized file path.</param>
	/// <returns>The cached semantic tokens, or an empty list when none are stored.</returns>
	internal IReadOnlyList<LuaSemanticToken> GetSemanticTokens(string filePath)
		=> WithTrackedDocument(filePath, static state => state.SemanticTokensCache.Tokens, defaultValue: []);

	internal int TrackedDocumentCount
		=> TrackedDocumentCountCore;

	/// <summary>
	/// Stores a diagnostics payload when it is not stale for the tracked document version.
	/// </summary>
	/// <param name="publishedDiagnostics">The diagnostics payload to cache.</param>
	/// <param name="expectedDocumentVersion">The tracked document version observed when the payload was parsed.</param>
	/// <returns><see langword="true"/> when the payload was stored; otherwise, <see langword="false"/>.</returns>
	internal bool TryStoreDiagnostics(LuaPublishedDiagnostics publishedDiagnostics, int expectedDocumentVersion)
	{
		return WithTrackedDocument(
			publishedDiagnostics.FilePath,
			state => !HasTrackedDocumentVersionAdvanced(state, expectedDocumentVersion)
				&& state.DiagnosticsCache.TryStore(publishedDiagnostics),
			defaultValue: false);
	}

	/// <summary>
	/// Stores semantic tokens when they are not stale for the tracked document version.
	/// </summary>
	/// <param name="filePath">The normalized file path.</param>
	/// <param name="version">The document version associated with the tokens.</param>
	/// <param name="semanticTokens">The semantic tokens to cache.</param>
	/// <returns><see langword="true"/> when the token set was stored; otherwise, <see langword="false"/>.</returns>
	internal bool TryStoreSemanticTokens(string filePath, int version, IReadOnlyList<LuaSemanticToken> semanticTokens)
	{
		return WithTrackedDocument(
			filePath,
			state => !HasTrackedDocumentVersionAdvanced(state, version)
				&& state.SemanticTokensCache.TryStore(version, semanticTokens),
			defaultValue: false);
	}

	/// <summary>
	/// Returns the cached semantic-tokens delta state for <paramref name="filePath"/>, if any.
	/// Used by the provider to send `semanticTokens/full/delta` requests with the previous result id.
	/// </summary>
	/// <param name="filePath">The normalized file path.</param>
	/// <returns>The cached delta state, if available.</returns>
	internal SemanticTokensDeltaState GetSemanticTokensDeltaState(string filePath)
		=> WithTrackedDocument(filePath, static state => state.SemanticTokensCache.GetDeltaState(), new SemanticTokensDeltaState(null, null));

	/// <summary>
	/// Stores the raw `data` payload returned by `semanticTokens/full(/delta)` along with the
	/// associated `resultId`, so subsequent requests can ask LuaLS for incremental edits.
	/// </summary>
	/// <param name="filePath">The normalized file path.</param>
	/// <param name="resultId">The server-provided semantic-token result id.</param>
	/// <param name="data">The cached integer token stream.</param>
	internal void StoreSemanticTokensDeltaState(string filePath, string? resultId, int[]? data)
		=> WithTrackedDocument(filePath, state => state.SemanticTokensCache.StoreDeltaState(resultId, data));

	/// <summary>
	/// Clears the cached semantic tokens for the specified normalized file path.
	/// </summary>
	/// <param name="filePath">The normalized file path.</param>
	/// <returns>The cleared semantic-token list, or an empty list when the document is not tracked.</returns>
	internal IReadOnlyList<LuaSemanticToken> ClearSemanticTokens(string filePath)
	{
		return WithTrackedDocument(
			filePath,
			state =>
			{
				state.SemanticTokensCache.Clear();
				return state.SemanticTokensCache.Tokens;
			},
			defaultValue: []);
	}

	/// <summary>
	/// Marks the specified document as needing a fresh server-side open/sync before incremental updates can resume.
	/// </summary>
	/// <param name="filePath">The normalized file path.</param>
	/// <returns><see langword="true"/> when the document was found and invalidated; otherwise, <see langword="false"/>.</returns>
	internal bool InvalidateServerSynchronization(string filePath)
	{
		return WithTrackedDocument(filePath,
			state =>
			{
				MarkTrackedDocumentClosed(state);
				state.SemanticTokensCache.InvalidateServerSynchronization();
				return true;
			},
			defaultValue: false);
	}

	protected override LuaDocumentState CreateTrackedDocumentState(
		string filePath,
		string uri,
		string content,
		int version,
		bool isOpen,
		int openReferenceCount,
		int requestReferenceCount,
		long lastAccessStamp)
	{
		return new(
			filePath,
			uri,
			content,
			version,
			isOpen,
			openReferenceCount,
			requestReferenceCount,
			lastAccessStamp);
	}

	protected override long GetLastAccessStamp(LuaDocumentState state)
		=> state.LastAccessStamp;

	protected override void TouchTrackedDocumentState(LuaDocumentState state, long lastAccessStamp)
		=> state.Touch(lastAccessStamp);

	protected override void ReopenTrackedDocumentState(LuaDocumentState state, string content)
		=> state.Reopen(content);

	protected override string ReplaceTrackedDocumentContent(LuaDocumentState state, string content)
		=> state.UpdateContent(content);

	protected override void RenameTrackedDocumentState(LuaDocumentState state, string filePath, string uri)
		=> state.RenameTo(filePath, uri);

	protected override void MarkTrackedDocumentClosed(LuaDocumentState state)
		=> state.MarkClosed();

	protected override void OnTrackedDocumentRenamed(LuaDocumentState state, bool contentChanged)
	{
		if (contentChanged)
			ClearCachedState(state);
	}

	private static void ClearCachedState(LuaDocumentState state)
	{
		state.DiagnosticsCache.Clear();
		state.SemanticTokensCache.Clear();
	}

	private static bool HasTrackedDocumentVersionAdvanced(LuaDocumentState state, int expectedVersion)
		=> expectedVersion > 0 && state.Version > 0 && state.Version != expectedVersion;
}
