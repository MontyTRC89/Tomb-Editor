namespace Nickelony.LanguageServer.Core;

/// <summary>
/// Identifies the document-synchronization action that should be sent for a tracked file.
/// </summary>
public enum DocumentSynchronizationKind
{
	/// <summary>
	/// The document must be opened on the server.
	/// </summary>
	Open,

	/// <summary>
	/// The document content changed and should be updated on the server.
	/// </summary>
	Change
}
