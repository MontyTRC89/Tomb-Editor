namespace TombLib.Scripting.Editing;

/// <summary>
/// Represents a workspace-wide set of document edits.
/// </summary>
public sealed class TextWorkspaceEdit
{
	/// <summary>
	/// Initializes a new instance of the <see cref="TextWorkspaceEdit"/> class.
	/// </summary>
	/// <param name="documentEdits">The per-document edits in the workspace change set.</param>
	public TextWorkspaceEdit(IReadOnlyList<TextDocumentEdit> documentEdits)
	{
		DocumentEdits = documentEdits ?? [];
	}

	/// <summary>
	/// Gets the per-document edits in the workspace change set.
	/// </summary>
	public IReadOnlyList<TextDocumentEdit> DocumentEdits { get; }

	/// <summary>
	/// Gets a value indicating whether the workspace edit contains any text edits.
	/// </summary>
	public bool HasEdits => DocumentEdits.Any(documentEdit => documentEdit.TextEdits.Count > 0);
}
