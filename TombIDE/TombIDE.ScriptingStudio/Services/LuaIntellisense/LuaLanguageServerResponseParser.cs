using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TombLib.Scripting.Lua.Objects;

namespace TombIDE.ScriptingStudio.Services.LuaIntellisense
{
	internal static class LuaLanguageServerResponseParser
	{
		private enum LuaLanguageServerCompletionKind
		{
			Text = 1,
			Method = 2,
			Function = 3,
			Constructor = 4,
			Field = 5,
			Variable = 6,
			Class = 7,
			Interface = 8,
			Module = 9,
			Property = 10,
			Unit = 11,
			Value = 12,
			Enum = 13,
			Keyword = 14,
			Snippet = 15,
			Color = 16,
			File = 17,
			Reference = 18,
			Folder = 19,
			EnumMember = 20,
			Constant = 21,
			Struct = 22,
			Event = 23,
			Operator = 24,
			TypeParameter = 25
		}

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
			=> ParseCompletionItems(ExtractCompletionItems(response));

		public static IReadOnlyList<JsonElement> ExtractCompletionItems(JsonElement response)
		{
			JsonElement itemsElement = response;

			if (response.ValueKind == JsonValueKind.Object && response.TryGetProperty("items", out JsonElement completionItemsElement))
				itemsElement = completionItemsElement;

			if (itemsElement.ValueKind != JsonValueKind.Array)
				return Array.Empty<JsonElement>();

			var itemElements = new List<JsonElement>();

			foreach (JsonElement itemElement in itemsElement.EnumerateArray())
				itemElements.Add(itemElement.Clone());

			return itemElements;
		}

		public static IReadOnlyList<LuaCompletionItem> ParseCompletionItems(IEnumerable<JsonElement> itemElements,
			Func<JsonElement, int, Func<CancellationToken, Task<LuaCompletionItem>>> resolveFactory = null)
		{
			var items = new List<LuaCompletionItem>();
			int itemIndex = 0;

			foreach (JsonElement itemElement in itemElements)
			{
				LuaCompletionItem item = ParseCompletionItem(itemElement, itemIndex, resolveFactory?.Invoke(itemElement, itemIndex));

				if (item is not null)
					items.Add(item);

				itemIndex++;
			}

			return items
				.GroupBy(item => $"{item.Label}\0{item.InsertText}", StringComparer.OrdinalIgnoreCase)
				.Select(group => group.First())
				.ToList();
		}

		public static bool CompletionItemNeedsResolve(JsonElement itemElement)
			=> string.IsNullOrWhiteSpace(BuildCompletionDetail(itemElement))
				|| string.IsNullOrWhiteSpace(BuildCompletionDescription(itemElement).Text);

		public static LuaCompletionItem ParseCompletionItem(JsonElement itemElement, int itemIndex,
			Func<CancellationToken, Task<LuaCompletionItem>> resolveAsync = null)
		{
			if (!itemElement.TryGetProperty("label", out JsonElement labelElement))
				return null;

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

			string filterText = itemElement.TryGetProperty("filterText", out JsonElement filterTextElement)
				? filterTextElement.GetString()
				: label;

			LuaLanguageServerCompletionKind completionKind = TryReadCompletionKind(itemElement, out LuaLanguageServerCompletionKind parsedCompletionKind)
				? parsedCompletionKind
				: LuaLanguageServerCompletionKind.Text;

			string detail = BuildCompletionDetail(itemElement);
			MarkupContent description = BuildCompletionDescription(itemElement);
			string searchableDescription = NormalizeMarkupText(description.Text);

			return new LuaCompletionItem(
				label,
				insertText,
				detail,
				description.Text,
				filterText,
				BuildCompletionPriority(itemElement, detail, searchableDescription, itemIndex),
				BuildCompletionIconKind(completionKind, detail),
				description.IsMarkdown,
				resolveAsync);
		}

		private static double BuildCompletionPriority(JsonElement itemElement, string detail, string description, int itemIndex)
		{
			const double responseOrderWeight = 100000.0;
			double priority = responseOrderWeight - itemIndex;
			string searchableText = CombineCompletionText(detail, description);

			if (itemElement.TryGetProperty("preselect", out JsonElement preselectElement)
				&& preselectElement.ValueKind == JsonValueKind.True)
			{
				priority += 1000000.0;
			}

			if (itemElement.TryGetProperty("kind", out JsonElement kindElement)
				&& kindElement.TryGetInt32(out int completionKind))
			{
				priority += completionKind switch
				{
					6 => 10000.0,
					5 => 9000.0,
					10 => 9000.0,
					2 => 7000.0,
					3 => 7000.0,
					14 => -5000.0,
					_ => 0.0
				};
			}

			if (!string.IsNullOrWhiteSpace(searchableText))
			{
				if (CompletionTextContains(searchableText, "local"))
					priority += 20000.0;

				if (CompletionTextContains(searchableText, "upvalue")
					|| CompletionTextContains(searchableText, "parameter"))
				{
					priority += 15000.0;
				}
			}

			return priority;
		}

