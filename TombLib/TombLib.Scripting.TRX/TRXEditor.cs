#nullable enable

using ICSharpCode.AvalonEdit.Document;
using System;
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

namespace TombLib.Scripting.TRX
{
	public sealed partial class TRXEditor : TextEditorBase
	{
		public override string DefaultFileExtension => ".json5";

		private DocumentLine? _cachedLine;
		private bool _suppressBracketAutospacing;

		private readonly ITextDefinitionProvider _definitionProvider;
		private readonly IGameflowSchemaService _schemaService;
		private readonly ITextCompletionProvider _autocompleteService;
		private readonly ITextHoverProvider _hoverService;
		private readonly TextAnalysisService _textAnalysisService;
		private readonly AutocompleteManager _autocompleteManager;
		private readonly CompletionSessionCoordinator _completionCoordinator;

		public TRXEditor(Version engineVersion, TRXLanguageServices languageServices) : base(engineVersion)
		{
			ArgumentNullException.ThrowIfNull(languageServices);

			_definitionProvider = languageServices.DefinitionProvider;
			_schemaService = languageServices.SchemaService;
			_autocompleteService = languageServices.AutocompleteService;
			_hoverService = languageServices.HoverService;
			_textAnalysisService = new TextAnalysisService();
			_autocompleteManager = new AutocompleteManager(languageServices.LineService);
			_completionCoordinator = new CompletionSessionCoordinator(_autocompleteService, _textAnalysisService, _autocompleteManager);

			InitializeDefinitionNavigation((offset, cancellationToken) => Task.FromResult(TryGoToDefinition(_definitionProvider, _hoverService, offset)));
			InitializeHover(BuildStandardHoverRequestState, RequestHoverAsync);

			var errorDetector = new ErrorDetector(languageServices.LineService);
			InitializeDiagnostics(EngineVersion, errorDetector, errorDetector);

			CommentPrefix = "//";
		}

		#region Event handlers

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

		#endregion Event handlers

		#region Text manipulation helpers

		private char? GetPreviousChar()
			=> CaretOffset > 0 ? Document.GetCharAt(CaretOffset - 1) : null;

		private char? GetNextChar()
			=> CaretOffset < Document.TextLength ? Document.GetCharAt(CaretOffset) : null;

		private void HandleBracketAutospacing()
		{
			if (!_suppressBracketAutospacing || _cachedLine is not null)
				return;

			_cachedLine = Document.GetLineByOffset(CaretOffset);

			TextArea.PerformTextInput("\n");
			CaretOffset = _cachedLine.EndOffset;
			TextArea.PerformTextInput("\t");

			_cachedLine = null;
			_suppressBracketAutospacing = false;
		}

		#endregion Text manipulation helpers

		#region Public methods

		public override void UpdateSettings(TombLib.Scripting.UI.Bases.ConfigurationBase configuration)
		{
			var config = configuration as TRXEditorConfiguration;

			SyntaxHighlighting = new SyntaxHighlighting(config!.ColorScheme, _schemaService);

			Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(config.ColorScheme.Background));
			Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(config.ColorScheme.Foreground));

			BracesClosingString = config.AutoAddCommas ? "}," : "}";
			BracketsClosingString = config.AutoAddCommas ? "]," : "]";

			base.UpdateSettings(configuration);
		}

		public override void GoToObject(string objectName, object? identifyingObject = null)
			=> GoToDefinition(_definitionProvider, objectName, identifyingObject);

		#endregion Public methods
	}
}
