using ICSharpCode.AvalonEdit.Document;
using TombLib.Scripting.ClassicScript.Mnemonics;
using TombLib.Scripting.ClassicScript.Parsers;
using TombLib.Scripting.Hover;

namespace TombLib.Scripting.ClassicScript.Hover;

public sealed class ClassicScriptHoverProvider : ITextHoverProvider
{
	private static readonly ClassicScriptMnemonicCatalogService MnemonicCatalogService = new();

	public TextHoverInfo? GetHoverInfo(TextHoverRequest request)
	{
		var document = new TextDocument(request.DocumentText);

		return TryGetHoverWord(document, request.HoveredOffset, out string hoveredWord, out WordType type)
			? CreateHoverInfo(document, request.HoveredOffset, hoveredWord, type)
			: null;
	}

	private static bool TryGetHoverWord(TextDocument document, int hoveredOffset, out string hoveredWord, out WordType type)
	{
		string? word = WordParser.GetWordFromOffset(document, hoveredOffset);
		type = WordParser.GetWordTypeFromOffset(document, hoveredOffset);

		if (type == WordType.MnemonicConstant && !MnemonicCatalogService.ContainsFlag(word))
			type = WordType.Unknown;

		if (type == WordType.Unknown && int.TryParse(word, out _))
			type = WordType.Decimal;

		if (word is not null && word.StartsWith("#", StringComparison.Ordinal))
		{
			type = WordType.Directive;
			word = word.Split(' ')[0];
		}

		hoveredWord = word ?? string.Empty;
		return hoveredWord.Length > 0 && type != WordType.Unknown;
	}

	private static TextHoverInfo? CreateHoverInfo(TextDocument document, int hoveredOffset, string hoveredWord, WordType type)
	{
		if (type is WordType.MnemonicConstant or WordType.Hexadecimal or WordType.Decimal)
			return CreateConstantHoverInfo(document, hoveredOffset, hoveredWord, type);

		return new TextHoverInfo($"For more information about the \"{hoveredWord}\" {type}, Press F12.", SymbolName: hoveredWord, Identifier: type);
	}

	private static TextHoverInfo? CreateConstantHoverInfo(TextDocument document, int hoveredOffset, string hoveredWord, WordType type)
	{
		string? currentFlagPrefix = ArgumentParser.GetFlagPrefixOfCurrentArgument(document, hoveredOffset);

		if (currentFlagPrefix is null)
		{
			return type == WordType.MnemonicConstant
				? new TextHoverInfo($"For more information about the \"{hoveredWord}\" Constant, Press F12.", SymbolName: hoveredWord, Identifier: type)
				: null;
		}

		if (!TryGetMnemonicInfo(hoveredWord, type, currentFlagPrefix, out string flagName, out string hexValue, out string decimalValue))
			return null;

		string content = $"{flagName}\n{hexValue}\n{decimalValue}\n\nFor more information about the \"{flagName}\" Constant, Press F12.";

		return new TextHoverInfo(content, SymbolName: hoveredWord, Identifier: type);
	}

	private static bool TryGetMnemonicInfo(string hoveredWord, WordType type, string currentFlagPrefix, out string flagName, out string hexValue, out string decimalValue)
	{
		flagName = string.Empty;
		hexValue = string.Empty;
		decimalValue = string.Empty;

		bool found = type switch
		{
			WordType.MnemonicConstant => MnemonicCatalogService.TryResolveFlagByValue(hoveredWord, false, null, out flagName)
				&& MnemonicCatalogService.TryGetDescription(flagName, out _),
			WordType.Hexadecimal => MnemonicCatalogService.TryResolveFlagByValue(hoveredWord, true, currentFlagPrefix, out flagName),
			WordType.Decimal => MnemonicCatalogService.TryResolveFlagByValue(hoveredWord, false, currentFlagPrefix, out flagName),
			_ => false
		};

		if (!found)
			return false;

		if (!MnemonicCatalogService.TryGetEntry(flagName, out ClassicScriptMnemonicEntry entry))
			return false;

		hexValue = entry.HexValue;
		decimalValue = entry.DecimalValue;
		return true;
	}

}