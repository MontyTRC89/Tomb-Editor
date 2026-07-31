using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nickelony.LanguageServer.Core;

/// <summary>
/// Represents a single signature entry within a signature-help response.
/// </summary>
public sealed record SignatureHelpSignaturePayload
{
	/// <summary>
	/// Gets the display label for the signature.
	/// </summary>
	[JsonPropertyName("label")]
	public string? Label { get; init; }

	/// <summary>
	/// Gets the optional documentation payload for the signature.
	/// </summary>
	[JsonPropertyName("documentation")]
	public JsonElement Documentation { get; init; }

	/// <summary>
	/// Gets the index of the active parameter within this signature.
	/// </summary>
	[JsonPropertyName("activeParameter")]
	public int? ActiveParameter { get; init; }

	/// <summary>
	/// Gets the parameter entries defined by the signature.
	/// </summary>
	[JsonPropertyName("parameters")]
	public SignatureHelpParameterPayload[]? Parameters { get; init; }
}
