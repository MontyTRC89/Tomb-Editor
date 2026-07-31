using System.Text.Json.Serialization;

namespace Nickelony.LanguageServer.Core;

/// <summary>
/// Represents a single typed reference location returned by a language server.
/// </summary>
public sealed record ReferenceResponse
{
	/// <summary>
	/// Gets the referenced document URI.
	/// </summary>
	[JsonPropertyName("uri")]
	public string? Uri { get; init; }

	/// <summary>
	/// Gets the referenced document range.
	/// </summary>
	[JsonPropertyName("range")]
	public ProtocolRangePayload? Range { get; init; }
}
