using System.Text.Json.Serialization;

namespace Nickelony.LanguageServer.Core;

/// <summary>
/// Represents a nullable protocol range payload.
/// </summary>
/// <param name="Start">The nullable start position.</param>
/// <param name="End">The nullable end position.</param>
public readonly record struct ProtocolRangePayload(
	[property: JsonPropertyName("start")] ProtocolNullablePosition? Start,
	[property: JsonPropertyName("end")] ProtocolNullablePosition? End);
