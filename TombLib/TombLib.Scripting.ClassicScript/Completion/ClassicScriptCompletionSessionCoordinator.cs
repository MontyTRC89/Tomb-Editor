#nullable enable

using ICSharpCode.AvalonEdit.Document;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;
using TombLib.Scripting.ClassicScript.Parsers;
using TombLib.Scripting.ClassicScript.Resources;
using TombLib.Scripting.Completion;
using TombLib.Scripting.Extensions;
using TombLib.Scripting.UI.Completion;

namespace TombLib.Scripting.ClassicScript.Completion;

public sealed class ClassicScriptCompletionSessionCoordinator
{
	private static readonly ITextCompletionProvider CompletionProvider = new ClassicScriptCompletionProvider();

	private int _latestRequestId;

	public Task<TextCompletionSessionDecision> GetCtrlSpaceDecisionAsync(
		string documentText,
		string? filePath,
		int caretOffset,
		bool completionWindowIsOpen)
	{
		if (completionWindowIsOpen)
			return Task.FromResult(TextCompletionSessionDecision.None);

		TextDocument document = CreateDocument(documentText, filePath);
		string? wholeLineText = CommandParser.GetWholeCommandLineText(document, caretOffset);

		return string.IsNullOrEmpty(wholeLineText)
			? ResolveCtrlSpaceFromEmptyLineAsync(document, filePath, caretOffset)
			: ResolveCtrlSpaceFromContextAsync(document, filePath, caretOffset);
	}

	public async Task<TextCompletionSessionDecision> GetTextEnteredDecisionAsync(
		string documentText,
		string? filePath,
		int caretOffset,
		string inputText,
		bool completionWindowIsOpen)
	{
		if (completionWindowIsOpen || caretOffset <= 0)
			return TextCompletionSessionDecision.None;

		TextDocument document = CreateDocument(documentText, filePath);

		if (EditorCompletionTriggerHelper.IsSingleCharacterLine(document.GetText(document.GetLineByOffset(caretOffset))))
			return GetEmptyLineDecision(document, caretOffset);

		if (inputText == "_" && caretOffset > 1)
			return GetWordDecision(document, caretOffset);

		if (inputText == "\"" && caretOffset > 1)
		{
			TextCompletionSessionDecision includeDecision = GetIncludeDecision(document, filePath, caretOffset);
			TextCompletionSessionDecision afterSpaceDecision = await GetAfterSpaceDecisionAsync(document, filePath, caretOffset).ConfigureAwait(false);

			return HasItems(afterSpaceDecision)
				? afterSpaceDecision
				: includeDecision;
		}

		if (caretOffset > 1)
			return await GetAfterSpaceDecisionAsync(document, filePath, caretOffset).ConfigureAwait(false);

		return TextCompletionSessionDecision.None;
	}

	private async Task<TextCompletionSessionDecision> ResolveCtrlSpaceFromEmptyLineAsync(TextDocument document, string? filePath, int caretOffset)
	{
		TextCompletionSessionDecision emptyLineDecision = GetEmptyLineDecision(document, caretOffset);

		if (HasItems(emptyLineDecision))
			return emptyLineDecision;

		TextCompletionSessionDecision includeDecision = GetIncludeDecision(document, filePath, caretOffset);

		return HasItems(includeDecision)
			? includeDecision
			: GetWordDecision(document, caretOffset);
	}

	private async Task<TextCompletionSessionDecision> ResolveCtrlSpaceFromContextAsync(TextDocument document, string? filePath, int caretOffset)
	{
		TextCompletionSessionDecision includeDecision = GetIncludeDecision(document, filePath, caretOffset);
		TextCompletionSessionDecision wordDecision = HasItems(includeDecision)
			? TextCompletionSessionDecision.None
			: GetWordDecision(document, caretOffset);

		TextCompletionSessionDecision contextualDecision = await GetContextualDecisionAsync(document, caretOffset, -1).ConfigureAwait(false);

		if (HasItems(contextualDecision))
			return contextualDecision;

		return HasItems(includeDecision)
			? includeDecision
			: wordDecision;
	}

	private async Task<TextCompletionSessionDecision> GetAfterSpaceDecisionAsync(TextDocument document, string? filePath, int caretOffset)
	{
		char previousCharacter = document.GetCharAt(caretOffset - 2);

		if (previousCharacter is '=' or ',' or '_' or '+' or '-' or '*' or '/')
			return await GetContextualDecisionAsync(document, caretOffset, -1, insertAtCaret: true).ConfigureAwait(false);

		return GetIncludeDecision(document, filePath, caretOffset);
	}

