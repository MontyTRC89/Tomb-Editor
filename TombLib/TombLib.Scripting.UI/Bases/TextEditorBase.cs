#nullable enable

using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Nickelony.LanguageServer.Core.Diagnostics;
using Nickelony.LanguageServer.Core.Hover;
using TombLib.Scripting.Hover;
using Nickelony.LanguageServer.Core.Navigation;
using TombLib.Scripting.Navigation;
using TombLib.Scripting.UI.Cleaning;
using TombLib.Scripting.UI.Completion;
using TombLib.Scripting.UI.Diagnostics;
using TombLib.Scripting.UI.Documents;
using TombLib.Scripting.UI.Editing;
using TombLib.Scripting.UI.Editors;
using TombLib.Scripting.UI.Navigation;
using TombLib.Scripting.UI.Presentation;
using TombLib.Scripting.UI.Rendering;
using TombLib.Scripting.UI.Resources;

namespace TombLib.Scripting.UI.Bases
{
	public abstract class TextEditorBase : TextEditor, IEditorControl
	{
		public const double ToolTipTextMaxWidth = 500.0;
		public static readonly double ToolTipTextFontSize = Math.Max(SystemFonts.MessageFontSize + 1.0, 14.0);
		public static readonly SolidColorBrush DefaultToolTipBorder = TextEditorColorPalette.ToolTipBorder;
		public static readonly SolidColorBrush DefaultToolTipBackground = TextEditorColorPalette.ToolTipBackground;
		public static readonly SolidColorBrush ToolTipForeground = TextEditorColorPalette.ToolTipForeground;
		private static readonly TextEditorFormattingService FormattingService = new();

		public EditorType EditorType => EditorType.Text;
		public abstract string DefaultFileExtension { get; }

		#region Properties

		public string FilePath
		{
			get => Document.FileName;
			set
			{
				Document.FileName = value;
				_contentPersistenceCoordinator.FilePath = value;
			}
		}

		public bool IsSilentSession { get; set; }

		public bool CreateBackupFiles
		{
			get => IsSilentSession ? false : _contentPersistenceCoordinator.CreateBackupFiles;
			set => _contentPersistenceCoordinator.CreateBackupFiles = value;
		}

		public string Content
		{
			get => Text;
			set => SetContent(value);
		}

		public bool IsContentChanged { get; set; }

		public DateTime LastModified { get; set; }

		public int CurrentRow => TextArea.Caret.Position.Line;
		public int CurrentColumn => TextArea.Caret.Position.Column;

		public object SelectedContent => SelectedText;

		protected virtual ITextDocumentFormatter DocumentFormatter => TrimTrailingWhitespaceFormatter.Instance;

		public int MinZoom { get; set; } = 25;
		public int MaxZoom { get; set; } = 400;
		public int ZoomStepSize { get; set; } = 15;

		public string CommentPrefix { get; set; } = string.Empty;

		public TimeSpan TextChangedDelayedInterval
		{
			get => _contentPersistenceCoordinator.DelayedInterval;
			set => _contentPersistenceCoordinator.DelayedInterval = value;
		}

		public string ParenthesesClosingString { get; set; } = ")";
		public string BracesClosingString { get; set; } = "}";
		public string BracketsClosingString { get; set; } = "]";
		public string QuotesClosingString { get; set; } = "\"";

		public Version EngineVersion { get; set; } = new Version(0, 0);

		#endregion Properties

		#region Configuration

		/// <summary>
		/// Basically FontSize but zooming doesn't affect its value.
		/// </summary>
		public double DefaultFontSize { get; set; } = TextEditorBaseDefaults.FontSize;

		public bool IntellisenseEnabled { get; set; } = TextEditorBaseDefaults.IntellisenseEnabled;
		public bool AutocompleteEnabled { get; set; } = TextEditorBaseDefaults.AutocompleteEnabled;
		public bool LiveErrorUnderlining { get; set; } = TextEditorBaseDefaults.LiveErrorUnderlining;
		public bool SignatureHelpPopupsEnabled { get; set; } = TextEditorBaseDefaults.SignatureHelpPopupsEnabled;

