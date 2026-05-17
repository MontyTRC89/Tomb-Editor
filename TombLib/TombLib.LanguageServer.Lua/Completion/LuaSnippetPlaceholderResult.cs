namespace TombLib.LanguageServer.Lua;

/// <summary>
/// Represents snippet text after placeholder stripping and the resolved caret placement.
/// </summary>
public readonly record struct LuaSnippetPlaceholderResult(string Text, int? CaretOffset);
