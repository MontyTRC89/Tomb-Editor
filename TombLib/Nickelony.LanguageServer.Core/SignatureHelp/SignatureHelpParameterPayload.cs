using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nickelony.LanguageServer.Core;

/// <summary>
/// Represents a single parameter entry within a signature-help response.
/// </summary>
public sealed record SignatureHelpParameterPayload
{
	/// <summary>
	/// Gets the label payload identifying the parameter span or text.
	/// </summary>
	[JsonPropertyName("label")]
	public JsonElement Label { get; init; }

	/// <summary>
	/// Gets the optional documentation payload for the parameter.
	/// </summary>
	[JsonPropertyName("documentation")]
	public JsonElement Documentation { get; init; }
}
