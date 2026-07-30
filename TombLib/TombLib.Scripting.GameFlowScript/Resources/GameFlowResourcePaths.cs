#nullable enable

using System.IO;

namespace TombLib.Scripting.GameFlowScript;

internal static class GameFlowResourcePaths
{
	public static string GetResourcePath(params string[] relativeSegments)
	{
		var pathSegments = new string[relativeSegments.Length + 3];
		pathSegments[0] = AppContext.BaseDirectory;
		pathSegments[1] = "Resources";
		pathSegments[2] = "GameFlow";

		Array.Copy(relativeSegments, 0, pathSegments, 3, relativeSegments.Length);

		return Path.Combine(pathSegments);
	}
}
