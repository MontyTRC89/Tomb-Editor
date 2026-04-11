using ICSharpCode.AvalonEdit.Document;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TombLib.Scripting.Lua.Objects;

namespace TombLib.Scripting.Lua
{
	public sealed partial class LuaEditor
	{
		private CancellationTokenSource _hoverCancellationTokenSource;
		private CancellationTokenSource _signatureCancellationTokenSource;
		private int _hoverRequestToken;
		private Window _hostWindow;

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

		private void LuaEditor_Loaded(object sender, RoutedEventArgs e)
			=> AttachHostWindowHandlers();

		private void LuaEditor_IsKeyboardFocusWithinChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			if (e.NewValue is bool hasKeyboardFocus && !hasKeyboardFocus)
				DismissTransientToolTips();
		}

		private void AttachHostWindowHandlers()
		{
			Window window = Window.GetWindow(this);

			if (window == _hostWindow)
				return;

			if (_hostWindow is not null)
				_hostWindow.Deactivated -= HostWindow_Deactivated;

			_hostWindow = window;

			if (_hostWindow is not null)
				_hostWindow.Deactivated += HostWindow_Deactivated;
		}

		private void HostWindow_Deactivated(object sender, EventArgs e)
			=> DismissTransientToolTips();

		private void LuaEditor_Unloaded(object sender, RoutedEventArgs e)
		{
			_hoverCancellationTokenSource?.Cancel();
			_hoverCancellationTokenSource?.Dispose();
			_hoverCancellationTokenSource = null;

			_signatureCancellationTokenSource?.Cancel();
			_signatureCancellationTokenSource?.Dispose();
			_signatureCancellationTokenSource = null;

			if (_hostWindow is not null)
				_hostWindow.Deactivated -= HostWindow_Deactivated;

			_hostWindow = null;

			DismissSignatureHelp();
			ClearDiagnostics();

			IntellisenseProvider?.CloseDocument(FilePath);
		}

		private async void TextArea_TextEntering(object sender, TextCompositionEventArgs e)
		{
			if (!AutocompleteEnabled || !IsIntellisenseAvailable())
				return;

			if (e.Text == " " && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
			{
				e.Handled = true;

				await RequestCompletionAsync(CaretOffset).ConfigureAwait(true);
			}
		}

		private async void TextArea_TextEntered(object sender, TextCompositionEventArgs e)
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

			if (_completionWindow is not null && !ShouldKeepCompletionWindowOpen(e.Text))
			{
				CloseCompletionWindow();
				return;
			}

			if (!AutocompleteEnabled)
				return;

			if (ShouldTriggerAutocomplete(e.Text))
				await RequestCompletionAsync(CaretOffset).ConfigureAwait(true);
		}

		protected override async void HandleMouseHover(MouseEventArgs e)
		{
			int hoveredOffset = GetOffsetFromPoint(e.GetPosition(this));

			if (hoveredOffset == -1)
				return;

			if (TryShowDiagnosticToolTip(hoveredOffset))
				return;

			if (!IsIntellisenseAvailable())
				return;

			string hoveredWord = GetWordFromOffset(hoveredOffset);

			if (string.IsNullOrWhiteSpace(hoveredWord))
				return;

			_hoverCancellationTokenSource?.Cancel();
			_hoverCancellationTokenSource?.Dispose();

			_hoverCancellationTokenSource = new CancellationTokenSource();
			CancellationToken cancellationToken = _hoverCancellationTokenSource.Token;
			int hoverRequestToken = ++_hoverRequestToken;

			try
			{
				LuaHoverInfo hoverInfo = await RequestHoverAsync(hoveredOffset, cancellationToken).ConfigureAwait(true);

				if (hoverInfo is null || string.IsNullOrWhiteSpace(hoverInfo.Content))
					return;

				if (cancellationToken.IsCancellationRequested || hoverRequestToken != _hoverRequestToken)
					return;

				int currentHoveredOffset = GetOffsetFromPoint(Mouse.GetPosition(this));

				if (currentHoveredOffset != hoveredOffset)
					return;

				if (hoverInfo.IsMarkdown)
					ShowMarkdownToolTip(hoverInfo.Content);
				else
					ShowToolTip(hoverInfo.Content);
			}
			catch
			{
				// Ignore hover failures and keep the editor responsive.
			}
		}

