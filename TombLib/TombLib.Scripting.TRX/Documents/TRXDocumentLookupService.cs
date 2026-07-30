#nullable enable

using System;
using TombLib.Scripting.Text;
using TombLib.Scripting.TRX.Services;

namespace TombLib.Scripting.TRX.Documents;

public sealed class TRXDocumentLookupService
{
	private readonly ITRXDocumentService _documentService;

	public TRXDocumentLookupService(ITRXDocumentService documentService)
	{
		ArgumentNullException.ThrowIfNull(documentService);
		_documentService = documentService;
	}

	public bool IsLevelScriptDefined(ITextSnapshot source, string levelName)
	{
		ArgumentNullException.ThrowIfNull(source);
		ArgumentNullException.ThrowIfNull(levelName);

		return _documentService.IsLevelScriptDefined(source, levelName);
	}
}
