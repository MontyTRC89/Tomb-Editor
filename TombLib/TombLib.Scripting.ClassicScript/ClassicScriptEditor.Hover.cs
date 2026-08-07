#nullable enable

using Nickelony.LanguageServer.Abstractions.Diagnostics;
using Nickelony.LanguageServer.Abstractions.Hover;
using System.Diagnostics.CodeAnalysis;
using TombLib.Scripting.ClassicScript.Navigation;
using TombLib.Scripting.Hover;
using TombLib.Scripting.UI.Hover;
using TombLib.Scripting.UI.Presentation;

namespace TombLib.Scripting.ClassicScript;

public sealed partial class ClassicScriptEditor
{
	private sealed class ClassicScriptHoverController
	{
		private readonly ClassicScriptEditor _editor;
		private WordDefinitionEventArgs? _hoveredWordArgs;

		internal ClassicScriptHoverController(ClassicScriptEditor editor)
		{
			_editor = editor;
		}

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

		internal TextHoverRequestState BuildRequestState(int hoveredOffset)
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

		internal Task<TextHoverInfo?> RequestAsync(int hoveredOffset, CancellationToken cancellationToken)
			=> Task.FromResult(_editor._languageServices.HoverProvider.GetHoverInfo(new TextHoverRequest(_editor.Document.Text, hoveredOffset)));

		internal void ApplyHoverState(TextHoverPresentationState state)
			=> _hoveredWordArgs = state.HoverInfo is null ? null : CreateWordDefinitionArgs(state.HoverInfo, state.HoveredOffset);

		private static WordDefinitionEventArgs? CreateWordDefinitionArgs(TextHoverInfo hoverInfo, int hoveredOffset)
		{
			if (string.IsNullOrWhiteSpace(hoverInfo.SymbolName) || hoverInfo.Identifier is not WordType wordType)
				return null;

			return new WordDefinitionEventArgs(hoverInfo.SymbolName, wordType, hoveredOffset);
		}
	}
}
