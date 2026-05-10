namespace TombLib.Scripting.Lua.LanguageServer;

/// <summary>
/// Describes the text-document synchronization mode negotiated with LuaLS.
/// </summary>
public enum LuaTextDocumentSyncKind
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
