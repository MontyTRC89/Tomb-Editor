using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using TombLib.Scripting.Lua.Objects;

namespace TombLib.Scripting.Lua.LanguageServer;

/// <summary>
/// Provides shared markup and payload-conversion helpers used by the LuaLS response parsers.
/// </summary>
public static partial class LuaLanguageServerResponseParser
{
	private readonly struct MarkupContent(string? text, bool isMarkdown)
	{
		public string Text { get; } = text ?? string.Empty;
		public bool IsMarkdown { get; } = isMarkdown;
	}

	private static string? ExtractMarkupText(JsonElement element)
		=> NormalizeMarkupText(ExtractMarkupContent(element).Text);

	private static MarkupContent ExtractMarkupContent(JsonElement element)
	{
		return element.ValueKind switch
		{
			JsonValueKind.String => new MarkupContent(element.GetString(), true),
			JsonValueKind.Array => CombineArrayMarkupContent(element),
			JsonValueKind.Object when element.TryGetProperty("value", out JsonElement valueElement)
				&& element.TryGetProperty("kind", out JsonElement kindElement)
					=> new MarkupContent(valueElement.GetString(),
						string.Equals(kindElement.GetString(), "markdown", StringComparison.OrdinalIgnoreCase)),
			JsonValueKind.Object when element.TryGetProperty("language", out JsonElement languageElement)
				&& element.TryGetProperty("value", out JsonElement codeValueElement)
					=> new MarkupContent($"```{languageElement.GetString()}\n{codeValueElement.GetString()}\n```", true),
			JsonValueKind.Object when element.TryGetProperty("value", out JsonElement plainValueElement)
				=> new MarkupContent(plainValueElement.GetString(), false),
			_ => default
		};
	}

	private static MarkupContent CombineArrayMarkupContent(JsonElement arrayElement)
	{
		bool isMarkdown = false;
		List<string>? parts = null;

		foreach (JsonElement child in arrayElement.EnumerateArray())
		{
			MarkupContent item = ExtractMarkupContent(child);

			if (string.IsNullOrWhiteSpace(item.Text))
				continue;

			parts ??= [];
			parts.Add(item.Text.Trim());
			isMarkdown |= item.IsMarkdown;
		}

		return parts is null
			? default
			: new MarkupContent(string.Join(Environment.NewLine + Environment.NewLine, parts), isMarkdown);
	}

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
	{
		return string.IsNullOrWhiteSpace(text)
			? null
			: text.Replace("\r\n", "\n", StringComparison.Ordinal)
				.Replace('\r', '\n')
				.Trim();
	}

	private static bool TryParseDocumentRange(LuaProtocolRangePayload? rangePayload, [NotNullWhen(true)] out LuaDocumentRange? range)
	{
		range = null;

		if (!TryGetOneBasedLineAndColumn(rangePayload?.Start, out int startLineNumber, out int startColumnNumber)
			|| !TryGetOneBasedLineAndColumn(rangePayload?.End, out int endLineNumber, out int endColumnNumber))
		{
			return false;
		}

		range = new LuaDocumentRange(startLineNumber, startColumnNumber, endLineNumber, endColumnNumber);
		return true;
	}

	private static bool TryGetOneBasedLineAndColumn(LuaProtocolNullablePosition? position, out int lineNumber, out int columnNumber)
	{
		lineNumber = 1;
		columnNumber = 1;

		if (position is not { Line: int parsedLine, Character: int parsedCharacter })
			return false;

		lineNumber = parsedLine + 1;
		columnNumber = parsedCharacter + 1;
		return true;
	}
}
