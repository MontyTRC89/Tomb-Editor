using System.Windows.Input;
using System.Windows.Media;
using TombLib.Scripting.GameFlowScript.Completion;
using TombLib.Scripting.GameFlowScript.Highlighting;
using TombLib.Scripting.UI.Bases;

namespace TombLib.Scripting.GameFlowScript;

public sealed partial class GameFlowEditor : TextEditorBase
{
	private readonly GameFlowLanguageServices _languageServices;
	private readonly GameFlowCompletionSessionCoordinator _completionCoordinator;

	public override string DefaultFileExtension => ".txt";

	public GameFlowEditor(Version engineVersion, GameFlowLanguageServices languageServices) : base(engineVersion)
	{
		ArgumentNullException.ThrowIfNull(languageServices);

		_languageServices = languageServices;
		_completionCoordinator = new GameFlowCompletionSessionCoordinator(_languageServices.CompletionProvider, _languageServices.LineService);

		InitializeDefinitionNavigation(TryNavigateDefinition);
		InitializeHover(BuildStandardHoverRequestState, RequestHover);

		CommentPrefix = "//";
	}

	protected override void OnLanguageTextEntering(TextCompositionEventArgs e)
	{
		TryHandleCtrlSpaceCompletion(
			e,
			() => CompletionController.ApplyDecision(
				_completionCoordinator.GetOpenDecision(Document, CaretOffset, CompletionController.ActiveWindow is not null)));
	}

	protected override void OnLanguageTextEntered(TextCompositionEventArgs e)
	{
		if (AutocompleteEnabled)
			CompletionController.ApplyDecision(
				_completionCoordinator.GetOpenDecision(Document, CaretOffset, CompletionController.ActiveWindow is not null));
	}

	public override void UpdateSettings(TombLib.Scripting.UI.Bases.ConfigurationBase configuration)
	{
		if (configuration is not GameFlowEditorConfiguration config)
			return;

		SyntaxHighlighting = new SyntaxHighlighting(config.ColorScheme);

		Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(config.ColorScheme.Background));
		Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(config.ColorScheme.Foreground));

		base.UpdateSettings(configuration);
	}

	private Task<bool> TryNavigateDefinition(int offset, CancellationToken cancellationToken)
		=> Task.FromResult(TryGoToDefinition(_languageServices.DefinitionProvider, _languageServices.HoverProvider, offset));

	public override void GoToObject(string objectName, object? identifyingObject = null)
		=> GoToDefinition(_languageServices.DefinitionProvider, objectName, identifyingObject);
}
