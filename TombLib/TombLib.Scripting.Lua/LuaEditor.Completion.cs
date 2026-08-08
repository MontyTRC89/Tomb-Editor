using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using Nickelony.LanguageServer.Abstractions.Completion;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TombLib.Scripting.Lua.Completion;
using TombLib.Scripting.Lua.Editing;
using TombLib.Scripting.Lua.Parsing;
using TombLib.Scripting.Lua.Resources;
using TombLib.Scripting.UI.Completion;

namespace TombLib.Scripting.Lua;

public sealed partial class LuaEditor
{
	private void CloseCompletionWindow()
	{
		CompletionController.InvalidateRequests();
		CompletionController.CloseWindow();
	}

	private void ScheduleCompletionRequest()
		=> CompletionController.ScheduleRequest();

	private void CancelPendingCompletionRequest()
		=> CompletionController.CancelPendingRequest();

	private Task RequestCompletionAsync(int offset, char? triggerCharacter)
		=> RequestCompletionAsyncCore(offset, triggerCharacter);

	private bool CanApplyCompletionItem(TextCompletionItem item)
		=> IsCompletionItemCurrent(item.RequestDocumentVersion, _editorDocumentVersion,
			item.RequestGeneration, _editorRequestGeneration, IsLoaded, IsIntelliSenseAvailable());

	private void RebaseOpenCompletionItems()
		=> CompletionController.RebaseOpenCompletionItems(_editorDocumentVersion, _editorRequestGeneration);

	private static bool IsCompletionItemCurrent(int? requestDocumentVersion,
		int currentDocumentVersion,
		int? requestGeneration,
		int currentGeneration,
		bool isEditorLoaded,
		bool isIntelliSenseAvailable)
	{
		if (!isEditorLoaded || !isIntelliSenseAvailable)
			return false;

		if (!requestDocumentVersion.HasValue && !requestGeneration.HasValue)
			return true;

		if (!requestDocumentVersion.HasValue || !requestGeneration.HasValue)
			return false;

		return requestDocumentVersion.Value == currentDocumentVersion
			&& requestGeneration.Value == currentGeneration;
	}

	private void ScheduleCloseIfEmpty()
		=> CompletionController.ScheduleCloseIfEmpty();

	private async Task RequestScheduledCompletionAsync()
	{
		if (!CompletionEnabled || !IsIntelliSenseAvailable())
			return;

		if (!LuaEditorInteractionRules.IsValidCompletionContext(Document, CaretOffset, triggerCharacter: null))
			return;

		await RequestCompletionAsyncCore(CaretOffset, null).ConfigureAwait(true);
	}

	private async Task RequestCompletionAsyncCore(int offset, char? triggerCharacter)
	{
		CancellationToken cancellationToken = CancellationToken.None;
		int requestToken = CompletionController.BeginRequest();
		int requestDocumentVersion = _editorDocumentVersion;
		int requestGeneration = _editorRequestGeneration;

		try
		{
			if (!IsIntelliSenseAvailable())
				return;

			var intelliSenseProvider = IntelliSenseProvider;

			if (intelliSenseProvider is null)
				return;

			DismissSignatureHelp();
			CloseDefinitionToolTip(true);

			(int line, int column) = GetPositionFromOffset(offset);

			IReadOnlyList<TextCompletionItem> items = await intelliSenseProvider
				.GetCompletionItemsAsync(FilePath, Text, line, column, triggerCharacter, cancellationToken)
				.ConfigureAwait(true);

			if (!IsCompletionRequestCurrent(cancellationToken, requestToken, requestDocumentVersion, requestGeneration))
				return;

			if (items.Count == 0)
			{
				CloseCompletionWindow();
				return;
			}

			CompletionData[] completionDataItems = CreateCompletionDataItems(items, requestDocumentVersion, requestGeneration);

			if (!IsCompletionRequestCurrent(cancellationToken, requestToken, requestDocumentVersion, requestGeneration))
				return;

			(int startOffset, int endOffset) = GetCompletionWindowOffsets(offset);
			CompletionController.OpenOrRefresh(completionDataItems, startOffset, endOffset);
		}
		catch (OperationCanceledException)
		{
		}
		catch (Exception exception)
		{
			CloseCompletionWindow();
			LogEditorFailure("Completion request", exception);
		}
	}

	private bool IsCompletionRequestCurrent(CancellationToken cancellationToken, int requestToken, int requestDocumentVersion, int requestGeneration)
		=> CompletionController.IsRequestCurrent(requestToken)
			&& IsAsyncEditorResultCurrent(cancellationToken, requestToken, requestToken, requestDocumentVersion, requestGeneration);

	private CompletionData[] CreateCompletionDataItems(IReadOnlyList<TextCompletionItem> items, int requestDocumentVersion, int requestGeneration)
	{
		var completionDataItems = new CompletionData[items.Count];
		LuaThemeBrushSet brushSet = GetThemeBrushSet();

		for (int i = 0; i < items.Count; i++)
		{
			TextCompletionItem completionItem = items[i].WithRequestContext(requestDocumentVersion, requestGeneration);
			completionDataItems[i] = new CompletionData(
				completionItem,
				item => LuaCompletionIconFactory.GetIcon(item.Kind, brushSet),
				CanApplyCompletionItem,
				NormalizeCompletionInsertion);
		}

		return completionDataItems;
	}

	private (int StartOffset, int EndOffset) GetCompletionWindowOffsets(int offset)
	{
		int endOffset = Math.Max(0, Math.Min(offset, Document.TextLength));
		int startOffset = endOffset;

		while (startOffset > 0)
		{
			char currentChar = Document.GetCharAt(startOffset - 1);

			if (LuaLineParser.IsIdentifierCharacter(currentChar))
				startOffset--;
			else
				break;
		}

		return (startOffset, endOffset);
	}

	private static CompletionDataInsertionResult NormalizeCompletionInsertion(TextArea textArea, int replacementOffset, string insertText, int? insertCaretOffset)
	{
		TextDocument document = textArea.Document;
		DocumentLine line = document.GetLineByOffset(Math.Clamp(replacementOffset, 0, document.TextLength));
		string currentLineIndentation = LuaIndentationStrategy.GetLeadingWhitespace(document.GetText(line));

		LuaCompletionNormalizationResult normalizedInsertion = LuaIndentationStrategy.NormalizeCompletionInsertion(
			insertText,
			insertCaretOffset,
			currentLineIndentation,
			LuaIndentationStrategy.CreateIndentationUnit(
				textArea.Options.ConvertTabsToSpaces,
				textArea.Options.IndentationSize,
				textArea.Options.IndentationSize));

		return new CompletionDataInsertionResult(normalizedInsertion.Text, normalizedInsertion.CaretOffset);
	}
}
