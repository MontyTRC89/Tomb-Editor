namespace TombLib.Scripting.Completion;

/// <summary>
/// Represents a zero-based text range used by completion edit payloads.
/// </summary>
/// <remarks>
/// This range is intended for protocol-boundary completion edits and is converted to offsets
/// only when applied against a concrete editor document snapshot.
/// </remarks>
/// <param name="Start">The start position of the range.</param>
/// <param name="End">The end position of the range.</param>
public readonly record struct TextCompletionRange(TextCompletionPosition Start, TextCompletionPosition End);
