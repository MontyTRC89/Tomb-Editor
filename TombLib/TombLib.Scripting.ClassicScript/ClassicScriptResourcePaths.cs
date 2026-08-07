using TombLib.Scripting.Resources;

namespace TombLib.Scripting.ClassicScript;

public static class ClassicScriptResourcePaths
{
	public static string GetResourcePath(params string[] relativeSegments)
		=> ScriptingResourcePaths.GetResourcePath("ClassicScript", relativeSegments);

	public static string GetCommandsPath()
		=> GetResourcePath("Commands.json");

	public static string GetMnemonicConstantsPath()
		=> GetResourcePath("MnemonicConstants.json");
}