		public bool AutoCloseParentheses { get; set; } = TextEditorBaseDefaults.AutoCloseParentheses;
		public bool AutoCloseBraces { get; set; } = TextEditorBaseDefaults.AutoCloseBraces;
		public bool AutoCloseBrackets { get; set; } = TextEditorBaseDefaults.AutoCloseBrackets;
		public bool AutoCloseDoubleQuotes { get; set; } = TextEditorBaseDefaults.AutoCloseDoubleQuotes;
		public bool AutoCloseSingleQuotes { get; set; } = TextEditorBaseDefaults.AutoCloseSingleQuotes;

		public bool AutoCloseQuotes
		{
			get => AutoCloseDoubleQuotes && AutoCloseSingleQuotes;
			set
			{
				AutoCloseDoubleQuotes = value;
				AutoCloseSingleQuotes = value;
			}
		}

		#endregion Configuration

		#region Fields

		protected Popup _specialToolTip;

		private readonly BookmarkCoordinator _bookmarkCoordinator;
		private readonly TextAutoClosingService _autoClosingService;
		private readonly TextLineCommentService _commentService;
		private readonly CompletionWindowCoordinator _completionWindowCoordinator;
		private readonly ContentPersistenceCoordinator _contentPersistenceCoordinator;
		private readonly TextDefinitionNavigationService _definitionNavigationService;
		private readonly TextDiagnosticToolTipService _diagnosticToolTipService;
		private readonly TextEditorStatusCoordinator _statusCoordinator;
		private readonly EditorToolTipPresenter _toolTipPresenter;
		private readonly TextEditorViewService _viewService;

		private IBackgroundRenderer _bookmarkRenderer;
		private IBackgroundRenderer _errorRenderer;

		internal IReadOnlyList<TextEditorDiagnostic> Diagnostics => _diagnosticToolTipService.Diagnostics;

		#endregion Fields

		#region Construction

		public TextEditorBase(Version engineVersion)
		{
			TextEditorBaseServiceCollection services = TextEditorBaseServiceCollection.Create(this);

			SetNewDefaultSettings();
			_autoClosingService = services.AutoClosingService;
			_bookmarkCoordinator = services.BookmarkCoordinator;
			_commentService = services.CommentService;
			_completionWindowCoordinator = services.CompletionWindowCoordinator;
			_contentPersistenceCoordinator = services.ContentPersistenceCoordinator;
			_definitionNavigationService = services.DefinitionNavigationService;
			_diagnosticToolTipService = services.DiagnosticToolTipService;
			_statusCoordinator = services.StatusCoordinator;
			_toolTipPresenter = services.ToolTipPresenter;
			_viewService = services.ViewService;
			_specialToolTip = _toolTipPresenter.Popup;

			InitializePersistenceCoordinator();
			InitializeRenderers();

			BindEventMethods();

			EngineVersion = engineVersion;
		}

		private void SetNewDefaultSettings()
		{
			Options.AllowScrollBelowDocument = true;
			TextArea.Margin = new Thickness(3, 0, 0, 0);

			FontWeight = FontWeights.Normal;

			TextArea.TextView.CurrentLineBackground = new SolidColorBrush(Color.FromArgb(16, 160, 160, 160));
			TextArea.TextView.CurrentLineBorder = new Pen(new SolidColorBrush(Color.FromArgb(24, 192, 192, 192)), 1);

			TextArea.SelectionCornerRadius = 0; // Why does this even exist?
			TextArea.SelectionBorder = new Pen(Brushes.SteelBlue, 1);

			HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
			VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
		}

		private void InitializePersistenceCoordinator()
		{
			_contentPersistenceCoordinator.ContentChangedWorkerRunCompleted += ContentPersistenceCoordinator_ContentChangedWorkerRunCompleted;
			_contentPersistenceCoordinator.TextChangedDelayed += ContentPersistenceCoordinator_TextChangedDelayed;
		}

