using TombLib.Scripting.Lua.Objects;

namespace TombLib.LanguageServer.Lua;

/// <summary>
/// Normalizes the visible insert and replace ranges that participate in completion-item duplicate detection.
/// </summary>
/// <param name="InsertStart">The start position of the insert range.</param>
/// <param name="InsertEnd">The end position of the insert range.</param>
/// <param name="ReplaceStart">The start position of the replace range, when present.</param>
/// <param name="ReplaceEnd">The end position of the replace range, when present.</param>
internal readonly record struct LuaCompletionTextEditIdentity(
	LuaCompletionPosition? InsertStart,
	LuaCompletionPosition? InsertEnd,
	LuaCompletionPosition? ReplaceStart,
	LuaCompletionPosition? ReplaceEnd)
{
	/// <summary>
	/// Creates a normalized text-edit identity from a parsed completion text edit.
	/// </summary>
	/// <param name="textEdit">The parsed completion text edit.</param>
	/// <returns>The normalized identity.</returns>
	internal static LuaCompletionTextEditIdentity Create(LuaCompletionTextEdit? textEdit)
		=> textEdit is not { } value
			? default
			: new(
				value.InsertRange.Start,
				value.InsertRange.End,
				value.ReplaceRange?.Start,
				value.ReplaceRange?.End);
}
