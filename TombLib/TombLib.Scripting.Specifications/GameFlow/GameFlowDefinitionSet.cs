#nullable enable

using System.Collections.Generic;

namespace TombLib.Scripting.Specifications.GameFlow;

public sealed class GameFlowDefinitionSet
{
	public IReadOnlyList<string> SpecialProperties { get; init; } = [];
	public IReadOnlyList<string> Sections { get; init; } = [];
	public IReadOnlyList<string> Constants { get; init; } = [];
	public IReadOnlyList<string> Properties { get; init; } = [];
}