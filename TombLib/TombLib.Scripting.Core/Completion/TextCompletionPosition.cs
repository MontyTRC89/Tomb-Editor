namespace TombLib.Scripting.Completion;

/// <summary>
/// Represents a zero-based text position used by completion edit payloads.
/// </summary>
/// <remarks>
/// This DTO mirrors external protocol coordinates such as LSP completion text edits.
/// It is not an AvalonEdit offset and should be converted at the editor boundary.
/// </remarks>
public readonly record struct TextCompletionPosition(int Line, int Character);
