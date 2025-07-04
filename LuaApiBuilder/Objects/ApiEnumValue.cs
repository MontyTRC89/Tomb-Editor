using LuaApiBuilder.Interfaces;

namespace LuaApiBuilder.Objects;

public sealed class ApiEnumValue : INamedObject, ISummarizedObject, IDescribedObject
{
	public string Name { get; init; } = string.Empty;
	public string Summary { get; init; } = string.Empty;
	public string Description { get; init; } = string.Empty;
}
