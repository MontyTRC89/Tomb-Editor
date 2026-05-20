namespace TombLib.Scripting.Lua.Objects;

/// <summary>
/// Identifies a one-based document range inside a Lua source file.
/// </summary>
public sealed class LuaDocumentRange
{
	/// <summary>
	/// Initializes a new instance of the <see cref="LuaDocumentRange"/> class.
	/// </summary>
	/// <param name="startLineNumber">The one-based start line number.</param>
	/// <param name="startColumnNumber">The one-based start column number.</param>
	/// <param name="endLineNumber">The one-based end line number.</param>
	/// <param name="endColumnNumber">The one-based end column number.</param>
	public LuaDocumentRange(int startLineNumber, int startColumnNumber, int endLineNumber, int endColumnNumber)
	{
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
}