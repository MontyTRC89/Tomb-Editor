using ICSharpCode.AvalonEdit.Document;
using Nickelony.LanguageServer.Abstractions.Completion;
using System.Collections.Generic;
using TombLib.Scripting.Completion;
using TombLib.Scripting.UI.Text;

namespace TombLib.Scripting.TRX.Completion;

/// <summary>
/// Coordinates completion session decisions for the TRX editor.
/// </summary>
public sealed class CompletionSessionCoordinator
{
	private readonly ITextCompletionProvider _completionService;
	private readonly TextAnalysisService _textAnalysisService;
	private readonly CompletionManager _completionManager;

	/// <summary>
	/// Initializes a new instance of the <see cref="CompletionSessionCoordinator"/> class.
	/// </summary>
	/// <param name="completionService">The completion provider used to source completion items.</param>
	/// <param name="textAnalysisService">The text analysis service used to validate completion contexts.</param>
	/// <param name="completionManager">The completion manager used to filter completion items.</param>
	public CompletionSessionCoordinator(
		ITextCompletionProvider completionService,
		TextAnalysisService textAnalysisService,
		CompletionManager completionManager)
	{
		_completionService = completionService;
		_textAnalysisService = textAnalysisService;
		_completionManager = completionManager;
	}

	/// <summary>
	/// Computes the completion session decision for a Ctrl+Space invocation.
	/// </summary>
	/// <param name="document">The current document.</param>
	/// <param name="caretOffset">The current caret offset.</param>
	/// <param name="completionWindowIsOpen">Whether a completion window is already open.</param>
	/// <returns>The completion session decision to apply.</returns>
	public TextCompletionSessionDecision GetCtrlSpaceDecision(TextDocument document, int caretOffset, bool completionWindowIsOpen)
	{
		if (completionWindowIsOpen || !_textAnalysisService.IsValidPositionForCtrlSpaceCompletion(document, caretOffset))
			return TextCompletionSessionDecision.None;

		string currentWord = _textAnalysisService.GetCurrentWordBeingTyped(document, caretOffset);
		return CreateOpenDecision(document, caretOffset, currentWord);
	}

	/// <summary>
	/// Computes the completion session decision for text entered into the editor.
	/// </summary>
	/// <param name="document">The current document.</param>
	/// <param name="caretOffset">The current caret offset.</param>
	/// <param name="inputText">The text that was entered.</param>
	/// <param name="completionWindowIsOpen">Whether a completion window is already open.</param>
	/// <returns>The completion session decision to apply.</returns>
	public TextCompletionSessionDecision GetTextEnteredDecision(TextDocument document, int caretOffset, string inputText, bool completionWindowIsOpen)
	{
		if (completionWindowIsOpen)
			return HasMatchingCompletions(document, caretOffset)
				? TextCompletionSessionDecision.None
				: TextCompletionSessionDecision.Close();

		if (ShouldTriggerCompletion(inputText))
		{
			int contextOffset = caretOffset > 0 ? caretOffset - 1 : 0;

			if (!_textAnalysisService.IsValidContextForCompletion(document, contextOffset))
				return TextCompletionSessionDecision.None;

			string currentWord = _textAnalysisService.GetCurrentWordBeingTyped(document, caretOffset);
			return CreateOpenDecision(document, caretOffset, currentWord);
		}

		var source = new TextDocumentSnapshot(document);

		if (_completionManager.ShouldTriggerCompletionOnEmptyLine(source, caretOffset))
		{
			string currentWord = _textAnalysisService.GetCurrentWordBeingTyped(document, caretOffset);
			return CreateOpenDecision(document, caretOffset, currentWord);
		}

		return TextCompletionSessionDecision.None;
	}

	private static bool ShouldTriggerCompletion(string inputText)
		=> inputText == "\"";

	private bool HasMatchingCompletions(TextDocument document, int caretOffset)
	{
		string currentWord = _textAnalysisService.GetCurrentWordBeingTyped(document, caretOffset);
		IReadOnlyList<TextCompletionItem> completionData = _completionService.GetCompletionItems(new TextCompletionContext(document.Text, caretOffset));
		IReadOnlyList<TextCompletionItem> matchingCompletions = _completionManager.FilterCompletions(completionData, currentWord);

		return matchingCompletions.Count > 0;
	}

	private TextCompletionSessionDecision CreateOpenDecision(TextDocument document, int caretOffset, string currentWord)
	{
		IReadOnlyList<TextCompletionItem> completionData = _completionService.GetCompletionItems(new TextCompletionContext(document.Text, caretOffset));

		if (completionData.Count == 0)
			return TextCompletionSessionDecision.None;

		IReadOnlyList<TextCompletionItem> filteredCompletions = _completionManager.FilterCompletions(completionData, currentWord);

		if (filteredCompletions.Count == 0)
			return TextCompletionSessionDecision.None;

		var source = new TextDocumentSnapshot(document);
		(int startOffset, int endOffset) = _completionManager.GetCompletionWindowOffsets(source, caretOffset, currentWord);
		return TextCompletionSessionDecision.Open(filteredCompletions, startOffset, endOffset);
	}
}
