#nullable enable

namespace TombIDE.ScriptingStudio.Services.LuaIntellisense;

/// <summary>
/// Lightweight value-type carrier for an in-flight LuaLS document synchronization request. Returned
/// from <see cref="LuaIntellisenseDocumentManager.Synchronize"/> as a nullable so the per-keystroke
/// path does not allocate a heap instance for every character.
/// </summary>
internal readonly record struct LuaDocumentSynchronizationRequest(
	LuaDocumentSynchronizationKind Kind,
	LuaDocumentSnapshot Document,
	LuaDocumentChangeRange? ChangeRange = null);

/// <summary>
/// Describes a single incremental `textDocument/didChange` range edit computed from the difference
/// between the previously-synced and newly-synced document contents.
/// </summary>
internal readonly record struct LuaDocumentChangeRange(
	int StartLine,
	int StartCharacter,
	int EndLine,
	int EndCharacter,
	string Text);
