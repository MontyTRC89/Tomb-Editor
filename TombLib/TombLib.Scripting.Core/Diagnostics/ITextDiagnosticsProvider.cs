namespace TombLib.Scripting.Diagnostics;

/// <summary>
/// Produces diagnostics for a document snapshot.
/// </summary>
public interface ITextDiagnosticsProvider
{
	/// <summary>
	/// Gets diagnostics for the supplied request.
	/// </summary>
	/// <param name="request">The current document and engine-context request.</param>
	/// <returns>The diagnostics produced for the supplied snapshot.</returns>
	IReadOnlyList<TextEditorDiagnostic> GetDiagnostics(TextDiagnosticsRequest request);
}
