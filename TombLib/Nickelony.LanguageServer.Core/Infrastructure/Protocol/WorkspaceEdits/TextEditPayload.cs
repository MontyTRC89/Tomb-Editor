using System.Text.Json.Serialization;

namespace Nickelony.LanguageServer.Core;

/// <summary>
/// Represents a single text edit returned by the language server.
/// </summary>
/// <param name="Range">The replaced document range.</param>
/// <param name="NewText">The replacement text.</param>
public readonly record struct TextEditPayload(
	[property: JsonPropertyName("range")] ProtocolRangePayload? Range,
	[property: JsonPropertyName("newText")] string? NewText);
