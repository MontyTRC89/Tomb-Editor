#nullable enable

using System.Collections.Generic;

namespace TombLib.Scripting.Specifications.ClassicScript.Mnemonics.Models;

public sealed record MnemonicDefinitions(
	IReadOnlyList<MnemonicConstantDefinition> StandardConstants,
	IReadOnlyList<PluginMnemonicDefinition> PluginConstants);