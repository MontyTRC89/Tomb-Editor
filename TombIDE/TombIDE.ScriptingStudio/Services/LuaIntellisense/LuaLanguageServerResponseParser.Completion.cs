#nullable enable

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TombLib.Scripting.Lua.Objects;

namespace TombIDE.ScriptingStudio.Services.LuaIntellisense;

internal static partial class LuaLanguageServerResponseParser
{
	// Lua identifiers are case-sensitive, so `Player` and `player` must be reported as distinct
	// completion items. Use ordinal (case-sensitive) comparison everywhere completion identity
	// is computed; using OrdinalIgnoreCase here would silently hide legitimate symbols.
	private sealed class CompletionIdentityComparer : IEqualityComparer<(string Label, string InsertText)>
	{
		public static CompletionIdentityComparer Instance { get; } = new();

		public bool Equals((string Label, string InsertText) x, (string Label, string InsertText) y)
			=> StringComparer.Ordinal.Equals(x.Label, y.Label)
				&& StringComparer.Ordinal.Equals(x.InsertText, y.InsertText);

		public int GetHashCode((string Label, string InsertText) value)
			=> HashCode.Combine(
				StringComparer.Ordinal.GetHashCode(value.Label),
				StringComparer.Ordinal.GetHashCode(value.InsertText));
	}

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

	private readonly struct CompletionTextAnalysis
	{
		public CompletionTextAnalysis(string? detail, string? description)
		{
			HasLocalScope = ContainsToken(detail, "local")
				|| ContainsToken(description, "local");

			HasUpvalueOrParameter = ContainsToken(detail, "upvalue")
				|| ContainsToken(description, "upvalue")
				|| ContainsToken(detail, "parameter")
				|| ContainsToken(description, "parameter");

			IconKindOverride = ResolveCompletionIconKind(detail);
		}

		public bool HasLocalScope { get; }
		public bool HasUpvalueOrParameter { get; }
		public LuaCompletionIconKind? IconKindOverride { get; }
	}

	/// <summary>
	/// Extracts the completion-item array from either an LSP completion list or a plain array response.
	/// </summary>
	/// <param name="response">The raw completion response payload.</param>
	/// <returns>The cloned completion-item elements.</returns>
	public static IReadOnlyList<JsonElement> ExtractCompletionItems(JsonElement response)
	{
		JsonElement itemsElement = response;

		if (response.ValueKind == JsonValueKind.Object && response.TryGetProperty("items", out JsonElement completionItemsElement))
			itemsElement = completionItemsElement;

		if (itemsElement.ValueKind != JsonValueKind.Array)
			return [];

		var itemElements = new List<JsonElement>(itemsElement.GetArrayLength());

		foreach (JsonElement itemElement in itemsElement.EnumerateArray())
			itemElements.Add(itemElement.Clone());

		return itemElements;
	}

	/// <summary>
	/// Parses a sequence of raw completion items into editor completion entries.
	/// </summary>
	/// <param name="itemElements">The raw completion-item payloads.</param>
	/// <param name="resolveFactory">Builds an optional lazy-resolve callback for each item.</param>
	/// <returns>The parsed completion items.</returns>
	public static IReadOnlyList<LuaCompletionItem> ParseCompletionItems(IEnumerable<JsonElement> itemElements,
		Func<LuaCompletionItem, JsonElement, int, Func<CancellationToken, Task<LuaCompletionItem>>?>? resolveFactory = null)
	{
		var items = new List<LuaCompletionItem>();
		var seenItems = new HashSet<(string Label, string InsertText)>(CompletionIdentityComparer.Instance);
		int itemIndex = 0;

		foreach (JsonElement itemElement in itemElements)
		{
			LuaCompletionItem? item = ParseCompletionItem(itemElement, itemIndex);
			itemIndex++;

			if (item is null)
				continue;

			if (resolveFactory is not null && CompletionItemNeedsResolve(item))
			{
				Func<CancellationToken, Task<LuaCompletionItem>>? resolveAsync = resolveFactory(item, itemElement, itemIndex - 1);

				if (resolveAsync is not null)
					item = item.WithResolveCallback(resolveAsync);
			}

			if (seenItems.Add((item.Label, item.InsertText)))
				items.Add(item);
		}

		return items;
	}

