using ICSharpCode.AvalonEdit.Document;
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
	#region Error handling

	public void SetDiagnostics(IReadOnlyList<TextEditorDiagnostic> diagnostics)
		=> _diagnosticToolTipService.SetDiagnostics(diagnostics);

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

	public void ShowDiagnosticToolTip(string message, TextEditorDiagnosticSeverity severity)
	{
		TextEditorToolTipHelper.GetDiagnosticToolTipColors(severity, out SolidColorBrush border, out SolidColorBrush background);
		ShowToolTip(message, border, background, ToolTipForeground);
	}

	protected bool TryShowDiagnosticToolTip(int hoveredOffset)
	{
		if (!TryGetDiagnosticInfo(hoveredOffset, out string? message, out TextEditorDiagnosticSeverity severity)
			|| string.IsNullOrWhiteSpace(message))
			return false;

		ShowDiagnosticToolTip(message, severity);
		return true;
	}

	protected bool HasDiagnosticsOnLine(DocumentLine line)
		=> _diagnosticToolTipService.HasDiagnosticsOnLine(Document, line);

	#endregion Error handling
}
