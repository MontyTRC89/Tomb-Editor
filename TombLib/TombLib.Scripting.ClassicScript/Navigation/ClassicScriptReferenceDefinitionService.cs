using ICSharpCode.AvalonEdit.Document;
using TombLib.Scripting.ClassicScript.Mnemonics;
using TombLib.Scripting.ClassicScript.Parsers;
using TombLib.Scripting.Specifications.ClassicScript.Descriptions;

namespace TombLib.Scripting.ClassicScript.Navigation;

public sealed class ClassicScriptReferenceDefinitionService
{
	private readonly ClassicScriptDescriptionArchiveService _descriptionArchiveService = new();
	private readonly ClassicScriptMnemonicCatalogService _mnemonicCatalogService = new();

	public ClassicScriptReferenceDefinition ResolveReference(TextDocument document, string word, WordType wordType, int offset)
	{
		ArgumentNullException.ThrowIfNull(document);
		ArgumentNullException.ThrowIfNull(word);

		int safeOffset = Math.Max(0, Math.Min(offset, document.TextLength));

		if (wordType is WordType.Hexadecimal or WordType.Decimal)
		{
			string? mnemonicConstant = TryResolveMnemonicConstant(document, word, wordType, safeOffset);

			if (!string.IsNullOrWhiteSpace(mnemonicConstant))
				return new ClassicScriptReferenceDefinition(mnemonicConstant, ReferenceType.MnemonicConstant);
		}

		ReferenceType referenceType = wordType switch
		{
			WordType.Header => ReferenceType.OldCommand,
			WordType.Command => _descriptionArchiveService.IsOldCommand(word) ? ReferenceType.OldCommand : ReferenceType.NewCommand,
			WordType.Directive => ReferenceType.NewCommand,
			_ => ReferenceType.MnemonicConstant
		};

		return new ClassicScriptReferenceDefinition(word, referenceType);
	}

	private string? TryResolveMnemonicConstant(TextDocument document, string word, WordType wordType, int offset)
	{
		string? currentFlagPrefix = ArgumentParser.GetFlagPrefixOfCurrentArgument(document, offset);

		if (string.IsNullOrWhiteSpace(currentFlagPrefix))
			return null;

		return _mnemonicCatalogService.TryResolveFlagByValue(word, wordType == WordType.Hexadecimal, currentFlagPrefix, out string flagName)
			? flagName
			: null;
	}
}

public readonly record struct ClassicScriptReferenceDefinition(string Keyword, ReferenceType Type);
