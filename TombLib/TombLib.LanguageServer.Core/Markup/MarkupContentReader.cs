using System.Text.Json;

namespace TombLib.LanguageServer.Core;

/// <summary>
/// Extracts typed markup content from standard LSP hover, completion, and signature-help payload shapes.
/// </summary>
public static class MarkupContentReader
{
	/// <summary>
	/// Extracts typed markup content from a JsonElement representing an LSP markup payload.
	/// </summary>
	/// <param name="element">The protocol markup payload to interpret.</param>
	/// <returns>The extracted markup content.</returns>
	public static MarkupContent ExtractContent(JsonElement element) => element.ValueKind switch
	{
		JsonValueKind.String => new MarkupContent(element.GetString(), true),
		JsonValueKind.Array => CombineArrayMarkupContent(element),

		JsonValueKind.Object when TryGetStringProperty(element, "value", out string? value)
			&& TryGetStringProperty(element, "kind", out string? kind)
				=> new MarkupContent(value,
					string.Equals(kind, "markdown", StringComparison.OrdinalIgnoreCase)),

		JsonValueKind.Object when TryGetStringProperty(element, "language", out string? language)
			&& TryGetStringProperty(element, "value", out string? codeValue)
				=> new MarkupContent(BuildFencedCodeBlock(language, codeValue), true),

		JsonValueKind.Object when TryGetStringProperty(element, "value", out string? plainValue)
			=> new MarkupContent(plainValue, false),

		_ => default
	};

	/// <summary>
	/// Normalizes Markdown text by standardizing line endings while preserving surrounding whitespace.
	/// </summary>
	/// <param name="text">The Markdown text to normalize.</param>
	/// <returns>The normalized Markdown text, or <see langword="null"/> when the input is blank.</returns>
	public static string? NormalizeMarkdownText(string? text)
	{
		return string.IsNullOrWhiteSpace(text)
			? null
			: text.Replace("\r\n", "\n", StringComparison.Ordinal)
				.Replace('\r', '\n');
	}

	/// <summary>
	/// Extracts and joins markup fragments from an LSP markup-content array.
	/// </summary>
	/// <param name="arrayElement">The array payload to combine.</param>
	/// <returns>The combined markup content.</returns>
	private static MarkupContent CombineArrayMarkupContent(JsonElement arrayElement)
	{
		bool isMarkdown = false;
		List<string>? parts = null;

		foreach (JsonElement child in arrayElement.EnumerateArray())
		{
			MarkupContent item = ExtractContent(child);

			if (string.IsNullOrWhiteSpace(item.Text))
				continue;

			parts ??= [];
			parts.Add(TrimLineBoundaryPadding(item.Text));

			isMarkdown |= item.IsMarkdown;
		}

		return parts is null
			? default
			: new MarkupContent(string.Join(Environment.NewLine + Environment.NewLine, parts), isMarkdown);
	}

	/// <summary>
	/// Builds a fenced Markdown code block that remains valid even when the payload already contains backticks.
	/// </summary>
	/// <param name="language">The optional code language identifier.</param>
	/// <param name="code">The code payload.</param>
	/// <returns>The fenced Markdown code block.</returns>
	private static string BuildFencedCodeBlock(string? language, string? code)
	{
		string codeText = code ?? string.Empty;
		int longestFenceRun = GetLongestBacktickRun(codeText);
		string fence = new('`', Math.Max(3, longestFenceRun + 1));

		return string.IsNullOrWhiteSpace(language)
			? $"{fence}\n{codeText}\n{fence}"
			: $"{fence}{language}\n{codeText}\n{fence}";
	}

	/// <summary>
	/// Trims only line-boundary padding while preserving meaningful leading and trailing spaces inside Markdown lines.
	/// </summary>
	/// <param name="text">The text fragment to trim.</param>
	/// <returns>The fragment without surrounding CR/LF padding.</returns>
	private static string TrimLineBoundaryPadding(string text)
		=> text.Trim('\r', '\n');

	/// <summary>
	/// Finds the longest contiguous run of backticks in the supplied text.
	/// </summary>
	/// <param name="text">The text to inspect.</param>
	/// <returns>The length of the longest backtick run.</returns>
	private static int GetLongestBacktickRun(string text)
	{
		int longestRun = 0;
		int currentRun = 0;

		for (int i = 0; i < text.Length; i++)
		{
			if (text[i] == '`')
			{
				currentRun++;
				longestRun = Math.Max(longestRun, currentRun);
			}
			else
			{
				currentRun = 0;
			}
		}

		return longestRun;
	}

	/// <summary>
	/// Reads one string property while tolerating <see langword="null"/> values and rejecting non-string payloads.
	/// </summary>
	/// <param name="element">The JSON object to inspect.</param>
	/// <param name="propertyName">The property name to read.</param>
	/// <param name="value">Receives the string value when present.</param>
	/// <returns><see langword="true"/> when the property is present and either a string or <see langword="null"/>.</returns>
	private static bool TryGetStringProperty(JsonElement element, string propertyName, out string? value)
	{
		value = null;

		if (!element.TryGetProperty(propertyName, out JsonElement propertyElement))
			return false;

		if (propertyElement.ValueKind is JsonValueKind.Null)
			return true;

		if (propertyElement.ValueKind is not JsonValueKind.String)
			return false;

		value = propertyElement.GetString();
		return true;
	}
}
