#nullable enable

namespace TombIDE.ScriptingStudio.Services.LuaIntellisense;

/// <summary>
/// Describes the text-document synchronization mode negotiated with LuaLS.
/// </summary>
internal enum LuaTextDocumentSyncKind
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
