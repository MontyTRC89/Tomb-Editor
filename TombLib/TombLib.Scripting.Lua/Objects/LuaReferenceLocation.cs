namespace TombLib.Scripting.Lua.Objects;

/// <summary>
/// Identifies a source location for a Lua symbol reference.
/// </summary>
public sealed class LuaReferenceLocation
{
	/// <summary>
	/// Initializes a new instance of the <see cref="LuaReferenceLocation"/> class.
	/// </summary>
	/// <param name="filePath">The file containing the reference.</param>
	/// <param name="range">The referenced range within the file.</param>
	public LuaReferenceLocation(string filePath, LuaDocumentRange range)
	{
		FilePath = filePath;
		Range = range;
	}

	/// <summary>
	/// Gets the file containing the reference.
	/// </summary>
	public string FilePath { get; }

	/// <summary>
	/// Gets the referenced range within the file.
	/// </summary>
	public LuaDocumentRange Range { get; }
}