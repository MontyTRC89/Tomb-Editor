namespace Nickelony.LanguageServer.Core.Navigation;

/// <summary>
/// Identifies a source location for a symbol reference.
/// </summary>
/// <remarks>
/// Reference locations use one-based line and column coordinates to match the LSP convention
/// and remain stable across documents without depending on an editor-specific offset or segment
/// instance.
/// </remarks>
public sealed class TextReferenceLocation
{
	/// <summary>
	/// Initializes a new instance of the <see cref="TextReferenceLocation"/> class.
	/// </summary>
	/// <param name="filePath">The file containing the reference.</param>
	/// <param name="startLineNumber">The one-based start line number.</param>
	/// <param name="startColumnNumber">The one-based start column number.</param>
	/// <param name="endLineNumber">The one-based end line number.</param>
	/// <param name="endColumnNumber">The one-based end column number.</param>
	public TextReferenceLocation(string filePath, int startLineNumber, int startColumnNumber, int endLineNumber, int endColumnNumber)
	{
		FilePath = filePath;

		int safeStartLineNumber = Math.Max(1, startLineNumber);
		int safeStartColumnNumber = Math.Max(1, startColumnNumber);
		int safeEndLineNumber = Math.Max(1, endLineNumber);
		int safeEndColumnNumber = Math.Max(1, endColumnNumber);

		if (safeEndLineNumber < safeStartLineNumber
			|| (safeEndLineNumber == safeStartLineNumber && safeEndColumnNumber < safeStartColumnNumber))
		{
			safeEndLineNumber = safeStartLineNumber;
			safeEndColumnNumber = safeStartColumnNumber;
		}

		StartLineNumber = safeStartLineNumber;
		StartColumnNumber = safeStartColumnNumber;
		EndLineNumber = safeEndLineNumber;
		EndColumnNumber = safeEndColumnNumber;
	}

	/// <summary>
	/// Gets the file containing the reference.
	/// </summary>
	public string FilePath { get; }

	/// <summary>
	/// Gets the one-based start line number.
	/// </summary>
	public int StartLineNumber { get; }

	/// <summary>
	/// Gets the one-based start column number.
	/// </summary>
	public int StartColumnNumber { get; }

	/// <summary>
	/// Gets the one-based end line number.
	/// </summary>
	public int EndLineNumber { get; }

	/// <summary>
	/// Gets the one-based end column number.
	/// </summary>
	public int EndColumnNumber { get; }

	/// <summary>
	/// Gets a value indicating whether the location is contained on a single line.
	/// </summary>
	public bool IsSingleLine => StartLineNumber == EndLineNumber;
}
