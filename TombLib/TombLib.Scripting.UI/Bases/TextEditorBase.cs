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

public abstract partial class TextEditorBase : TextEditor, IEditorControl
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

	private TextDefinitionTriggerController? _definitionTriggerController;
	private TextHoverController? _hoverController;
	private TextDiagnosticsCoordinator? _diagnosticsCoordinator;

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

	#endregion Construction

	#region Settings

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

	#endregion Settings

	#region IEditorControl methods

	void IEditorControl.Undo() => Undo();

	void IEditorControl.Redo() => Redo();

	public virtual void GoToObject(string objectName, object? identifyingObject = null)
	{ }

	public void Dispose()
	{
		_statusCoordinator.Dispose();
		_contentPersistenceCoordinator.Dispose();
	}

	#endregion IEditorControl methods
}
