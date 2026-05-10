using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using TombLib.Scripting.Objects;

namespace TombLib.Scripting.Lua.LanguageServer;

public static class LuaLanguageServerDiagnosticsParser
{
	/// <summary>
	/// Parses a LuaLS diagnostics notification into editor diagnostics for a tracked document.
	/// </summary>
	public static bool TryParse(LuaPublishDiagnosticsParams parameters, string filePath,
		string documentContent, int documentVersion, [NotNullWhen(true)] out LuaPublishedDiagnostics? publishedDiagnostics)
	{
		publishedDiagnostics = null;

		int diagnosticsVersion = parameters.Version is > 0 ? parameters.Version.Value : 0;

		if (diagnosticsVersion > 0 && documentVersion > 0 && diagnosticsVersion != documentVersion)
			return false;

		IReadOnlyList<TextEditorDiagnostic> diagnostics = parameters.Diagnostics is { Length: > 0 }
			? BuildDiagnostics(documentContent, parameters.Diagnostics)
			: [];

		publishedDiagnostics = new LuaPublishedDiagnostics(filePath, diagnostics, diagnosticsVersion);
		return true;
	}

	private static IReadOnlyList<TextEditorDiagnostic> BuildDiagnostics(string content, LuaDiagnosticPayload[] diagnosticsPayloads)
	{
		LuaDocumentLineOffsets lineOffsets = LuaDocumentLineOffsets.Build(content);
		var diagnostics = new List<TextEditorDiagnostic>();

		foreach (LuaDiagnosticPayload diagnosticElement in diagnosticsPayloads)
		{
			TextEditorDiagnosticSeverity severity = GetDiagnosticSeverity(diagnosticElement);

			if (severity > TextEditorDiagnosticSeverity.Warning)
				continue;

			if (!TryCreateDiagnostic(lineOffsets, diagnosticElement, severity, out TextEditorDiagnostic? diagnostic))
				continue;

			diagnostics.Add(diagnostic);
		}

		return [.. diagnostics
			.OrderBy(diagnostic => diagnostic.StartOffset)
			.ThenBy(diagnostic => diagnostic.Severity)];
	}

	private static bool TryCreateDiagnostic(LuaDocumentLineOffsets lineOffsets, LuaDiagnosticPayload diagnosticElement,
		TextEditorDiagnosticSeverity severity, [NotNullWhen(true)] out TextEditorDiagnostic? diagnostic)
	{
		diagnostic = null;

		if (lineOffsets.LineCount == 0
			|| diagnosticElement.Range is not { } rangeElement
			|| rangeElement.Start is not { } startElement
			|| startElement.Line is not int lineIndex)
		{
			return false;
		}

		lineIndex = Math.Max(0, Math.Min(lineIndex, lineOffsets.LineCount - 1));

		int startCharacter = startElement.Character is int character
			? Math.Max(0, character)
			: 0;

		int endLineIndex = lineIndex;
		int endCharacter = startCharacter;

		if (rangeElement.End is { } endElement && endElement.Line is int rawEndLineIndex)
		{
			endLineIndex = Math.Max(lineIndex, Math.Min(rawEndLineIndex, lineOffsets.LineCount - 1));

			if (endElement.Character is int endCharacterValue)
				endCharacter = Math.Max(0, endCharacterValue);
		}

		if (!TryGetDiagnosticOffsets(lineOffsets, lineIndex, startCharacter, endLineIndex, endCharacter,
			out int startOffset, out int endOffset))
		{
			return false;
		}

		diagnostic = new TextEditorDiagnostic(severity, BuildDiagnosticMessage(diagnosticElement, severity), startOffset, endOffset);
		return true;
	}

	private static bool TryGetDiagnosticOffsets(LuaDocumentLineOffsets lineOffsets,
		int startLineIndex, int startCharacter, int endLineIndex, int endCharacter,
		out int startOffset, out int endOffset)
	{
		startOffset = 0;
		endOffset = 0;

		if (lineOffsets.LineCount == 0)
			return false;

		startOffset = lineOffsets.GetOffset(startLineIndex, startCharacter);
		endOffset = lineOffsets.GetOffset(endLineIndex, endCharacter);

		if (endOffset > startOffset)
			return true;

		string lineText = lineOffsets.GetLineText(startLineIndex);
		int lineStartOffset = lineOffsets.GetLineStartOffset(startLineIndex);

		if (string.IsNullOrEmpty(lineText))
			return TryGetEmptyLineFallbackOffsets(lineOffsets, startLineIndex, out startOffset, out endOffset);

		int safeCharacter = Math.Max(0, Math.Min(startCharacter, Math.Max(0, lineText.Length - 1)));

		if (TryGetWordBounds(lineText, safeCharacter, out int wordStart, out int wordEnd))
		{
			startOffset = lineStartOffset + wordStart;
			endOffset = lineStartOffset + wordEnd;
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
			startOffset = lineStartOffset + trimmedStart;
			endOffset = lineStartOffset + trimmedEnd;
			return true;
		}

		startOffset = lineStartOffset + safeCharacter;
		endOffset = Math.Min(startOffset + 1, lineOffsets.TextLength);
		return endOffset > startOffset;
	}

	private static bool TryGetEmptyLineFallbackOffsets(LuaDocumentLineOffsets lineOffsets, int lineIndex,
		out int startOffset, out int endOffset)
	{
		startOffset = 0;
		endOffset = 0;

		for (int nextLineIndex = lineIndex + 1; nextLineIndex < lineOffsets.LineCount; nextLineIndex++)
		{
			if (lineOffsets.GetLineLength(nextLineIndex) == 0)
				continue;

			startOffset = lineOffsets.GetLineStartOffset(nextLineIndex);
			endOffset = Math.Min(startOffset + 1, lineOffsets.TextLength);
			return endOffset > startOffset;
		}

		for (int previousLineIndex = lineIndex - 1; previousLineIndex >= 0; previousLineIndex--)
		{
			int previousLineLength = lineOffsets.GetLineLength(previousLineIndex);

			if (previousLineLength == 0)
				continue;

			startOffset = lineOffsets.GetLineStartOffset(previousLineIndex) + previousLineLength - 1;
			endOffset = Math.Min(startOffset + 1, lineOffsets.TextLength);
			return endOffset > startOffset;
		}

		return false;
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

	private static TextEditorDiagnosticSeverity GetDiagnosticSeverity(LuaDiagnosticPayload diagnosticElement)
	{
		return diagnosticElement.Severity is int severity
			&& severity > 0
				? (TextEditorDiagnosticSeverity)severity
				: TextEditorDiagnosticSeverity.Warning;
	}

	private static string BuildDiagnosticMessage(LuaDiagnosticPayload diagnosticElement, TextEditorDiagnosticSeverity severity)
	{
		string? message = diagnosticElement.Message?.Trim();

		if (string.IsNullOrWhiteSpace(message))
			message = "Unknown Lua diagnostic.";

		var builder = new StringBuilder();
		builder.Append(severity.GetLabel());
		builder.Append(": ");
		builder.Append(message);

		string? source = diagnosticElement.Source;

		string? code = diagnosticElement.Code is { } codeElement
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
