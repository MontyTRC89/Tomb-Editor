using ICSharpCode.AvalonEdit.Document;
using Nickelony.LanguageServer.Abstractions.Completion;
using System.Text.RegularExpressions;
using System.Windows.Documents;
using TombLib.Scripting.ClassicScript.Commands;
using TombLib.Scripting.ClassicScript.Mnemonics;
using TombLib.Scripting.ClassicScript.Services;
using TombLib.Scripting.Completion;
using TombLib.Scripting.Extensions;
using TombLib.Scripting.Text;

namespace TombLib.Scripting.ClassicScript.Completion;

public sealed class ClassicScriptCompletionProvider : ITextCompletionProvider
{
	private readonly IClassicScriptCommandService _commandService;
	private readonly ClassicScriptMnemonicCatalogService _mnemonicCatalogService;
	private readonly ClassicScriptCommandCatalogService _commandCatalogService = new();

	public ClassicScriptCompletionProvider(
		IClassicScriptCommandService commandService,
		ClassicScriptMnemonicCatalogService mnemonicCatalogService)
	{
		_commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
		_mnemonicCatalogService = mnemonicCatalogService ?? throw new ArgumentNullException(nameof(mnemonicCatalogService));
	}

	public IReadOnlyList<TextCompletionItem> GetCompletionItems(TextCompletionContext context)
	{
		var source = new StringTextSnapshot(context.DocumentText);

		return context.Trigger switch
		{
			TextCompletionTrigger.EmptyLine => GetNewLineCompletionItems(source, context.CaretOffset),
			TextCompletionTrigger.Contextual => GetContextualCompletionItems(source, context.CaretOffset, context.ArgumentIndex),
			TextCompletionTrigger.Word => GetWordCompletionItems(context.DocumentText, context.CaretOffset),
			_ => []
		};
	}

	private IReadOnlyList<TextCompletionItem> GetNewLineCompletionItems(ITextSnapshot source, int caretOffset)
	{
		string? currentSection = _commandService.GetCurrentSectionName(source, caretOffset);

		if (currentSection is not null && currentSection.IgnoreCaseEqualsAny("Strings", "PSXStrings", "PCStrings", "ExtraNG"))
			return [];

		var items = new List<TextCompletionItem>();

		AddItems(items, _commandCatalogService.OldCommands, "=", TextCompletionItemKind.OldCommand);
		AddItems(items, _commandCatalogService.NewCommands.Where(name => !name.StartsWith('#')), "=", TextCompletionItemKind.NewCommand);
		AddItems(items, _commandCatalogService.Sections.Select(section => $"[{section}]"), string.Empty, TextCompletionItemKind.Section);

		items.Add(CreateItem("#INCLUDE ", "#INCLUDE ", TextCompletionItemKind.Directive));
		items.Add(CreateItem("#DEFINE ", "#DEFINE ", TextCompletionItemKind.Directive));
		items.Add(CreateItem("#FIRST_ID ", "#FIRST_ID ", TextCompletionItemKind.Directive));

		return items;
	}

	private IReadOnlyList<TextCompletionItem> GetContextualCompletionItems(ITextSnapshot source, int caretOffset, int argumentIndex)
	{
		string? syntax = _commandService.GetCommandSyntax(source, caretOffset);

		if (string.IsNullOrEmpty(syntax))
			return [];

		var regex = new Regex(@"\(.*_\.*\)");

		if (!regex.IsMatch(syntax) && !syntax.Contains("ENABLED", StringComparison.OrdinalIgnoreCase) && !syntax.Contains("DISABLED", StringComparison.OrdinalIgnoreCase))
			return [];

		string[] arguments = syntax.Split(',');

		if (argumentIndex == -1)
			argumentIndex = _commandService.GetArgumentIndexAtOffset(source, caretOffset);

		if (arguments.Length <= argumentIndex || argumentIndex == -1)
			return [];

		string currentArgument = arguments[argumentIndex];
		var items = new List<TextCompletionItem>();

		if (regex.IsMatch(currentArgument))
		{
			string mnemonicPrefix = currentArgument.Split('(')[1].Split(')')[0].Trim('.').Trim();

			foreach (string mnemonicConstant in _mnemonicCatalogService.GetAllFlags())
			{
				if (mnemonicConstant.StartsWith(mnemonicPrefix, StringComparison.OrdinalIgnoreCase))
					items.Add(CreateItem(mnemonicConstant, mnemonicConstant, TextCompletionItemKind.Constant));
			}
		}
		else if (currentArgument.Contains("ENABLED", StringComparison.OrdinalIgnoreCase) || currentArgument.Contains("DISABLED", StringComparison.OrdinalIgnoreCase))
		{
			items.Add(CreateItem("ENABLED", "ENABLED", TextCompletionItemKind.Constant));
			items.Add(CreateItem("DISABLED", "DISABLED", TextCompletionItemKind.Constant));
		}

		return items;
	}

	private IReadOnlyList<TextCompletionItem> GetWordCompletionItems(string documentText, int caretOffset)
	{
		if (caretOffset <= 0)
			return [];

		// Word completion uses AvalonEdit's TextUtilities for caret positioning.
		// This remains a UI-level concern that requires a TextDocument.
		var document = new TextDocument(documentText);

		int wordStartOffset = TextUtilities.GetNextCaretPosition(document, caretOffset - 1, LogicalDirection.Backward, CaretPositioningMode.WordStart);

		if (wordStartOffset < 0)
			return [];

		string word = document.GetText(wordStartOffset, caretOffset - wordStartOffset);

		if (!_mnemonicCatalogService.GetAllFlags().Any(constant => constant.StartsWith(word, StringComparison.OrdinalIgnoreCase)))
			return [];

		var items = new List<TextCompletionItem>();

		foreach (string mnemonicConstant in _mnemonicCatalogService.GetAllFlags())
		{
			if (mnemonicConstant.StartsWith(word, StringComparison.OrdinalIgnoreCase))
				items.Add(CreateItem(mnemonicConstant, mnemonicConstant, TextCompletionItemKind.Constant));
		}

		return items;
	}

	private static void AddItems(List<TextCompletionItem> items, IEnumerable<string> values, string suffix, TextCompletionItemKind kind)
	{
		foreach (string value in values)
			items.Add(CreateItem(value, value + suffix, kind));
	}

	private static TextCompletionItem CreateItem(string label, string insertText, TextCompletionItemKind kind)
		=> new(label, insertText, kind: kind, detail: GetDetail(kind));

	private static string GetDetail(TextCompletionItemKind kind)
		=> kind switch
		{
			TextCompletionItemKind.OldCommand => "Old Command",
			TextCompletionItemKind.NewCommand => "New Command",
			TextCompletionItemKind.Constant => "Constant",
			TextCompletionItemKind.Section => "Section",
			TextCompletionItemKind.Directive => "Directive",
			_ => kind.ToString()
		};
}
