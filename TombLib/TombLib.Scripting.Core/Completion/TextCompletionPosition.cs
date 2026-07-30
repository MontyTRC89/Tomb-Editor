namespace TombLib.Scripting.Completion;

/// <summary>
/// Represents a zero-based text position used by completion edit payloads.
/// </summary>
/// <remarks>
/// This DTO mirrors external protocol coordinates such as LSP completion text edits.
/// It is not an AvalonEdit offset and should be converted at the editor boundary.
/// </remarks>
/// <param name="Line">The zero-based line index.</param>
/// <param name="Character">The zero-based character index within the line.</param>
public readonly record struct TextCompletionPosition(int Line, int Character);