	/// <summary>
	/// Parses a single raw LSP completion item into a <see cref="LuaCompletionItem"/>.
	/// </summary>
	/// <param name="itemElement">The raw completion-item payload.</param>
	/// <param name="itemIndex">The zero-based response index used for priority weighting.</param>
	/// <param name="resolveAsync">An optional lazy-resolve callback.</param>
	/// <returns>The parsed completion item, or <see langword="null"/> when the payload is invalid.</returns>
	public static LuaCompletionItem? ParseCompletionItem(JsonElement itemElement, int itemIndex,
		Func<CancellationToken, Task<LuaCompletionItem>>? resolveAsync = null)
	{
		if (!itemElement.TryGetProperty("label", out JsonElement labelElement))
			return null;

		string? label = labelElement.GetString();

		if (string.IsNullOrWhiteSpace(label))
			return null;

		LuaCompletionTextEdit? textEdit = ExtractCompletionTextEdit(itemElement, out string? textEditText);

		string insertText = textEditText ?? string.Empty;

		if (string.IsNullOrWhiteSpace(insertText))
		{
			insertText = itemElement.TryGetProperty("insertText", out JsonElement insertTextElement)
				? insertTextElement.GetString() ?? label
				: label;
		}

		int? insertCaretOffset = null;

		if (itemElement.TryGetProperty("insertTextFormat", out JsonElement insertTextFormatElement)
			&& insertTextFormatElement.TryGetInt32(out int insertTextFormat) && insertTextFormat == 2)
		{
			(insertText, insertCaretOffset) = StripSnippetPlaceholders(insertText);
		}

		string filterText = itemElement.TryGetProperty("filterText", out JsonElement filterTextElement)
			? filterTextElement.GetString() ?? label
			: label;

		LuaLanguageServerCompletionKind completionKind = TryReadCompletionKind(itemElement, out LuaLanguageServerCompletionKind parsedCompletionKind)
			? parsedCompletionKind
			: LuaLanguageServerCompletionKind.Text;

		string? detail = BuildCompletionDetail(itemElement);
		MarkupContent description = BuildCompletionDescription(itemElement);
		string? searchableDescription = NormalizeMarkupText(description.Text);
		var textAnalysis = new CompletionTextAnalysis(detail, searchableDescription);

		return new LuaCompletionItem(
			label,
			insertText,
			detail,
			description.Text,
			filterText,
			BuildCompletionPriority(itemElement, textAnalysis, itemIndex),
			BuildCompletionIconKind(completionKind, textAnalysis),
			description.IsMarkdown,
			resolveAsync,
			textEdit,
			insertCaretOffset: insertCaretOffset);
	}

	private static LuaCompletionTextEdit? ExtractCompletionTextEdit(JsonElement itemElement, out string? textEditText)
	{
		textEditText = null;

		if (!itemElement.TryGetProperty("textEdit", out JsonElement textEditElement)
			|| textEditElement.ValueKind != JsonValueKind.Object)
		{
			return null;
		}

		textEditText = textEditElement.TryGetProperty("newText", out JsonElement newTextElement)
			? newTextElement.GetString()
			: null;

		return ParseCompletionTextEdit(textEditElement);
	}

	private static LuaCompletionTextEdit? ParseCompletionTextEdit(JsonElement textEditElement)
	{
		if (textEditElement.ValueKind != JsonValueKind.Object)
			return null;

		if (textEditElement.TryGetProperty("range", out JsonElement rangeElement)
			&& TryParseCompletionRange(rangeElement, out LuaCompletionRange range))
		{
			return new LuaCompletionTextEdit(range);
		}

		if (textEditElement.TryGetProperty("insert", out JsonElement insertRangeElement)
			&& textEditElement.TryGetProperty("replace", out JsonElement replaceRangeElement)
			&& TryParseCompletionRange(insertRangeElement, out LuaCompletionRange insertRange)
			&& TryParseCompletionRange(replaceRangeElement, out LuaCompletionRange replaceRange))
		{
			return new LuaCompletionTextEdit(insertRange, replaceRange);
		}

		return null;
	}

