#nullable enable

using ICSharpCode.AvalonEdit.Document;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using TombLib.Scripting.Completion;
using TombLib.Scripting.Diagnostics;
using TombLib.Scripting.Hover;
using TombLib.Scripting.Navigation;
using TombLib.Scripting.Specifications.TRX.Services;
using TombLib.Scripting.TRX.Completion;
using TombLib.Scripting.TRX.Diagnostics;
using TombLib.Scripting.TRX.Highlighting;
using TombLib.Scripting.TRX.Parsers;
using TombLib.Scripting.TRX.Hover;
using TombLib.Scripting.UI.Bases;
using TombLib.Scripting.UI.Cleaning;
using TombLib.Scripting.UI.Completion;
using TombLib.Scripting.UI.Diagnostics;
using TombLib.Scripting.UI.Hover;
using TombLib.Scripting.UI.Navigation;

namespace TombLib.Scripting.TRX
{
	public sealed partial class TRXEditor : TextEditorBase
	{
		public override string DefaultFileExtension => ".json5";

		private DocumentLine? _cachedLine;
		private bool _suppressBracketAutospacing;
		private readonly TextDiagnosticsCoordinator _diagnosticsCoordinator;

		private readonly ITextDefinitionProvider _definitionProvider;
		private readonly IGameflowSchemaService _schemaService;
		private readonly ITextCompletionProvider _autocompleteService;
		private readonly ITextHoverProvider _hoverService;
		private readonly TextAnalysisService _textAnalysisService;
		private readonly AutocompleteManager _autocompleteManager;
		private readonly CompletionSessionCoordinator _completionCoordinator;
		private readonly TextCompletionController _completionController;
		private readonly TextDefinitionTriggerController _definitionTriggerController;
		private readonly TextHoverController _hoverController;

		public TRXEditor(Version engineVersion)
			: this(engineVersion, TRXLanguageServices.Default)
		{
		}

		public TRXEditor(Version engineVersion, TRXLanguageServices languageServices) : base(engineVersion)
		{
			ArgumentNullException.ThrowIfNull(languageServices);

			_definitionProvider = languageServices.DefinitionProvider;
			_schemaService = languageServices.SchemaService;
			_autocompleteService = languageServices.AutocompleteService;
			_hoverService = languageServices.HoverService;
			_textAnalysisService = new TextAnalysisService();
			_autocompleteManager = new AutocompleteManager();
			_completionCoordinator = new CompletionSessionCoordinator(_autocompleteService, _textAnalysisService, _autocompleteManager);
			_completionController = new TextCompletionController(this);
			_definitionTriggerController = new TextDefinitionTriggerController(
				this,
				GetOffsetFromPoint,
				(offset, cancellationToken) => Task.FromResult(TryGoToDefinition(_definitionProvider, _hoverService, offset)));
			_hoverController = CreateHoverController();

			var errorDetector = new ErrorDetector();
			_diagnosticsCoordinator = new TextDiagnosticsCoordinator(this, EngineVersion, errorDetector, errorDetector);

			BindEventMethods();
			CommentPrefix = "//";
		}

		private void BindEventMethods()
		{
			TextArea.TextEntering += TextArea_TextEntering;
			TextArea.TextEntered += TextEditor_TextEntered;
			TextChanged += TextEditor_TextChanged;
			AddHandler(PreviewKeyDownEvent, new KeyEventHandler(TextEditor_KeyDown), true);
			AddHandler(PreviewMouseLeftButtonDownEvent, new MouseButtonEventHandler(TextEditor_PreviewMouseLeftButtonDown), true);
			TextArea.TextView.MouseHover += TextView_MouseHover;
		}

		#region Event handlers

		private void TextArea_TextEntering(object sender, TextCompositionEventArgs e)
		{
			if (TryHandleCtrlSpaceCompletion(
				e,
				() => _completionController.ApplyDecision(
					_completionCoordinator.GetCtrlSpaceDecision(Document, CaretOffset, _completionController.ActiveWindow is not null),
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

		private void TextEditor_TextEntered(object sender, TextCompositionEventArgs e)
		{
			if (AutocompleteEnabled)
				_completionController.ApplyDecision(
					_completionCoordinator.GetTextEnteredDecision(Document, CaretOffset, e.Text, _completionController.ActiveWindow is not null),
					item => new CompletionData(item, TRXCompletionIconProvider.GetImage));

			HandleBracketAutospacing();
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

		private async void TextView_MouseHover(object? sender, MouseEventArgs e)
			=> await _hoverController.HandleMouseHoverAsync(e).ConfigureAwait(true);

		#endregion Event handlers

		#region Autocomplete handling

		#endregion Autocomplete handling

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

		public override void TidyCode(bool trimOnly = false)
			=> base.TidyCode(trimOnly);

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
