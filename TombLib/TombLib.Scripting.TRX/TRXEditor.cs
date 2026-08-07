using ICSharpCode.AvalonEdit.Document;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using TombLib.Scripting.Completion;
using TombLib.Scripting.Hover;
using TombLib.Scripting.Navigation;
using TombLib.Scripting.TRX.Completion;
using TombLib.Scripting.TRX.Diagnostics;
using TombLib.Scripting.TRX.Highlighting;
using TombLib.Scripting.TRX.Services;
using TombLib.Scripting.UI.Bases;
using TombLib.Scripting.UI.Completion;

namespace TombLib.Scripting.TRX;

/// <summary>
/// The TRX (Tomb Raider X) gameflow script editor.
/// </summary>
public sealed partial class TRXEditor : TextEditorBase
{
	/// <inheritdoc />
	public override string DefaultFileExtension => ".json5";

	// These fields are re-entrancy temporaries that span a single keystroke cycle: entering sets
	// _suppressBracketAutospacing, and HandleBracketAutospacing performs nested
	// TextArea.PerformTextInput calls that re-enter OnLanguageTextEntered, so the in-progress
	// marker and the captured line must survive across those nested calls.
	private DocumentLine? _bracketAutospacingLine;
	private bool _suppressBracketAutospacing;

	private readonly ITextDefinitionProvider _definitionProvider;
	private readonly IGameFlowSchemaService _schemaService;
	private readonly ITextCompletionProvider _completionService;
	private readonly ITextHoverProvider _hoverService;
	private readonly TextAnalysisService _textAnalysisService;
	private readonly CompletionManager _completionManager;
	private readonly CompletionSessionCoordinator _completionCoordinator;

	/// <summary>
	/// Initializes a new instance of the <see cref="TRXEditor"/> class.
	/// </summary>
	/// <param name="engineVersion">The engine version the editor targets.</param>
	/// <param name="languageServices">The language services used by the editor.</param>
	public TRXEditor(Version engineVersion, TRXLanguageServices languageServices) : base(engineVersion)
	{
		ArgumentNullException.ThrowIfNull(languageServices);

		_definitionProvider = languageServices.DefinitionProvider;
		_schemaService = languageServices.SchemaService;
		_completionService = languageServices.CompletionService;
		_hoverService = languageServices.HoverService;
		_textAnalysisService = new TextAnalysisService();
		_completionManager = new CompletionManager(languageServices.LineService);
		_completionCoordinator = new CompletionSessionCoordinator(_completionService, _textAnalysisService, _completionManager);

		InitializeDefinitionNavigation(TryNavigateDefinition);
		InitializeHover(BuildStandardHoverRequestState, RequestHover);

		var errorDetector = new ErrorDetector(languageServices.LineService);
		InitializeDiagnostics(EngineVersion, errorDetector, errorDetector);

		CommentPrefix = "//";
	}

	// Event handlers

	protected override void OnLanguageTextEntering(TextCompositionEventArgs e)
	{
		if (TryHandleCtrlSpaceCompletion(
			e,
			() => CompletionController.ApplyDecision(
				_completionCoordinator.GetCtrlSpaceDecision(Document, CaretOffset, CompletionController.ActiveWindow is not null),
				item => new CompletionData(item, TRXCompletionIconProvider.GetImage))))
		{
			return;
		}

		if (e.Text == "\n" && CaretOffset > 0 && CaretOffset < Document.TextLength)
		{
			char? prevChar = GetPreviousChar();
			char? nextChar = GetNextChar();

			if (prevChar.HasValue && nextChar.HasValue)
			{
				char prev = prevChar.Value;
				char next = nextChar.Value;

				if ((prev == '{' && next == '}') || (prev == '[' && next == ']'))
					_suppressBracketAutospacing = true;
			}
		}
	}

	protected override void OnLanguageTextEntered(TextCompositionEventArgs e)
	{
		if (AutocompleteEnabled)
			CompletionController.ApplyDecision(
				_completionCoordinator.GetTextEnteredDecision(Document, CaretOffset, e.Text, CompletionController.ActiveWindow is not null),
				item => new CompletionData(item, TRXCompletionIconProvider.GetImage));

		HandleBracketAutospacing();
	}

	// Text manipulation helpers

	private char? GetPreviousChar()
		=> CaretOffset > 0 ? Document.GetCharAt(CaretOffset - 1) : null;

	private char? GetNextChar()
		=> CaretOffset < Document.TextLength ? Document.GetCharAt(CaretOffset) : null;

	private void HandleBracketAutospacing()
	{
		if (!_suppressBracketAutospacing || _bracketAutospacingLine is not null)
			return;

		// Capture the line before inserting the newline: splitting the line shrinks the captured
		// line's end, so its end offset is only meaningful when read after the insert.
		_bracketAutospacingLine = Document.GetLineByOffset(CaretOffset);

		try
		{
			TextArea.PerformTextInput("\n");
			CaretOffset = _bracketAutospacingLine.EndOffset;
			TextArea.PerformTextInput("\t");
		}
		finally
		{
			_bracketAutospacingLine = null;
			_suppressBracketAutospacing = false;
		}
	}

	// Public methods

	/// <inheritdoc />
	public override void UpdateSettings(TombLib.Scripting.UI.Bases.ConfigurationBase configuration)
	{
		var config = configuration as TRXEditorConfiguration;

		ArgumentNullException.ThrowIfNull(config);
		SyntaxHighlighting = new SyntaxHighlighting(config.ColorScheme, _schemaService);

		Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(config.ColorScheme.Background));
		Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(config.ColorScheme.Foreground));

		BracesClosingString = config.AutoAddCommas ? "}," : "}";
		BracketsClosingString = config.AutoAddCommas ? "]," : "]";

		base.UpdateSettings(configuration);
	}

	private Task<bool> TryNavigateDefinition(int offset, CancellationToken cancellationToken)
		=> Task.FromResult(TryGoToDefinition(_definitionProvider, _hoverService, offset));

	/// <inheritdoc />
	public override void GoToObject(string objectName, object? identifyingObject = null)
		=> GoToDefinition(_definitionProvider, objectName, identifyingObject);

}
