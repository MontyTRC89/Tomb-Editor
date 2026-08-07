using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Rendering;
using Nickelony.LanguageServer.Abstractions.Diagnostics;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using TombLib.Scripting.UI.Cleaning;
using TombLib.Scripting.UI.Completion;
using TombLib.Scripting.UI.Diagnostics;
using TombLib.Scripting.UI.Documents;
using TombLib.Scripting.UI.Editing;
using TombLib.Scripting.UI.Editors;
using TombLib.Scripting.UI.Hover;
using TombLib.Scripting.UI.Navigation;
using TombLib.Scripting.UI.Presentation;
using TombLib.Scripting.UI.Rendering;
using TombLib.Scripting.UI.Resources;

namespace TombLib.Scripting.UI.Bases;

/// <summary>
/// Base class for language-specific script editors built on AvalonEdit.
/// </summary>
public abstract partial class TextEditorBase : TextEditor, IEditorControl
{
	/// <summary>
	/// Maximum width of editor tooltip text.
	/// </summary>
	public const double ToolTipTextMaxWidth = ToolTipDefaults.TextMaxWidth;

	/// <summary>
	/// Font size used for editor tooltip text.
	/// </summary>
	public static readonly double ToolTipTextFontSize = ToolTipDefaults.TextFontSize;

	/// <summary>
	/// Default border brush used for editor tooltips.
	/// </summary>
	public static readonly SolidColorBrush DefaultToolTipBorder = TextEditorColorPalette.ToolTipBorder;

	/// <summary>
	/// Default background brush used for editor tooltips.
	/// </summary>
	public static readonly SolidColorBrush DefaultToolTipBackground = TextEditorColorPalette.ToolTipBackground;

	/// <summary>
	/// Foreground brush used for editor tooltip text.
	/// </summary>
	public static readonly SolidColorBrush ToolTipForeground = TextEditorColorPalette.ToolTipForeground;

	private static readonly TextEditorFormattingService FormattingService = new();

	/// <summary>
	/// Gets the editor type of this control.
	/// </summary>
	public EditorType EditorType => EditorType.Text;

	/// <summary>
	/// Gets the default file extension (including the leading dot) used for documents of this editor.
	/// </summary>
	public abstract string DefaultFileExtension { get; }

	// Properties

	/// <summary>
	/// Gets or sets the file path of the current document.
	/// </summary>
	public string FilePath
	{
		get => Document.FileName;
		set
		{
			Document.FileName = value;
			_contentPersistenceCoordinator.FilePath = value;
		}
	}

	/// <inheritdoc />
	public bool IsSilentSession { get; set; }

	/// <summary>
	/// Gets or sets whether backup files are created for the current document.
	/// </summary>
	public bool CreateBackupFiles
	{
		get => IsSilentSession ? false : _contentPersistenceCoordinator.CreateBackupFiles;
		set => _contentPersistenceCoordinator.CreateBackupFiles = value;
	}

	/// <inheritdoc />
	public string Content
	{
		get => Text;
		set => SetContent(value);
	}

	/// <summary>
	/// Gets or sets whether the content of the current document has unsaved changes.
	/// </summary>
	public bool IsContentChanged { get; set; }

	/// <summary>
	/// Gets or sets the timestamp of the last content modification.
	/// </summary>
	public DateTime LastModified { get; set; }

	/// <summary>
	/// Gets the line number of the caret position.
	/// </summary>
	public int CurrentRow => TextArea.Caret.Position.Line;

	/// <summary>
	/// Gets the column of the caret position.
	/// </summary>
	public int CurrentColumn => TextArea.Caret.Position.Column;

	/// <summary>
	/// Gets the currently selected content.
	/// </summary>
	public object SelectedContent => SelectedText;

	/// <summary>
	/// Gets the formatter used when tidying the document.
	/// </summary>
	protected virtual ITextDocumentFormatter DocumentFormatter => TrimTrailingWhitespaceFormatter.Instance;

	/// <summary>
	/// Gets or sets the minimum allowed zoom percentage.
	/// </summary>
	public int MinZoom { get; set; } = 25;

	/// <summary>
	/// Gets or sets the maximum allowed zoom percentage.
	/// </summary>
	public int MaxZoom { get; set; } = 400;

	/// <summary>
	/// Gets or sets the zoom percentage change per step.
	/// </summary>
	public int ZoomStepSize { get; set; } = 15;

	/// <summary>
	/// Gets or sets the prefix used to comment out lines.
	/// </summary>
	public string CommentPrefix { get; set; } = string.Empty;

	/// <summary>
	/// Gets or sets the delay before the delayed text-changed notification fires.
	/// </summary>
	public TimeSpan TextChangedDelayedInterval
	{
		get => _contentPersistenceCoordinator.DelayedInterval;
		set => _contentPersistenceCoordinator.DelayedInterval = value;
	}

	/// <summary>
	/// Gets or sets the string inserted to close an auto-closed parenthesis.
	/// </summary>
	public string ParenthesesClosingString { get; set; } = ")";

	/// <summary>
	/// Gets or sets the string inserted to close an auto-closed brace.
	/// </summary>
	public string BracesClosingString { get; set; } = "}";

	/// <summary>
	/// Gets or sets the string inserted to close an auto-closed bracket.
	/// </summary>
	public string BracketsClosingString { get; set; } = "]";

