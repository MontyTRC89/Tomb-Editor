using ICSharpCode.AvalonEdit.Document;
using System;
using System.Collections.Generic;
using TombLib.Scripting.Diagnostics;
using TombLib.Scripting.TRX.Parsers;
using TombLib.Scripting.TRX.Resources;
using TombLib.Scripting.UI.Diagnostics;

namespace TombLib.Scripting.TRX.Diagnostics;

public class ErrorDetector : IErrorDetector, ITextDiagnosticsProvider
{
	public IReadOnlyList<TextEditorDiagnostic> FindErrors(string editorContent, Version engineVersion)
	{
		// Anything before 4.8 should not have errors checked
		if (engineVersion < new Version(4, 8))
			return Array.Empty<TextEditorDiagnostic>();

		return DetectErrorLines(new TextDocument(editorContent), engineVersion);
	}

	public IReadOnlyList<TextEditorDiagnostic> GetDiagnostics(TextDiagnosticsRequest request)
	{
		ArgumentNullException.ThrowIfNull(request);

		return FindErrors(request.DocumentText, request.EngineVersion);
	}

	private static List<TextEditorDiagnostic> DetectErrorLines(TextDocument document, Version engineVersion)
	{
		var errorLines = new List<TextEditorDiagnostic>();

		foreach (DocumentLine processedLine in document.Lines)
		{
			string processedLineText = document.GetText(processedLine);

			if (LineParser.IsEmptyOrComments(processedLineText))
				continue;

			processedLineText = LineParser.EscapeComments(processedLineText);
			TextEditorDiagnostic error = FindErrorsInLine(processedLine, processedLineText, engineVersion);

			if (error != null)
				errorLines.Add(error);
		}

		return errorLines;
	}

	private static TextEditorDiagnostic FindErrorsInLine(DocumentLine line, string lineText, Version engineVersion)
	{
		// Check whether there are JSON keys which are marked as "Removed"
		foreach (RemovedKeyword keyword in Keywords.RemovedProperties)
		{
			if (engineVersion < keyword.RemovedVersion)
				continue;

			string keyPattern = $"\"{keyword.Keyword}\"";

			if (lineText.Contains(keyPattern))
			{
				return CreateDiagnostic(line, lineText,
					$"This property has been removed from the script syntax and cannot be used in TRX {keyword.RemovedVersion} or newer."
					+ (string.IsNullOrEmpty(keyword.Message) ? "" : "\n" + keyword.Message), keyPattern);
			}
		}

		foreach (RemovedKeyword keyword in Keywords.RemovedConstants)
		{
			if (engineVersion < keyword.RemovedVersion)
				continue;

			string keyPattern = $"\"{keyword.Keyword}\"";

			if (lineText.Contains(keyPattern))
			{
				return CreateDiagnostic(line, lineText,
					$"This constant has been removed from the script syntax and cannot be used in TRX {keyword.RemovedVersion} or newer."
					+ (string.IsNullOrEmpty(keyword.Message) ? "" : "\n" + keyword.Message), keyPattern);
			}
		}

		return null;
	}

	private static TextEditorDiagnostic CreateDiagnostic(DocumentLine line, string lineText, string message, string keyPattern)
	{
		int matchIndex = string.IsNullOrWhiteSpace(keyPattern)
			? -1
			: lineText.IndexOf(keyPattern, StringComparison.Ordinal);

		int startOffset = matchIndex >= 0 ? line.Offset + matchIndex : line.Offset;
		int endOffset = matchIndex >= 0
			? startOffset + keyPattern.Length
			: Math.Max(line.Offset + 1, line.EndOffset);

		return new TextEditorDiagnostic(TextEditorDiagnosticSeverity.Error, message, startOffset, endOffset);
	}
}
