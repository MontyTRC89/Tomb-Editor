#nullable enable

using System;
using TombLib.Scripting.GameFlowScript.Completion;
using TombLib.Scripting.GameFlowScript.Hover;
using TombLib.Scripting.GameFlowScript.Navigation;
using TombLib.Scripting.Hover;
using TombLib.Scripting.Navigation;

namespace TombLib.Scripting.GameFlowScript;

public sealed class GameFlowLanguageServices
{
	private static readonly Lazy<GameFlowLanguageServices> DefaultInstance = new(CreateDefault);

	public GameFlowLanguageServices(
		ITextDefinitionProvider definitionProvider,
		ITextHoverProvider hoverProvider,
		GameFlowAutocompleteService autocompleteService)
	{
		ArgumentNullException.ThrowIfNull(definitionProvider);
		ArgumentNullException.ThrowIfNull(hoverProvider);
		ArgumentNullException.ThrowIfNull(autocompleteService);

		DefinitionProvider = definitionProvider;
		HoverProvider = hoverProvider;
		AutocompleteService = autocompleteService;
	}

	public static GameFlowLanguageServices Default => DefaultInstance.Value;

	public ITextDefinitionProvider DefinitionProvider { get; }

	public ITextHoverProvider HoverProvider { get; }

	public GameFlowAutocompleteService AutocompleteService { get; }

	private static GameFlowLanguageServices CreateDefault()
		=> new(
			new GameFlowDefinitionProvider(),
			new GameFlowHoverProvider(),
			new GameFlowAutocompleteService());
}