	/// <summary>
	/// Gets or sets the string inserted to close an auto-closed quote.
	/// </summary>
	public string QuotesClosingString { get; set; } = "\"";

	/// <summary>
	/// Gets or sets the engine version targeted by this editor.
	/// </summary>
	public Version EngineVersion { get; set; } = new Version(0, 0);

	// Configuration

	/// <summary>
	/// Basically FontSize but zooming doesn't affect its value.
	/// </summary>
	public double DefaultFontSize { get; set; } = TextEditorBaseDefaults.FontSize;

	/// <summary>
	/// Gets or sets whether intellisense features are enabled for this editor.
	/// </summary>
	public bool IntellisenseEnabled { get; set; } = TextEditorBaseDefaults.IntellisenseEnabled;

	/// <summary>
	/// Gets or sets whether autocomplete suggestions are shown while typing.
	/// </summary>
	public bool AutocompleteEnabled { get; set; } = TextEditorBaseDefaults.AutocompleteEnabled;

	/// <summary>
	/// Gets or sets whether errors are underlined as they are detected.
	/// </summary>
	public bool LiveErrorUnderlining { get; set; } = TextEditorBaseDefaults.LiveErrorUnderlining;

	/// <summary>
	/// Gets or sets whether signature help popups are shown.
	/// </summary>
	public bool SignatureHelpPopupsEnabled { get; set; } = TextEditorBaseDefaults.SignatureHelpPopupsEnabled;

	/// <summary>
	/// Gets or sets whether opening parentheses are auto-closed.
	/// </summary>
	public bool AutoCloseParentheses { get; set; } = TextEditorBaseDefaults.AutoCloseParentheses;

	/// <summary>
	/// Gets or sets whether opening braces are auto-closed.
	/// </summary>
	public bool AutoCloseBraces { get; set; } = TextEditorBaseDefaults.AutoCloseBraces;

	/// <summary>
	/// Gets or sets whether opening brackets are auto-closed.
	/// </summary>
	public bool AutoCloseBrackets { get; set; } = TextEditorBaseDefaults.AutoCloseBrackets;

	/// <summary>
	/// Gets or sets whether double quotes are auto-closed.
	/// </summary>
	public bool AutoCloseDoubleQuotes { get; set; } = TextEditorBaseDefaults.AutoCloseDoubleQuotes;

	/// <summary>
	/// Gets or sets whether single quotes are auto-closed.
	/// </summary>
	public bool AutoCloseSingleQuotes { get; set; } = TextEditorBaseDefaults.AutoCloseSingleQuotes;

	/// <summary>
	/// Gets or sets whether both double and single quotes are auto-closed.
	/// </summary>
	public bool AutoCloseQuotes
	{
		get => AutoCloseDoubleQuotes && AutoCloseSingleQuotes;
		set
		{
			AutoCloseDoubleQuotes = value;
			AutoCloseSingleQuotes = value;
		}
	}

	// Fields

	/// <summary>
	/// The popup used to show special (non-text) tooltips over the editor.
	/// </summary>
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

	private TextDefinitionTriggerController? _definitionTriggerController;
	private TextHoverController? _hoverController;
	private TextDiagnosticsCoordinator? _diagnosticsCoordinator;

	internal IReadOnlyList<TextEditorDiagnostic> Diagnostics => _diagnosticToolTipService.Diagnostics;

	// Construction

	/// <summary>
	/// Initializes a new instance of the <see cref="TextEditorBase"/> class for the given engine version.
	/// </summary>
	/// <param name="engineVersion">The engine version the editor targets.</param>
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

		CompletionController = new TextCompletionController(this);

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

		TextArea.SelectionCornerRadius = 0;
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
		TextArea.TextEntered += TextEditor_TextEntered;
		TextChanged += TextEditor_TextChanged;

		AddHandler(PreviewKeyDownEvent, new KeyEventHandler(TextEditor_KeyDown), true);
		AddHandler(PreviewMouseLeftButtonDownEvent, new MouseButtonEventHandler(TextEditor_PreviewMouseLeftButtonDown), true);

		MouseHover += TextEditor_MouseHover;
		MouseHoverStopped += TextEditor_MouseHoverStopped;

		PreviewMouseWheel += TextEditor_PreviewMouseWheel;
		MouseRightButtonDown += TextEditor_MouseRightButtonDown;
	}

	// Settings

	/// <summary>
	/// Applies the given configuration to this editor.
	/// </summary>
	/// <param name="configuration">The configuration to apply.</param>
	public virtual void UpdateSettings(ConfigurationBase configuration)
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

	// IEditorControl methods

	void IEditorControl.Undo() => Undo();

	void IEditorControl.Redo() => Redo();

	/// <summary>
	/// Navigates to the definition of <paramref name="objectName"/> when the language has a
	/// synchronous name-based lookup path (ClassicScript, GameFlowScript, TRX override this).
	/// The default is an intentional no-op: Lua resolves definitions through the LSP
	/// (<c>NavigateToDefinitionAtCaretAsync</c>) and <see cref="PlainTextEditor"/> has no
	/// definitions, so this base implementation deliberately does nothing rather than throw.
	/// </summary>
	public virtual void GoToObject(string objectName, object? identifyingObject = null)
	{ }

	/// <summary>
	/// Releases the resources used by this editor.
	/// </summary>
	public void Dispose()
	{
		_statusCoordinator.Dispose();
		_contentPersistenceCoordinator.Dispose();
	}
}