		private bool IsIntellisenseAvailable()
			=> IntellisenseProvider is not null && IntellisenseProvider.IsAvailable && !string.IsNullOrWhiteSpace(FilePath);

		private static bool ShouldTriggerAutocomplete(string inputText)
			=> inputText == "." || inputText == ":";

		private static bool ShouldKeepCompletionWindowOpen(string inputText)
			=> inputText?.Length == 1 && (char.IsLetterOrDigit(inputText[0]) || inputText[0] == '_');

		private void CloseCompletionWindow()
		{
			if (_completionWindow is null)
				return;

			_completionWindow.Close();
			_completionWindow = null;
		}

		private async Task RequestCompletionAsync(int offset)
		{
			try
			{
				if (!IsIntellisenseAvailable())
					return;

				(int line, int column) = GetPositionFromOffset(offset);
				var items = await IntellisenseProvider
					.GetCompletionItemsAsync(FilePath, Text, line, column, CancellationToken.None)
					.ConfigureAwait(true);

				if (items is null || items.Count == 0)
				{
					CloseCompletionWindow();
					return;
				}

				CloseCompletionWindow();

				InitializeCompletionWindow();
				SetCompletionWindowOffsets(offset);

				foreach (LuaCompletionItem item in items)
					_completionWindow.CompletionList.CompletionData.Add(new LuaCompletionData(item));

				if (_completionWindow.CompletionList.CompletionData.Count > 0)
					ShowCompletionWindow();
			}
			catch
			{
				CloseCompletionWindow();
				// Ignore completion failures and keep the editor responsive.
			}
		}

		private async Task<LuaHoverInfo> RequestHoverAsync(int offset, CancellationToken cancellationToken)
		{
			if (!IsIntellisenseAvailable())
				return null;

			(int line, int column) = GetPositionFromOffset(offset);

			return await IntellisenseProvider
				.GetHoverAsync(FilePath, Text, line, column, cancellationToken)
				.ConfigureAwait(true);
		}

		private async Task RequestSignatureHelpAsync(int offset)
		{
			if (!IsIntellisenseAvailable())
			{
				DismissSignatureHelp();
				return;
			}

			_signatureCancellationTokenSource?.Cancel();
			_signatureCancellationTokenSource?.Dispose();
			_signatureCancellationTokenSource = new CancellationTokenSource();

			CancellationToken cancellationToken = _signatureCancellationTokenSource.Token;

			try
			{
				(int line, int column) = GetPositionFromOffset(offset);
				LuaSignatureInfo signatureInfo = await IntellisenseProvider
					.GetSignatureHelpAsync(FilePath, Text, line, column, cancellationToken)
					.ConfigureAwait(true);

				if (cancellationToken.IsCancellationRequested)
					return;

				if (signatureInfo is null)
				{
					DismissSignatureHelp();
					return;
				}

				ShowSignatureToolTip(signatureInfo);
			}
			catch
			{
				// Ignore signature help failures and keep the editor responsive.
			}
		}

		private void DismissTransientToolTips()
		{
			_hoverCancellationTokenSource?.Cancel();
			_hoverRequestToken++;
			DismissSignatureHelp();
			CloseDefinitionToolTip(true);
		}

		private void ScheduleSignatureHelpRefresh()
			=> Dispatcher.BeginInvoke(new Action(() => _ = RequestSignatureHelpAsync(CaretOffset)));

		private (int line, int column) GetPositionFromOffset(int offset)
		{
			int safeOffset = Math.Max(0, Math.Min(offset, Document.TextLength));
			TextLocation location = Document.GetLocation(safeOffset);

			return (location.Line - 1, location.Column - 1);
		}

		private void SetCompletionWindowOffsets(int offset)
		{
			int startOffset = Math.Max(0, Math.Min(offset, Document.TextLength));

			while (startOffset > 0)
			{
				char currentChar = Document.GetCharAt(startOffset - 1);

				if (char.IsLetterOrDigit(currentChar) || currentChar == '_')
					startOffset--;
				else
					break;
			}

			_completionWindow.StartOffset = startOffset;
			_completionWindow.EndOffset = Math.Max(0, Math.Min(offset, Document.TextLength));
		}
	}
}