	private TextCompletionSessionDecision GetIncludeDecision(TextDocument document, string? filePath, int caretOffset)
	{
		DocumentLine currentLine = document.GetLineByOffset(caretOffset);
		string lineText = document.GetText(currentLine);

		if (!Regex.IsMatch(lineText, Patterns.IncludeCommand, RegexOptions.IgnoreCase))
			return TextCompletionSessionDecision.None;

		int? startOffset = null;
		int? endOffset = null;

		if (document.GetCharAt(caretOffset - 1) == '"')
		{
			startOffset = caretOffset - 1;
		}
		else if (document.GetCharAt(caretOffset - 1) != ' ')
		{
			int wordStartOffset = TextUtilities.GetNextCaretPosition(document, caretOffset, LogicalDirection.Backward, CaretPositioningMode.WordStart);
			string word = document.GetText(wordStartOffset, caretOffset - wordStartOffset);

			if (!word.StartsWith('#'))
			{
				startOffset = wordStartOffset;

				if (wordStartOffset - 1 > 0 && document.GetCharAt(wordStartOffset - 1) == '"')
					startOffset--;
			}
		}

		if (caretOffset < document.TextLength && document.GetCharAt(caretOffset) == '"')
			endOffset = caretOffset + 1;

		string? directoryPath = Path.GetDirectoryName(filePath);

		if (string.IsNullOrWhiteSpace(directoryPath))
			return TextCompletionSessionDecision.None;

		var fileDirectory = new DirectoryInfo(directoryPath);
		var completionItems = new List<TextCompletionItem>();

		foreach (FileInfo file in fileDirectory.GetFiles("*.txt", SearchOption.AllDirectories).Where(file => !file.FullName.Equals(filePath, StringComparison.OrdinalIgnoreCase)))
		{
			string pathPart = file.FullName.Replace(directoryPath, string.Empty).TrimStart('\\');
			string completionDataString = $"\"{pathPart}\"";

			completionItems.Add(new TextCompletionItem(completionDataString));
		}

		return completionItems.Count == 0
			? TextCompletionSessionDecision.None
			: CreateOpenDecision(completionItems, startOffset, endOffset);
	}

	private TextCompletionSessionDecision GetWordDecision(TextDocument document, int caretOffset)
	{
		int wordStartOffset = TextUtilities.GetNextCaretPosition(document, caretOffset - 1, LogicalDirection.Backward, CaretPositioningMode.WordStart);

		if (wordStartOffset < 0)
			return TextCompletionSessionDecision.None;

		IReadOnlyList<TextCompletionItem> completionItems = CompletionProvider.GetCompletionItems(
			new TextCompletionContext(document.Text, caretOffset, TextCompletionTrigger.Word));

		return completionItems.Count == 0
			? TextCompletionSessionDecision.None
			: CreateOpenDecision(completionItems, wordStartOffset, caretOffset);
	}

	private TextCompletionSessionDecision GetEmptyLineDecision(TextDocument document, int caretOffset)
	{
		string? currentSection = DocumentParser.GetCurrentSectionName(document, caretOffset);

		if (currentSection is not null && currentSection.IgnoreCaseEqualsAny("Strings", "PSXStrings", "PCStrings", "ExtraNG"))
			return TextCompletionSessionDecision.None;

		IReadOnlyList<TextCompletionItem> completionItems = CompletionProvider.GetCompletionItems(
			new TextCompletionContext(document.Text, caretOffset, TextCompletionTrigger.EmptyLine));

		return completionItems.Count == 0
			? TextCompletionSessionDecision.None
			: CreateOpenDecision(completionItems, document.GetLineByOffset(caretOffset).Offset, caretOffset);
	}

	private async Task<TextCompletionSessionDecision> GetContextualDecisionAsync(TextDocument document, int caretOffset, int argumentIndex, bool insertAtCaret = false)
	{
		int requestId = Interlocked.Increment(ref _latestRequestId);
		string documentText = document.Text;

		int wordStartOffset = insertAtCaret
			? caretOffset
			: TextUtilities.GetNextCaretPosition(document, caretOffset, LogicalDirection.Backward, CaretPositioningMode.WordStart);

		string word = wordStartOffset >= 0
			? document.GetText(wordStartOffset, caretOffset - wordStartOffset)
			: string.Empty;

		IReadOnlyList<TextCompletionItem> completionItems;

		try
		{
			completionItems = await Task.Run(() => CompletionProvider.GetCompletionItems(
				new TextCompletionContext(documentText, caretOffset, TextCompletionTrigger.Contextual, argumentIndex))).ConfigureAwait(false);
		}
		catch
		{
			return TextCompletionSessionDecision.None;
		}

		if (requestId != Volatile.Read(ref _latestRequestId) || completionItems.Count == 0 || wordStartOffset < 0)
			return TextCompletionSessionDecision.None;

		int? startOffset = null;

		if (insertAtCaret)
			startOffset = caretOffset;
		else if (!word.StartsWithAny('=', ',', '+', '-', '*', '/'))
			startOffset = wordStartOffset;

		return CreateOpenDecision(completionItems, startOffset, caretOffset);
	}

	private static TextCompletionSessionDecision CreateOpenDecision(IReadOnlyList<TextCompletionItem> items, int? startOffset, int? endOffset)
	{
		if (items.Count == 0 || !startOffset.HasValue || !endOffset.HasValue)
			return TextCompletionSessionDecision.None;

		return TextCompletionSessionDecision.Open(items, startOffset.Value, endOffset.Value);
	}

	private static bool HasItems(TextCompletionSessionDecision decision)
		=> decision.Items is not null && decision.Items.Count > 0;

	private static TextDocument CreateDocument(string documentText, string? filePath) => new(documentText)
	{
		FileName = filePath ?? string.Empty
	};
}
