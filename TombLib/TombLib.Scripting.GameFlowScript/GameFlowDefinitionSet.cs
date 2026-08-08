namespace TombLib.Scripting.GameFlowScript;

/// <summary>
/// Describes the set of GameFlow definitions.
/// </summary>
public sealed class GameFlowDefinitionSet
{
	/// <summary>
	/// Gets or initializes the special property names.
	/// </summary>
	public IReadOnlyList<string> SpecialProperties { get; init; } = [];

	/// <summary>
	/// Gets or initializes the section names.
	/// </summary>
	public IReadOnlyList<string> Sections { get; init; } = [];

	/// <summary>
	/// Gets or initializes the constant names.
	/// </summary>
	public IReadOnlyList<string> Constants { get; init; } = [];

	/// <summary>
	/// Gets or initializes the property names.
	/// </summary>
	public IReadOnlyList<string> Properties { get; init; } = [];
}
