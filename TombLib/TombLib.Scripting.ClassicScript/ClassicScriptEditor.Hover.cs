#nullable enable

using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using TombLib.Scripting.Diagnostics;
using TombLib.Scripting.Hover;
using TombLib.Scripting.ClassicScript.Navigation;
using TombLib.Scripting.ClassicScript.Parsers;
using TombLib.Scripting.UI.Hover;
using TombLib.Scripting.UI.Presentation;
using TombLib.Scripting.UI.Rendering;

namespace TombLib.Scripting.ClassicScript;

public sealed partial class ClassicScriptEditor
{
	private sealed class ClassicScriptHoverController
	{
		private readonly ClassicScriptEditor _editor;
		private readonly TextHoverController _controller;
		private WordDefinitionEventArgs? _hoveredWordArgs;

		internal ClassicScriptHoverController(ClassicScriptEditor editor)
		{
			_editor = editor;
			_controller = new TextHoverController(
				owner: editor,
				getOffsetFromPoint: editor.GetOffsetFromPoint,
				buildRequestState: BuildRequestState,
				requestHoverAsync: RequestAsync,
				getCurrentRequestOffset: hoveredOffset => hoveredOffset,
				showDiagnosticToolTip: editor.ShowDiagnosticToolTip,
				showHoverToolTip: ShowHoverToolTip,
				showCombinedToolTip: ShowCombinedToolTip,
				applyHoverState: ApplyHoverState);
		}

		internal Task HandleMouseHoverAsync(MouseEventArgs e)
			=> _controller.HandleMouseHoverAsync(e);

		internal bool TryGetRequestedDefinitionArgs(int caretOffset, bool specialToolTipIsOpen, [NotNullWhen(true)] out WordDefinitionEventArgs? definitionArgs)
		{
			if (specialToolTipIsOpen && _hoveredWordArgs is not null)
			{
				definitionArgs = _hoveredWordArgs;
				_hoveredWordArgs = null;
				return true;
			}

			return TryGetWordDefinitionArgs(caretOffset, out definitionArgs);
		}

		private bool TryGetWordDefinitionArgs(int hoveredOffset, [NotNullWhen(true)] out WordDefinitionEventArgs? definitionArgs)
		{
			definitionArgs = null;

			TextHoverInfo? hoverInfo = _editor._languageServices.HoverProvider.GetHoverInfo(new TextHoverRequest(_editor.Document.Text, hoveredOffset));

			if (hoverInfo is null)
				return false;

			definitionArgs = CreateWordDefinitionArgs(hoverInfo, hoveredOffset);
			return definitionArgs is not null;
		}

		private TextHoverRequestState BuildRequestState(int hoveredOffset)
		{
			bool hasDiagnostic = _editor.TryGetDiagnosticInfo(hoveredOffset, out string? diagnosticMessage, out TextEditorDiagnosticSeverity diagnosticSeverity, allowLineFallback: false);

			return new TextHoverRequestState(
				ShouldRequestHover: true,
				RequestOffset: hoveredOffset,
				CanShowToolTip: true,
				CanShowDiagnosticFallback: false,
				HasDiagnostic: hasDiagnostic,
				DiagnosticMessage: diagnosticMessage,
				DiagnosticSeverity: diagnosticSeverity);
		}

		private Task<TextHoverInfo?> RequestAsync(int hoveredOffset, CancellationToken cancellationToken)
			=> Task.FromResult(_editor._languageServices.HoverProvider.GetHoverInfo(new TextHoverRequest(_editor.Document.Text, hoveredOffset)));

		private void ShowHoverToolTip(TextHoverInfo hoverInfo)
			=> _editor.ShowToolTip(
				TextHoverToolTipContentFactory.CreateHoverContent(hoverInfo, ToolTipForeground, DefaultToolTipBackground),
				DefaultToolTipBorder,
				DefaultToolTipBackground);

		private void ShowCombinedToolTip(TextHoverInfo hoverInfo, string diagnosticMessage, TextEditorDiagnosticSeverity diagnosticSeverity)
			=> _editor.ShowToolTip(
				TextHoverToolTipContentFactory.CreateCombinedContent(
					hoverInfo,
					diagnosticMessage,
					diagnosticSeverity,
					ToolTipForeground,
					DefaultToolTipBackground,
					ToolTipTextMaxWidth,
					ToolTipTextFontSize,
					GetDiagnosticColors),
				DefaultToolTipBorder,
				DefaultToolTipBackground);

		private void ApplyHoverState(TextHoverPresentationState state)
			=> _hoveredWordArgs = state.HoverInfo is null ? null : CreateWordDefinitionArgs(state.HoverInfo, state.HoveredOffset);

		private static (SolidColorBrush Border, SolidColorBrush Background) GetDiagnosticColors(TextEditorDiagnosticSeverity severity)
		{
			GetDiagnosticToolTipColors(severity, out SolidColorBrush border, out SolidColorBrush background);
			return (border, background);
		}

		private static WordDefinitionEventArgs? CreateWordDefinitionArgs(TextHoverInfo hoverInfo, int hoveredOffset)
		{
			if (string.IsNullOrWhiteSpace(hoverInfo.SymbolName) || hoverInfo.Identifier is not WordType wordType)
				return null;

			return new WordDefinitionEventArgs(hoverInfo.SymbolName, wordType, hoveredOffset);
		}
	}
}