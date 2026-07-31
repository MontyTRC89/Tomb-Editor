#nullable enable

using System;
using Nickelony.LanguageServer.Core.Navigation;
using TombLib.Scripting.Navigation;
using TombLib.Scripting.Text;
using TombLib.Scripting.TRX.Services;

namespace TombLib.Scripting.TRX.Navigation;

public sealed class TRXDefinitionProvider : ITextDefinitionProvider
{
	private readonly ITRXDocumentService _documentService;

	public TRXDefinitionProvider(ITRXDocumentService documentService)
	{
		ArgumentNullException.ThrowIfNull(documentService);
		_documentService = documentService;
	}

	public TextDefinitionLocation? GetDefinition(TextDefinitionRequest request)
	{
		if (string.IsNullOrWhiteSpace(request.SymbolName))
			return null;

		var source = new StringTextSnapshot(request.DocumentText);
		int? lineNumber = _documentService.FindDocumentLineOfLevel(source, request.SymbolName);

		return lineNumber is null ? null : new TextDefinitionLocation(lineNumber.Value);
	}
}
