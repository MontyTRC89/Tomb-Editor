namespace TombLib.Scripting.GameFlowScript;

public sealed class GameFlowDefinitionSet
{
	public IReadOnlyList<string> SpecialProperties { get; init; } = [];
	public IReadOnlyList<string> Sections { get; init; } = [];
	public IReadOnlyList<string> Constants { get; init; } = [];
	public IReadOnlyList<string> Properties { get; init; } = [];
}
