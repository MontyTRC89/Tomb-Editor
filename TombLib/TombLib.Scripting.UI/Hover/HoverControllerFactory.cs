#nullable enable

using Nickelony.LanguageServer.Abstractions.Diagnostics;
using Nickelony.LanguageServer.Abstractions.Hover;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using TombLib.Scripting.UI.Bases;
using TombLib.Scripting.UI.Presentation;
using TombLib.Scripting.UI.Rendering;

namespace TombLib.Scripting.UI.Hover;

/// <summary>
/// Creates <see cref="TextHoverController"/> instances with standard tooltip presentation wiring,
/// eliminating boilerplate across language-specific editor projects.
/// </summary>
public static class HoverControllerFactory
{
	/// <summary>
	/// Creates a <see cref="TextHoverController"/> wired with the standard hover and combined tooltip presentation.
	/// </summary>
	/// <param name="editor">The text editor that owns the controller.</param>
	/// <param name="buildRequestState">Produces the hover request state for a given offset.</param>
	/// <param name="requestHoverAsync">Resolves hover information asynchronously for a given offset.</param>
	/// <param name="applyHoverState">Optional callback invoked with the final presentation state after each hover resolution.</param>
	public static TextHoverController Create(
		TextEditorBase editor,
		Func<int, TextHoverRequestState> buildRequestState,
		Func<int, CancellationToken, Task<TextHoverInfo?>> requestHoverAsync,
		Action<TextHoverPresentationState>? applyHoverState = null)
	{
		ArgumentNullException.ThrowIfNull(editor);
		ArgumentNullException.ThrowIfNull(buildRequestState);
		ArgumentNullException.ThrowIfNull(requestHoverAsync);

		return new TextHoverController(
			owner: editor,
			getOffsetFromPoint: editor.GetOffsetFromPoint,
			buildRequestState: buildRequestState,
			requestHoverAsync: requestHoverAsync,
			getCurrentRequestOffset: hoveredOffset => hoveredOffset,
			showDiagnosticToolTip: editor.ShowDiagnosticToolTip,
			showHoverToolTip: hoverInfo => ShowStandardHoverToolTip(editor, hoverInfo),
			showCombinedToolTip: (hoverInfo, diagnosticMessage, diagnosticSeverity) =>
				ShowStandardCombinedToolTip(editor, hoverInfo, diagnosticMessage, diagnosticSeverity),
			applyHoverState: applyHoverState);
	}

	/// <summary>
	/// Shows a standard hover tooltip populated from <see cref="TextHoverToolTipContentFactory.CreateHoverContent"/>.
	/// </summary>
	public static void ShowStandardHoverToolTip(TextEditorBase editor, TextHoverInfo hoverInfo)
	{
		ArgumentNullException.ThrowIfNull(editor);
		ArgumentNullException.ThrowIfNull(hoverInfo);

		editor.ShowToolTip(
			TextHoverToolTipContentFactory.CreateHoverContent(hoverInfo, TextEditorBase.ToolTipForeground, TextEditorBase.DefaultToolTipBackground),
			TextEditorBase.DefaultToolTipBorder,
			TextEditorBase.DefaultToolTipBackground);
	}

	/// <summary>
	/// Shows a combined hover + diagnostic tooltip populated from <see cref="TextHoverToolTipContentFactory.CreateCombinedContent"/>.
	/// </summary>
	public static void ShowStandardCombinedToolTip(
		TextEditorBase editor,
		TextHoverInfo hoverInfo,
		string diagnosticMessage,
		TextEditorDiagnosticSeverity diagnosticSeverity)
	{
		ArgumentNullException.ThrowIfNull(editor);
		ArgumentNullException.ThrowIfNull(hoverInfo);
		ArgumentNullException.ThrowIfNull(diagnosticMessage);

		editor.ShowToolTip(
			TextHoverToolTipContentFactory.CreateCombinedContent(
				hoverInfo,
				diagnosticMessage,
				diagnosticSeverity,
				TextEditorBase.ToolTipForeground,
				TextEditorBase.DefaultToolTipBackground,
				TextEditorBase.ToolTipTextMaxWidth,
				TextEditorBase.ToolTipTextFontSize,
				GetDiagnosticColors),
			TextEditorBase.DefaultToolTipBorder,
			TextEditorBase.DefaultToolTipBackground);
	}

	/// <summary>
	/// Resolves diagnostic tooltip border and background colors for the given severity.
	/// </summary>
	public static (SolidColorBrush Border, SolidColorBrush Background) GetDiagnosticColors(TextEditorDiagnosticSeverity severity)
	{
		TextEditorToolTipHelper.GetDiagnosticToolTipColors(severity, out SolidColorBrush border, out SolidColorBrush background);
		return (border, background);
	}
}
