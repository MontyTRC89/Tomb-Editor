#nullable enable

namespace TombIDE.ScriptingStudio.Services.LuaIntellisense;

/// <summary>
/// Describes a tracked-document path rekey that may need to be mirrored to LuaLS as a close/open pair.
/// </summary>
internal readonly record struct LuaDocumentRenameRequest(
	LuaDocumentSnapshot? PreviousDocument,
	LuaDocumentSnapshot RenamedDocument,
	bool ReopenServerDocument);