namespace TombLib.Scripting.Lua.Objects;

/// <summary>
/// Represents the text edits that should be applied to a single Lua document.
/// </summary>
public sealed class LuaDocumentEdit
{
	/// <summary>
	/// Initializes a new instance of the <see cref="LuaDocumentEdit"/> class.
	/// </summary>
	/// <param name="filePath">The file to update.</param>
	/// <param name="textEdits">The edits to apply.</param>
	public LuaDocumentEdit(string filePath, IReadOnlyList<LuaTextEdit> textEdits)
	{
		FilePath = filePath;
		TextEdits = textEdits ?? [];
	}

	/// <summary>
	/// Gets the file to update.
	/// </summary>
	public string FilePath { get; }

	/// <summary>
	/// Gets the edits to apply to the file.
	/// </summary>
	public IReadOnlyList<LuaTextEdit> TextEdits { get; }
}