namespace TombLib.Scripting.Navigation;

/// <summary>
/// Identifies a navigable text definition location.
/// </summary>
/// <remarks>
/// Definition locations use one-based line and column coordinates so they can describe targets across
/// documents without relying on an editor-specific offset or segment instance.
/// </remarks>
public sealed class TextDefinitionLocation
{
	/// <summary>
	/// Initializes a new instance of the <see cref="TextDefinitionLocation"/> class.
	/// </summary>
	/// <param name="lineNumber">The one-based line number.</param>
	/// <param name="columnNumber">The one-based column number.</param>
	/// <param name="filePath">The optional file path for cross-file navigation.</param>
	public TextDefinitionLocation(int lineNumber, int columnNumber = 1, string? filePath = null)
	{
		LineNumber = Math.Max(1, lineNumber);
		ColumnNumber = Math.Max(1, columnNumber);
		FilePath = filePath;
	}

	/// <summary>
	/// Gets the one-based line number of the definition.
	/// </summary>
	public int LineNumber { get; }

	/// <summary>
	/// Gets the one-based column number of the definition.
	/// </summary>
	public int ColumnNumber { get; }

	/// <summary>
	/// Gets the optional file path for the definition.
	/// </summary>
	public string? FilePath { get; }
}
