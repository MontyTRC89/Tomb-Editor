namespace Nickelony.LanguageServer.Core;

/// <summary>
/// Describes the text-document synchronization mode negotiated with the language server.
/// </summary>
public enum TextDocumentSyncKind
{
	/// <summary>
	/// No document synchronization is supported.
	/// </summary>
	None = 0,

	/// <summary>
	/// Each change sends the full document content.
	/// </summary>
	Full = 1,

	/// <summary>
	/// Each change sends an incremental range edit.
	/// </summary>
	Incremental = 2
}
