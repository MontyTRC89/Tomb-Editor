using LuaApiBuilder.Interfaces;

namespace LuaApiBuilder.Objects;

public sealed class ApiEnum : INamedObject, ISummarizedObject, IDescribedObject
{
	public string Name { get; init; } = string.Empty;
	public string Summary { get; init; } = string.Empty;
	public string Description { get; init; } = string.Empty;

	/// <summary>
	/// The list of values for this enum.
	/// </summary>
	public List<ApiEnumValue> Values { get; init; } = new();
}
