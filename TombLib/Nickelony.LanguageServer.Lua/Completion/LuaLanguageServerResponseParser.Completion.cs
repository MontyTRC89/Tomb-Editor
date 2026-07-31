using System.Text;
using System.Text.Json;
using Nickelony.LanguageServer.Core.Completion;

namespace Nickelony.LanguageServer.Lua;

/// <summary>
/// Parses typed Lua language-server responses into shared editor-facing models (completion items, hover info,
/// definition locations, reference locations, workspace edits, signature help, and formatting edits).
/// </summary>
internal static partial class LuaLanguageServerResponseParser
{
	/// <summary>
	/// Parses a sequence of typed completion-item payloads into editor completion entries.
	/// </summary>
	/// <param name="itemPayloads">The typed completion-item payloads.</param>
	/// <param name="resolveFactory">Builds an optional lazy-resolve callback for each item.</param>
	/// <returns>The parsed completion items.</returns>
	internal static IReadOnlyList<TextCompletionItem> ParseCompletionItems(IEnumerable<CompletionItemPayload> itemPayloads,
		Func<TextCompletionItem, CompletionItemPayload, int, Func<CancellationToken, Task<TextCompletionItem>>?>? resolveFactory = null)
	{
		var items = new List<TextCompletionItem>();
		var seenItems = new HashSet<LuaCompletionItemIdentity>(LuaCompletionItemIdentityComparer.Instance);
		int itemIndex = 0;

		foreach (CompletionItemPayload itemPayload in itemPayloads)
		{
			TextCompletionItem? item = ParseCompletionItem(itemPayload, itemIndex);
			itemIndex++;

			if (item is null)
				continue;

			if (resolveFactory is not null && CompletionItemNeedsResolve(item))
			{
				Func<CancellationToken, Task<TextCompletionItem>>? resolveAsync = resolveFactory(item, itemPayload, itemIndex - 1);

				if (resolveAsync is not null)
					item = item.WithResolveCallback(resolveAsync);
			}

			if (seenItems.Add(LuaCompletionItemIdentity.Create(item)))
				items.Add(item);
		}

		return items;
	}

	/// <summary>
	/// Parses a single typed LSP completion-item payload into a <see cref="TextCompletionItem"/>.
	/// </summary>
	/// <param name="itemPayload">The typed completion-item payload.</param>
	/// <param name="itemIndex">The zero-based response index used for priority weighting.</param>
	/// <param name="resolveAsync">An optional lazy-resolve callback.</param>
	/// <returns>The parsed completion item, or <see langword="null"/> when the payload is invalid.</returns>
	internal static TextCompletionItem? ParseCompletionItem(CompletionItemPayload itemPayload, int itemIndex,
		Func<CancellationToken, Task<TextCompletionItem>>? resolveAsync = null)
	{
		string? label = itemPayload.Label;

		if (string.IsNullOrWhiteSpace(label))
			return null;

		TextCompletionTextEdit? textEdit = ExtractCompletionTextEdit(itemPayload, out string? textEditText);

		string insertText = textEditText ?? string.Empty;

		if (string.IsNullOrWhiteSpace(insertText))
			insertText = itemPayload.InsertText ?? label;

		int? insertCaretOffset = null;

		if (itemPayload.InsertTextFormat == 2)
		{
			LuaSnippetPlaceholderResult snippetResult = StripSnippetPlaceholders(insertText);
			insertText = snippetResult.Text;
			insertCaretOffset = snippetResult.CaretOffset;
		}

		string filterText = itemPayload.FilterText ?? label;

		LuaLanguageServerCompletionKind completionKind = TryReadCompletionKind(itemPayload, out LuaLanguageServerCompletionKind parsedCompletionKind)
			? parsedCompletionKind
			: LuaLanguageServerCompletionKind.Text;

		string? detail = BuildCompletionDetail(itemPayload);
		MarkupContent description = BuildCompletionDescription(itemPayload);
		string? searchableDescription = LuaMarkupTextHelper.NormalizeMarkupText(description.Text);
		var textAnalysis = new LuaCompletionTextAnalysis(detail, searchableDescription);

		return new TextCompletionItem(
			label,
			insertText,
			description.Text,
			BuildCompletionPriority(itemPayload, textAnalysis, itemIndex),
			BuildCompletionKind(completionKind, textAnalysis),
			detail: detail,
			filterText: filterText,
			isDescriptionMarkdown: description.IsMarkdown,
			resolveAsync: resolveAsync,
			textEdit: textEdit,
			insertCaretOffset: insertCaretOffset);
	}

