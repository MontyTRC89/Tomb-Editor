namespace TombLib.Scripting.Editing;

/// <summary>
/// Produces text edits or workspace edits for editor commands such as formatting and rename.
/// </summary>
public interface ITextEditProvider : ITextFormattingProvider
{
	/// <summary>
	/// Gets a value indicating whether symbol rename is currently supported.
	/// </summary>
	bool SupportsRename { get; }

	/// <summary>
	/// Produces rename edits for the supplied symbol location.
	/// </summary>
	/// <param name="request">The document, caret position, and replacement name.</param>
	/// <param name="cancellationToken">A token that can cancel the request.</param>
	/// <returns>The workspace edit to apply, or <see langword="null"/> when no changes are available.</returns>
	Task<TextWorkspaceEdit?> RenameSymbolAsync(TextRenameRequest request, CancellationToken cancellationToken = default);
}
