using NLog;
using System.IO;
using System.Text.Json;

namespace TombLib.Scripting.GameFlowScript;

public static class GameFlowDefinitionCatalog
{
	private static readonly Logger Log = LogManager.GetCurrentClassLogger();

	private static readonly Lazy<GameFlowDefinitionSet> _definitions = new(LoadDefinitions);

	public static IReadOnlyList<string> SpecialProperties => _definitions.Value.SpecialProperties;
	public static IReadOnlyList<string> Sections => _definitions.Value.Sections;
	public static IReadOnlyList<string> Constants => _definitions.Value.Constants;
	public static IReadOnlyList<string> Properties => _definitions.Value.Properties;

	private static GameFlowDefinitionSet LoadDefinitions()
	{
		string filePath = GameFlowResourcePaths.GetResourcePath("GameFlowDefinitions.json");

		if (!File.Exists(filePath))
		{
			Log.Warn("GameFlow definitions resource '{Path}' was not found; using an empty catalog.", filePath);
			return new GameFlowDefinitionSet();
		}

		try
		{
			string json = File.ReadAllText(filePath);
			return JsonSerializer.Deserialize<GameFlowDefinitionSet>(json) ?? new GameFlowDefinitionSet();
		}
		catch (Exception exception)
		{
			Log.Warn(exception, "Failed to load GameFlow definitions from '{Path}'; using an empty catalog.", filePath);
			return new GameFlowDefinitionSet();
		}
	}
}
