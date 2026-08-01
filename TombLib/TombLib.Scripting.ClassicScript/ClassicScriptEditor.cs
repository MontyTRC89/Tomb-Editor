using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;
using System.Windows.Media;
using TombLib.Scripting.ClassicScript.Cleaning;
using TombLib.Scripting.ClassicScript.Completion;
using TombLib.Scripting.ClassicScript.Highlighting;
using TombLib.Scripting.ClassicScript.Mnemonics;
using TombLib.Scripting.ClassicScript.Navigation;
using TombLib.Scripting.ClassicScript.Services;
using Nickelony.LanguageServer.Abstractions.Completion;
using TombLib.Scripting.Completion;
using Nickelony.LanguageServer.Abstractions.Signatures;
using TombLib.Scripting.Signatures;
using TombLib.Scripting.Text;
using TombLib.Scripting.UI.Bases;
using TombLib.Scripting.UI.Cleaning;
using TombLib.Scripting.UI.Completion;
using TombLib.Scripting.UI.Diagnostics;
using TombLib.Scripting.UI.Navigation;
using TombLib.Scripting.UI.Signatures;
using TombLib.Scripting.UI.Text;

namespace TombLib.Scripting.ClassicScript
{
	public sealed partial class ClassicScriptEditor : TextEditorBase, ISyntaxPreviewSource
	{
		private readonly ClassicScriptLanguageServices _languageServices;

		public override string DefaultFileExtension => ".txt";

		#region Properties

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

		public bool SuppressAutocomplete { get; set; }

		#endregion Properties

		#region Fields

		private TextDiagnosticsCoordinator _diagnosticsCoordinator;
		private readonly ClassicScriptCompletionSessionCoordinator _completionCoordinator;
		private readonly TextCompletionController _completionController;
		private readonly TextDefinitionTriggerController _definitionTriggerController;
		private readonly ClassicScriptHoverController _hoverController;

		private IBackgroundRenderer _sectionRenderer;

		#endregion Fields

		#region Construction

		public ClassicScriptEditor(Version engineVersion, ClassicScriptLanguageServices languageServices) : base(engineVersion)
		{
			ArgumentNullException.ThrowIfNull(languageServices);

			_languageServices = languageServices;
			_completionCoordinator = new ClassicScriptCompletionSessionCoordinator(
				languageServices.LineService,
				languageServices.CommandService,
				new ClassicScriptMnemonicCatalogService());
			_completionController = new TextCompletionController(this);
			_definitionTriggerController = new TextDefinitionTriggerController(this, GetOffsetFromPoint, TryNavigateDefinitionAsync);
			_hoverController = new ClassicScriptHoverController(this);
			InitializeBackgroundWorkers();
			InitializeRenderers();

			BindEventMethods();

			CommentPrefix = ";";
		}

		[MemberNotNull(nameof(_diagnosticsCoordinator))]
		private void InitializeBackgroundWorkers()
		{
			_diagnosticsCoordinator = new TextDiagnosticsCoordinator(
				this,
				new Version(1, 3, 0, 7),
				_languageServices.ErrorDetector,
				_languageServices.ErrorDetector);
		}

		[MemberNotNull(nameof(_sectionRenderer))]
		private void InitializeRenderers()
		{
			_sectionRenderer = new SectionRenderer(this, _languageServices.LineService);

			if (ShowSectionSeparators)
				TextArea.TextView.BackgroundRenderers.Add(_sectionRenderer);
		}

		private void BindEventMethods()
		{
			TextArea.TextEntering += TextArea_TextEntering;
			TextArea.TextEntered += TextEditor_TextEntered;
			TextChanged += TextEditor_TextChanged;

			AddHandler(PreviewKeyDownEvent, new KeyEventHandler(TextEditor_KeyDown), true);
			AddHandler(PreviewMouseLeftButtonDownEvent, new MouseButtonEventHandler(TextEditor_PreviewMouseLeftButtonDown), true);
			MouseHover += TextEditor_MouseHover;
		}

		#endregion Construction

		#region Events

		private void TextArea_TextEntering(object? sender, TextCompositionEventArgs e)
		{
			if (!SuppressAutocomplete)
				TryHandleCtrlSpaceCompletion(e, QueueCtrlSpaceCompletionDecision);
		}

