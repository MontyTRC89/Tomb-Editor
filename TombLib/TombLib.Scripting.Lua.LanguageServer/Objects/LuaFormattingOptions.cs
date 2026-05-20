namespace TombLib.Scripting.Lua.Objects;

/// <summary>
/// Represents the editor formatting preferences used for a Lua document formatting request.
/// </summary>
public sealed class LuaFormattingOptions
{
	/// <summary>
	/// Initializes a new instance of the <see cref="LuaFormattingOptions"/> class.
	/// </summary>
	/// <param name="tabSize">The preferred indentation width.</param>
	/// <param name="insertSpaces"><see langword="true"/> to indent with spaces; otherwise, tabs.</param>
	public LuaFormattingOptions(int tabSize, bool insertSpaces)
	{
		TabSize = tabSize > 0 ? tabSize : 4;
		InsertSpaces = insertSpaces;
	}

	/// <summary>
	/// Gets the preferred indentation width.
	/// </summary>
	public int TabSize { get; }

	/// <summary>
	/// Gets a value indicating whether indentation should use spaces.
	/// </summary>
	public bool InsertSpaces { get; }
}