using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using TombLib.Scripting.Lua.Objects;
using TombLib.Scripting.Objects;
using TombLib.Scripting.Rendering;
using TombLib.WPF;

namespace TombLib.Scripting.Lua
{
	public sealed partial class LuaEditor
	{
		private static readonly FieldInfo CompletionToolTipField =
			typeof(CompletionWindow).GetField("toolTip", BindingFlags.NonPublic | BindingFlags.Instance);

		private CancellationTokenSource _hoverCancellationTokenSource;
		private CancellationTokenSource _completionCancellationTokenSource;
		private CancellationTokenSource _completionToolTipCancellationTokenSource;
		private CancellationTokenSource _signatureCancellationTokenSource;
		private int _completionRequestToken;
		private int _completionToolTipUpdateToken;
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
			_textMateHighlighting?.Dispose();
			_textMateHighlighting = null;

			_hoverCancellationTokenSource?.Cancel();
			_hoverCancellationTokenSource?.Dispose();
			_hoverCancellationTokenSource = null;

			_completionCancellationTokenSource?.Cancel();
			_completionCancellationTokenSource?.Dispose();
			_completionCancellationTokenSource = null;

			CancelCompletionToolTipUpdate();

			_signatureCancellationTokenSource?.Cancel();
			_signatureCancellationTokenSource?.Dispose();
			_signatureCancellationTokenSource = null;

			if (_hostWindow is not null)
				_hostWindow.Deactivated -= HostWindow_Deactivated;

			_hostWindow = null;

			DismissSignatureHelp();
			ClearDiagnostics();
			ClearSemanticTokens();

			IntellisenseProvider?.CloseDocument(FilePath);
		}

