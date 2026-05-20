namespace TombLib.Scripting.Lua.Objects;

/// <summary>
/// Represents a workspace-wide set of Lua document edits.
/// </summary>
public sealed class LuaWorkspaceEdit
{
	/// <summary>
	/// Initializes a new instance of the <see cref="LuaWorkspaceEdit"/> class.
	/// </summary>
	/// <param name="documentEdits">The per-document edits in the workspace change set.</param>
	public LuaWorkspaceEdit(IReadOnlyList<LuaDocumentEdit> documentEdits)
	{
		DocumentEdits = documentEdits ?? [];
	}

	/// <summary>
	/// Gets the per-document edits in the workspace change set.
	/// </summary>
	public IReadOnlyList<LuaDocumentEdit> DocumentEdits { get; }

	/// <summary>
	/// Gets a value indicating whether the workspace edit contains any text edits.
	/// </summary>
	public bool HasEdits => DocumentEdits.Count > 0;
}