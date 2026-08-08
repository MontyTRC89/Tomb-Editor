using Nickelony.LanguageServer.Abstractions.Diagnostics;
using Nickelony.LanguageServer.Abstractions.Hover;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using TombLib.Scripting.Diagnostics;
using TombLib.Scripting.Hover;
using TombLib.Scripting.Navigation;
using TombLib.Scripting.Presentation;
using TombLib.Scripting.UI.Completion;
using TombLib.Scripting.UI.Diagnostics;
using TombLib.Scripting.UI.Hover;
using TombLib.Scripting.UI.Navigation;

namespace TombLib.Scripting.UI.Bases;

public abstract partial class TextEditorBase
{
	// Language seam

	/// <summary>
	/// Gets the completion controller that drives completion windows and decisions for this editor.
	/// </summary>
	protected TextCompletionController CompletionController { get; }

	/// <summary>
	/// Initializes definition navigation for the language.
	/// </summary>
	/// <param name="tryNavigateAsync">The callback used to resolve and navigate to a definition.</param>
	protected void InitializeDefinitionNavigation(Func<int, CancellationToken, Task<bool>> tryNavigateAsync)
		=> _definitionTriggerController = new TextDefinitionTriggerController(this, GetOffsetFromPoint, tryNavigateAsync);

	/// <summary>
	/// Initializes hover tooltips for the language.
	/// </summary>
	/// <param name="buildRequestState">Builds the hover request state for a hovered offset.</param>
	/// <param name="requestHoverAsync">Requests the hover content for a hovered offset.</param>
	/// <param name="applyHoverState">Applies a resolved hover state (optional).</param>
	protected void InitializeHover(
		Func<int, TextHoverRequestState> buildRequestState,
		Func<int, CancellationToken, Task<TextHoverInfo?>> requestHoverAsync,
		Action<TextHoverPresentationState>? applyHoverState = null)
		=> _hoverController = HoverControllerFactory.Create(this, buildRequestState, requestHoverAsync, applyHoverState);

	/// <summary>
	/// Gets whether hover tooltips may fall back to diagnostic tooltips for this language.
	/// </summary>
	protected virtual bool CanShowDiagnosticFallback => false;

	/// <summary>
	/// Builds the standard hover request state that always requests hover for the hovered offset.
	/// </summary>
	protected TextHoverRequestState BuildStandardHoverRequestState(int hoveredOffset)
	{
		bool hasDiagnostic = TryGetDiagnosticInfo(hoveredOffset, out string? diagnosticMessage, out TextEditorDiagnosticSeverity diagnosticSeverity);

		return new TextHoverRequestState(
			ShouldRequestHover: true,
			RequestOffset: hoveredOffset,
			CanShowToolTip: true,
			CanShowDiagnosticFallback: CanShowDiagnosticFallback,
			HasDiagnostic: hasDiagnostic,
			DiagnosticMessage: diagnosticMessage,
			DiagnosticSeverity: diagnosticSeverity);
	}

	/// <summary>
	/// Initializes background error detection for the language.
	/// </summary>
	/// <param name="engineVersion">The engine version diagnostics should target.</param>
	/// <param name="diagnosticsProvider">The provider used to source diagnostics (optional).</param>
	protected void InitializeDiagnostics(Version engineVersion, ITextDiagnosticsProvider? diagnosticsProvider = null)
		=> _diagnosticsCoordinator = new TextDiagnosticsCoordinator(this, engineVersion, diagnosticsProvider);

	/// <summary>
	/// Called synchronously when text is being entered into the editor.
	/// </summary>
	protected virtual void OnLanguageTextEntering(TextCompositionEventArgs e) { }

	/// <summary>
	/// Called synchronously after text has been entered into the editor.
	/// </summary>
	protected virtual void OnLanguageTextEntered(TextCompositionEventArgs e) { }

	/// <summary>
	/// Handles language-specific key input, including definition navigation when initialized.
	/// </summary>
	protected virtual async Task OnLanguageKeyDown(KeyEventArgs e)
	{
		if (_definitionTriggerController is null)
			return;

		await _definitionTriggerController.TryHandleKeyDownAsync(e, CaretOffset).ConfigureAwait(true);
	}

	/// <summary>
	/// Handles language-specific mouse input for definition navigation when initialized.
	/// </summary>
	protected virtual async Task OnLanguagePreviewMouseLeftButtonDown(MouseButtonEventArgs e)
	{
		if (_definitionTriggerController is null)
			return;

		await _definitionTriggerController.TryHandlePointerNavigationAsync(e).ConfigureAwait(true);
	}

	/// <summary>
	/// Handles hover tooltips and error tooltips for the language.
	/// </summary>
	protected virtual async Task OnLanguageMouseHover(MouseEventArgs e)
	{
		await HandleMouseHover(e).ConfigureAwait(true);

		if (_hoverController is null)
			return;

		await _hoverController.HandleMouseHoverAsync(e).ConfigureAwait(true);
	}

	/// <summary>
	/// Handles document changes, including live error re-checking when initialized.
	/// </summary>
	protected virtual void OnLanguageTextChanged(EventArgs e)
	{
		if (_diagnosticsCoordinator is null || !LiveErrorUnderlining)
			return;

		_diagnosticsCoordinator.RunOnIdle(Text);
	}

	// Definition navigation

	/// <summary>
	/// Navigates to the definition of the given object name using the specified provider.
	/// </summary>
	/// <param name="definitionProvider">The provider used to resolve the definition.</param>
	/// <param name="objectName">The name of the object to navigate to.</param>
	/// <param name="identifyingObject">An optional discriminator used to disambiguate the target.</param>
	/// <returns>True if a definition was found and navigated to; otherwise false.</returns>
	protected bool GoToDefinition(ITextDefinitionProvider definitionProvider, string objectName, TextDefinitionDiscriminator? identifyingObject = null)
		=> _definitionNavigationService.TryGoToObject(this, definitionProvider, objectName, identifyingObject);

	/// <summary>
	/// Attempts to navigate to the definition at the given offset using the specified providers.
	/// </summary>
	/// <param name="definitionProvider">The provider used to resolve the definition.</param>
	/// <param name="hoverProvider">The provider used to identify the hovered symbol.</param>
	/// <param name="offset">The document offset to inspect.</param>
	/// <returns>True if a definition was found and navigated to; otherwise false.</returns>
	protected bool TryGoToDefinition(ITextDefinitionProvider definitionProvider, ITextHoverProvider hoverProvider, int offset)
		=> _definitionNavigationService.TryGoToDefinition(this, definitionProvider, hoverProvider, offset);
}
