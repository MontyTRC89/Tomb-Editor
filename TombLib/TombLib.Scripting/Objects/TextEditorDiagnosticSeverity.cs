namespace TombLib.Scripting.Objects
{
	public enum TextEditorDiagnosticSeverity
	{
		Error = 1,
		Warning = 2,
		Information = 3,
		Hint = 4
	}

	public static class TextEditorDiagnosticSeverityExtensions
	{
		public static string GetLabel(this TextEditorDiagnosticSeverity severity)
			=> severity switch
			{
				TextEditorDiagnosticSeverity.Error => "Error",
				TextEditorDiagnosticSeverity.Warning => "Warning",
				TextEditorDiagnosticSeverity.Information => "Information",
				TextEditorDiagnosticSeverity.Hint => "Hint",
				_ => "Diagnostic"
			};
	}
}