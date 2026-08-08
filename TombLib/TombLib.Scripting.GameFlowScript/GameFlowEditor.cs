using System.Windows.Input;
using System.Windows.Media;
using TombLib.Scripting.GameFlowScript.Completion;
using TombLib.Scripting.GameFlowScript.Highlighting;
using TombLib.Scripting.UI.Bases;

namespace TombLib.Scripting.GameFlowScript;

/// <summary>
/// The GameFlow script editor.
/// </summary>
public sealed partial class GameFlowEditor : TextEditorBase
{
	private readonly GameFlowLanguageServices _languageServices;
	private readonly GameFlowCompletionSessionCoordinator _completionCoordinator;

	/// <inheritdoc/>
	public override string DefaultFileExtension => ".txt";

	/// <summary>
	/// Initializes a new instance of the <see cref="GameFlowEditor"/> class.
	/// </summary>
	/// <param name="engineVersion">The engine version the editor targets.</param>
	/// <param name="languageServices">The language services used by the editor.</param>
	public GameFlowEditor(Version engineVersion, GameFlowLanguageServices languageServices) : base(engineVersion)
	{
		ArgumentNullException.ThrowIfNull(languageServices);

		_languageServices = languageServices;
		_completionCoordinator = new GameFlowCompletionSessionCoordinator(_languageServices.CompletionProvider, _languageServices.LineService);

		InitializeDefinitionNavigation(TryNavigateDefinition);
		InitializeHover(BuildStandardHoverRequestState, RequestHover);

		CommentPrefix = "//";
	}

	/// <inheritdoc/>
	protected override void OnLanguageTextEntering(TextCompositionEventArgs e)
	{
		TryHandleCtrlSpaceCompletion(
			e,
			() => CompletionController.ApplyDecision(
				_completionCoordinator.GetOpenDecision(Document, CaretOffset, CompletionController.ActiveWindow is not null)));
	}

	/// <inheritdoc/>
	protected override void OnLanguageTextEntered(TextCompositionEventArgs e)
	{
		if (CompletionEnabled)
			CompletionController.ApplyDecision(
				_completionCoordinator.GetOpenDecision(Document, CaretOffset, CompletionController.ActiveWindow is not null));
	}

	/// <inheritdoc/>
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

	/// <inheritdoc/>
	public override void GoToObject(string objectName, object? identifyingObject = null)
		=> GoToDefinition(_languageServices.DefinitionProvider, objectName, identifyingObject);
}
