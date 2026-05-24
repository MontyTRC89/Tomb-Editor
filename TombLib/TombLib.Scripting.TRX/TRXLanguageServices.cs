#nullable enable

using System;
using TombLib.Scripting.Completion;
using TombLib.Scripting.Hover;
using TombLib.Scripting.TRX.Completion;
using TombLib.Scripting.TRX.Hover;
using TombLib.Scripting.TRX.Navigation;
using TombLib.Scripting.Navigation;
using TombLib.Scripting.Specifications.TRX;
using TombLib.Scripting.Specifications.TRX.Services;

namespace TombLib.Scripting.TRX;

public sealed class TRXLanguageServices
{
	private static readonly Lazy<TRXLanguageServices> DefaultInstance = new(CreateDefault);

	public static TRXLanguageServices Default => DefaultInstance.Value;

	public TRXLanguageServices(IGameflowSchemaService schemaService)
	{
		ArgumentNullException.ThrowIfNull(schemaService);

		SchemaService = schemaService;
		DefinitionProvider = new TRXDefinitionProvider();
		AutocompleteService = new GameflowAutocompleteService(schemaService);
		HoverService = new GameflowHoverService(schemaService);
	}

	public IGameflowSchemaService SchemaService { get; }

	public ITextDefinitionProvider DefinitionProvider { get; }

	public ITextCompletionProvider AutocompleteService { get; }

	public ITextHoverProvider HoverService { get; }

	private static TRXLanguageServices CreateDefault()
		=> new(new GameflowSchemaService(TrxResourcePaths.GetGameflowSchemaPath()));
}