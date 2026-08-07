using System;
using TombLib.Scripting.Text;
using TombLib.Scripting.TRX.Services;

namespace TombLib.Scripting.TRX.Documents;

/// <summary>
/// Boundary facade over <see cref="ITRXDocumentService"/> that answers document-level
/// lookups (level-script existence) on behalf of host consumers such as TombIDE.
/// Retained as a thin abstraction so host consumers do not depend on the document
/// service surface directly; it is consumed by TombIDE and tests.
/// </summary>
public sealed class TRXDocumentLookupService
{
	private readonly ITRXDocumentService _documentService;

	/// <summary>
	/// Initializes a new instance of the <see cref="TRXDocumentLookupService"/> class.
	/// </summary>
	/// <param name="documentService">The document service used to answer lookups.</param>
	public TRXDocumentLookupService(ITRXDocumentService documentService)
	{
		ArgumentNullException.ThrowIfNull(documentService);
		_documentService = documentService;
	}

	/// <summary>
	/// Determines whether a level with the given name is defined in the source.
	/// </summary>
	/// <param name="source">The document snapshot to inspect.</param>
	/// <param name="levelName">The level name to look up.</param>
	/// <returns>True if the level script is defined; otherwise false.</returns>
	public bool IsLevelScriptDefined(ITextSnapshot source, string levelName)
	{
		ArgumentNullException.ThrowIfNull(source);
		ArgumentNullException.ThrowIfNull(levelName);

		return _documentService.IsLevelScriptDefined(source, levelName);
	}
}
