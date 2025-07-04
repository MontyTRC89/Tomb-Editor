using LuaApiBuilder.Interfaces;

namespace LuaApiBuilder.Objects;

public sealed class ApiClass : INamedObject, ITypedObject, ISummarizedObject, IDescribedObject
{
	public string Name { get; init; } = string.Empty;
	public string Type { get; init; } = string.Empty;
	public string Summary { get; init; } = string.Empty;
	public string Description { get; init; } = string.Empty;

	/// <summary>
	/// The list of methods for this class.
	/// </summary>
	public List<ApiMethod> Methods { get; init; } = new();

	/// <summary>
	/// The list of fields for this class.
	/// </summary>
	public List<ApiField> Fields { get; init; } = new();
}
