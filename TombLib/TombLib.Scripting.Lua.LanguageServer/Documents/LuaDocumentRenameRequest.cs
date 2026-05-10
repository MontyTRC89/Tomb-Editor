namespace TombLib.Scripting.Lua.LanguageServer;

/// <summary>
/// Describes a tracked-document path rekey that may need to be mirrored to LuaLS as a close/open pair.
/// </summary>
public readonly record struct LuaDocumentRenameRequest(
	LuaDocumentSnapshot? PreviousDocument,
	LuaDocumentSnapshot RenamedDocument,
	bool ReopenServerDocument);
