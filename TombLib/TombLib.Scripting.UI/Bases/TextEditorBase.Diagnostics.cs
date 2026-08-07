using ICSharpCode.AvalonEdit.Rendering;
using Nickelony.LanguageServer.Abstractions.Diagnostics;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;
using System.Windows.Media;
using TombLib.Scripting.UI.Diagnostics;
using TombLib.Scripting.UI.Rendering;

namespace TombLib.Scripting.UI.Bases;

public abstract partial class TextEditorBase
{
	// Error handling

	/// <summary>
	/// Sets the diagnostics displayed for the current document.
	/// </summary>
	/// <param name="diagnostics">The diagnostics to display.</param>
	public void SetDiagnostics(IReadOnlyList<TextEditorDiagnostic> diagnostics)
		=> _diagnosticToolTipService.SetDiagnostics(diagnostics);

	/// <summary>
	/// Clears all diagnostics currently displayed for the document.
	/// </summary>
	public void ClearDiagnostics()
		=> _diagnosticToolTipService.ClearDiagnostics();

	internal void InvalidateDiagnosticLayer()
	{
		TextArea.TextView.InvalidateLayer(KnownLayer.Background);
		TextArea.TextView.InvalidateLayer(KnownLayer.Selection);
		TextArea.TextView.InvalidateLayer(KnownLayer.Caret);
		TextArea.TextView.InvalidateVisual();
	}

	private void HandleErrorToolTips(MouseEventArgs e)
	{
		int hoveredOffset = GetOffsetFromPoint(e.GetPosition(this));

		if (hoveredOffset == -1)
			return;

		TryShowDiagnosticToolTip(hoveredOffset);
	}

	/// <summary>
	/// Attempts to retrieve diagnostic information for the given offset.
	/// </summary>
	/// <param name="hoveredOffset">The document offset to inspect.</param>
	/// <param name="message">The diagnostic message, when found.</param>
	/// <param name="severity">The diagnostic severity, when found.</param>
	/// <param name="allowLineFallback">Whether to fall back to a line-wide diagnostic.</param>
	/// <returns>True if diagnostic information was found; otherwise false.</returns>
	protected bool TryGetDiagnosticInfo(int hoveredOffset, [NotNullWhen(true)] out string? message, out TextEditorDiagnosticSeverity severity, bool allowLineFallback = true)
	{
		message = null;
		severity = TextEditorDiagnosticSeverity.Error;

		if (!_diagnosticToolTipService.TryGetDiagnosticInfo(Document, hoveredOffset, LiveErrorUnderlining, allowLineFallback, out TextDiagnosticToolTipInfo info))
			return false;

		message = info.Message;
		severity = info.Severity;
		return !string.IsNullOrWhiteSpace(message);
	}

	/// <summary>
	/// Shows a diagnostic tooltip with the given message and severity.
	/// </summary>
	/// <param name="message">The message to display.</param>
	/// <param name="severity">The severity that determines the tooltip colors.</param>
	public void ShowDiagnosticToolTip(string message, TextEditorDiagnosticSeverity severity)
	{
		TextEditorToolTipHelper.GetDiagnosticToolTipColors(severity, out SolidColorBrush border, out SolidColorBrush background);
		ShowToolTip(message, border, background, ToolTipForeground);
	}

	/// <summary>
	/// Attempts to show a diagnostic tooltip for the given offset.
	/// </summary>
	/// <param name="hoveredOffset">The document offset to inspect.</param>
	/// <returns>True if a diagnostic tooltip was shown; otherwise false.</returns>
	protected bool TryShowDiagnosticToolTip(int hoveredOffset)
	{
		if (!TryGetDiagnosticInfo(hoveredOffset, out string? message, out TextEditorDiagnosticSeverity severity)
			|| string.IsNullOrWhiteSpace(message))
			return false;

		ShowDiagnosticToolTip(message, severity);
		return true;
	}

}
