using Nickelony.LanguageServer.Abstractions.Diagnostics;
using System;
using System.Collections.Generic;

namespace TombLib.Scripting.Diagnostics;

/// <summary>
/// Detects errors in editor content for a specific engine version.
/// </summary>
public interface IErrorDetector
{
	/// <summary>
	/// Finds the errors present in the given editor content. Implementations must be safe to call
	/// from any thread: the content is an immutable snapshot and no UI state may be touched. This
	/// permits the host to run full-document detection on the thread pool when the work is CPU-bound.
	/// </summary>
	/// <param name="editorContent">The content of the editor.</param>
	/// <param name="engineVersion">The engine version used to detect the errors.</param>
	/// <returns>The diagnostics describing the detected errors.</returns>
	IReadOnlyList<TextEditorDiagnostic> FindErrors(string editorContent, Version engineVersion);
}
