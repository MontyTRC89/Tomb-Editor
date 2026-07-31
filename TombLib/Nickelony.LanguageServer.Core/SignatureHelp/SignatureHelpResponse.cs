using System.Text.Json.Serialization;

namespace Nickelony.LanguageServer.Core;

/// <summary>
/// Represents the typed top-level signature-help payload returned by a language server.
/// </summary>
public sealed record SignatureHelpResponse
{
	/// <summary>
	/// Gets the index of the active signature.
	/// </summary>
	[JsonPropertyName("activeSignature")]
	public int? ActiveSignature { get; init; }

	/// <summary>
	/// Gets the index of the active parameter within the active signature.
	/// </summary>
	[JsonPropertyName("activeParameter")]
	public int? ActiveParameter { get; init; }

	/// <summary>
	/// Gets the available signature entries.
	/// </summary>
	[JsonPropertyName("signatures")]
	public SignatureHelpSignaturePayload[]? Signatures { get; init; }
}
