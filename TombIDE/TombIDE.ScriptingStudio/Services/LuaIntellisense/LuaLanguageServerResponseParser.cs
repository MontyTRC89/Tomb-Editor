using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using TombLib.Scripting.Lua.Objects;

namespace TombIDE.ScriptingStudio.Services.LuaIntellisense
{
	internal static class LuaLanguageServerResponseParser
	{
		private readonly struct MarkupContent
		{
			public MarkupContent(string text, bool isMarkdown)
			{
				Text = text;
				IsMarkdown = isMarkdown;
			}

			public string Text { get; }
			public bool IsMarkdown { get; }
		}

		public static IReadOnlyList<LuaCompletionItem> ParseCompletionItems(JsonElement response)
		{
			JsonElement itemsElement = response;

			if (response.ValueKind == JsonValueKind.Object && response.TryGetProperty("items", out JsonElement completionItemsElement))
				itemsElement = completionItemsElement;

			if (itemsElement.ValueKind != JsonValueKind.Array)
				return Array.Empty<LuaCompletionItem>();

			var items = new List<LuaCompletionItem>();

			foreach (JsonElement itemElement in itemsElement.EnumerateArray())
			{
				if (!itemElement.TryGetProperty("label", out JsonElement labelElement))
					continue;

				string label = labelElement.GetString();
				string insertText = itemElement.TryGetProperty("textEdit", out JsonElement textEditElement)
					&& textEditElement.TryGetProperty("newText", out JsonElement newTextElement)
						? newTextElement.GetString()
						: itemElement.TryGetProperty("insertText", out JsonElement insertTextElement)
							? insertTextElement.GetString()
							: label;

				if (itemElement.TryGetProperty("insertTextFormat", out JsonElement insertTextFormatElement)
					&& insertTextFormatElement.TryGetInt32(out int insertTextFormat) && insertTextFormat == 2)
				{
					insertText = StripSnippetPlaceholders(insertText);
				}

				string description = BuildCompletionDescription(itemElement);
				items.Add(new LuaCompletionItem(label, insertText, description));
			}

			return items
				.GroupBy(item => $"{item.Label}\0{item.InsertText}", StringComparer.OrdinalIgnoreCase)
				.Select(group => group.First())
				.ToList();
		}

		public static LuaHoverInfo ParseHoverInfo(JsonElement response)
		{
			if (response.ValueKind != JsonValueKind.Object || !response.TryGetProperty("contents", out JsonElement contentsElement))
				return null;

			MarkupContent hoverContent = ExtractMarkupContent(contentsElement);

			return string.IsNullOrWhiteSpace(hoverContent.Text)
				? null
				: new LuaHoverInfo(hoverContent.Text.Trim(), hoverContent.IsMarkdown);
		}

		public static LuaDefinitionLocation ParseDefinitionLocation(JsonElement response)
		{
			JsonElement definitionElement = response;

			if (response.ValueKind == JsonValueKind.Array)
			{
				definitionElement = response.EnumerateArray().FirstOrDefault();

				if (definitionElement.ValueKind == JsonValueKind.Undefined)
					return null;
			}

			if (definitionElement.ValueKind != JsonValueKind.Object)
				return null;

			string uri = definitionElement.TryGetProperty("targetUri", out JsonElement targetUriElement)
				? targetUriElement.GetString()
				: definitionElement.TryGetProperty("uri", out JsonElement uriElement)
					? uriElement.GetString()
					: null;

			if (string.IsNullOrWhiteSpace(uri) || !Uri.TryCreate(uri, UriKind.Absolute, out Uri parsedUri) || !parsedUri.IsFile)
				return null;

			JsonElement startElement = definitionElement.TryGetProperty("targetSelectionRange", out JsonElement targetSelectionRangeElement)
				&& targetSelectionRangeElement.TryGetProperty("start", out JsonElement targetSelectionStartElement)
					? targetSelectionStartElement
					: definitionElement.TryGetProperty("targetRange", out JsonElement targetRangeElement)
					&& targetRangeElement.TryGetProperty("start", out JsonElement targetRangeStartElement)
						? targetRangeStartElement
						: definitionElement.TryGetProperty("range", out JsonElement rangeElement)
						&& rangeElement.TryGetProperty("start", out JsonElement rangeStartElement)
							? rangeStartElement
							: default;

			if (startElement.ValueKind != JsonValueKind.Object)
				return null;

			int lineNumber = startElement.TryGetProperty("line", out JsonElement lineElement) ? lineElement.GetInt32() + 1 : 1;
			int columnNumber = startElement.TryGetProperty("character", out JsonElement characterElement) ? characterElement.GetInt32() + 1 : 1;

			return new LuaDefinitionLocation(LuaLanguageServerPathHelper.NormalizeLocalPath(parsedUri), lineNumber, columnNumber);
		}

