using LuaApiBuilder.Interfaces;

namespace LuaApiBuilder.Objects;

public sealed class ApiFunction : IApiFunction
{
	/// <summary>
	/// The name of the module this function belongs to.
	/// </summary>
	public string Module { get; init; } = string.Empty;

	public string Name { get; init; } = string.Empty;
	public string Summary { get; init; } = string.Empty;
	public string Description { get; init; } = string.Empty;

	/// <summary>
	/// The name or identifier of the caller associated with the current operation.
	/// It indicated that this function is part of an instance of a class.
	/// </summary>
	public string? Caller { get; init; }

	public List<ApiParameter> Parameters { get; init; } = new();
	public List<ApiReturn> Returns { get; init; } = new();
}