		private async void TextArea_TextEntering(object sender, TextCompositionEventArgs e)
		{
			if (!AutocompleteEnabled || !IsIntellisenseAvailable())
				return;

			if (e.Text == " " && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
			{
				e.Handled = true;

				await RequestCompletionAsync(CaretOffset, null).ConfigureAwait(true);
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

			if (_completionWindow is not null)
			{
				if (!ShouldKeepCompletionWindowOpen(e.Text))
					CloseCompletionWindow();
				else
				{
					ScheduleCloseIfEmpty();
					return;
				}
			}

			if (!AutocompleteEnabled)
				return;

			if (TryGetCompletionTrigger(e.Text, out char? triggerCharacter) && IsValidAutocompleteContext(CaretOffset, triggerCharacter))
				await RequestCompletionAsync(CaretOffset, triggerCharacter).ConfigureAwait(true);
		}

		protected override async void HandleMouseHover(MouseEventArgs e)
		{
			int hoveredOffset = GetOffsetFromPoint(e.GetPosition(this));

			if (hoveredOffset == -1)
				return;

			bool hasDiagnostic = TryGetDiagnosticInfo(hoveredOffset, out string diagnosticMessage, out TextEditorDiagnosticSeverity diagnosticSeverity);

			if (!IsIntellisenseAvailable())
			{
				if (hasDiagnostic)
					ShowDiagnosticToolTip(diagnosticMessage, diagnosticSeverity);

				return;
			}

			string hoveredWord = GetWordFromOffset(hoveredOffset);

			if (string.IsNullOrWhiteSpace(hoveredWord))
			{
				if (hasDiagnostic)
					ShowDiagnosticToolTip(diagnosticMessage, diagnosticSeverity);

				return;
			}

			_hoverCancellationTokenSource?.Cancel();
			_hoverCancellationTokenSource?.Dispose();

			_hoverCancellationTokenSource = new CancellationTokenSource();
			CancellationToken cancellationToken = _hoverCancellationTokenSource.Token;
			int hoverRequestToken = ++_hoverRequestToken;

			try
			{
				LuaHoverInfo hoverInfo = await RequestHoverAsync(hoveredOffset, cancellationToken).ConfigureAwait(true);

				if (cancellationToken.IsCancellationRequested || hoverRequestToken != _hoverRequestToken)
					return;

				int currentHoveredOffset = GetOffsetFromPoint(Mouse.GetPosition(this));

				if (currentHoveredOffset != hoveredOffset)
					return;

				bool hasHover = hoverInfo is not null && !string.IsNullOrWhiteSpace(hoverInfo.Content);

				if (hasHover && hasDiagnostic)
					ShowCombinedHoverAndDiagnosticToolTip(hoverInfo, diagnosticMessage, diagnosticSeverity);
				else if (hasHover)
					ShowHoverToolTip(hoverInfo);
				else if (hasDiagnostic)
				{
					ShowDiagnosticToolTip(diagnosticMessage, diagnosticSeverity);
				}
			}
			catch
			{
				if (hasDiagnostic)
					ShowDiagnosticToolTip(diagnosticMessage, diagnosticSeverity);
			}
		}

		private void ShowCombinedHoverAndDiagnosticToolTip(LuaHoverInfo hoverInfo, string diagnosticMessage, TextEditorDiagnosticSeverity severity)
		{
			GetDiagnosticToolTipColors(severity, out SolidColorBrush diagnosticBorder, out SolidColorBrush diagnosticBackground);

			var panel = new StackPanel { MaxWidth = ToolTipTextMaxWidth };

			panel.Children.Add(CreateHoverToolTipContent(hoverInfo));

			panel.Children.Add(new Border
			{
				Background = diagnosticBackground,
				BorderBrush = diagnosticBorder,
				BorderThickness = new Thickness(1.0),
				CornerRadius = new CornerRadius(3.0),
				Padding = new Thickness(8.0, 4.0, 8.0, 4.0),
				Margin = new Thickness(0.0, 6.0, 0.0, 0.0),
				Child = new TextBlock
				{
					Text = diagnosticMessage,
					Foreground = ToolTipForeground,
					FontFamily = SystemFonts.MessageFontFamily,
					FontSize = ToolTipTextFontSize,
					TextWrapping = TextWrapping.Wrap,
					MaxWidth = ToolTipTextMaxWidth
				}
			});

			ShowToolTip(panel, DefaultToolTipBorder, DefaultToolTipBackground);
		}

		private void ShowHoverToolTip(LuaHoverInfo hoverInfo)
			=> ShowToolTip(CreateHoverToolTipContent(hoverInfo), DefaultToolTipBorder, DefaultToolTipBackground);

		private FrameworkElement CreateHoverToolTipContent(LuaHoverInfo hoverInfo)
		{
			if (hoverInfo.IsMarkdown)
				return MarkdownToolTipRenderer.CreateContent(hoverInfo.Content, ToolTipForeground, DefaultToolTipBackground);

			return MarkdownToolTipRenderer.CreatePlainTextContent(hoverInfo.Content, ToolTipForeground);
		}

		private bool IsIntellisenseAvailable()
			=> IntellisenseProvider is not null && IntellisenseProvider.IsAvailable && !string.IsNullOrWhiteSpace(FilePath);

		private static bool TryGetCompletionTrigger(string inputText, out char? triggerCharacter)
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

			return IsIdentifierTriggerCharacter(typedChar);
		}

		private static bool ShouldKeepCompletionWindowOpen(string inputText)
			=> inputText?.Length == 1 && (char.IsLetterOrDigit(inputText[0]) || inputText[0] == '_');

		private void CloseCompletionWindow()
		{
			CancelCompletionToolTipUpdate();

			if (_completionWindow is null)
				return;

			if (CompletionToolTipField?.GetValue(_completionWindow) is ToolTip tooltip)
				tooltip.IsOpen = false;

			_completionWindow.Close();
			_completionWindow = null;
		}

		private void InitializeLuaCompletionWindow()
		{
			InitializeCompletionWindow(420, 320);
			LuaCompletionWindowStyle.Apply(_completionWindow);
			StyleCompletionTooltip();
			MakeCompletionWindowNonActivatable();
		}

		private async Task RequestCompletionAsync(int offset, char? triggerCharacter)
		{
			_completionCancellationTokenSource?.Cancel();
			_completionCancellationTokenSource?.Dispose();
			_completionCancellationTokenSource = new CancellationTokenSource();

			CancellationToken cancellationToken = _completionCancellationTokenSource.Token;
			int requestToken = ++_completionRequestToken;

			try
			{
				if (!IsIntellisenseAvailable())
					return;

				(int line, int column) = GetPositionFromOffset(offset);
				var items = await IntellisenseProvider
					.GetCompletionItemsAsync(FilePath, Text, line, column, triggerCharacter, cancellationToken)
					.ConfigureAwait(true);

				if (cancellationToken.IsCancellationRequested || requestToken != _completionRequestToken)
					return;

				if (items is null || items.Count == 0)
				{
					CloseCompletionWindow();
					return;
				}

				var completionDataItems = new LuaCompletionData[items.Count];

				for (int i = 0; i < items.Count; i++)
					completionDataItems[i] = new LuaCompletionData(items[i]);

				if (cancellationToken.IsCancellationRequested || requestToken != _completionRequestToken)
					return;

				CloseCompletionWindow();

				InitializeLuaCompletionWindow();
				SetCompletionWindowOffsets(offset);

				foreach (LuaCompletionData completionDataItem in completionDataItems)
					_completionWindow.CompletionList.CompletionData.Add(completionDataItem);

				if (_completionWindow.CompletionList.CompletionData.Count > 0)
				{
					ShowCompletionWindow();
					ScheduleInitialSelection();
				}
			}
			catch
			{
				CloseCompletionWindow();
				// Ignore completion failures and keep the editor responsive.
			}
		}

		private bool IsValidAutocompleteContext(int offset, char? triggerCharacter)
		{
			if (offset <= 0 || Document is null)
				return false;

			if (IsInsideCommentOrString(offset))
				return false;

			if (triggerCharacter is '.' || triggerCharacter is ':')
				return true;

			char typedCharacter = Document.GetCharAt(offset - 1);

			if (!IsIdentifierCharacter(typedCharacter))
				return false;

			if (offset >= 2)
			{
				char previousCharacter = Document.GetCharAt(offset - 2);

				if (previousCharacter == '.')
					return false;
			}

			return true;
		}

		private bool IsInsideCommentOrString(int offset)
		{
			DocumentLine currentLine = Document.GetLineByOffset(Math.Max(0, Math.Min(offset, Document.TextLength)));
			int lineStart = currentLine.Offset;
			int inspectedLength = Math.Max(0, Math.Min(offset, currentLine.EndOffset) - lineStart);
			string lineText = Document.GetText(lineStart, inspectedLength);

			bool isInsideSingleQuotedString = false;
			bool isInsideDoubleQuotedString = false;
			bool isEscaped = false;

			for (int i = 0; i < lineText.Length; i++)
			{
				char currentChar = lineText[i];

				if (!isInsideSingleQuotedString && !isInsideDoubleQuotedString && currentChar == '-'
					&& i + 1 < lineText.Length && lineText[i + 1] == '-')
				{
					return true;
				}

				if (isEscaped)
				{
					isEscaped = false;
					continue;
				}

				if ((isInsideSingleQuotedString || isInsideDoubleQuotedString) && currentChar == '\\')
				{
					isEscaped = true;
					continue;
				}

				if (!isInsideDoubleQuotedString && currentChar == '\'')
					isInsideSingleQuotedString = !isInsideSingleQuotedString;
				else if (!isInsideSingleQuotedString && currentChar == '"')
					isInsideDoubleQuotedString = !isInsideDoubleQuotedString;
			}

			return isInsideSingleQuotedString || isInsideDoubleQuotedString;
		}

		private static bool IsIdentifierCharacter(char character)
			=> char.IsLetterOrDigit(character) || character == '_';

		private static bool IsIdentifierTriggerCharacter(char character)
			=> char.IsLetter(character) || character == '_';

		private void StyleCompletionTooltip()
		{
			if (_completionWindow?.CompletionList.ListBox is not ListBox listBox)
				return;

			if (CompletionToolTipField?.GetValue(_completionWindow) is not ToolTip tooltip)
				return;

			tooltip.Background = DefaultToolTipBackground;
			tooltip.BorderBrush = DefaultToolTipBorder;
			tooltip.BorderThickness = new Thickness(0.0);
			tooltip.Padding = new Thickness(0.0);
			tooltip.PlacementTarget = listBox;
			tooltip.Placement = PlacementMode.Right;
			tooltip.HorizontalOffset = 10.0;
			tooltip.StaysOpen = true;

			listBox.SelectionChanged += (s, e) => ScheduleCompletionTooltipUpdate(tooltip);
			listBox.PreviewMouseLeftButtonUp += (s, e) => HandleCompletionListClick(listBox, tooltip, e);
		}

		private void HandleCompletionListClick(ListBox listBox, ToolTip tooltip, MouseButtonEventArgs e)
		{
			ListBoxItem listBoxItem = (e.OriginalSource as DependencyObject)?.FindVisualAncestorOrSelf<ListBoxItem>();

			if (listBoxItem is null)
				return;

			if (!ReferenceEquals(listBox.SelectedItem, listBoxItem.DataContext))
				listBox.SelectedItem = listBoxItem.DataContext;

			listBox.ScrollIntoView(listBoxItem.DataContext);
			ScheduleCompletionTooltipUpdate(tooltip);
		}

		private void ScheduleCompletionTooltipUpdate(ToolTip tooltip)
		{
			_completionToolTipCancellationTokenSource?.Cancel();
			_completionToolTipCancellationTokenSource?.Dispose();
			_completionToolTipCancellationTokenSource = new CancellationTokenSource();

			CancellationToken cancellationToken = _completionToolTipCancellationTokenSource.Token;
			int updateToken = ++_completionToolTipUpdateToken;

			Dispatcher.BeginInvoke(
				new Action(() => _ = UpdateCompletionTooltipAsync(tooltip, cancellationToken, updateToken)),
				DispatcherPriority.Background);
		}

		private async Task UpdateCompletionTooltipAsync(ToolTip tooltip, CancellationToken cancellationToken, int updateToken)
		{
			if (_completionWindow?.CompletionList.ListBox is not ListBox listBox)
				return;

			ICompletionData item = listBox.SelectedItem as ICompletionData;

			if (item is null)
			{
				tooltip.IsOpen = false;
				return;
			}

			try
			{
				object description = item.Description;

				if (description is not null)
					ApplyCompletionToolTipContent(tooltip, description);
				else
					tooltip.IsOpen = false;

				if (item is LuaCompletionData luaCompletionData && luaCompletionData.CanResolve)
				{
					object resolvedDescription = await luaCompletionData.GetDescriptionAsync(cancellationToken).ConfigureAwait(true);

					if (cancellationToken.IsCancellationRequested || updateToken != _completionToolTipUpdateToken)
						return;

					if (_completionWindow?.CompletionList.ListBox is not ListBox currentListBox
						|| !ReferenceEquals(currentListBox, listBox)
						|| !ReferenceEquals(currentListBox.SelectedItem, item))
					{
						return;
					}

					if (resolvedDescription is not null)
						ApplyCompletionToolTipContent(tooltip, resolvedDescription);
					else
						tooltip.IsOpen = false;
				}
			}
			catch (OperationCanceledException)
			{
			}
			catch
			{
				tooltip.IsOpen = false;
			}
		}

		private static void ApplyCompletionToolTipContent(ToolTip tooltip, object content)
		{
			tooltip.Content = content;

			if (!tooltip.IsOpen)
				tooltip.IsOpen = true;
			else
			{
				tooltip.InvalidateMeasure();
				tooltip.InvalidateVisual();
			}
		}

		private void CancelCompletionToolTipUpdate()
		{
			_completionToolTipCancellationTokenSource?.Cancel();
			_completionToolTipCancellationTokenSource?.Dispose();
			_completionToolTipCancellationTokenSource = null;
			_completionToolTipUpdateToken++;
		}

		private void ScheduleCloseIfEmpty()
			=> Dispatcher.BeginInvoke(new Action(() => CloseCompletionWindowIfEmpty()), DispatcherPriority.Background);

		private void ScheduleInitialSelection()
			=> Dispatcher.BeginInvoke(new Action(SelectInitialItem), DispatcherPriority.Background);

		private void SelectInitialItem()
		{
			if (_completionWindow is null)
				return;

			_completionWindow.CompletionList.SelectItem(GetCompletionWindowQuery());
			CloseCompletionWindowIfEmpty();
		}

		// AvalonEdit's CompletionWindowBase does not set WS_EX_NOACTIVATE, so clicking the
		// completion list activates the popup window and steals keyboard focus from the editor.
		// This hook returns MA_NOACTIVATE to prevent that while still allowing clicks through.

		private void MakeCompletionWindowNonActivatable()
		{
			_completionWindow.SourceInitialized += (s, e) =>
			{
				if (s is Window window && PresentationSource.FromVisual(window) is HwndSource source)
					source.AddHook(CompletionWindowWndProc);
			};
		}

		private static IntPtr CompletionWindowWndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
		{
			const int WM_MOUSEACTIVATE = 0x0021;
			const int MA_NOACTIVATE = 3;

			if (msg == WM_MOUSEACTIVATE)
			{
				handled = true;
				return new IntPtr(MA_NOACTIVATE);
			}

			return IntPtr.Zero;
		}

		private bool CloseCompletionWindowIfEmpty()
		{
			if (_completionWindow is null)
				return false;

			var listBox = _completionWindow.CompletionList.ListBox;

			if (listBox is null || listBox.HasItems)
				return false;

			CloseCompletionWindow();
			return true;
		}

		private string GetCompletionWindowQuery()
		{
			if (_completionWindow is null || Document is null)
				return string.Empty;

			int startOffset = Math.Max(0, Math.Min(_completionWindow.StartOffset, Document.TextLength));
			int endOffset = Math.Max(startOffset, Math.Min(_completionWindow.EndOffset, Document.TextLength));

			return endOffset > startOffset
				? Document.GetText(startOffset, endOffset - startOffset)
				: string.Empty;
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