#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace TombLib.Scripting.Specifications.GameFlow;

public static class GameFlowDefinitionsProvider
{
	private static readonly Lazy<GameFlowDefinitionSet> _definitions = new(LoadDefinitions);

	public static IReadOnlyList<string> SpecialProperties => _definitions.Value.SpecialProperties;
	public static IReadOnlyList<string> Sections => _definitions.Value.Sections;
	public static IReadOnlyList<string> Constants => _definitions.Value.Constants;
	public static IReadOnlyList<string> Properties => _definitions.Value.Properties;

	private static GameFlowDefinitionSet LoadDefinitions()
	{
		string filePath = GameFlowResourcePaths.GetResourcePath("GameFlowDefinitions.json");

		if (!File.Exists(filePath))
			return new GameFlowDefinitionSet();

		try
		{
			string json = File.ReadAllText(filePath);
			return JsonSerializer.Deserialize<GameFlowDefinitionSet>(json) ?? new GameFlowDefinitionSet();
		}
		catch (Exception)
		{
			return new GameFlowDefinitionSet();
		}
	}
}