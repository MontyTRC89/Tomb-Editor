using ICSharpCode.AvalonEdit.Document;
using System;
using System.Collections.Generic;
using TombLib.Scripting.Interfaces;
using TombLib.Scripting.Tomb1Main.Parsers;
using TombLib.Scripting.Tomb1Main.Resources;

namespace TombLib.Scripting.Tomb1Main.Utils;

public class ErrorDetector : IErrorDetector
{
	public object FindErrors(string editorContent, Version engineVersion)
	{
		// Anything before 4.8 should not have errors checked
		if (engineVersion < new Version(4, 8))
			return null;

		return DetectErrorLines(new TextDocument(editorContent));
	}

	private static List<ErrorLine> DetectErrorLines(TextDocument document)
	{
		var errorLines = new List<ErrorLine>();

		foreach (DocumentLine processedLine in document.Lines)
		{
			string processedLineText = document.GetText(processedLine);

			if (LineParser.IsEmptyOrComments(processedLineText))
				continue;

			processedLineText = LineParser.EscapeComments(processedLineText);
			ErrorLine error = FindErrorsInLine(processedLine, processedLineText);

			if (error != null)
				errorLines.Add(error);
		}

		return errorLines;
	}

	private static ErrorLine FindErrorsInLine(DocumentLine line, string lineText)
	{
		// Check whether there are JSON keys which are marked as "Removed"
		foreach (RemovedKeyword keyword in Keywords.RemovedProperties)
		{
			string keyPattern = $"\"{keyword.Keyword}\"";

			if (lineText.Contains(keyPattern))
			{
				return new ErrorLine("This property has been removed from the script syntax and cannot be used in TR1X 4.8 or newer."
					+ (string.IsNullOrEmpty(keyword.Message) ? "" : "\n" + keyword.Message),
					line.LineNumber, keyPattern);
			}
		}

		foreach (RemovedKeyword keyword in Keywords.RemovedConstants)
		{
			string keyPattern = $"\"{keyword.Keyword}\"";

			if (lineText.Contains(keyPattern))
			{
				return new ErrorLine("This constant has been removed from the script syntax and cannot be used in TR1X 4.8 or newer."
					+ (string.IsNullOrEmpty(keyword.Message) ? "" : "\n" + keyword.Message),
					line.LineNumber, keyPattern);
			}
		}

		return null;
	}
}
