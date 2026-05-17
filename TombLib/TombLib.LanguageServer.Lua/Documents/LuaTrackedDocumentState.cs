namespace TombLib.LanguageServer.Lua;

internal sealed class LuaTrackedDocumentState : TrackedDocumentState
{
	public LuaTrackedDocumentState(
		string filePath,
		string uri,
		string content,
		int version,
		bool isOpen,
		int openReferenceCount,
		int requestReferenceCount,
		long lastAccessStamp)
		: base(filePath, uri, content, version, isOpen, openReferenceCount, requestReferenceCount, lastAccessStamp)
	{
	}

	internal void Touch(long lastAccessStamp)
		=> SetLastAccessStamp(lastAccessStamp);

	internal void Reopen(string content)
		=> ReopenDocument(content);

	internal string UpdateContent(string content)
		=> ReplaceContent(content);

	internal void RenameTo(string filePath, string uri)
		=> RenameDocument(filePath, uri);

	internal void MarkClosed()
		=> MarkDocumentClosed();

	public LuaDocumentDiagnosticsCache DiagnosticsCache { get; } = new();

	public LuaDocumentSemanticTokensCache SemanticTokensCache { get; } = new();
}
