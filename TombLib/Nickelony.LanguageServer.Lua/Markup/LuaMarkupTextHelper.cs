using System.Text.Json;

namespace Nickelony.LanguageServer.Lua;

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
	internal static string? ExtractMarkupText(JsonElement element)
		=> NormalizeMarkupText(MarkupContentReader.ExtractContent(element).Text);

	/// <summary>
	/// Normalizes plain or markdown-like text emitted by LuaLS into the editor's display format.
	/// Standalone fenced code-block marker lines are removed, while inline backticks are preserved.
	/// </summary>
	/// <param name="text">The text to normalize.</param>
	/// <returns>The normalized text, or <see langword="null"/> when the input is blank.</returns>
	internal static string? NormalizeMarkupText(string? text)
	{
		if (string.IsNullOrWhiteSpace(text))
			return null;

		string[] lines = [.. text
			.Replace("\r", string.Empty, StringComparison.Ordinal)
			.Split('\n')
			.Select(line => line.TrimEnd())];

		var normalizedLines = new List<string>(lines.Length);

		for (int i = 0; i < lines.Length; i++)
		{
			if (IsFenceLine(lines[i]))
				continue;

			normalizedLines.Add(lines[i]);
		}

		string normalized = string.Join(Environment.NewLine, normalizedLines).Trim();
		return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
	}

	/// <summary>
	/// Normalizes markdown text while preserving markdown semantics expected by the editor.
	/// </summary>
	/// <param name="text">The markdown text to normalize.</param>
	/// <returns>The normalized markdown text.</returns>
	internal static string? NormalizeMarkdownText(string? text)
		=> MarkupContentReader.NormalizeMarkdownText(text);

	/// <summary>
	/// Reports whether one trimmed line is a standalone markdown fence marker.
	/// </summary>
	private static bool IsFenceLine(string line)
	{
		string trimmedLine = line.Trim();

		if (!trimmedLine.StartsWith("```", StringComparison.Ordinal))
			return false;

		if (trimmedLine.Length == 3)
			return true;

		for (int i = 3; i < trimmedLine.Length; i++)
		{
			char character = trimmedLine[i];

			if (!char.IsLetterOrDigit(character) && character != '_' && character != '-' && character != '.')
				return false;
		}

		return true;
	}
}
