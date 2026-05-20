namespace TombLib.Scripting.Lua.Objects;

/// <summary>
/// Identifies a source location for a resolved Lua symbol definition.
/// </summary>
public sealed class LuaDefinitionLocation
{
	/// <summary>
	/// Initializes a new instance of the <see cref="LuaDefinitionLocation"/> class.
	/// </summary>
	/// <param name="filePath">The file containing the definition.</param>
	/// <param name="lineNumber">The one-based line number.</param>
	/// <param name="columnNumber">The one-based column number.</param>
	public LuaDefinitionLocation(string filePath, int lineNumber, int columnNumber)
	{
		FilePath = filePath;
		LineNumber = Math.Max(1, lineNumber);
		ColumnNumber = Math.Max(1, columnNumber);
	}

	/// <summary>
	/// Gets the file containing the definition.
	/// </summary>
	public string FilePath { get; }

	/// <summary>
	/// Gets the one-based line number of the definition.
	/// </summary>
	public int LineNumber { get; }

	/// <summary>
	/// Gets the one-based column number of the definition.
	/// </summary>
	public int ColumnNumber { get; }
}