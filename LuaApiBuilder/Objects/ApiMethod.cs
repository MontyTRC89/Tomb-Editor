using LuaApiBuilder.Interfaces;

namespace LuaApiBuilder.Objects;

public sealed class ApiMethod : IApiFunction
{
	public string Name { get; init; } = string.Empty;
	public string Summary { get; init; } = string.Empty;
	public string Description { get; init; } = string.Empty;

	/// <summary>
	/// The list of parameters for this method.
	/// </summary>
	public List<ApiParameter> Parameters { get; init; } = new();

	/// <summary>
	/// The list of return values for this method.
	/// </summary>
	public List<ApiReturn> Returns { get; init; } = new();
}
