#nullable enable

namespace TombIDE.ScriptingStudio.Services.LuaIntellisense;

/// <summary>
/// Identifies the LSP document-synchronization action that should be sent for a tracked file.
/// </summary>
internal enum LuaDocumentSynchronizationKind
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