	private static TextCompletionTextEdit? ExtractCompletionTextEdit(CompletionItemPayload itemPayload, out string? textEditText)
	{
		textEditText = null;

		if (itemPayload.TextEdit is not { } textEditElement)
			return null;

		textEditText = textEditElement.NewText;

		return ParseCompletionTextEdit(textEditElement);
	}

	private static TextCompletionTextEdit? ParseCompletionTextEdit(CompletionTextEditPayload textEditElement)
	{
		if (TryParseCompletionRange(textEditElement.Range, out TextCompletionRange range))
			return new TextCompletionTextEdit(range);

		if (TryParseCompletionRange(textEditElement.Insert, out TextCompletionRange insertRange)
			&& TryParseCompletionRange(textEditElement.Replace, out TextCompletionRange replaceRange))
		{
			return new TextCompletionTextEdit(insertRange, replaceRange);
		}

		return null;
	}

	private static bool TryParseCompletionRange(ProtocolRangePayload? rangeElement, out TextCompletionRange range)
	{
		range = default;

		if (!TryParseCompletionPosition(rangeElement?.Start, out TextCompletionPosition start)
			|| !TryParseCompletionPosition(rangeElement?.End, out TextCompletionPosition end))
		{
			return false;
		}

		range = new TextCompletionRange(start, end);
		return true;
	}

	private static bool TryParseCompletionPosition(ProtocolNullablePosition? positionElement, out TextCompletionPosition position)
	{
		position = default;

		if (positionElement is not { Line: int line, Character: int character })
			return false;

		if (line < 0 || character < 0)
			return false;

		position = new TextCompletionPosition(line, character);
		return true;
	}

	private static bool CompletionItemNeedsResolve(TextCompletionItem item)
		=> string.IsNullOrEmpty(item.Detail) || string.IsNullOrEmpty(item.Description);

	private static double BuildCompletionPriority(CompletionItemPayload itemPayload, LuaCompletionTextAnalysis textAnalysis, int itemIndex)
	{
		double priority = LuaCompletionPriorityWeights.ResponseOrderWeight - itemIndex;

		if (itemPayload.Preselect == true)
			priority += LuaCompletionPriorityWeights.PreselectedBonus;

		if (itemPayload.Kind is int completionKind)
		{
			priority += completionKind switch
			{
				(int)LuaLanguageServerCompletionKind.Variable => LuaCompletionPriorityWeights.VariableKind,
				(int)LuaLanguageServerCompletionKind.Field => LuaCompletionPriorityWeights.FieldOrPropertyKind,
				(int)LuaLanguageServerCompletionKind.Property => LuaCompletionPriorityWeights.FieldOrPropertyKind,
				(int)LuaLanguageServerCompletionKind.Method => LuaCompletionPriorityWeights.MethodOrFunctionKind,
				(int)LuaLanguageServerCompletionKind.Function => LuaCompletionPriorityWeights.MethodOrFunctionKind,
				(int)LuaLanguageServerCompletionKind.Keyword => LuaCompletionPriorityWeights.KeywordKindPenalty,
				_ => 0.0
			};
		}

		if (textAnalysis.HasLocalScope)
			priority += LuaCompletionPriorityWeights.LocalScope;

		if (textAnalysis.HasUpvalueOrParameter)
			priority += LuaCompletionPriorityWeights.UpvalueOrParameter;

		return priority;
	}

	private static bool TryReadCompletionKind(CompletionItemPayload itemPayload, out LuaLanguageServerCompletionKind kind)
	{
		kind = LuaLanguageServerCompletionKind.Text;

		if (itemPayload.Kind is not int rawKind
			|| !Enum.IsDefined(typeof(LuaLanguageServerCompletionKind), rawKind))
		{
			return false;
		}

		kind = (LuaLanguageServerCompletionKind)rawKind;
		return true;
	}

