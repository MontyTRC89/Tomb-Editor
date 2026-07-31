using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nickelony.LanguageServer.Core;

/// <summary>
/// Represents the typed top-level hover payload returned by a language server.
/// </summary>
public sealed record HoverResponse
{
	/// <summary>
	/// Gets the hover contents payload returned by the language server.
	/// </summary>
	[JsonPropertyName("contents")]
	public JsonElement Contents { get; init; }
}
