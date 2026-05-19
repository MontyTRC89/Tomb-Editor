using System.Text.Json;

namespace TombLib.LanguageServer.Lua;

/// <summary>
/// Normalizes Lua language-server markup payloads into editor-friendly plain text or markdown.
/// </summary>
internal static class LuaMarkupTextHelper
{
	/// <summary>
	/// Extracts and normalizes a markup payload represented as a raw JSON element.
	/// </summary>
	/// <param name="element">The raw markup payload.</param>
	/// <returns>The normalized text, or <see langword="null"/> when the payload is empty.</returns>
	public static string? ExtractMarkupText(JsonElement element)
		=> NormalizeMarkupText(MarkupContentReader.ExtractContent(element).Text);

	/// <summary>
	/// Normalizes plain or markdown-like text emitted by LuaLS into the editor's display format.
	/// </summary>
	/// <param name="text">The text to normalize.</param>
	/// <returns>The normalized text, or <see langword="null"/> when the input is blank.</returns>
	public static string? NormalizeMarkupText(string? text)
	{
		if (string.IsNullOrWhiteSpace(text))
			return null;

		string normalized = text
			.Replace("```lua", string.Empty, StringComparison.OrdinalIgnoreCase)
			.Replace("```", string.Empty, StringComparison.Ordinal)
			.Replace("`", string.Empty, StringComparison.Ordinal)
			.Replace("\r", string.Empty, StringComparison.Ordinal)
			.Trim();

		string[] lines = [.. normalized
			.Split('\n')
			.Select(line => line.TrimEnd())];

		return string.Join(Environment.NewLine, lines).Trim();
	}

	/// <summary>
	/// Normalizes markdown text while preserving markdown semantics expected by the editor.
	/// </summary>
	/// <param name="text">The markdown text to normalize.</param>
	/// <returns>The normalized markdown text.</returns>
	public static string? NormalizeMarkdownText(string? text)
		=> MarkupContentReader.NormalizeMarkdownText(text);
}