		private static bool TryReadCompletionKind(JsonElement itemElement, out LuaLanguageServerCompletionKind kind)
		{
			kind = LuaLanguageServerCompletionKind.Text;

			if (!itemElement.TryGetProperty("kind", out JsonElement kindElement)
				|| !kindElement.TryGetInt32(out int rawKind)
				|| !Enum.IsDefined(typeof(LuaLanguageServerCompletionKind), rawKind))
			{
				return false;
			}

			kind = (LuaLanguageServerCompletionKind)rawKind;
			return true;
		}

		private static LuaCompletionIconKind BuildCompletionIconKind(LuaLanguageServerCompletionKind kind, string detail)
		{
			if (CompletionTextContains(detail, "parameter"))
				return LuaCompletionIconKind.Parameter;

			if (CompletionTextContains(detail, "module") || CompletionTextContains(detail, "namespace"))
				return LuaCompletionIconKind.Namespace;

			if (CompletionTextContains(detail, "method") || CompletionTextContains(detail, "function"))
				return LuaCompletionIconKind.Method;

			if (CompletionTextContains(detail, "field"))
				return LuaCompletionIconKind.Field;

			if (CompletionTextContains(detail, "property") || CompletionTextContains(detail, "global")
				|| CompletionTextContains(detail, "default library"))
			{
				return LuaCompletionIconKind.Property;
			}

			if (CompletionTextContains(detail, "constant"))
				return LuaCompletionIconKind.Constant;

			if (CompletionTextContains(detail, "keyword"))
				return LuaCompletionIconKind.Keyword;

			if (CompletionTextContains(detail, "class") || CompletionTextContains(detail, "interface")
				|| CompletionTextContains(detail, "enum") || CompletionTextContains(detail, "struct"))
			{
				return LuaCompletionIconKind.Class;
			}

			return kind switch
			{
				LuaLanguageServerCompletionKind.Method => LuaCompletionIconKind.Method,
				LuaLanguageServerCompletionKind.Function => LuaCompletionIconKind.Method,
				LuaLanguageServerCompletionKind.Constructor => LuaCompletionIconKind.Method,
				LuaLanguageServerCompletionKind.Field => LuaCompletionIconKind.Field,
				LuaLanguageServerCompletionKind.Variable => LuaCompletionIconKind.Variable,
				LuaLanguageServerCompletionKind.Class => LuaCompletionIconKind.Class,
				LuaLanguageServerCompletionKind.Interface => LuaCompletionIconKind.Class,
				LuaLanguageServerCompletionKind.Module => LuaCompletionIconKind.Namespace,
				LuaLanguageServerCompletionKind.Property => LuaCompletionIconKind.Property,
				LuaLanguageServerCompletionKind.Value => LuaCompletionIconKind.Variable,
				LuaLanguageServerCompletionKind.Enum => LuaCompletionIconKind.Class,
				LuaLanguageServerCompletionKind.Keyword => LuaCompletionIconKind.Keyword,
				LuaLanguageServerCompletionKind.Snippet => LuaCompletionIconKind.Keyword,
				LuaLanguageServerCompletionKind.File => LuaCompletionIconKind.File,
				LuaLanguageServerCompletionKind.Reference => LuaCompletionIconKind.Variable,
				LuaLanguageServerCompletionKind.Folder => LuaCompletionIconKind.Folder,
				LuaLanguageServerCompletionKind.EnumMember => LuaCompletionIconKind.Constant,
				LuaLanguageServerCompletionKind.Constant => LuaCompletionIconKind.Constant,
				LuaLanguageServerCompletionKind.Struct => LuaCompletionIconKind.Class,
				LuaLanguageServerCompletionKind.Event => LuaCompletionIconKind.Method,
				LuaLanguageServerCompletionKind.Operator => LuaCompletionIconKind.Keyword,
				LuaLanguageServerCompletionKind.TypeParameter => LuaCompletionIconKind.Class,
				_ => LuaCompletionIconKind.Misc
			};
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

		private static string BuildCompletionDetail(JsonElement itemElement)
		{
			if (!itemElement.TryGetProperty("detail", out JsonElement detailElement))
				return null;

			string detail = detailElement.GetString();
			return string.IsNullOrWhiteSpace(detail) ? null : detail.Trim();
		}

		private static MarkupContent BuildCompletionDescription(JsonElement itemElement)
		{
			if (!itemElement.TryGetProperty("documentation", out JsonElement documentationElement))
				return default;

			MarkupContent documentation = ExtractMarkupContent(documentationElement);

			if (string.IsNullOrWhiteSpace(documentation.Text))
				return default;

			string normalizedText = documentation.IsMarkdown
				? NormalizeMarkdownText(documentation.Text)
				: NormalizeMarkupText(documentation.Text);

			return string.IsNullOrWhiteSpace(normalizedText)
				? default
				: new MarkupContent(normalizedText, documentation.IsMarkdown);
		}

		private static string CombineCompletionText(string detail, string description)
		{
			if (string.IsNullOrWhiteSpace(detail))
				return description;

			if (string.IsNullOrWhiteSpace(description))
				return detail;

			return detail + Environment.NewLine + description;
		}

		private static bool CompletionTextContains(string text, string token)
			=> !string.IsNullOrWhiteSpace(text) && text.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0;

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

		private static string NormalizeMarkdownText(string text)
			=> string.IsNullOrWhiteSpace(text)
				? null
				: text.Replace("\r\n", "\n", StringComparison.Ordinal)
					.Replace('\r', '\n')
					.Trim();

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