	private static TextCompletionItemKind BuildCompletionKind(LuaLanguageServerCompletionKind kind, LuaCompletionTextAnalysis textAnalysis)
	{
		if (textAnalysis.KindOverride is TextCompletionItemKind kindOverride)
			return kindOverride;

		return kind switch
		{
			LuaLanguageServerCompletionKind.Method => TextCompletionItemKind.Method,
			LuaLanguageServerCompletionKind.Function => TextCompletionItemKind.Method,
			LuaLanguageServerCompletionKind.Constructor => TextCompletionItemKind.Method,
			LuaLanguageServerCompletionKind.Field => TextCompletionItemKind.Field,
			LuaLanguageServerCompletionKind.Variable => TextCompletionItemKind.Variable,
			LuaLanguageServerCompletionKind.Class => TextCompletionItemKind.Class,
			LuaLanguageServerCompletionKind.Interface => TextCompletionItemKind.Class,
			LuaLanguageServerCompletionKind.Module => TextCompletionItemKind.Namespace,
			LuaLanguageServerCompletionKind.Property => TextCompletionItemKind.Property,
			LuaLanguageServerCompletionKind.Value => TextCompletionItemKind.Variable,
			LuaLanguageServerCompletionKind.Enum => TextCompletionItemKind.Class,
			LuaLanguageServerCompletionKind.Keyword => TextCompletionItemKind.Keyword,
			LuaLanguageServerCompletionKind.Snippet => TextCompletionItemKind.Keyword,
			LuaLanguageServerCompletionKind.File => TextCompletionItemKind.File,
			LuaLanguageServerCompletionKind.Reference => TextCompletionItemKind.Variable,
			LuaLanguageServerCompletionKind.Folder => TextCompletionItemKind.Folder,
			LuaLanguageServerCompletionKind.EnumMember => TextCompletionItemKind.Constant,
			LuaLanguageServerCompletionKind.Constant => TextCompletionItemKind.Constant,
			LuaLanguageServerCompletionKind.Struct => TextCompletionItemKind.Class,
			LuaLanguageServerCompletionKind.Event => TextCompletionItemKind.Method,
			LuaLanguageServerCompletionKind.Operator => TextCompletionItemKind.Keyword,
			LuaLanguageServerCompletionKind.TypeParameter => TextCompletionItemKind.Class,
			_ => TextCompletionItemKind.Generic
		};
	}

	private static string? BuildCompletionDetail(CompletionItemPayload itemPayload)
	{
		string? detail = itemPayload.Detail;
		return string.IsNullOrWhiteSpace(detail) ? null : detail.Trim();
	}

	private static MarkupContent BuildCompletionDescription(CompletionItemPayload itemPayload)
	{
		if (itemPayload.Documentation is not { } documentationElement
			|| documentationElement.ValueKind == JsonValueKind.Undefined)
		{
			return default;
		}

		MarkupContent documentation = MarkupContentReader.ExtractContent(documentationElement);

		if (string.IsNullOrWhiteSpace(documentation.Text))
			return default;

		string? normalizedText = documentation.IsMarkdown
			? LuaMarkupTextHelper.NormalizeMarkdownText(documentation.Text)
			: LuaMarkupTextHelper.NormalizeMarkupText(documentation.Text);

		return string.IsNullOrWhiteSpace(normalizedText)
			? default
			: new MarkupContent(normalizedText, documentation.IsMarkdown);
	}

	/// <summary>
	/// Removes Lua snippet placeholders while preserving the first final-caret location needed by the editor.
	/// </summary>
	private static LuaSnippetPlaceholderResult StripSnippetPlaceholders(string snippet)
	{
		if (string.IsNullOrWhiteSpace(snippet))
			return new LuaSnippetPlaceholderResult(snippet, null);

		var builder = new StringBuilder(snippet.Length);
		int? caretOffset = null;
		int index = 0;

		while (index < snippet.Length)
		{
			// Parse placeholder markers like ${1:name} or $0 and copy only the visible text.
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

			// Plain text passes through unchanged.
			builder.Append(snippet[index]);
			index++;
		}

		return new LuaSnippetPlaceholderResult(builder.ToString(), caretOffset);
	}
}