		[MemberNotNull(nameof(_bookmarkRenderer), nameof(_errorRenderer))]
		private void InitializeRenderers()
		{
			_bookmarkRenderer = new BookmarkRenderer(_bookmarkCoordinator);
			_errorRenderer = new ErrorRenderer(this);

			TextArea.TextView.BackgroundRenderers.Add(_bookmarkRenderer);
			TextArea.TextView.BackgroundRenderers.Add(_errorRenderer);
		}

		private void BindEventMethods()
		{
			_statusCoordinator.Attach();

			TextArea.TextEntering += TextArea_TextEntering;
			TextChanged += TextEditor_TextChanged;

			MouseHover += TextEditor_MouseHover;
			MouseHoverStopped += TextEditor_MouseHoverStopped;

			PreviewMouseWheel += TextEditor_PreviewMouseWheel;
			MouseRightButtonDown += TextEditor_MouseRightButtonDown;
		}

		#endregion Construction

		#region Events

		public event EventHandler? StatusChanged;

		protected virtual void OnStatusChanged(EventArgs e)
			=> StatusChanged?.Invoke(this, e);

		internal void RaiseStatusChanged()
			=> OnStatusChanged(EventArgs.Empty);

		public event EventHandler? ZoomChanged;

		protected virtual void OnZoomChanged(EventArgs e)
		{
			ZoomChanged?.Invoke(this, e);
			OnStatusChanged(EventArgs.Empty);
		}

		internal void RaiseZoomChanged()
			=> OnZoomChanged(EventArgs.Empty);

		public event EventHandler? TextChangedDelayed;

		protected virtual void OnTextChangedDelayed(EventArgs e)
			=> TextChangedDelayed?.Invoke(this, e);

		public event EventHandler? ContentChangedWorkerRunCompleted;

		protected virtual void OnContentChangedWorkerRunCompleted(EventArgs e)
			=> ContentChangedWorkerRunCompleted?.Invoke(this, e);

		private void ContentPersistenceCoordinator_ContentChangedWorkerRunCompleted(object? sender, EventArgs e)
		{
			OnContentChangedWorkerRunCompleted(EventArgs.Empty);
		}

		private void TextArea_TextEntering(object? sender, TextCompositionEventArgs e)
		{
			CloseDefinitionToolTip(true); // Prevents the ToolTip from covering the screen while typing
			HandleAutoClosing(e);
		}

		private void TextEditor_TextChanged(object? sender, EventArgs e)
		{
			LastModified = DateTime.Now;
			IsContentChanged = _contentPersistenceCoordinator.HandleContentChanged();
		}

		private void ContentPersistenceCoordinator_TextChangedDelayed(object? sender, EventArgs e)
		{
			OnTextChangedDelayed(EventArgs.Empty);
		}

		private void TextEditor_MouseHover(object? sender, MouseEventArgs e)
			=> HandleMouseHover(e);

		protected virtual void HandleMouseHover(MouseEventArgs e)
			=> HandleErrorToolTips(e);

		private void TextEditor_MouseHoverStopped(object? sender, MouseEventArgs e)
			=> ScheduleDefinitionToolTipClose();

		private void TextEditor_PreviewMouseWheel(object? sender, MouseWheelEventArgs e)
		{
			if (Keyboard.Modifiers == ModifierKeys.Control
				&& _statusCoordinator.TryHandleZoom(e.Delta, MinZoom, MaxZoom, ZoomStepSize, DefaultFontSize, fontSize => FontSize = fontSize))
			{
				e.Handled = true;
			}
		}

		private void TextEditor_MouseRightButtonDown(object? sender, MouseButtonEventArgs e)
		{
			_viewService.TryMoveCaretToMousePosition();

			if (ContextMenu is null)
				ContextMenu = BuildDefaultContextMenu();

			ContextMenu.IsOpen = true;
			e.Handled = true;
		}

		protected virtual ContextMenu BuildDefaultContextMenu()
		{
			var menu = new ContextMenu();

			var cutItem = new MenuItem { Header = "Cut", Command = ApplicationCommands.Cut };
			var copyItem = new MenuItem { Header = "Copy", Command = ApplicationCommands.Copy };
			var pasteItem = new MenuItem { Header = "Paste", Command = ApplicationCommands.Paste };
			var selectAllItem = new MenuItem { Header = "Select All", Command = ApplicationCommands.SelectAll };

			menu.Items.Add(cutItem);
			menu.Items.Add(copyItem);
			menu.Items.Add(pasteItem);
			menu.Items.Add(new Separator());
			menu.Items.Add(selectAllItem);

			return menu;
		}

