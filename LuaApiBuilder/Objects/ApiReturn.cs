using LuaApiBuilder.Interfaces;

namespace LuaApiBuilder.Objects;

public sealed class ApiReturn : ITypedObject, IDescribedObject
{
	public string Type { get; init; } = string.Empty;
	public string Description { get; init; } = string.Empty;
}
