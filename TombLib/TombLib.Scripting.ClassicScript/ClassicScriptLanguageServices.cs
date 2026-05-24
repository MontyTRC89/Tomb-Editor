#nullable enable

using System;
using TombLib.Scripting.ClassicScript.Diagnostics;
using TombLib.Scripting.ClassicScript.Hover;
using TombLib.Scripting.ClassicScript.Navigation;
using TombLib.Scripting.ClassicScript.Signatures;
using TombLib.Scripting.Hover;
using TombLib.Scripting.Navigation;
using TombLib.Scripting.Signatures;

namespace TombLib.Scripting.ClassicScript;

public sealed class ClassicScriptLanguageServices
{
	private static readonly Lazy<ClassicScriptLanguageServices> DefaultInstance = new(CreateDefault);

	public ClassicScriptLanguageServices(
		ITextDefinitionProvider definitionProvider,
		ITextHoverProvider hoverProvider,
		ITextSignatureHelpProvider signatureHelpProvider,
		ErrorDetector errorDetector)
	{
		ArgumentNullException.ThrowIfNull(definitionProvider);
		ArgumentNullException.ThrowIfNull(hoverProvider);
		ArgumentNullException.ThrowIfNull(signatureHelpProvider);
		ArgumentNullException.ThrowIfNull(errorDetector);

		DefinitionProvider = definitionProvider;
		HoverProvider = hoverProvider;
		SignatureHelpProvider = signatureHelpProvider;
		ErrorDetector = errorDetector;
	}

	public static ClassicScriptLanguageServices Default => DefaultInstance.Value;

	public ITextDefinitionProvider DefinitionProvider { get; }

	public ITextHoverProvider HoverProvider { get; }

	public ITextSignatureHelpProvider SignatureHelpProvider { get; }

	public ErrorDetector ErrorDetector { get; }

	private static ClassicScriptLanguageServices CreateDefault()
		=> new(
			new ClassicScriptDefinitionProvider(),
			new ClassicScriptHoverProvider(),
			new ClassicScriptSignatureHelpProvider(),
			new ErrorDetector());
}