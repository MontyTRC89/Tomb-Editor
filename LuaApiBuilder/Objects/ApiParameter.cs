using LuaApiBuilder.Interfaces;

namespace LuaApiBuilder.Objects;

public sealed class ApiParameter : INamedObject, ITypedObject, IDescribedObject
{
	public string Name { get; init; } = string.Empty;
	public string Type { get; init; } = string.Empty;
	public string Description { get; init; } = string.Empty;
}
