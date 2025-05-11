namespace ScriptLib.IniScript.Structs;

/// <summary>
/// Represents a constant value used by a plugin with decimal and hexadecimal representations.
/// </summary>
public readonly record struct PluginConstant
{
	/// <summary>
	/// Initializes a new instance of the <see cref="PluginConstant"/> struct.
	/// </summary>
	/// <param name="flagName">The name of the flag.</param>
	/// <param name="description">The description of the constant.</param>
	/// <param name="decimalValue">The decimal value of the constant.</param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="flagName"/> or <paramref name="description"/> is null.</exception>
	public PluginConstant(string flagName, string description, short decimalValue)
	{
		FlagName = flagName;
		Description = description;
		DecimalValue = decimalValue;
		HexValue = $"${decimalValue:X4}";
	}

	/// <summary>
	/// The name of the flag.
	/// </summary>
	public readonly string FlagName;

	/// <summary>
	/// The description of the constant.
	/// </summary>
	public readonly string Description;

	/// <summary>
	/// The decimal value of the constant.
	/// </summary>
	public readonly short DecimalValue;

	/// <summary>
	/// The hexadecimal representation of the constant, formatted as $XXXX.
	/// </summary>
	public readonly string HexValue;
}
