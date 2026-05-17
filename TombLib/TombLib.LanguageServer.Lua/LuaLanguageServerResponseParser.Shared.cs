using System.Text.Json;

namespace TombLib.LanguageServer.Lua;

/// <summary>
/// Provides shared markup and payload-conversion helpers used by the LuaLS response parsers.
/// </summary>
public static partial class LuaLanguageServerResponseParser
{
	private static string? ExtractMarkupText(JsonElement element)
		=> NormalizeMarkupText(MarkupContentReader.ExtractContent(element).Text);

	private static string? NormalizeMarkupText(string? text)
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

	private static string? NormalizeMarkdownText(string? text)
		=> MarkupContentReader.NormalizeMarkdownText(text);
}
