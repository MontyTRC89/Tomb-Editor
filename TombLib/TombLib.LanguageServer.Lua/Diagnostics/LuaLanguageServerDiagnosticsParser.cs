using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using TombLib.Scripting.Diagnostics;

namespace TombLib.LanguageServer.Lua;

/// <summary>
/// Parses Lua language-server diagnostics payloads into editor diagnostics tied to tracked document versions.
/// </summary>
internal static class LuaLanguageServerDiagnosticsParser
{
	/// <summary>
	/// Parses a LuaLS diagnostics notification into editor diagnostics for a tracked document.
	/// </summary>
	/// <param name="parameters">The published diagnostics notification payload from the language server.</param>
	/// <param name="filePath">The normalized file path of the tracked document.</param>
	/// <param name="documentContent">The current document content.</param>
	/// <param name="documentVersion">The tracked document version to match against the diagnostics version.</param>
	/// <param name="publishedDiagnostics">When this method returns <see langword="true"/>, contains the parsed diagnostics payload.</param>
	/// <returns><see langword="true"/> when the payload was parsed; otherwise, <see langword="false"/>.</returns>
	internal static bool TryParse(PublishDiagnosticsParams parameters, string filePath,
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

	private static IReadOnlyList<TextEditorDiagnostic> BuildDiagnostics(string content, DiagnosticPayload[] diagnosticsPayloads)
	{
		DocumentLineOffsets lineOffsets = DocumentLineOffsets.Build(content);
		var diagnostics = new List<TextEditorDiagnostic>();

		foreach (DiagnosticPayload diagnosticElement in diagnosticsPayloads)
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

	private static bool TryCreateDiagnostic(DocumentLineOffsets lineOffsets, DiagnosticPayload diagnosticElement,
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

		if (!DocumentRangeOffsetResolver.TryResolveOffsets(lineOffsets, lineIndex, startCharacter, endLineIndex, endCharacter,
			out int startOffset, out int endOffset))
		{
			return false;
		}

		diagnostic = new TextEditorDiagnostic(severity, BuildDiagnosticMessage(diagnosticElement, severity), startOffset, endOffset);
		return true;
	}

	private static TextEditorDiagnosticSeverity GetDiagnosticSeverity(DiagnosticPayload diagnosticElement)
	{
		return diagnosticElement.Severity is int severity
			&& severity > 0
				? (TextEditorDiagnosticSeverity)severity
				: TextEditorDiagnosticSeverity.Warning;
	}

	private static string BuildDiagnosticMessage(DiagnosticPayload diagnosticElement, TextEditorDiagnosticSeverity severity)
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
