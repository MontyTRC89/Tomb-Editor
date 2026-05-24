namespace TombLib.Scripting.Diagnostics;

/// <summary>
/// Describes the severity of a text-editor diagnostic.
/// </summary>
public enum TextEditorDiagnosticSeverity
{
	/// <summary>
	/// The diagnostic is an error.
	/// </summary>
	Error = 1,

	/// <summary>
	/// The diagnostic is a warning.
	/// </summary>
	Warning = 2,

	/// <summary>
	/// The diagnostic is informational.
	/// </summary>
	Information = 3,

	/// <summary>
	/// The diagnostic is a hint.
	/// </summary>
	Hint = 4
}

/// <summary>
/// Provides helpers for working with <see cref="TextEditorDiagnosticSeverity"/> values.
/// </summary>
public static class TextEditorDiagnosticSeverityExtensions
{
	/// <summary>
	/// Gets a user-facing label for the supplied severity.
	/// </summary>
	/// <param name="severity">The severity value.</param>
	/// <returns>The display label for the supplied severity.</returns>
	public static string GetLabel(this TextEditorDiagnosticSeverity severity) => severity switch
	{
		TextEditorDiagnosticSeverity.Error => "Error",
		TextEditorDiagnosticSeverity.Warning => "Warning",
		TextEditorDiagnosticSeverity.Information => "Information",
		TextEditorDiagnosticSeverity.Hint => "Hint",
		_ => "Diagnostic"
	};
}
