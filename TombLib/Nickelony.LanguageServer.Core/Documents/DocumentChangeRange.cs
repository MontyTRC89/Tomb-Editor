namespace Nickelony.LanguageServer.Core;

/// <summary>
/// Describes a single incremental <c>textDocument/didChange</c> range edit computed from the difference
/// between the previously-synced and newly-synced document contents.
/// </summary>
/// <param name="StartLine">The zero-based start line of the replaced range.</param>
/// <param name="StartCharacter">The zero-based start character within <paramref name="StartLine"/>.</param>
/// <param name="EndLine">The zero-based end line of the replaced range.</param>
/// <param name="EndCharacter">The zero-based end character within <paramref name="EndLine"/>.</param>
/// <param name="Text">The replacement text for the changed range.</param>
public readonly record struct DocumentChangeRange(
	int StartLine,
	int StartCharacter,
	int EndLine,
	int EndCharacter,
	string Text);
