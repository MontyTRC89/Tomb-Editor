using ICSharpCode.AvalonEdit.Document;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using TombLib.Scripting.Lua.Parsing;

namespace TombLib.Scripting.Lua;

public sealed partial class LuaEditor
{
	private Window? _hostWindow;

	private void BindLuaIntellisenseEvents()
	{
		Document.Changed += LuaEditor_DocumentChanged;
		IsKeyboardFocusWithinChanged += LuaEditor_IsKeyboardFocusWithinChanged;
		Loaded += LuaEditor_Loaded;
		AddHandler(PreviewMouseDownEvent, new MouseButtonEventHandler(TextEditor_PreviewMouseDown), true);
		Unloaded += LuaEditor_Unloaded;
	}

	private void LuaEditor_Loaded(object? sender, RoutedEventArgs e)
		=> AttachHostWindowHandlers();

	private void LuaEditor_DocumentChanged(object? sender, DocumentChangeEventArgs e)
		=> ClearDiagnostics();

	protected override void OnLanguageTextChanged(EventArgs e)
	{
		_editorDocumentVersion++;
		RebaseOpenCompletionItems();
	}

	private void LuaEditor_IsKeyboardFocusWithinChanged(object? sender, DependencyPropertyChangedEventArgs e)
	{
		if (e.NewValue is bool hasKeyboardFocus && !hasKeyboardFocus)
		{
			CloseCompletionWindow();
			DismissTransientToolTips();
		}
	}

	private void AttachHostWindowHandlers()
	{
		Window? window = Window.GetWindow(this);

		if (window == _hostWindow)
			return;

		if (_hostWindow is not null)
			_hostWindow.Deactivated -= HostWindow_Deactivated;

		_hostWindow = window;

		if (_hostWindow is not null)
			_hostWindow.Deactivated += HostWindow_Deactivated;
	}

	private void HostWindow_Deactivated(object? sender, EventArgs e)
	{
		CloseCompletionWindow();
		DismissTransientToolTips();
	}

	private void LuaEditor_Unloaded(object? sender, RoutedEventArgs e)
	{
		_textMateHighlighting?.Dispose();
		_textMateHighlighting = null;
		InvalidateAsyncEditorRequests();

		CancelPendingCompletionRequest();
		_hoverController.CancelPendingRequest();

		CompletionController.CancelTooltipUpdate();

		_definitionNavigationController.CancelPendingRequest();
		CloseCompletionWindow();

		if (_hostWindow is not null)
			_hostWindow.Deactivated -= HostWindow_Deactivated;

		_hostWindow = null;

		DismissTransientToolTips();
		ClearDiagnostics();
		ClearSemanticTokens();

		IntellisenseProvider?.CloseDocument(FilePath);
	}

