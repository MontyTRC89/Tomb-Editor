#nullable enable

using System;
using TombLib.Scripting.Completion;
using TombLib.Scripting.Hover;
using TombLib.Scripting.Navigation;
using TombLib.Scripting.TRX.Completion;
using TombLib.Scripting.TRX.Documents;
using TombLib.Scripting.TRX.Hover;
using TombLib.Scripting.TRX.Navigation;
using TombLib.Scripting.TRX.Services;

namespace TombLib.Scripting.TRX;

public sealed class TRXLanguageServices
{
    public TRXLanguageServices(IGameflowSchemaService schemaService, ITRXLineService lineService, ITRXDocumentService documentService)
    {
        ArgumentNullException.ThrowIfNull(schemaService);
        ArgumentNullException.ThrowIfNull(lineService);
        ArgumentNullException.ThrowIfNull(documentService);

        SchemaService = schemaService;
        LineService = lineService;
        DocumentService = documentService;
        DefinitionProvider = new TRXDefinitionProvider(documentService);
        AutocompleteService = new GameflowAutocompleteService(schemaService);
        HoverService = new GameflowHoverService(schemaService);
        DocumentLookupService = new TRXDocumentLookupService(documentService);
    }

    public IGameflowSchemaService SchemaService { get; }

    public ITRXLineService LineService { get; }

    public ITRXDocumentService DocumentService { get; }

    public ITextDefinitionProvider DefinitionProvider { get; }

    public ITextCompletionProvider AutocompleteService { get; }

    public ITextHoverProvider HoverService { get; }

    public TRXDocumentLookupService DocumentLookupService { get; }
}