		protected void CloseDefinitionToolTip(bool force = false)
			=> _toolTipPresenter.Close(force);

		protected bool TryHandleCtrlSpaceCompletion(TextCompositionEventArgs e, Action onTriggered)
		{
			if (!AutocompleteEnabled || !EditorCompletionTriggerHelper.IsCtrlSpaceInput(e.Text, Keyboard.Modifiers))
				return false;

			if (!IsCompletionWindowOpen)
				onTriggered();

			e.Handled = true;
			return true;
		}

		private void ScheduleDefinitionToolTipClose()
			=> _toolTipPresenter.ScheduleClose();

		#endregion Events

		#region File I/O

		public new void Load(string filePath)
			=> Load(filePath, false);

		public void Load(string filePath, bool silentSession)
		{
			base.Load(filePath);
			FilePath = filePath;
			_contentPersistenceCoordinator.SetPersistedContent(Content);

			IsContentChanged = _contentPersistenceCoordinator.HasChanges(Content);
			IsSilentSession = silentSession;

			_bookmarkCoordinator.Restore(FilePath);
		}

		public void Save()
			=> Save(FilePath);

		public new void Save(string filePath)
		{
			base.Save(filePath);
			_contentPersistenceCoordinator.SetPersistedContent(Content);
			IsContentChanged = _contentPersistenceCoordinator.HasChanges(Content);
			LastModified = DateTime.Now;
		}

		internal void SaveBookmarks()
			=> _bookmarkCoordinator.Save(FilePath);

		#endregion File I/O

		#region Content

		public void TryRunContentChangedWorker()
		{
			IsContentChanged = _contentPersistenceCoordinator.RunContentChangedCheck();
		}

		public void ApplyPersistedContent(string content)
		{
			SetContent(content);
			_contentPersistenceCoordinator.SetPersistedContent(Content);
			IsContentChanged = _contentPersistenceCoordinator.HasChanges(Content);
			LastModified = DateTime.Now;
		}

		private void SetContent(string content)
		{
			DocumentLine cachedLine = Document.GetLineByOffset(CaretOffset);

			Document.UndoStack.StartUndoGroup();

			SelectAll();
			SelectedText = content;

			Document.UndoStack.EndUndoGroup();

			if (cachedLine.EndOffset <= Document.TextLength)
				ResetSelectionAt(cachedLine);
			else
				ResetSelection();

			TryRunContentChangedWorker();
		}

		#endregion Content

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

		#region Auto bracket closing

		private void HandleAutoClosing(TextCompositionEventArgs e)
		{
			var options = new TextAutoClosingOptions(
				AutoCloseParentheses,
				AutoCloseBraces,
				AutoCloseBrackets,
				AutoCloseDoubleQuotes,
				AutoCloseSingleQuotes,
				ParenthesesClosingString,
				BracesClosingString,
				BracketsClosingString,
				QuotesClosingString,
				"'");

			if (!_autoClosingService.TryGetAction(Document, CaretOffset, e.Text, options, out TextAutoClosingAction action))
				return;

			ApplyAutoClosingAction(e, action);
		}

		private void ApplyAutoClosingAction(TextCompositionEventArgs e, TextAutoClosingAction action)
		{
			switch (action.Kind)
			{
				case TextAutoClosingActionKind.InsertClosingElement:
					SelectedText += action.Element;
					CaretOffset -= action.Element.Length;
					SelectionStart = CaretOffset;
					SelectionLength = 0;
					break;

				case TextAutoClosingActionKind.SkipExistingClosingElement:
					CaretOffset++;
					e.Handled = true;
					OnAutoClosingElementSkipped(action.Element);
					break;
			}
		}

		protected virtual void OnAutoClosingElementSkipped(string element)
		{ }

		#endregion Auto bracket closing

		#region Multiline commenting

		// TODO: Refactor

