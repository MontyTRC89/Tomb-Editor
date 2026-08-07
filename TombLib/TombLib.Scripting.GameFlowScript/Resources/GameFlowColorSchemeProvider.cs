using TombLib.Scripting.UI.Bases;
using TombLib.Scripting.UI.Providers;

namespace TombLib.Scripting.GameFlowScript.Resources;

/// <summary>
/// Provides the GameFlow color schemes through the shared scripting color provider contract.
/// </summary>
public sealed class GameFlowColorSchemeProvider : FileSystemColorSchemeProviderBase
{
	/// <summary>
	/// Initializes a new instance of the <see cref="GameFlowColorSchemeProvider"/> class.
	/// </summary>
	public GameFlowColorSchemeProvider()
		: base(DefaultPaths.GameFlowColorConfigsDirectory)
	{
	}

	/// <inheritdoc />
	public override string GetSelectedName(TextEditorConfigBase config)
		=> ((GameFlowEditorConfiguration)config).SelectedColorSchemeName;

	/// <inheritdoc />
	public override void SetSelectedName(TextEditorConfigBase config, string name)
		=> ((GameFlowEditorConfiguration)config).SelectedColorSchemeName = name;
}
