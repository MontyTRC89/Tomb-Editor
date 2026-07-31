using TombLib.Scripting.Completion;

namespace TombLib.LanguageServer.Lua;

/// <summary>
/// Captures the completion fields that define whether two parsed Lua completion items should be treated as duplicates.
/// </summary>
/// <param name="Label">The completion label shown to the user.</param>
/// <param name="InsertText">The text inserted when the completion is accepted.</param>
/// <param name="FilterText">The filter text used during completion matching.</param>
/// <param name="Detail">The normalized detail text.</param>
/// <param name="Description">The normalized description text.</param>
/// <param name="Kind">The resolved completion icon kind.</param>
/// <param name="TextEdit">The normalized text-edit identity.</param>
internal readonly record struct LuaCompletionItemIdentity(
	string Label,
	string InsertText,
	string FilterText,
	string Detail,
	string Description,
	TextCompletionItemKind Kind,
	LuaCompletionTextEditIdentity TextEdit)
{
	/// <summary>
	/// Creates a duplicate-detection identity from a parsed completion item.
	/// </summary>
	/// <param name="item">The parsed completion item.</param>
	/// <returns>The normalized identity.</returns>
	internal static LuaCompletionItemIdentity Create(TextCompletionItem item) => new(
		item.Label,
		item.InsertText,
		item.FilterText,
		item.Detail ?? string.Empty,
		item.Description ?? string.Empty,
		item.Kind,
		LuaCompletionTextEditIdentity.Create(item.TextEdit));
}