		private void TextEditor_TextEntered(object? sender, TextCompositionEventArgs e)
		{
			if (AutocompleteEnabled && !SuppressAutocomplete)
				QueueTextEnteredCompletionDecision(e.Text);
		}

		private void TextEditor_TextChanged(object? sender, EventArgs e)
		{
			if (LiveErrorUnderlining)
				_diagnosticsCoordinator.RunOnIdle(Text);
		}

		private async void TextEditor_KeyDown(object? sender, KeyEventArgs e)
			=> await _definitionTriggerController.TryHandleKeyDownAsync(e, CaretOffset).ConfigureAwait(true);

		private async void TextEditor_PreviewMouseLeftButtonDown(object? sender, MouseButtonEventArgs e)
			=> await _definitionTriggerController.TryHandlePointerNavigationAsync(e).ConfigureAwait(true);

		private async void TextEditor_MouseHover(object? sender, MouseEventArgs e)
			=> await _hoverController.HandleMouseHoverAsync(e).ConfigureAwait(true);

		#endregion Events

		#region Autocomplete

		private void QueueCtrlSpaceCompletionDecision()
			=> QueueCompletionDecision(_completionCoordinator.GetCtrlSpaceDecisionAsync(Text, FilePath, CaretOffset, _completionController.ActiveWindow is not null));

		private void QueueTextEnteredCompletionDecision(string inputText)
			=> QueueCompletionDecision(_completionCoordinator.GetTextEnteredDecisionAsync(Text, FilePath, CaretOffset, inputText, _completionController.ActiveWindow is not null));

		private void QueueCompletionDecision(Task<TextCompletionSessionDecision> decisionTask)
		{
			string requestText = Text;
			int requestCaretOffset = CaretOffset;
			int requestToken = _completionController.BeginRequest();
			_ = ApplyCompletionDecisionAsync(decisionTask, requestText, requestCaretOffset, requestToken);
		}

		private async Task ApplyCompletionDecisionAsync(
			Task<TextCompletionSessionDecision> decisionTask,
			string requestText,
			int requestCaretOffset,
			int requestToken)
		{
			TextCompletionSessionDecision decision = await decisionTask;

			if (!_completionController.IsRequestCurrent(requestToken)
				|| _completionController.ActiveWindow is not null
				|| decision.Items is null
				|| !decision.StartOffset.HasValue
				|| !decision.EndOffset.HasValue
				|| !string.Equals(Text, requestText, StringComparison.Ordinal)
				|| CaretOffset != requestCaretOffset)
			{
				return;
			}

			_completionController.ApplyDecision(decision, item => new CompletionData(item, ClassicScriptCompletionIconProvider.GetImage));
		}

		#endregion Autocomplete

		#region Error handling

		public void CheckForErrors()
		{
			_diagnosticsCoordinator.CheckAsync(Text);
		}

		private Task<bool> TryNavigateDefinitionAsync(int offset, CancellationToken cancellationToken)
		{
			if (_hoverController.TryGetRequestedDefinitionArgs(offset, _specialToolTip.IsOpen, out WordDefinitionEventArgs? definitionArgs))
			{
				OnWordDefinitionRequested(definitionArgs);
				return Task.FromResult(true);
			}

			return Task.FromResult(false);
		}

		#endregion Error handling

		#region Other public methods

		#endregion Other public methods

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

			ShowSectionSeparators = config.ShowSectionSeparators;

			Formatter.PreEqualSpace = config.Tidy_PreEqualSpace;
			Formatter.PostEqualSpace = config.Tidy_PostEqualSpace;
			Formatter.PreCommaSpace = config.Tidy_PreCommaSpace;
			Formatter.PostCommaSpace = config.Tidy_PostCommaSpace;
			Formatter.ReduceSpaces = config.Tidy_ReduceSpaces;

			base.UpdateSettings(configuration);
		}

		public delegate void WordDefinitionRequestedEventHandler(object sender, WordDefinitionEventArgs e);

		public event WordDefinitionRequestedEventHandler? WordDefinitionRequested;
		public void OnWordDefinitionRequested(WordDefinitionEventArgs e) => WordDefinitionRequested?.Invoke(this, e);

		[Obsolete("This method shouldn't be used for ClassicScript.\nUse IClassicScriptLineService.GetWordAtOffset() instead.")]
		public new void GetWordFromOffset(int offset)
			=> base.GetWordFromOffset(offset);

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
}
