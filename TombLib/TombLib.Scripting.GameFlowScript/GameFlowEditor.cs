#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using TombLib.Scripting.Completion;
using TombLib.Scripting.GameFlowScript.Completion;
using TombLib.Scripting.GameFlowScript.Highlighting;
using TombLib.Scripting.GameFlowScript.Hover;
using TombLib.Scripting.GameFlowScript.Navigation;
using TombLib.Scripting.Hover;
using TombLib.Scripting.Navigation;
using TombLib.Scripting.UI.Bases;
using TombLib.Scripting.UI.Completion;
using TombLib.Scripting.UI.Hover;
using TombLib.Scripting.UI.Navigation;

namespace TombLib.Scripting.GameFlowScript
{
	public sealed partial class GameFlowEditor : TextEditorBase
	{
		private readonly GameFlowLanguageServices _languageServices;
		private readonly TextCompletionController _completionController;
		private readonly GameFlowCompletionSessionCoordinator _completionCoordinator;
		private readonly TextDefinitionTriggerController _definitionTriggerController;
		private readonly TextHoverController _hoverController;

		public override string DefaultFileExtension => ".txt";

		public GameFlowEditor(Version engineVersion)
			: this(engineVersion, GameFlowLanguageServices.Default)
		{
		}

		public GameFlowEditor(Version engineVersion, GameFlowLanguageServices languageServices) : base(engineVersion)
		{
			ArgumentNullException.ThrowIfNull(languageServices);

			_languageServices = languageServices;
			_completionController = new TextCompletionController(this);
			_completionCoordinator = new GameFlowCompletionSessionCoordinator(_languageServices.AutocompleteService);
			_definitionTriggerController = new TextDefinitionTriggerController(
				this,
				GetOffsetFromPoint,
				(offset, cancellationToken) => Task.FromResult(TryGoToDefinition(_languageServices.DefinitionProvider, _languageServices.HoverProvider, offset)));
			_hoverController = CreateHoverController();
			BindEventMethods();

			CommentPrefix = "//";
		}

		private void BindEventMethods()
		{
			TextArea.TextEntering += TextArea_TextEntering;
			TextArea.TextEntered += TextEditor_TextEntered;
			AddHandler(PreviewKeyDownEvent, new KeyEventHandler(TextEditor_KeyDown), true);
			AddHandler(PreviewMouseLeftButtonDownEvent, new MouseButtonEventHandler(TextEditor_PreviewMouseLeftButtonDown), true);
			MouseHover += TextEditor_MouseHover;
		}

		private void TextArea_TextEntering(object sender, TextCompositionEventArgs e)
		{
			TryHandleCtrlSpaceCompletion(
				e,
				() => _completionController.ApplyDecision(
					_completionCoordinator.GetCtrlSpaceDecision(Document, CaretOffset, _completionController.ActiveWindow is not null)));
		}

		private void TextEditor_TextEntered(object sender, TextCompositionEventArgs e)
		{
			if (AutocompleteEnabled)
				_completionController.ApplyDecision(
					_completionCoordinator.GetTextEnteredDecision(Document, CaretOffset, _completionController.ActiveWindow is not null));
		}

		private async void TextEditor_KeyDown(object? sender, KeyEventArgs e)
			=> await _definitionTriggerController.TryHandleKeyDownAsync(e, CaretOffset).ConfigureAwait(true);

		private async void TextEditor_PreviewMouseLeftButtonDown(object? sender, MouseButtonEventArgs e)
			=> await _definitionTriggerController.TryHandlePointerNavigationAsync(e).ConfigureAwait(true);

		private async void TextEditor_MouseHover(object? sender, MouseEventArgs e)
			=> await _hoverController.HandleMouseHoverAsync(e).ConfigureAwait(true);

		public override void TidyCode(bool trimOnly = false)
			=> base.TidyCode(trimOnly);

		public override void UpdateSettings(TombLib.Scripting.UI.Bases.ConfigurationBase configuration)
		{
			if (configuration is not GameFlowEditorConfiguration config)
				return;

			SyntaxHighlighting = new SyntaxHighlighting(config.ColorScheme);

			Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(config.ColorScheme.Background));
			Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(config.ColorScheme.Foreground));

			base.UpdateSettings(configuration);
		}

		public override void GoToObject(string objectName, object? identifyingObject = null)
			=> GoToDefinition(_languageServices.DefinitionProvider, objectName, identifyingObject);
	}
}