	private static bool TryParseCompletionRange(JsonElement rangeElement, out LuaCompletionRange range)
	{
		range = default;

		if (!TryParseCompletionPosition(rangeElement, "start", out LuaCompletionPosition start)
			|| !TryParseCompletionPosition(rangeElement, "end", out LuaCompletionPosition end))
		{
			return false;
		}

		range = new LuaCompletionRange(start, end);
		return true;
	}

	private static bool TryParseCompletionPosition(JsonElement parentElement, string propertyName, out LuaCompletionPosition position)
	{
		position = default;

		if (!parentElement.TryGetProperty(propertyName, out JsonElement positionElement)
			|| positionElement.ValueKind != JsonValueKind.Object)
		{
			return false;
		}

		if (!positionElement.TryGetProperty("line", out JsonElement lineElement)
			|| !lineElement.TryGetInt32(out int line)
			|| !positionElement.TryGetProperty("character", out JsonElement characterElement)
			|| !characterElement.TryGetInt32(out int character)
			|| line < 0
			|| character < 0)
		{
			return false;
		}

		position = new LuaCompletionPosition(line, character);
		return true;
	}

	private static class CompletionPriorityWeights
	{
		public const double PreselectedBonus = 1000000.0;
		public const double ResponseOrderWeight = 100000.0;
		public const double LocalScope = 20000.0;
		public const double UpvalueOrParameter = 15000.0;
		public const double VariableKind = 10000.0;
		public const double FieldOrPropertyKind = 9000.0;
		public const double MethodOrFunctionKind = 7000.0;
		public const double KeywordKindPenalty = -5000.0;
	}

	private static bool CompletionItemNeedsResolve(LuaCompletionItem item)
		=> string.IsNullOrEmpty(item.Detail) || string.IsNullOrEmpty(item.Description);

