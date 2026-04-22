using ICSharpCode.AvalonEdit.Document;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using TombLib.Scripting.Lua.Utils;

namespace TombLib.Scripting.Lua;

public sealed partial class LuaEditor
{
	private Window? _hostWindow;

	private void BindLuaIntellisenseEvents()
	{
		IsKeyboardFocusWithinChanged += LuaEditor_IsKeyboardFocusWithinChanged;
		Loaded += LuaEditor_Loaded;
		TextArea.TextEntering += TextArea_TextEntering;
		TextArea.TextEntered += TextArea_TextEntered;
		AddHandler(PreviewKeyDownEvent, new KeyEventHandler(TextEditor_KeyDown), true);
		AddHandler(PreviewMouseDownEvent, new MouseButtonEventHandler(TextEditor_PreviewMouseDown), true);
		AddHandler(PreviewMouseLeftButtonDownEvent, new MouseButtonEventHandler(TextEditor_PreviewMouseLeftButtonDown), true);
		Unloaded += LuaEditor_Unloaded;
	}

	private void LuaEditor_Loaded(object? sender, RoutedEventArgs e)
		=> AttachHostWindowHandlers();

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

		CancelAndDispose(ref _hoverCancellationTokenSource);
		CancelAndDispose(ref _completionCancellationTokenSource);

		CancelCompletionToolTipUpdate();

		CancelAndDispose(ref _signatureCancellationTokenSource);
		CloseCompletionWindow();

		if (_hostWindow is not null)
			_hostWindow.Deactivated -= HostWindow_Deactivated;

		_hostWindow = null;

		DismissTransientToolTips();
		ClearDiagnostics();
		ClearSemanticTokens();

		IntellisenseProvider?.CloseDocument(FilePath);
	}

	private async void TextArea_TextEntering(object? sender, TextCompositionEventArgs e)
	{
		if (!AutocompleteEnabled || !IsIntellisenseAvailable())
			return;

		if (e.Text == " " && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
		{
			e.Handled = true;

			if (LuaEditorInteractionRules.IsValidManualCompletionContext(Document, CaretOffset))
				await RequestCompletionAsync(CaretOffset, null).ConfigureAwait(true);
		}
	}

	private async void TextArea_TextEntered(object? sender, TextCompositionEventArgs e)
	{
		if (!IsIntellisenseAvailable())
			return;

		if (e.Text == "(" || e.Text == ",")
		{
			CloseCompletionWindow();
			await RequestSignatureHelpAsync(CaretOffset).ConfigureAwait(true);
			return;
		}

		if (e.Text == ")")
		{
			CloseCompletionWindow();
			DismissSignatureHelp();
			return;
		}

		if (_completionWindow is not null)
		{
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

		if (!AutocompleteEnabled)
			return;

		if (TryGetCompletionTrigger(e.Text, out char? triggerCharacter)
			&& LuaEditorInteractionRules.IsValidAutocompleteContext(Document, CaretOffset, triggerCharacter))
		{
			await RequestCompletionAsync(CaretOffset, triggerCharacter).ConfigureAwait(true);
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
		CancelAndDispose(ref _hoverCancellationTokenSource);
		_hoverRequestToken++;
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
}
