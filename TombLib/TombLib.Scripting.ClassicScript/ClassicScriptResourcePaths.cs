#nullable enable

using System.IO;

namespace TombLib.Scripting.ClassicScript;

public static class ClassicScriptResourcePaths
{
	public static string GetResourcePath(params string[] relativeSegments)
	{
		var pathSegments = new string[relativeSegments.Length + 3];
		pathSegments[0] = AppContext.BaseDirectory;
		pathSegments[1] = "Resources";
		pathSegments[2] = "ClassicScript";

		Array.Copy(relativeSegments, 0, pathSegments, 3, relativeSegments.Length);

		return Path.Combine(pathSegments);
	}

	public static string GetCommandsPath()
		=> GetResourcePath("Commands.json");

	public static string GetMnemonicConstantsPath()
		=> GetResourcePath("MnemonicConstants.json");

	public static string GetReferenceTablePath(string fileName)
		=> GetResourcePath(fileName);
}
