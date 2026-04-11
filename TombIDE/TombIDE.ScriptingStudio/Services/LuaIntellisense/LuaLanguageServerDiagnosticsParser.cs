using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using ICSharpCode.AvalonEdit.Document;
using TombLib.Scripting.Objects;

namespace TombIDE.ScriptingStudio.Services.LuaIntellisense
{
	internal static class LuaLanguageServerDiagnosticsParser
	{
		public static bool TryParse(JsonElement parameters, string filePath,
			string documentContent, int documentVersion, out LuaPublishedDiagnostics publishedDiagnostics)
		{
			publishedDiagnostics = null;

			if (parameters.ValueKind != JsonValueKind.Object)
				return false;

			int diagnosticsVersion = 0;

			if (parameters.TryGetProperty("version", out JsonElement versionElement)
				&& versionElement.ValueKind == JsonValueKind.Number
				&& versionElement.TryGetInt32(out int parsedVersion)
				&& parsedVersion > 0)
			{
				diagnosticsVersion = parsedVersion;
			}

			if (diagnosticsVersion > 0 && documentVersion > 0 && diagnosticsVersion > documentVersion)
				return false;

			IReadOnlyList<TextEditorDiagnostic> diagnostics = Array.Empty<TextEditorDiagnostic>();
			string resolvedContent = ResolveDocumentContent(filePath, documentContent);

			if (parameters.TryGetProperty("diagnostics", out JsonElement diagnosticsElement)
				&& diagnosticsElement.ValueKind == JsonValueKind.Array)
			{
				diagnostics = BuildDiagnostics(resolvedContent, diagnosticsElement);
			}

			publishedDiagnostics = new LuaPublishedDiagnostics(filePath, diagnostics, diagnosticsVersion);
			return true;
		}

		private static string ResolveDocumentContent(string filePath, string documentContent)
		{
			if (!string.IsNullOrEmpty(documentContent))
				return documentContent;

			if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
				return string.Empty;

			try
			{
				return File.ReadAllText(filePath);
			}
			catch
			{
				return string.Empty;
			}
		}

		private static IReadOnlyList<TextEditorDiagnostic> BuildDiagnostics(string content, JsonElement diagnosticsElement)
		{
			var document = new TextDocument(content ?? string.Empty);
			var diagnostics = new List<TextEditorDiagnostic>();

			foreach (JsonElement diagnosticElement in diagnosticsElement.EnumerateArray())
			{
				TextEditorDiagnosticSeverity severity = GetDiagnosticSeverity(diagnosticElement);

				if (severity > TextEditorDiagnosticSeverity.Warning)
					continue;

				if (!TryCreateDiagnostic(document, diagnosticElement, severity, out TextEditorDiagnostic diagnostic))
					continue;

				diagnostics.Add(diagnostic);
			}

			return diagnostics
				.OrderBy(diagnostic => diagnostic.StartOffset)
				.ThenBy(diagnostic => diagnostic.Severity)
				.ToList();
		}

		private static bool TryCreateDiagnostic(TextDocument document, JsonElement diagnosticElement,
			TextEditorDiagnosticSeverity severity, out TextEditorDiagnostic diagnostic)
		{
			diagnostic = null;

			if (document.LineCount == 0
				|| !diagnosticElement.TryGetProperty("range", out JsonElement rangeElement)
				|| !rangeElement.TryGetProperty("start", out JsonElement startElement)
				|| startElement.ValueKind != JsonValueKind.Object
				|| !startElement.TryGetProperty("line", out JsonElement lineElement)
				|| !lineElement.TryGetInt32(out int lineIndex))
			{
				return false;
			}

			lineIndex = Math.Max(0, Math.Min(lineIndex, document.LineCount - 1));

			int startCharacter = startElement.TryGetProperty("character", out JsonElement characterElement)
				&& characterElement.TryGetInt32(out int character)
					? Math.Max(0, character)
					: 0;

			int endLineIndex = lineIndex;
			int endCharacter = startCharacter;

			if (rangeElement.TryGetProperty("end", out JsonElement endElement)
				&& endElement.ValueKind == JsonValueKind.Object
				&& endElement.TryGetProperty("line", out JsonElement endLineElement)
				&& endLineElement.TryGetInt32(out int rawEndLineIndex))
			{
				endLineIndex = Math.Max(lineIndex, Math.Min(rawEndLineIndex, document.LineCount - 1));

				if (endElement.TryGetProperty("character", out JsonElement endCharacterElement)
					&& endCharacterElement.TryGetInt32(out int endCharacterValue))
				{
					endCharacter = Math.Max(0, endCharacterValue);
				}
			}

			if (!TryGetDiagnosticOffsets(document, lineIndex, startCharacter, endLineIndex, endCharacter,
				out int startOffset, out int endOffset))
			{
				return false;
			}

			diagnostic = new TextEditorDiagnostic(severity, BuildDiagnosticMessage(diagnosticElement, severity), startOffset, endOffset);
			return true;
		}

