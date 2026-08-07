using TombLib.Scripting.UI.Providers;

namespace TombLib.Scripting.ClassicScript.Resources;

/// <summary>
/// Provides the Classic Script color schemes through the shared scripting color provider contract.
/// </summary>
public sealed class ClassicScriptColorSchemeProvider : FileSystemColorSchemeProvider<ClassicScriptEditorConfiguration>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="ClassicScriptColorSchemeProvider"/> class.
	/// </summary>
	public ClassicScriptColorSchemeProvider()
		: base(DefaultPaths.ClassicScriptColorConfigsDirectory)
	{
	}
}
