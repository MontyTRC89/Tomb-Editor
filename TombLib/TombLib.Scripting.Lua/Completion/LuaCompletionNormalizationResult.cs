namespace TombLib.Scripting.Lua.Completion;

/// <summary>
/// Represents the normalized completion text and its optional caret placement.
/// </summary>
internal readonly record struct LuaCompletionNormalizationResult(string Text, int? CaretOffset);
