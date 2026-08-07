using ICSharpCode.AvalonEdit.Rendering;
using Nickelony.LanguageServer.Abstractions.Signatures;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;
using System.Windows.Media;
using TombLib.Scripting.ClassicScript.Cleaning;
using TombLib.Scripting.ClassicScript.Completion;
using TombLib.Scripting.ClassicScript.Highlighting;
using TombLib.Scripting.ClassicScript.Mnemonics;
using TombLib.Scripting.Completion;
using TombLib.Scripting.Signatures;
using TombLib.Scripting.Text;
using TombLib.Scripting.UI.Bases;
using TombLib.Scripting.UI.Cleaning;
using TombLib.Scripting.UI.Completion;
using TombLib.Scripting.UI.Signatures;
using TombLib.Scripting.UI.Text;

namespace TombLib.Scripting.ClassicScript;

public sealed partial class ClassicScriptEditor : TextEditorBase, ISyntaxPreviewSource
{
	private readonly ClassicScriptLanguageServices _languageServices;

	public override string DefaultFileExtension => ".txt";

	// Properties

	public ClassicScriptDocumentFormatter Formatter { get; } = new ClassicScriptDocumentFormatter();

	protected override ITextDocumentFormatter DocumentFormatter => Formatter;

	private bool _showSectionSeparators;
	public bool ShowSectionSeparators
	{
		get => _showSectionSeparators;
		set
		{
			_showSectionSeparators = value;

			if (_showSectionSeparators)
			{
				if (!TextArea.TextView.BackgroundRenderers.Contains(_sectionRenderer))
					TextArea.TextView.BackgroundRenderers.Add(_sectionRenderer);
			}
			else
			{
				if (TextArea.TextView.BackgroundRenderers.Contains(_sectionRenderer))
					TextArea.TextView.BackgroundRenderers.Remove(_sectionRenderer);
			}

			TextArea.TextView.InvalidateLayer(KnownLayer.Caret);
		}
	}

	public bool SuppressCompletion { get; set; }

	// Fields

	private readonly ClassicScriptCompletionSessionCoordinator _completionCoordinator;

	private SectionRenderer _sectionRenderer;

	// Construction

	public ClassicScriptEditor(Version engineVersion, ClassicScriptLanguageServices languageServices) : base(engineVersion)
	{
		ArgumentNullException.ThrowIfNull(languageServices);

		_languageServices = languageServices;
		_completionCoordinator = new ClassicScriptCompletionSessionCoordinator(
			languageServices.LineService,
			languageServices.CommandService,
			new ClassicScriptMnemonicCatalogService());

		InitializeDefinitionNavigation((offset, cancellationToken) => TryNavigateDefinition(offset, cancellationToken));
		InitializeHover(BuildHoverRequestState, RequestHover);
		InitializeDiagnostics(new Version(1, 3, 0, 7), _languageServices.ErrorDetector, _languageServices.ErrorDetector);

		InitializeRenderers();

		CommentPrefix = ";";
	}

	[MemberNotNull(nameof(_sectionRenderer))]
	private void InitializeRenderers()
	{
		_sectionRenderer = new SectionRenderer(this, _languageServices.LineService);

		if (ShowSectionSeparators)
			TextArea.TextView.BackgroundRenderers.Add(_sectionRenderer);
	}

	// Events

	protected override void OnLanguageTextEntering(TextCompositionEventArgs e)
	{
		if (!SuppressCompletion)
			TryHandleCtrlSpaceCompletion(e, QueueCtrlSpaceCompletionDecision);
	}

	protected override void OnLanguageTextEntered(TextCompositionEventArgs e)
	{
		if (AutocompleteEnabled && !SuppressCompletion)
			QueueTextEnteredCompletionDecision(e.Text);
	}

	// Autocomplete

	private void QueueCtrlSpaceCompletionDecision()
		=> QueueCompletionDecision(_completionCoordinator.GetCtrlSpaceDecisionAsync(Text, FilePath, CaretOffset, CompletionController.ActiveWindow is not null));

	private void QueueTextEnteredCompletionDecision(string inputText)
		=> QueueCompletionDecision(_completionCoordinator.GetTextEnteredDecisionAsync(Text, FilePath, CaretOffset, inputText, CompletionController.ActiveWindow is not null));