	private static double BuildCompletionPriority(JsonElement itemElement, CompletionTextAnalysis textAnalysis, int itemIndex)
	{
		double priority = CompletionPriorityWeights.ResponseOrderWeight - itemIndex;

		if (itemElement.TryGetProperty("preselect", out JsonElement preselectElement)
			&& preselectElement.ValueKind == JsonValueKind.True)
		{
			priority += CompletionPriorityWeights.PreselectedBonus;
		}

		if (itemElement.TryGetProperty("kind", out JsonElement kindElement)
			&& kindElement.TryGetInt32(out int completionKind))
		{
			priority += completionKind switch
			{
				(int)LuaLanguageServerCompletionKind.Variable => CompletionPriorityWeights.VariableKind,
				(int)LuaLanguageServerCompletionKind.Field => CompletionPriorityWeights.FieldOrPropertyKind,
				(int)LuaLanguageServerCompletionKind.Property => CompletionPriorityWeights.FieldOrPropertyKind,
				(int)LuaLanguageServerCompletionKind.Method => CompletionPriorityWeights.MethodOrFunctionKind,
				(int)LuaLanguageServerCompletionKind.Function => CompletionPriorityWeights.MethodOrFunctionKind,
				(int)LuaLanguageServerCompletionKind.Keyword => CompletionPriorityWeights.KeywordKindPenalty,
				_ => 0.0
			};
		}

		if (textAnalysis.HasLocalScope)
			priority += CompletionPriorityWeights.LocalScope;

		if (textAnalysis.HasUpvalueOrParameter)
			priority += CompletionPriorityWeights.UpvalueOrParameter;

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

	private static LuaCompletionIconKind BuildCompletionIconKind(LuaLanguageServerCompletionKind kind, CompletionTextAnalysis textAnalysis)
	{
		if (textAnalysis.IconKindOverride is LuaCompletionIconKind iconKindOverride)
			return iconKindOverride;

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

	private static bool ContainsToken(string? text, string token)
		=> !string.IsNullOrEmpty(text) && text.Contains(token, StringComparison.OrdinalIgnoreCase);

	private static LuaCompletionIconKind? ResolveCompletionIconKind(string? detailText)
	{
		if (ContainsToken(detailText, "parameter"))
			return LuaCompletionIconKind.Parameter;

		if (ContainsToken(detailText, "module") || ContainsToken(detailText, "namespace"))
			return LuaCompletionIconKind.Namespace;

		if (ContainsToken(detailText, "method") || ContainsToken(detailText, "function"))
			return LuaCompletionIconKind.Method;

		if (ContainsToken(detailText, "field"))
			return LuaCompletionIconKind.Field;

		if (ContainsToken(detailText, "property") || ContainsToken(detailText, "global")
			|| ContainsToken(detailText, "default library"))
		{
			return LuaCompletionIconKind.Property;
		}

		if (ContainsToken(detailText, "constant"))
			return LuaCompletionIconKind.Constant;

		if (ContainsToken(detailText, "keyword"))
			return LuaCompletionIconKind.Keyword;

		if (ContainsToken(detailText, "class") || ContainsToken(detailText, "interface")
			|| ContainsToken(detailText, "enum") || ContainsToken(detailText, "struct"))
		{
			return LuaCompletionIconKind.Class;
		}

		return null;
	}

	private static string? BuildCompletionDetail(JsonElement itemElement)
	{
		if (!itemElement.TryGetProperty("detail", out JsonElement detailElement))
			return null;

		string? detail = detailElement.GetString();
		return string.IsNullOrWhiteSpace(detail) ? null : detail.Trim();
	}

	private static MarkupContent BuildCompletionDescription(JsonElement itemElement)
	{
		if (!itemElement.TryGetProperty("documentation", out JsonElement documentationElement))
			return default;

		MarkupContent documentation = ExtractMarkupContent(documentationElement);

		if (string.IsNullOrWhiteSpace(documentation.Text))
			return default;

		string? normalizedText = documentation.IsMarkdown
			? NormalizeMarkdownText(documentation.Text)
			: NormalizeMarkupText(documentation.Text);

		return string.IsNullOrWhiteSpace(normalizedText)
			? default
			: new MarkupContent(normalizedText, documentation.IsMarkdown);
	}

	private static (string Text, int? CaretOffset) StripSnippetPlaceholders(string snippet)
	{
		if (string.IsNullOrWhiteSpace(snippet))
			return (snippet, null);

		var builder = new StringBuilder(snippet.Length);
		int? caretOffset = null;
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
						ReadOnlySpan<char> placeholderNumber = separatorIndex >= 0
							? placeholder.AsSpan(0, separatorIndex)
							: placeholder.AsSpan();

						if (int.TryParse(placeholderNumber, out int placeholderIndex))
						{
							if (separatorIndex >= 0 && separatorIndex < placeholder.Length - 1)
								builder.Append(placeholder[(separatorIndex + 1)..]);

							if (placeholderIndex == 0)
								caretOffset ??= builder.Length;

							index = endIndex + 1;
							continue;
						}

						builder.Append(snippet, index, endIndex - index + 1);
						index = endIndex + 1;
						continue;
					}
				}

				index++;
				int placeholderStart = index;

				while (index < snippet.Length && char.IsDigit(snippet[index]))
					index++;

				if (placeholderStart < index)
				{
					if (index - placeholderStart == 1 && snippet[placeholderStart] == '0')
						caretOffset ??= builder.Length;

					continue;
				}

				builder.Append('$');
				continue;
			}

			builder.Append(snippet[index]);
			index++;
		}

		return (builder.ToString(), caretOffset);
	}
}
