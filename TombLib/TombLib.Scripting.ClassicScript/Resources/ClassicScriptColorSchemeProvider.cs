using TombLib.Scripting.UI.Bases;
using TombLib.Scripting.UI.Providers;

namespace TombLib.Scripting.ClassicScript.Resources;

/// <summary>
/// Provides the Classic Script color schemes through the shared scripting color provider contract.
/// </summary>
public sealed class ClassicScriptColorSchemeProvider : FileSystemColorSchemeProviderBase
{
	/// <summary>
	/// Initializes a new instance of the <see cref="ClassicScriptColorSchemeProvider"/> class.
	/// </summary>
	public ClassicScriptColorSchemeProvider()
		: base(DefaultPaths.ClassicScriptColorConfigsDirectory)
	{
	}

	/// <inheritdoc />
	public override string GetSelectedName(TextEditorConfigBase config)
		=> ((ClassicScriptEditorConfiguration)config).SelectedColorSchemeName;

	/// <inheritdoc />
	public override void SetSelectedName(TextEditorConfigBase config, string name)
		=> ((ClassicScriptEditorConfiguration)config).SelectedColorSchemeName = name;
}
