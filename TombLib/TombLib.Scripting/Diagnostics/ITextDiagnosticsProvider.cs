using Nickelony.LanguageServer.Abstractions.Diagnostics;
using System.Collections.Generic;

namespace TombLib.Scripting.Diagnostics;

/// <summary>
/// Produces diagnostics for a document snapshot.
/// </summary>
public interface ITextDiagnosticsProvider
{
	/// <summary>
	/// Gets diagnostics for the supplied request. Implementations must be safe to call from any
	/// thread: the request is an immutable snapshot and no UI state may be touched. This permits
	/// the host to run full-document detection on the thread pool when the work is CPU-bound.
	/// </summary>
	/// <param name="request">The current document and engine-context request.</param>
	/// <returns>The diagnostics produced for the supplied snapshot.</returns>
	IReadOnlyList<TextEditorDiagnostic> GetDiagnostics(TextDiagnosticsRequest request);
}
