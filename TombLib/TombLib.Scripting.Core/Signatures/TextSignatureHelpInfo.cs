namespace TombLib.Scripting.Signatures;

/// <summary>
/// Represents the active signature-help payload for a callable item.
/// </summary>
public sealed class TextSignatureHelpInfo
{
	/// <summary>
	/// Initializes a new instance of the <see cref="TextSignatureHelpInfo"/> class.
	/// </summary>
	/// <param name="label">The full signature label shown to the user.</param>
	/// <param name="activeParameterIndex">The zero-based active parameter index, or -1 when none is active.</param>
	/// <param name="documentation">Optional documentation shown beside or below the signature.</param>
	/// <param name="parameters">The parameters that compose the signature.</param>
	public TextSignatureHelpInfo(
		string label,
		int activeParameterIndex = -1,
		string? documentation = null,
		IReadOnlyList<TextSignatureParameterInfo>? parameters = null)
	{
		Label = label;
		Documentation = documentation;
		Parameters = parameters ?? [];

		ActiveParameterIndex = Parameters.Count == 0
			? activeParameterIndex
			: Math.Clamp(activeParameterIndex, 0, Parameters.Count - 1);
	}

	/// <summary>
	/// Gets the full signature label shown to the user.
	/// </summary>
	public string Label { get; }

	/// <summary>
	/// Gets the zero-based active parameter index, or -1 when no parameter is active.
	/// </summary>
	public int ActiveParameterIndex { get; }

	/// <summary>
	/// Gets optional documentation for the signature.
	/// </summary>
	public string? Documentation { get; }

	/// <summary>
	/// Gets the parameters that compose the signature.
	/// </summary>
	public IReadOnlyList<TextSignatureParameterInfo> Parameters { get; }
}
