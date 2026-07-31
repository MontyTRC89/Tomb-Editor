namespace Nickelony.LanguageServer.Lua;

/// <summary>
/// Represents snippet text after placeholder stripping and the resolved caret placement.
/// </summary>
/// <param name="Text">The snippet text with LuaLS placeholder markers removed.</param>
/// <param name="CaretOffset">The zero-based caret offset to apply after insertion, or <see langword="null"/> when no explicit caret position exists.</param>
internal readonly record struct LuaSnippetPlaceholderResult(string Text, int? CaretOffset);
