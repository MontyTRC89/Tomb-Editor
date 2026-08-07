using TombLib.Scripting.ClassicScript.Commands;
using TombLib.Scripting.ClassicScript.Mnemonics;
using TombLib.Scripting.ClassicScript.Services;
using TombLib.Scripting.Text;

namespace TombLib.Scripting.ClassicScript.Navigation;

public sealed class ClassicScriptReferenceDefinitionService
{
	private readonly ClassicScriptCommandCatalogService _commandCatalogService = new();
	private readonly ClassicScriptMnemonicCatalogService _mnemonicCatalogService = new();
	private readonly IClassicScriptCommandService _commandService;

	public ClassicScriptReferenceDefinitionService(IClassicScriptCommandService commandService)
	{
		_commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
	}

	public ClassicScriptReferenceDefinition ResolveReference(ITextSnapshot source, string word, WordType wordType, int offset)
	{
		ArgumentNullException.ThrowIfNull(source);
		ArgumentNullException.ThrowIfNull(word);

		int safeOffset = Math.Max(0, Math.Min(offset, source.TextLength));

		if (wordType is WordType.Hexadecimal or WordType.Decimal)
		{
			string? mnemonicConstant = TryResolveMnemonicConstant(source, word, wordType, safeOffset);

			if (!string.IsNullOrWhiteSpace(mnemonicConstant))
				return new ClassicScriptReferenceDefinition(mnemonicConstant, ReferenceType.MnemonicConstant);
		}

		ReferenceType referenceType = wordType switch
		{
			WordType.Header => ReferenceType.OldCommand,
			WordType.Command => _commandCatalogService.IsOldCommand(word) ? ReferenceType.OldCommand : ReferenceType.NewCommand,
			WordType.Directive => ReferenceType.NewCommand,
			_ => ReferenceType.MnemonicConstant
		};

		return new ClassicScriptReferenceDefinition(word, referenceType);
	}

	private string? TryResolveMnemonicConstant(ITextSnapshot source, string word, WordType wordType, int offset)
	{
		string? currentFlagPrefix = _commandService.GetFlagPrefixOfCurrentArgument(source, offset);

		if (string.IsNullOrWhiteSpace(currentFlagPrefix))
			return null;

		return _mnemonicCatalogService.TryResolveFlagByValue(word, wordType == WordType.Hexadecimal, currentFlagPrefix, out string flagName)
			? flagName
			: null;
	}
}

public readonly record struct ClassicScriptReferenceDefinition(string Keyword, ReferenceType Type);
