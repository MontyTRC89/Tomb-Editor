namespace Nickelony.LanguageServer.Lua;

/// <summary>
/// Extends the core tracked-document state with Lua-specific caches for diagnostics and semantic tokens.
/// </summary>
internal sealed class LuaDocumentState : TrackedDocumentState
{
	/// <summary>
	/// Initializes a new instance of the <see cref="LuaDocumentState"/> class.
	/// </summary>
	/// <param name="filePath">The normalized tracked file path.</param>
	/// <param name="uri">The mirrored file URI.</param>
	/// <param name="content">The latest synchronized document content.</param>
	/// <param name="version">The tracked document version.</param>
	/// <param name="isOpen">Whether the language server currently considers the document open.</param>
	/// <param name="openReferenceCount">The active editor-owned reference count.</param>
	/// <param name="requestReferenceCount">The active request-owned reference count.</param>
	/// <param name="lastAccessStamp">The access stamp used for request-only eviction ordering.</param>
	internal LuaDocumentState(
		string filePath,
		string uri,
		string content,
		int version,
		bool isOpen,
		int openReferenceCount,
		int requestReferenceCount,
		long lastAccessStamp)
		: base(filePath, uri, content, version, isOpen, openReferenceCount, requestReferenceCount, lastAccessStamp)
	{ }

	/// <summary>
	/// Updates the request-only eviction stamp for the tracked document.
	/// </summary>
	/// <param name="lastAccessStamp">The replacement access stamp.</param>
	internal void Touch(long lastAccessStamp)
		=> SetLastAccessStamp(lastAccessStamp);

	/// <summary>
	/// Marks the document as reopened with fresh synchronized content.
	/// </summary>
	/// <param name="content">The reopened content.</param>
	internal void Reopen(string content)
		=> ReopenDocument(content);

	/// <summary>
	/// Replaces the tracked content and advances the version.
	/// </summary>
	/// <param name="content">The replacement content.</param>
	/// <returns>The previous tracked content.</returns>
	internal string UpdateContent(string content)
		=> ReplaceContent(content);

	/// <summary>
	/// Replaces the tracked file path and URI after a rename.
	/// </summary>
	/// <param name="filePath">The normalized replacement file path.</param>
	/// <param name="uri">The replacement file URI.</param>
	internal void RenameTo(string filePath, string uri)
		=> RenameDocument(filePath, uri);

	/// <summary>
	/// Marks the tracked server document as closed while preserving cached state.
	/// </summary>
	internal void MarkClosed()
		=> MarkDocumentClosed();

	/// <summary>
	/// Gets the cached diagnostics for the tracked document.
	/// </summary>
	internal LuaDiagnosticsCache DiagnosticsCache { get; } = new();

	/// <summary>
	/// Gets the cached semantic token state for the tracked document.
	/// </summary>
	internal LuaSemanticTokensCache SemanticTokensCache { get; } = new();
}