		public void CommentOutLines()
		{
			ApplyLineCommentTransformation(TextLineCommentAction.Comment);
		}

		public void UncommentLines()
		{
			ApplyLineCommentTransformation(TextLineCommentAction.Uncomment);
		}

		public void ToggleCommentLines()
		{
			ApplyLineCommentTransformation(TextLineCommentAction.Toggle);
		}

		private void ApplyLineCommentTransformation(TextLineCommentAction action)
		{
			if (!_commentService.TryCreateEdit(Document, SelectionStart, SelectionLength, CommentPrefix, action, out TextLineCommentEdit edit))
				return;

			Select(edit.ReplaceOffset, edit.ReplaceLength);
			SelectedText = edit.ReplacementText;
			Select(edit.SelectionStart, edit.SelectionLength);
		}

		#endregion Multiline commenting

		#region Bookmarks

		// TODO: Refactor

		public void ToggleBookmark()
			=> _bookmarkCoordinator.ToggleBookmark(CaretOffset);

		public void GoToNextBookmark()
		{
			DocumentLine? nextBookmark = _bookmarkCoordinator.GetNextBookmarkLine(CaretOffset);

			if (nextBookmark is null)
				return;

			CaretOffset = nextBookmark.EndOffset;
			ScrollToLine(nextBookmark.LineNumber);
		}

		public void GoToPrevBookmark()
		{
			DocumentLine? previousBookmark = _bookmarkCoordinator.GetPreviousBookmarkLine(CaretOffset);

			if (previousBookmark is null)
				return;

			CaretOffset = previousBookmark.EndOffset;
			ScrollToLine(previousBookmark.LineNumber);
		}

		#endregion Bookmarks

		#region Zoom

		public int Zoom
		{
			get => _statusCoordinator.Zoom;
			set
			{
				FontSize = DefaultFontSize * value / 100;
				_statusCoordinator.Zoom = value;
			}
		}

		#endregion Zoom

		#region CompletionWindow

		public void InitializeCompletionWindow(int width = 300, int height = 300)
			=> _completionWindowCoordinator.Initialize(width, height);

		public void ShowCompletionWindow()
			=> _completionWindowCoordinator.Show();

		internal CompletionWindow? ActiveCompletionWindow => _completionWindowCoordinator.ActiveWindow;

		protected bool IsCompletionWindowOpen => _completionWindowCoordinator.IsWindowOpen;

		internal void CloseSharedCompletionWindow()
			=> _completionWindowCoordinator.Close();

		protected void CloseCompletionWindowCore()
			=> _completionWindowCoordinator.Close();

		protected bool TryOpenCompletionWindow(IEnumerable<ICompletionData> items,
			int? startOffset = null,
			int? endOffset = null,
			int width = 300,
			int height = 300)
			=> _completionWindowCoordinator.TryOpen(items, startOffset, endOffset, width, height);

		#endregion CompletionWindow

		#region Other public methods

		public void ClearAllBookmarks(Func<bool> confirmClearBookmarks)
		{
			ArgumentNullException.ThrowIfNull(confirmClearBookmarks);

			if (!confirmClearBookmarks())
				return;

			_bookmarkCoordinator.Clear();
		}

		public void ConvertSpacesToTabs()
			=> Content = WhiteSpaceConverter.ConvertSpacesToTabs(Content, 4);

		public void ConvertTabsToSpaces()
			=> Content = WhiteSpaceConverter.ConvertTabsToSpaces(Content, 4);

		public void SelectLine(int lineNumber) => SelectLine(Document.GetLineByNumber(lineNumber));

		public void SelectLine(DocumentLine line) => _viewService.SelectLine(line);

		public void ReplaceLine(int lineNumber, string replacement, bool deselectAfterwards = false)
			=> ReplaceLine(Document.GetLineByNumber(lineNumber), replacement, deselectAfterwards);

		public void ReplaceLine(DocumentLine line, string replacement, bool deselectAfterwards = false)
			=> _viewService.ReplaceLine(line, replacement, deselectAfterwards);

		public void ReplaceContent(string newContent)
			=> _viewService.ReplaceContent(newContent);

