#nullable enable

using ICSharpCode.AvalonEdit.Document;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Documents;
using TombLib.Scripting.ClassicScript.Mnemonics;
using TombLib.Scripting.ClassicScript.Services;
using Nickelony.LanguageServer.Core.Completion;
using TombLib.Scripting.Completion;
using TombLib.Scripting.Extensions;
using TombLib.Scripting.Text;
using TombLib.Scripting.UI.Completion;
using TombLib.Scripting.UI.Text;

namespace TombLib.Scripting.ClassicScript.Completion;

public sealed class ClassicScriptCompletionSessionCoordinator
{
	private readonly IClassicScriptLineService _lineService;
	private readonly IClassicScriptCommandService _commandService;
	private readonly ITextCompletionProvider _completionProvider;

	private int _latestRequestId;

	public ClassicScriptCompletionSessionCoordinator(
		IClassicScriptLineService lineService,
		IClassicScriptCommandService commandService,
		ClassicScriptMnemonicCatalogService mnemonicCatalogService)
	{
		_lineService = lineService ?? throw new ArgumentNullException(nameof(lineService));
		_commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
		_completionProvider = new ClassicScriptCompletionProvider(commandService, mnemonicCatalogService);
	}

	public Task<TextCompletionSessionDecision> GetCtrlSpaceDecisionAsync(
		string documentText,
		string? filePath,
		int caretOffset,
		bool completionWindowIsOpen)
	{
		if (completionWindowIsOpen)
			return Task.FromResult(TextCompletionSessionDecision.None);

		TextDocument document = CreateDocument(documentText, filePath);
		ITextSnapshot source = new TextDocumentSnapshot(document);
		string? wholeLineText = _commandService.GetWholeCommandLineText(source, caretOffset);

		return string.IsNullOrEmpty(wholeLineText)
			? ResolveCtrlSpaceFromEmptyLineAsync(document, source, filePath, caretOffset)
			: ResolveCtrlSpaceFromContextAsync(document, source, filePath, caretOffset);
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
		ITextSnapshot source = new TextDocumentSnapshot(document);

		if (EditorCompletionTriggerHelper.IsSingleCharacterLine(document.GetText(document.GetLineByOffset(caretOffset))))
			return GetEmptyLineDecision(source, caretOffset);

		if (inputText == "_" && caretOffset > 1)
			return GetWordDecision(document, caretOffset);

		if (inputText == "\"" && caretOffset > 1)
		{
			TextCompletionSessionDecision includeDecision = GetIncludeDecision(document, source, filePath, caretOffset);
			TextCompletionSessionDecision afterSpaceDecision = await GetAfterSpaceDecisionAsync(document, source, filePath, caretOffset).ConfigureAwait(false);

			return HasItems(afterSpaceDecision)
				? afterSpaceDecision
				: includeDecision;
		}

		if (caretOffset > 1)
			return await GetAfterSpaceDecisionAsync(document, source, filePath, caretOffset).ConfigureAwait(false);

		return TextCompletionSessionDecision.None;
	}

	private async Task<TextCompletionSessionDecision> ResolveCtrlSpaceFromEmptyLineAsync(TextDocument document, ITextSnapshot source, string? filePath, int caretOffset)
	{
		TextCompletionSessionDecision emptyLineDecision = GetEmptyLineDecision(source, caretOffset);

		if (HasItems(emptyLineDecision))
			return emptyLineDecision;

		TextCompletionSessionDecision includeDecision = GetIncludeDecision(document, source, filePath, caretOffset);

		return HasItems(includeDecision)
			? includeDecision
			: GetWordDecision(document, caretOffset);
	}

	private async Task<TextCompletionSessionDecision> ResolveCtrlSpaceFromContextAsync(TextDocument document, ITextSnapshot source, string? filePath, int caretOffset)
	{
		TextCompletionSessionDecision includeDecision = GetIncludeDecision(document, source, filePath, caretOffset);
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

	private async Task<TextCompletionSessionDecision> GetAfterSpaceDecisionAsync(TextDocument document, ITextSnapshot source, string? filePath, int caretOffset)
	{
		char previousCharacter = source.GetCharAt(caretOffset - 2);

		if (previousCharacter is '=' or ',' or '_' or '+' or '-' or '*' or '/')
			return await GetContextualDecisionAsync(document, caretOffset, -1, insertAtCaret: true).ConfigureAwait(false);

		return GetIncludeDecision(document, source, filePath, caretOffset);
	}

	private TextCompletionSessionDecision GetIncludeDecision(TextDocument document, ITextSnapshot source, string? filePath, int caretOffset)
	{
		ITextLine currentLine = source.GetLineByOffset(caretOffset);
		string lineText = source.GetText(currentLine.Offset, currentLine.Length);

		if (!_lineService.IsValidIncludeLine(lineText))
			return TextCompletionSessionDecision.None;

		int? startOffset = null;
		int? endOffset = null;

		if (source.GetCharAt(caretOffset - 1) == '"')
		{
			startOffset = caretOffset - 1;
		}
		else if (source.GetCharAt(caretOffset - 1) != ' ')
		{
			int wordStartOffset = TextUtilities.GetNextCaretPosition(document, caretOffset, LogicalDirection.Backward, CaretPositioningMode.WordStart);
			string word = document.GetText(wordStartOffset, caretOffset - wordStartOffset);

			if (!word.StartsWith('#'))
			{
				startOffset = wordStartOffset;

				if (wordStartOffset - 1 > 0 && source.GetCharAt(wordStartOffset - 1) == '"')
					startOffset--;
			}
		}

		if (caretOffset < source.TextLength && source.GetCharAt(caretOffset) == '"')
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

		IReadOnlyList<TextCompletionItem> completionItems = _completionProvider.GetCompletionItems(
			new TextCompletionContext(document.Text, caretOffset, TextCompletionTrigger.Word));

		return completionItems.Count == 0
			? TextCompletionSessionDecision.None
			: CreateOpenDecision(completionItems, wordStartOffset, caretOffset);
	}

	private TextCompletionSessionDecision GetEmptyLineDecision(ITextSnapshot source, int caretOffset)
	{
		string? currentSection = _commandService.GetCurrentSectionName(source, caretOffset);

		if (currentSection is not null && currentSection.IgnoreCaseEqualsAny("Strings", "PSXStrings", "PCStrings", "ExtraNG"))
			return TextCompletionSessionDecision.None;

		IReadOnlyList<TextCompletionItem> completionItems = _completionProvider.GetCompletionItems(
			new TextCompletionContext(source.GetText(0, source.TextLength), caretOffset, TextCompletionTrigger.EmptyLine));

		return completionItems.Count == 0
			? TextCompletionSessionDecision.None
			: CreateOpenDecision(completionItems, source.GetLineByOffset(caretOffset).Offset, caretOffset);
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
			completionItems = await Task.Run(() => _completionProvider.GetCompletionItems(
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