		public static LuaSignatureInfo ParseSignatureHelp(JsonElement response)
		{
			if (response.ValueKind != JsonValueKind.Object
				|| !response.TryGetProperty("signatures", out JsonElement signaturesElement)
				|| signaturesElement.ValueKind != JsonValueKind.Array)
			{
				return null;
			}

			int activeSignature = response.TryGetProperty("activeSignature", out JsonElement activeSignatureElement)
				&& activeSignatureElement.TryGetInt32(out int parsedActiveSignature)
					? Math.Max(0, parsedActiveSignature)
					: 0;

			int signatureIndex = 0;
			JsonElement signatureElement = default;

			foreach (JsonElement candidate in signaturesElement.EnumerateArray())
			{
				if (signatureIndex == activeSignature)
				{
					signatureElement = candidate;
					break;
				}

				signatureIndex++;
			}

			if (signatureElement.ValueKind != JsonValueKind.Object
				|| !signatureElement.TryGetProperty("label", out JsonElement labelElement))
			{
				return null;
			}

			string label = labelElement.GetString();

			if (string.IsNullOrWhiteSpace(label))
				return null;

			string documentation = signatureElement.TryGetProperty("documentation", out JsonElement documentationElement)
				? ExtractMarkupText(documentationElement)
				: null;

			int activeParameter = response.TryGetProperty("activeParameter", out JsonElement activeParameterElement)
				&& activeParameterElement.TryGetInt32(out int parsedActiveParameter)
					? Math.Max(0, parsedActiveParameter)
					: signatureElement.TryGetProperty("activeParameter", out JsonElement signatureActiveParameterElement)
					&& signatureActiveParameterElement.TryGetInt32(out int parsedSignatureActiveParameter)
						? Math.Max(0, parsedSignatureActiveParameter)
						: 0;

			var parameters = new List<LuaParameterInfo>();

			if (signatureElement.TryGetProperty("parameters", out JsonElement parametersElement)
				&& parametersElement.ValueKind == JsonValueKind.Array)
			{
				foreach (JsonElement paramElement in parametersElement.EnumerateArray())
				{
					string parameterLabel = paramElement.TryGetProperty("label", out JsonElement paramLabelElement)
						? paramLabelElement.ValueKind == JsonValueKind.String
							? paramLabelElement.GetString()
							: TryExtractParameterLabel(label, paramLabelElement, out string extractedLabel)
								? extractedLabel
								: null
						: null;

					string parameterDocumentation = paramElement.TryGetProperty("documentation", out JsonElement parameterDocumentationElement)
						? ExtractMarkupText(parameterDocumentationElement)
						: null;

					parameters.Add(new LuaParameterInfo(parameterLabel, parameterDocumentation));
				}
			}

			return new LuaSignatureInfo(label, documentation, parameters, activeParameter);
		}

		private static string BuildCompletionDescription(JsonElement itemElement)
		{
			var descriptionBuilder = new StringBuilder();

			if (itemElement.TryGetProperty("detail", out JsonElement detailElement))
			{
				string detail = detailElement.GetString();

				if (!string.IsNullOrWhiteSpace(detail))
					descriptionBuilder.AppendLine(detail.Trim());
			}

			if (itemElement.TryGetProperty("documentation", out JsonElement documentationElement))
			{
				string documentation = ExtractMarkupText(documentationElement);

				if (!string.IsNullOrWhiteSpace(documentation))
				{
					if (descriptionBuilder.Length > 0)
						descriptionBuilder.AppendLine();

					descriptionBuilder.Append(documentation.Trim());
				}
			}

			return descriptionBuilder.Length == 0 ? null : descriptionBuilder.ToString();
		}

