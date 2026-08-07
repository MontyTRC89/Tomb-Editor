using System.IO;
using TombLib.Scripting.GameFlowScript.Highlighting;
using TombLib.Scripting.GameFlowScript.Resources;
using TombLib.Scripting.UI.Bases;
using TombLib.Scripting.UI.Resources;
using TombLib.Utils;

namespace TombLib.Scripting.GameFlowScript;

public sealed class GameFlowEditorConfiguration : ColorSchemeConfigBase<ColorScheme>
{
	public override string DefaultPath { get; }

	// Color scheme

	protected override string GetSchemeFilePath(string schemeName)
		=> Path.Combine(DefaultPaths.GameFlowColorConfigsDirectory, schemeName + ScriptingDefaults.ColorSchemeFileExtension);

	protected override ColorScheme ReadSchemeFile(string schemeFilePath)
		=> JsonUtils.ReadJsonFile<ColorScheme>(schemeFilePath);

	// Construction

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
