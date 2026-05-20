namespace TombLib.Scripting.Lua.Objects;

/// <summary>
/// Describes signature-help data for a function call.
/// </summary>
public sealed class LuaSignatureInfo
{
	/// <summary>
	/// Initializes a new instance of the <see cref="LuaSignatureInfo"/> class.
	/// </summary>
	/// <param name="label">The full signature label to display.</param>
	/// <param name="documentation">Optional documentation for the signature.</param>
	/// <param name="parameters">The parameters available in the signature.</param>
	/// <param name="activeParameter">The zero-based index of the active parameter.</param>
	public LuaSignatureInfo(string label, string? documentation, IReadOnlyList<LuaParameterInfo> parameters,
		int activeParameter)
	{
		Label = label;
		Documentation = documentation;
		Parameters = parameters ?? [];
		ActiveParameter = Parameters.Count == 0 ? 0 : Math.Clamp(activeParameter, 0, Parameters.Count - 1);
	}

	/// <summary>
	/// Gets the full label shown in signature help.
	/// </summary>
	public string Label { get; }

	/// <summary>
	/// Gets the optional documentation associated with the signature.
	/// </summary>
	public string? Documentation { get; }

	/// <summary>
	/// Gets the parameter metadata associated with the signature.
	/// </summary>
	public IReadOnlyList<LuaParameterInfo> Parameters { get; }

	/// <summary>
	/// Gets the zero-based index of the active parameter.
	/// </summary>
	public int ActiveParameter { get; }
}

/// <summary>
/// Describes a single signature parameter.
/// </summary>
public sealed class LuaParameterInfo
{
	/// <summary>
	/// Initializes a new instance of the <see cref="LuaParameterInfo"/> class.
	/// </summary>
	/// <param name="label">The parameter label.</param>
	/// <param name="documentation">Optional documentation for the parameter.</param>
	public LuaParameterInfo(string label, string? documentation)
	{
		Label = label;
		Documentation = documentation;
	}

	/// <summary>
	/// Gets the display label for the parameter.
	/// </summary>
	public string Label { get; }

	/// <summary>
	/// Gets the optional documentation associated with the parameter.
	/// </summary>
	public string? Documentation { get; }
}