	private void QueueCompletionDecision(Task<TextCompletionSessionDecision> decisionTask)
	{
		string requestText = Text;
		int requestCaretOffset = CaretOffset;
		int requestToken = CompletionController.BeginRequest();
		_ = ApplyCompletionDecisionAsync(decisionTask, requestText, requestCaretOffset, requestToken);
	}

	private async Task ApplyCompletionDecisionAsync(
		Task<TextCompletionSessionDecision> decisionTask,
		string requestText,
		int requestCaretOffset,
		int requestToken)
	{
		TextCompletionSessionDecision decision = await decisionTask;

		if (!CompletionController.IsRequestCurrent(requestToken)
			|| CompletionController.ActiveWindow is not null
			|| decision.Items is null
			|| !decision.StartOffset.HasValue
			|| !decision.EndOffset.HasValue
			|| !string.Equals(Text, requestText, StringComparison.Ordinal)
			|| CaretOffset != requestCaretOffset)
		{
			return;
		}

		CompletionController.ApplyDecision(decision, item => new CompletionData(item, ClassicScriptCompletionIconProvider.GetImage));
	}

	// Navigation

	private Task<bool> TryNavigateDefinition(int offset, CancellationToken cancellationToken)
		=> Task.FromResult(TryGoToDefinition(_languageServices.DefinitionProvider, _languageServices.HoverProvider, offset));

	// TODO: Refactor

	public void InputFreeIndex()
	{
		ITextSnapshot source = new TextDocumentSnapshot(Document);
		int nextFreeIndex = _languageServices.IndexService.GetNextFreeIndex(source, CaretOffset);

		if (nextFreeIndex == -1)
			return;

		TextArea.PerformTextInput(nextFreeIndex.ToString());
	}

	public override void UpdateSettings(TombLib.Scripting.UI.Bases.ConfigurationBase configuration)
	{
		if (configuration is not ClassicScriptEditorConfiguration config)
			return;

		SyntaxHighlighting = new SyntaxHighlighting(config.ColorScheme);

		Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(config.ColorScheme.Background));
		Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(config.ColorScheme.Foreground));

		_sectionRenderer.UpdateSectionColor(config.ColorScheme.Sections.HtmlColor);

		ShowSectionSeparators = config.ShowSectionSeparators;

		Formatter.SpaceBeforeEquals = config.SpaceBeforeEquals;
		Formatter.SpaceAfterEquals = config.SpaceAfterEquals;
		Formatter.SpaceBeforeComma = config.SpaceBeforeComma;
		Formatter.SpaceAfterComma = config.SpaceAfterComma;
		Formatter.CollapseMultipleSpaces = config.CollapseMultipleSpaces;

		base.UpdateSettings(configuration);
	}

	public override void GoToObject(string objectName, object? identifyingObject = null)
		=> GoToDefinition(_languageServices.DefinitionProvider, objectName, identifyingObject);

	public TextSignatureHelpInfo? GetSyntaxPreview()
		=> _languageServices.SignatureHelpProvider.GetSignatureHelp(new TextSignatureHelpRequest(Document.Text, CaretOffset));

	public bool TryAddNewPluginEntry(string pluginString)
	{
		ITextSnapshot source = new TextDocumentSnapshot(Document);
		int? optionsSectionLineNumber = _languageServices.CommandService.FindDocumentLineOfSection(source, "Options");

		if (optionsSectionLineNumber is null)
			return false;

		if (_languageServices.CommandService.IsPluginDefined(source, pluginString))
			return false;

		ITextLine optionsLine = source.GetLineByNumber(optionsSectionLineNumber.Value);
		int nextFreePluginIndex = _languageServices.IndexService.GetNextFreeIndex(source, optionsLine.Offset, "Plugin");
		int? lastSectionLineNumber = _languageServices.CommandService.GetLastLineOfCurrentSection(source, optionsLine.Offset);

		if (lastSectionLineNumber is null)
			return false;

		ITextLine lastSectionLine = source.GetLineByNumber(lastSectionLineNumber.Value);
		CaretOffset = lastSectionLine.Offset + lastSectionLine.Length;

		TextArea.PerformTextInput($"{Environment.NewLine}Plugin= {nextFreePluginIndex}, {pluginString}, IGNORE");

		return true;
	}
}
