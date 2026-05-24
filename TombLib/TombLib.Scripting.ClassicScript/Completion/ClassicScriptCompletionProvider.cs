using ICSharpCode.AvalonEdit.Document;
using System.Text.RegularExpressions;
using System.Windows.Documents;
using TombLib.Scripting.ClassicScript.Mnemonics;
using TombLib.Scripting.ClassicScript.Parsers;
using TombLib.Scripting.ClassicScript.Resources;
using TombLib.Scripting.Completion;
using TombLib.Scripting.Extensions;
using TombLib.Scripting.Specifications.ClassicScript;

namespace TombLib.Scripting.ClassicScript.Completion;

public sealed class ClassicScriptCompletionProvider : ITextCompletionProvider
{
	private static readonly ClassicScriptMnemonicCatalogService MnemonicCatalogService = new();

	public IReadOnlyList<TextCompletionItem> GetCompletionItems(TextCompletionContext context)
	{
		var document = new TextDocument(context.DocumentText);

		return context.Trigger switch
		{
			TextCompletionTrigger.EmptyLine => GetNewLineCompletionItems(document, context.CaretOffset),
			TextCompletionTrigger.Contextual => GetContextualCompletionItems(document, context.CaretOffset, context.ArgumentIndex),
			TextCompletionTrigger.Word => GetWordCompletionItems(document, context.CaretOffset),
			_ => []
		};
	}

	private static IReadOnlyList<TextCompletionItem> GetNewLineCompletionItems(TextDocument document, int caretOffset)
	{
		string? currentSection = DocumentParser.GetCurrentSectionName(document, caretOffset);

		if (currentSection is not null && currentSection.IgnoreCaseEqualsAny("Strings", "PSXStrings", "PCStrings", "ExtraNG"))
			return [];

		var items = new List<TextCompletionItem>();

		AddItems(items, ClassicScriptKeywords.OldCommands, "=", TextCompletionItemKind.OldCommand);
		AddItems(items, ClassicScriptKeywords.NewCommands, "=", TextCompletionItemKind.NewCommand);
		AddItems(items, ClassicScriptKeywords.Sections.Select(section => $"[{section}]"), string.Empty, TextCompletionItemKind.Section);

		items.Add(CreateItem("#INCLUDE ", "#INCLUDE ", TextCompletionItemKind.Directive));
		items.Add(CreateItem("#DEFINE ", "#DEFINE ", TextCompletionItemKind.Directive));
		items.Add(CreateItem("#FIRST_ID ", "#FIRST_ID ", TextCompletionItemKind.Directive));

		return items;
	}

	private static IReadOnlyList<TextCompletionItem> GetContextualCompletionItems(TextDocument document, int caretOffset, int argumentIndex)
	{
		string? syntax = CommandParser.GetCommandSyntax(document, caretOffset);

		if (string.IsNullOrEmpty(syntax))
			return [];

		var regex = new Regex(Patterns.CommandPrefixInParenthesis);

		if (!regex.IsMatch(syntax) && !syntax.Contains("ENABLED", StringComparison.OrdinalIgnoreCase) && !syntax.Contains("DISABLED", StringComparison.OrdinalIgnoreCase))
			return [];

		string[] arguments = syntax.Split(',');

		if (argumentIndex == -1)
			argumentIndex = ArgumentParser.GetArgumentIndexAtOffset(document, caretOffset);

		if (arguments.Length <= argumentIndex || argumentIndex == -1)
			return [];

		string currentArgument = arguments[argumentIndex];
		var items = new List<TextCompletionItem>();

		if (regex.IsMatch(currentArgument))
		{
			string mnemonicPrefix = currentArgument.Split('(')[1].Split(')')[0].Trim('.').Trim();

			foreach (string mnemonicConstant in MnemonicCatalogService.GetAllFlags())
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

	private static IReadOnlyList<TextCompletionItem> GetWordCompletionItems(TextDocument document, int caretOffset)
	{
		if (caretOffset <= 0)
			return [];

		int wordStartOffset = TextUtilities.GetNextCaretPosition(document, caretOffset - 1, LogicalDirection.Backward, CaretPositioningMode.WordStart);

		if (wordStartOffset < 0)
			return [];

		string word = document.GetText(wordStartOffset, caretOffset - wordStartOffset);

		if (!MnemonicCatalogService.GetAllFlags().Any(constant => constant.StartsWith(word, StringComparison.OrdinalIgnoreCase)))
			return [];

		var items = new List<TextCompletionItem>();

		foreach (string mnemonicConstant in MnemonicCatalogService.GetAllFlags())
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
