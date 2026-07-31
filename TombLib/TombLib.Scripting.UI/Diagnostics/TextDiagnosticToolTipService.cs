#nullable enable

using ICSharpCode.AvalonEdit.Document;
using System;
using System.Collections.Generic;
using System.Linq;
using Nickelony.LanguageServer.Core.Diagnostics;

namespace TombLib.Scripting.UI.Diagnostics;

internal sealed class TextDiagnosticToolTipService
{
	private readonly Action? _onDiagnosticsChanged;
	private IReadOnlyList<TextEditorDiagnostic> _diagnostics = Array.Empty<TextEditorDiagnostic>();

	public TextDiagnosticToolTipService(Action? onDiagnosticsChanged = null)
		=> _onDiagnosticsChanged = onDiagnosticsChanged;

	public IReadOnlyList<TextEditorDiagnostic> Diagnostics => _diagnostics;

	public void SetDiagnostics(IReadOnlyList<TextEditorDiagnostic>? diagnostics)
	{
		_diagnostics = diagnostics ?? Array.Empty<TextEditorDiagnostic>();
		_onDiagnosticsChanged?.Invoke();
	}

	public bool ClearDiagnostics()
	{
		if (_diagnostics.Count == 0)
			return false;

		_diagnostics = Array.Empty<TextEditorDiagnostic>();
		_onDiagnosticsChanged?.Invoke();
		return true;
	}

	public bool TryGetDiagnosticInfo(
		TextDocument document,
		int hoveredOffset,
		bool liveErrorUnderlining,
		bool allowLineFallback,
		out TextDiagnosticToolTipInfo info)
	{
		ArgumentNullException.ThrowIfNull(document);

		info = default;

		if (!liveErrorUnderlining || _diagnostics.Count == 0)
			return false;

		List<TextEditorDiagnostic> hoveredDiagnostics = GetDiagnosticsAtOffset(hoveredOffset);

		if (hoveredDiagnostics.Count == 0 && allowLineFallback)
			hoveredDiagnostics = GetDiagnosticsForLine(document, document.GetLineByOffset(hoveredOffset));

		if (hoveredDiagnostics.Count == 0)
			return false;

		TextEditorDiagnosticSeverity severity = hoveredDiagnostics
			.OrderBy(diagnostic => diagnostic.Severity)
			.Select(diagnostic => diagnostic.Severity)
			.First();

		string message = string.Join(Environment.NewLine + Environment.NewLine,
			hoveredDiagnostics
				.OrderBy(diagnostic => diagnostic.Severity)
				.ThenBy(diagnostic => diagnostic.StartOffset)
				.Select(FormatDiagnosticMessage)
				.Distinct(StringComparer.Ordinal));

		if (string.IsNullOrWhiteSpace(message))
			return false;

		info = new TextDiagnosticToolTipInfo(message, severity);
		return true;
	}

	public bool HasDiagnosticsOnLine(TextDocument document, DocumentLine? line)
	{
		ArgumentNullException.ThrowIfNull(document);

		return line is not null && GetDiagnosticsForLine(document, line).Count > 0;
	}

	private List<TextEditorDiagnostic> GetDiagnosticsAtOffset(int offset)
		=> _diagnostics
			.Where(diagnostic => diagnostic.ContainsOffset(offset))
			.ToList();

	private List<TextEditorDiagnostic> GetDiagnosticsForLine(TextDocument document, DocumentLine? line)
	{
		if (line is null || _diagnostics.Count == 0)
			return [];

		int endOffset = Math.Max(line.EndOffset, line.Offset + 1);

		return _diagnostics
			.Where(diagnostic => diagnostic.Intersects(line.Offset, endOffset))
			.ToList();
	}

	private static string FormatDiagnosticMessage(TextEditorDiagnostic diagnostic)
	{
		if (string.IsNullOrWhiteSpace(diagnostic.Message))
			return string.Empty;

		if (IsSeverityPrefixed(diagnostic.Message))
			return diagnostic.Message;

		return diagnostic.Severity.GetLabel() + ":\n" + diagnostic.Message;
	}

	private static bool IsSeverityPrefixed(string message)
		=> !string.IsNullOrWhiteSpace(message)
			&& (message.StartsWith("Error:", StringComparison.OrdinalIgnoreCase)
				|| message.StartsWith("Warning:", StringComparison.OrdinalIgnoreCase)
				|| message.StartsWith("Information:", StringComparison.OrdinalIgnoreCase)
				|| message.StartsWith("Hint:", StringComparison.OrdinalIgnoreCase)
				|| message.StartsWith("Diagnostic:", StringComparison.OrdinalIgnoreCase));
}

internal readonly record struct TextDiagnosticToolTipInfo(
	string Message,
	TextEditorDiagnosticSeverity Severity);
