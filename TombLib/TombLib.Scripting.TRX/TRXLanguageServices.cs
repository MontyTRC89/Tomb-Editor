using System;
using TombLib.Scripting.Completion;
using TombLib.Scripting.Hover;
using TombLib.Scripting.Navigation;
using TombLib.Scripting.TRX.Documents;
using TombLib.Scripting.TRX.Services;

namespace TombLib.Scripting.TRX;

/// <summary>
/// Aggregates the services used by the TRX editor.
/// </summary>
public sealed class TRXLanguageServices
{
	/// <summary>
	/// Initializes a new instance of the <see cref="TRXLanguageServices"/> class.
	/// </summary>
	/// <param name="schemaService">The GameFlow schema service.</param>
	/// <param name="lineService">The TRX line service.</param>
	/// <param name="documentService">The TRX document service.</param>
	/// <param name="definitionProvider">The definition provider.</param>
	/// <param name="completionService">The completion provider.</param>
	/// <param name="hoverService">The hover provider.</param>
	/// <param name="documentLookupService">The document lookup service.</param>
	public TRXLanguageServices(
		IGameFlowSchemaService schemaService,
		ITRXLineService lineService,
		ITRXDocumentService documentService,
		ITextDefinitionProvider definitionProvider,
		ITextCompletionProvider completionService,
		ITextHoverProvider hoverService,
		TRXDocumentLookupService documentLookupService)
	{
		ArgumentNullException.ThrowIfNull(schemaService);
		ArgumentNullException.ThrowIfNull(lineService);
		ArgumentNullException.ThrowIfNull(documentService);
		ArgumentNullException.ThrowIfNull(definitionProvider);
		ArgumentNullException.ThrowIfNull(completionService);
		ArgumentNullException.ThrowIfNull(hoverService);
		ArgumentNullException.ThrowIfNull(documentLookupService);

		SchemaService = schemaService;
		LineService = lineService;
		DocumentService = documentService;
		DefinitionProvider = definitionProvider;
		CompletionService = completionService;
		HoverService = hoverService;
		DocumentLookupService = documentLookupService;
	}

	/// <summary>
	/// Gets the GameFlow schema service.
	/// </summary>
	public IGameFlowSchemaService SchemaService { get; }

	/// <summary>
	/// Gets the TRX line service.
	/// </summary>
	public ITRXLineService LineService { get; }

	/// <summary>
	/// Gets the TRX document service.
	/// </summary>
	public ITRXDocumentService DocumentService { get; }

	/// <summary>
	/// Gets the definition provider.
	/// </summary>
	public ITextDefinitionProvider DefinitionProvider { get; }

	/// <summary>
	/// Gets the completion provider.
	/// </summary>
	public ITextCompletionProvider CompletionService { get; }

	/// <summary>
	/// Gets the hover provider.
	/// </summary>
	public ITextHoverProvider HoverService { get; }

	/// <summary>
	/// Gets the document lookup service.
	/// </summary>
	public TRXDocumentLookupService DocumentLookupService { get; }
}
