namespace TombLib.Scripting.Lua.Objects;

/// <summary>
/// Represents a single text replacement inside a Lua document.
/// </summary>
public sealed class LuaTextEdit
{
	/// <summary>
	/// Initializes a new instance of the <see cref="LuaTextEdit"/> class.
	/// </summary>
	/// <param name="range">The range to replace.</param>
	/// <param name="newText">The replacement text.</param>
	public LuaTextEdit(LuaDocumentRange range, string newText)
	{
		Range = range;
		NewText = newText ?? string.Empty;
	}

	/// <summary>
	/// Gets the range to replace.
	/// </summary>
	public LuaDocumentRange Range { get; }

	/// <summary>
	/// Gets the replacement text.
	/// </summary>
	public string NewText { get; }
}