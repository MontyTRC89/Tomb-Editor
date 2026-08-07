using System.Collections.Generic;
using System.Runtime.Versioning;

namespace TombLib.Scripting.UI.Editing;

/// <summary>
/// Represents the before and after snapshots of a workspace-edit application.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class TextWorkspaceEditTransaction(IReadOnlyList<TextWorkspaceDocumentChange> documentChanges)
{
	/// <summary>
	/// Gets the per-document changes captured in the transaction.
	/// </summary>
	public IReadOnlyList<TextWorkspaceDocumentChange> DocumentChanges { get; } = documentChanges ?? [];

	/// <summary>
	/// Gets a value indicating whether the transaction contains any changes.
	/// </summary>
	public bool HasChanges => DocumentChanges.Count > 0;
}

/// <summary>
/// Represents the before and after contents of a changed document.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class TextWorkspaceDocumentChange(string filePath, string beforeContent, string afterContent)
{
	/// <summary>
	/// Gets the changed file path.
	/// </summary>
	public string FilePath { get; } = filePath ?? string.Empty;

	/// <summary>
	/// Gets the document content before the change.
	/// </summary>
	public string BeforeContent { get; } = beforeContent ?? string.Empty;

	/// <summary>
	/// Gets the document content after the change.
	/// </summary>
	public string AfterContent { get; } = afterContent ?? string.Empty;
}