		private static bool TryExtractParameterLabel(string signatureLabel, JsonElement parameterLabelElement, out string parameterLabel)
		{
			parameterLabel = null;

			if (string.IsNullOrEmpty(signatureLabel)
				|| parameterLabelElement.ValueKind != JsonValueKind.Array)
			{
				return false;
			}

			JsonElement.ArrayEnumerator labelParts = parameterLabelElement.EnumerateArray();

			if (!labelParts.MoveNext() || !labelParts.Current.TryGetInt32(out int startIndex))
				return false;

			if (!labelParts.MoveNext() || !labelParts.Current.TryGetInt32(out int endIndex))
				return false;

			startIndex = Math.Max(0, Math.Min(startIndex, signatureLabel.Length));
			endIndex = Math.Max(startIndex, Math.Min(endIndex, signatureLabel.Length));

			if (endIndex <= startIndex)
				return false;

			parameterLabel = signatureLabel[startIndex..endIndex];
			return true;
		}

		private static string ExtractMarkupText(JsonElement element)
			=> NormalizeMarkupText(ExtractMarkupContent(element).Text);

		private static MarkupContent ExtractMarkupContent(JsonElement element)
		{
			return element.ValueKind switch
			{
				JsonValueKind.String => new MarkupContent(element.GetString(), true),
				JsonValueKind.Array => CombineMarkupContent(element.EnumerateArray().Select(ExtractMarkupContent)),
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

		private static MarkupContent CombineMarkupContent(IEnumerable<MarkupContent> items)
		{
			bool isMarkdown = false;
			var parts = new List<string>();

			foreach (MarkupContent item in items)
			{
				if (string.IsNullOrWhiteSpace(item.Text))
					continue;

				parts.Add(item.Text.Trim());
				isMarkdown |= item.IsMarkdown;
			}

			return parts.Count == 0
				? default
				: new MarkupContent(string.Join(Environment.NewLine + Environment.NewLine, parts), isMarkdown);
		}

		private static string NormalizeMarkupText(string text)
		{
			if (string.IsNullOrWhiteSpace(text))
				return null;

			string normalized = text
				.Replace("```lua", string.Empty, StringComparison.OrdinalIgnoreCase)
				.Replace("```", string.Empty, StringComparison.Ordinal)
				.Replace("`", string.Empty, StringComparison.Ordinal)
				.Replace("\r", string.Empty, StringComparison.Ordinal)
				.Trim();

			string[] lines = normalized
				.Split('\n')
				.Select(line => line.TrimEnd())
				.ToArray();

			return string.Join(Environment.NewLine, lines).Trim();
		}

		private static string StripSnippetPlaceholders(string snippet)
		{
			if (string.IsNullOrWhiteSpace(snippet))
				return snippet;

			var builder = new StringBuilder(snippet.Length);
			int index = 0;

			while (index < snippet.Length)
			{
				if (snippet[index] == '$')
				{
					if (index + 1 < snippet.Length && snippet[index + 1] == '{')
					{
						int endIndex = snippet.IndexOf('}', index + 2);

						if (endIndex > index)
						{
							string placeholder = snippet[(index + 2)..endIndex];
							int separatorIndex = placeholder.IndexOf(':');

							if (separatorIndex >= 0 && separatorIndex < placeholder.Length - 1)
								builder.Append(placeholder[(separatorIndex + 1)..]);

							index = endIndex + 1;
							continue;
						}
					}

					index++;

					while (index < snippet.Length && char.IsDigit(snippet[index]))
						index++;

					continue;
				}

				builder.Append(snippet[index]);
				index++;
			}

			return builder.ToString();
		}
	}
}