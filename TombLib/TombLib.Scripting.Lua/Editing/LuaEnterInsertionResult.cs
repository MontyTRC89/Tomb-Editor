namespace TombLib.Scripting.Lua.Editing;

/// <summary>
/// Represents the text inserted for Enter along with caret and whitespace removal metadata.
/// </summary>
internal readonly record struct LuaEnterInsertionResult(string Text, int CaretOffset, int RemoveFollowingWhitespaceLength);