	protected override async void OnLanguageTextEntering(TextCompositionEventArgs e)
	{
		if (!AutocompleteEnabled || !IsIntellisenseAvailable())
			return;

		if (e.Text == " " && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
		{
			e.Handled = true;
			CancelPendingCompletionRequest();

			if (LuaEditorInteractionRules.IsValidManualCompletionContext(Document, CaretOffset))
				await RequestCompletionAsync(CaretOffset, null).ConfigureAwait(true);
		}
	}

	protected override async void OnLanguageTextEntered(TextCompositionEventArgs e)
	{
		if (!IsIntellisenseAvailable())
			return;

		if (e.Text == "(" || e.Text == ",")
		{
			CancelPendingCompletionRequest();
			CloseCompletionWindow();

			if (!SignatureHelpPopupsEnabled)
			{
				DismissSignatureHelp();
				return;
			}

			_signatureHelpController.CancelPendingRefresh();
			await RequestSignatureHelpAsync(CaretOffset).ConfigureAwait(true);
			return;
		}

		if (e.Text == ")")
		{
			CancelPendingCompletionRequest();
			CloseCompletionWindow();
			DismissSignatureHelp();
			return;
		}

		if (IsCompletionWindowOpen)
		{
			CancelPendingCompletionRequest();

			if (!ShouldKeepCompletionWindowOpen(e.Text))
			{
				CloseCompletionWindow();
			}
			else
			{
				ScheduleCloseIfEmpty();
				return;
			}
		}

		if (SignatureHelpPopupsEnabled && ShouldRefreshSignatureHelpAfterTextInput(e.Text))
			ScheduleSignatureHelpRefresh();

		if (!AutocompleteEnabled)
			return;

		if (TryGetCompletionTrigger(e.Text, out char? triggerCharacter)
			&& LuaEditorInteractionRules.IsValidAutocompleteContext(Document, CaretOffset, triggerCharacter))
		{
			if (triggerCharacter is null)
			{
				ScheduleCompletionRequest();
			}
			else
			{
				CancelPendingCompletionRequest();
				await RequestCompletionAsync(CaretOffset, triggerCharacter).ConfigureAwait(true);
			}
		}
	}

	[MemberNotNullWhen(true, nameof(IntellisenseProvider))]
	private bool IsIntellisenseAvailable()
		=> IntellisenseProvider?.IsAvailable == true && !string.IsNullOrWhiteSpace(FilePath);

	private static bool TryGetCompletionTrigger(string? inputText, out char? triggerCharacter)
	{
		triggerCharacter = null;

		if (string.IsNullOrEmpty(inputText) || inputText.Length != 1)
			return false;

		char typedChar = inputText[0];

		if (typedChar == '.' || typedChar == ':')
		{
			triggerCharacter = typedChar;
			return true;
		}

		return LuaLineParser.IsIdentifierTriggerCharacter(typedChar);
	}

	private static bool ShouldKeepCompletionWindowOpen(string? inputText)
		=> inputText?.Length == 1 && LuaLineParser.IsIdentifierCharacter(inputText[0]);

	private void DismissTransientToolTips()
	{
		CancelPendingCompletionRequest();
		_hoverController.CancelPendingRequest();
		_hoverController.InvalidateRequests();
		DismissSignatureHelp();
		CloseDefinitionToolTip(true);
	}

	private (int Line, int Column) GetPositionFromOffset(int offset)
	{
		int safeOffset = Math.Max(0, Math.Min(offset, Document.TextLength));
		TextLocation location = Document.GetLocation(safeOffset);

		return (location.Line - 1, location.Column - 1);
	}

	private static CancellationToken ResetCancellationTokenSource(ref CancellationTokenSource? cancellationTokenSource)
	{
		CancelAndDispose(ref cancellationTokenSource);
		cancellationTokenSource = new CancellationTokenSource();
		return cancellationTokenSource.Token;
	}

	private static void CancelAndDispose(ref CancellationTokenSource? cancellationTokenSource)
	{
		if (cancellationTokenSource is null)
			return;

		cancellationTokenSource.Cancel();
		cancellationTokenSource.Dispose();
		cancellationTokenSource = null;
	}

	private static void LogEditorFailure(string area, Exception exception)
		=> Log.Warn(exception, "Lua editor operation '{Area}' failed.", area);

	protected override void OnAutoClosingElementSkipped(string element)
	{
		if (!ShouldDismissSignatureHelpOnAutoClosingSkip(element, ParenthesesClosingString))
			return;

		CancelPendingCompletionRequest();
		CloseCompletionWindow();
		DismissSignatureHelp();
	}

	private void InvalidateAsyncEditorRequests()
	{
		_editorRequestGeneration++;
		CompletionController.InvalidateRequests();
		_hoverController.InvalidateRequests();
		_signatureHelpController.InvalidateRequests();
		_definitionNavigationController.InvalidateRequests();
	}

	private bool IsAsyncEditorResultCurrent(CancellationToken cancellationToken,
		int requestToken,
		int currentRequestToken,
		int requestDocumentVersion,
		int requestGeneration)
	{
		return IsAsyncEditorResultCurrent(
			cancellationToken.IsCancellationRequested,
			requestToken,
			currentRequestToken,
			requestDocumentVersion,
			_editorDocumentVersion,
			requestGeneration,
			_editorRequestGeneration,
			IsLoaded,
			IsIntellisenseAvailable());
	}

	private static bool IsAsyncEditorResultCurrent(bool isCancellationRequested,
		int requestToken,
		int currentRequestToken,
		int requestDocumentVersion,
		int currentDocumentVersion,
		int requestGeneration,
		int currentGeneration,
		bool isEditorLoaded,
		bool isIntellisenseAvailable)
	{
		return !isCancellationRequested
			&& requestToken == currentRequestToken
			&& requestDocumentVersion == currentDocumentVersion
			&& requestGeneration == currentGeneration
			&& isEditorLoaded
			&& isIntellisenseAvailable;
	}

	private bool ShouldRefreshSignatureHelpAfterTextInput(string? inputText)
		=> ShouldRefreshSignatureHelpAfterTextInput(inputText, _signatureHelpController.IsActiveOrPending);

	private static bool ShouldRefreshSignatureHelpAfterTextInput(string? inputText, bool isSignatureHelpActiveOrPending)
		=> isSignatureHelpActiveOrPending && inputText?.Length == 1;

	private static bool ShouldDismissSignatureHelpOnAutoClosingSkip(string element, string parenthesesClosingString)
		=> string.Equals(element, parenthesesClosingString, StringComparison.Ordinal);
}
