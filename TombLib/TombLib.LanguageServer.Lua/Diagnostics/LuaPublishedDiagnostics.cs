using TombLib.Scripting.Diagnostics;

namespace TombLib.LanguageServer.Lua;

/// <summary>
/// Represents a diagnostics payload published by LuaLS for a specific document version.
/// </summary>
internal sealed class LuaPublishedDiagnostics
{
	/// <summary>
	/// Initializes a new instance of the <see cref="LuaPublishedDiagnostics"/> class.
	/// </summary>
	/// <param name="filePath">The normalized file path associated with the diagnostics.</param>
	/// <param name="diagnostics">The parsed diagnostics for that file.</param>
	/// <param name="version">The synchronized document version that produced the diagnostics.</param>
	internal LuaPublishedDiagnostics(string filePath, IReadOnlyList<TextEditorDiagnostic> diagnostics, int version)
	{
		FilePath = filePath;
		Diagnostics = diagnostics ?? [];
		Version = version;
	}

	/// <summary>
	/// Gets the normalized file path associated with the diagnostics.
	/// </summary>
	public string FilePath { get; }

	/// <summary>
	/// Gets the parsed diagnostics payload.
	/// </summary>
	public IReadOnlyList<TextEditorDiagnostic> Diagnostics { get; }

	/// <summary>
	/// Gets the synchronized document version that produced the diagnostics.
	/// </summary>
	public int Version { get; }
}
