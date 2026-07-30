namespace TombLib.Scripting.Signatures;

/// <summary>
/// Represents one parameter entry within a signature-help payload.
/// </summary>
public sealed class TextSignatureParameterInfo
{
	/// <summary>
	/// Initializes a new instance of the <see cref="TextSignatureParameterInfo"/> class.
	/// </summary>
	/// <param name="label">The parameter label shown in the signature UI.</param>
	/// <param name="documentation">Optional parameter documentation.</param>
	public TextSignatureParameterInfo(string label, string? documentation = null)
	{
		Label = label;
		Documentation = documentation;
	}

	/// <summary>
	/// Gets the parameter label shown in the signature UI.
	/// </summary>
	public string Label { get; }

	/// <summary>
	/// Gets optional documentation for the parameter.
	/// </summary>
	public string? Documentation { get; }
}
