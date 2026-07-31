using TombLib.Scripting.Completion;

namespace TombLib.LanguageServer.Lua;

/// <summary>
/// Normalizes the visible insert and replace ranges that participate in completion-item duplicate detection.
/// </summary>
/// <param name="InsertStart">The start position of the insert range.</param>
/// <param name="InsertEnd">The end position of the insert range.</param>
/// <param name="ReplaceStart">The start position of the replace range, when present.</param>
/// <param name="ReplaceEnd">The end position of the replace range, when present.</param>
internal readonly record struct LuaCompletionTextEditIdentity(
	TextCompletionPosition? InsertStart,
	TextCompletionPosition? InsertEnd,
	TextCompletionPosition? ReplaceStart,
	TextCompletionPosition? ReplaceEnd)
{
	/// <summary>
	/// Creates a normalized text-edit identity from a parsed completion text edit.
	/// </summary>
	/// <param name="textEdit">The parsed completion text edit.</param>
	/// <returns>The normalized identity.</returns>
	internal static LuaCompletionTextEditIdentity Create(TextCompletionTextEdit? textEdit)
	{
		return textEdit is not { } value
			? default
			: new(
				value.InsertRange.Start,
				value.InsertRange.End,
				value.ReplaceRange?.Start,
				value.ReplaceRange?.End);
	}
}