		private static bool TryGetDiagnosticOffsets(TextDocument document,
			int startLineIndex, int startCharacter, int endLineIndex, int endCharacter,
			out int startOffset, out int endOffset)
		{
			startOffset = 0;
			endOffset = 0;

			if (document.LineCount == 0)
				return false;

			DocumentLine startLine = document.GetLineByNumber(startLineIndex + 1);
			DocumentLine endLine = document.GetLineByNumber(endLineIndex + 1);
			startOffset = startLine.Offset + Math.Max(0, Math.Min(startCharacter, startLine.Length));
			endOffset = endLine.Offset + Math.Max(0, Math.Min(endCharacter, endLine.Length));

			if (endOffset > startOffset)
				return true;

			string lineText = document.GetText(startLine);

			if (string.IsNullOrEmpty(lineText))
				return false;

			int safeCharacter = Math.Max(0, Math.Min(startCharacter, Math.Max(0, lineText.Length - 1)));

			if (TryGetWordBounds(lineText, safeCharacter, out int wordStart, out int wordEnd))
			{
				startOffset = startLine.Offset + wordStart;
				endOffset = startLine.Offset + wordEnd;
				return endOffset > startOffset;
			}

			int trimmedStart = 0;
			int trimmedEnd = lineText.Length;

			while (trimmedStart < trimmedEnd && char.IsWhiteSpace(lineText[trimmedStart]))
				trimmedStart++;

			while (trimmedEnd > trimmedStart && char.IsWhiteSpace(lineText[trimmedEnd - 1]))
				trimmedEnd--;

			if (trimmedEnd > trimmedStart)
			{
				startOffset = startLine.Offset + trimmedStart;
				endOffset = startLine.Offset + trimmedEnd;
				return true;
			}

			startOffset = startLine.Offset + safeCharacter;
			endOffset = Math.Min(startOffset + 1, document.TextLength);
			return endOffset > startOffset;
		}

		private static bool TryGetWordBounds(string lineText, int index, out int wordStart, out int wordEnd)
		{
			wordStart = 0;
			wordEnd = 0;

			if (string.IsNullOrEmpty(lineText))
				return false;

			int safeIndex = Math.Max(0, Math.Min(index, lineText.Length - 1));

			if (!IsDiagnosticSegmentCharacter(lineText[safeIndex]) && safeIndex > 0 && IsDiagnosticSegmentCharacter(lineText[safeIndex - 1]))
				safeIndex--;

			while (safeIndex < lineText.Length && !IsDiagnosticSegmentCharacter(lineText[safeIndex]))
			{
				safeIndex++;

				if (safeIndex >= lineText.Length)
					return false;
			}

			wordStart = safeIndex;
			wordEnd = safeIndex;

			while (wordStart > 0 && IsDiagnosticSegmentCharacter(lineText[wordStart - 1]))
				wordStart--;

			while (wordEnd < lineText.Length && IsDiagnosticSegmentCharacter(lineText[wordEnd]))
				wordEnd++;

			return wordEnd > wordStart;
		}

		private static bool IsDiagnosticSegmentCharacter(char c)
			=> char.IsLetterOrDigit(c) || c == '_' || c == '.' || c == ':' || c == '\'' || c == '"';

		private static TextEditorDiagnosticSeverity GetDiagnosticSeverity(JsonElement diagnosticElement)
			=> diagnosticElement.TryGetProperty("severity", out JsonElement severityElement)
				&& severityElement.TryGetInt32(out int severity)
				&& severity > 0
					? (TextEditorDiagnosticSeverity)severity
					: TextEditorDiagnosticSeverity.Warning;

		private static string BuildDiagnosticMessage(JsonElement diagnosticElement, TextEditorDiagnosticSeverity severity)
		{
			string message = diagnosticElement.TryGetProperty("message", out JsonElement messageElement)
				? messageElement.GetString()?.Trim()
				: null;

			if (string.IsNullOrWhiteSpace(message))
				message = "Unknown Lua diagnostic.";

			var builder = new StringBuilder();
			builder.Append(severity.GetLabel());
			builder.Append(": ");
			builder.Append(message);

			string source = diagnosticElement.TryGetProperty("source", out JsonElement sourceElement)
				? sourceElement.GetString()
				: null;

			string code = diagnosticElement.TryGetProperty("code", out JsonElement codeElement)
				? codeElement.ValueKind == JsonValueKind.String
					? codeElement.GetString()
					: codeElement.ValueKind == JsonValueKind.Number
						? codeElement.GetRawText()
						: null
				: null;

			if (!string.IsNullOrWhiteSpace(source) || !string.IsNullOrWhiteSpace(code))
			{
				builder.AppendLine();
				builder.AppendLine();
				builder.Append("Source: ");

				if (!string.IsNullOrWhiteSpace(source))
					builder.Append(source.Trim());
				else
					builder.Append("Lua language server");

				if (!string.IsNullOrWhiteSpace(code))
				{
					builder.Append(" (");
					builder.Append(code.Trim());
					builder.Append(')');
				}
			}

			return builder.ToString();
		}
	}

	internal sealed class LuaPublishedDiagnostics
	{
		public LuaPublishedDiagnostics(string filePath, IReadOnlyList<TextEditorDiagnostic> diagnostics, int version)
		{
			FilePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
			Diagnostics = diagnostics ?? Array.Empty<TextEditorDiagnostic>();
			Version = version;
		}

		public string FilePath { get; }
		public IReadOnlyList<TextEditorDiagnostic> Diagnostics { get; }
		public int Version { get; }
	}
}