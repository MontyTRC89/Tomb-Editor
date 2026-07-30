#nullable enable

namespace TombLib.Scripting.ClassicScript.Mnemonics.Models;

public sealed record MnemonicDefinitions(
	IReadOnlyList<MnemonicConstantDefinition> StandardConstants,
	IReadOnlyList<PluginMnemonicDefinition> PluginConstants);
