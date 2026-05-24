#nullable enable

using System;
using System.IO;

namespace TombLib.Scripting.Specifications.TRX;

public static class TrxResourcePaths
{
	public static string GetResourcePath(params string[] relativeSegments)
	{
		var pathSegments = new string[relativeSegments.Length + 3];
		pathSegments[0] = AppContext.BaseDirectory;
		pathSegments[1] = "Resources";
		pathSegments[2] = "TRX";

		Array.Copy(relativeSegments, 0, pathSegments, 3, relativeSegments.Length);

		return Path.Combine(pathSegments);
	}

	public static string GetGameflowSchemaPath()
		=> GetResourcePath("gameflow.schema.json");
}