		public void ResetSelection() => _viewService.ResetSelection();

		public void ResetSelectionAt(int lineNumber) => ResetSelectionAt(Document.GetLineByNumber(lineNumber));

		public void ResetSelectionAt(DocumentLine line) => _viewService.ResetSelectionAt(line);

		public int GetOffsetFromPoint(Point point)
			=> _viewService.GetOffsetFromPoint(point);

		public string? GetWordFromOffset(int offset)
			=> _viewService.GetWordFromOffset(offset);

		public void ShowToolTip(string content)
			=> ShowToolTip(content,
				DefaultToolTipBorder,
				DefaultToolTipBackground,
				ToolTipForeground);

		public void ShowMarkdownToolTip(string content)
			=> ShowMarkdownToolTip(content,
				DefaultToolTipBorder,
				DefaultToolTipBackground,
				ToolTipForeground);

		public void ShowToolTip(string content, SolidColorBrush border, SolidColorBrush background, SolidColorBrush foreground)
			=> ShowToolTip(TextEditorToolTipHelper.CreatePlainToolTipContent(content, foreground), border, background);

		public void ShowMarkdownToolTip(string content, SolidColorBrush border, SolidColorBrush background, SolidColorBrush foreground)
			=> ShowToolTip(TextEditorToolTipHelper.CreateMarkdownToolTipContent(content, foreground, background), border, background);

		public void ShowToolTip(object content, SolidColorBrush border, SolidColorBrush background)
			=> _toolTipPresenter.Show(content, border, background);

		internal IReadOnlyList<DocumentLine> GetBookmarkedLines()
			=> _bookmarkCoordinator.GetBookmarkedLines();

		public virtual void UpdateSettings(TombLib.Scripting.UI.Bases.ConfigurationBase configuration)
		{
			if (configuration is not TextEditorConfigBase config)
				return;

			FontSize = config.FontSize;
			DefaultFontSize = config.FontSize;
			FontFamily = new FontFamily(config.FontFamily);

			Document.UndoStack.SizeLimit = config.UndoStackSize;

			IntellisenseEnabled = config.IntellisenseEnabled;
			AutocompleteEnabled = config.IntellisenseEnabled && config.AutocompleteEnabled;
			LiveErrorUnderlining = config.IntellisenseEnabled && config.LiveErrorUnderlining;
			SignatureHelpPopupsEnabled = config.IntellisenseEnabled && config.SignatureHelpPopupsEnabled;

			AutoCloseParentheses = config.AutoCloseParentheses;
			AutoCloseBraces = config.AutoCloseBraces;
			AutoCloseBrackets = config.AutoCloseBrackets;
			AutoCloseDoubleQuotes = config.AutoCloseDoubleQuotes;
			AutoCloseSingleQuotes = config.AutoCloseSingleQuotes;

			WordWrap = config.WordWrapping;

			Options.HighlightCurrentLine = config.HighlightCurrentLine;

			ShowLineNumbers = config.ShowLineNumbers;

			Options.ShowSpaces = config.ShowVisualSpaces;
			Options.ShowTabs = config.ShowVisualTabs;
		}

		public virtual void TidyCode(bool trimOnly = false)
			=> FormattingService.FormatDocument(this, DocumentFormatter, trimOnly);

		protected void GoToDefinition(ITextDefinitionProvider definitionProvider, string objectName, object? identifyingObject = null)
			=> _definitionNavigationService.GoToObject(this, definitionProvider, objectName, identifyingObject);

		protected bool TryGoToDefinition(ITextDefinitionProvider definitionProvider, ITextHoverProvider hoverProvider, int offset)
			=> _definitionNavigationService.TryGoToDefinition(this, definitionProvider, hoverProvider, offset);

		#endregion Other public methods

		#region IEditorControl methods

		void IEditorControl.Undo() => Undo();

		void IEditorControl.Redo() => Redo();

		public virtual void GoToObject(string objectName, object? identifyingObject = null)
		{ } // Bruh

		public void Dispose()
		{
			_statusCoordinator.Dispose();
			_contentPersistenceCoordinator.Dispose();
		}

		#endregion IEditorControl methods
	}
}
