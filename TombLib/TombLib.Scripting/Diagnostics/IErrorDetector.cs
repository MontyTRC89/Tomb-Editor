using Nickelony.LanguageServer.Abstractions.Diagnostics;

namespace TombLib.Scripting.Diagnostics;

/// <summary>
/// Detects errors in editor content for a specific engine version.
/// </summary>
public interface IErrorDetector
{
	/// <summary>
	/// Finds the errors present in the given editor content.
	/// </summary>
	/// <param name="editorContent">The content of the editor.</param>
	/// <param name="engineVersion">The engine version used to detect the errors.</param>
	/// <returns>The diagnostics describing the detected errors.</returns>
	IReadOnlyList<TextEditorDiagnostic> FindErrors(string editorContent, Version engineVersion);
}
