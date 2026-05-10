namespace TombIDE.ScriptingStudio.Services.LuaIntellisense;

/// <summary>
/// Represents snippet text after placeholder stripping and the resolved caret placement.
/// </summary>
internal readonly record struct LuaSnippetPlaceholderResult(string Text, int? CaretOffset);