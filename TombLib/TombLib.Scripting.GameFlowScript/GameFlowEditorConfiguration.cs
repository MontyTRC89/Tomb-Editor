using System.IO;
using TombLib.Scripting.GameFlowScript.Highlighting;
using TombLib.Scripting.GameFlowScript.Resources;
using TombLib.Scripting.UI.Bases;
using TombLib.Scripting.UI.Resources;
using TombLib.Utils;

namespace TombLib.Scripting.GameFlowScript;

/// <summary>
/// Configuration for the GameFlow editor, including its color scheme.
/// </summary>
public sealed class GameFlowEditorConfiguration : ColorSchemeConfigBase<ColorScheme>
{
	/// <inheritdoc/>
	public override string DefaultPath { get; }

	// Color scheme

	/// <inheritdoc/>
	protected override string GetSchemeFilePath(string schemeName)
		=> Path.Combine(DefaultPaths.GameFlowColorConfigsDirectory, schemeName + ScriptingDefaults.ColorSchemeFileExtension);

	/// <inheritdoc/>
	protected override ColorScheme ReadSchemeFile(string schemeFilePath)
		=> JsonUtils.ReadJsonFile<ColorScheme>(schemeFilePath);

	// Construction

	/// <summary>
	/// Initializes a new instance of the <see cref="GameFlowEditorConfiguration"/> class.
	/// </summary>
	public GameFlowEditorConfiguration()
	{
		DefaultPath = Path.Combine(DefaultPaths.TextEditorConfigsDirectory, ConfigurationDefaults.ConfigurationFileName);

		AutoCloseParentheses = false;
		AutoCloseBraces = false;
		AutoCloseBrackets = false;
		AutoCloseDoubleQuotes = false;
		AutoCloseSingleQuotes = false;

		SelectedColorSchemeName = ScriptingDefaults.SelectedColorSchemeName;
	}
}
