using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nickelony.LanguageServer.Core;

/// <summary>
/// Represents a client capability unregistration request.
/// </summary>
/// <remarks>
/// The JSON wire contract accepts both the historical <c>unregisterations</c> spelling and the corrected
/// <c>unregistrations</c> spelling for protocol compatibility.
/// </remarks>
/// <param name="Unregistrations">The requested capability removals.</param>
[JsonConverter(typeof(CapabilityUnregistrationParamsJsonConverter))]
public readonly record struct CapabilityUnregistrationParams(
	CapabilityUnregistrationPayload[]? Unregistrations);

/// <summary>
/// Represents a single dynamic capability unregistration entry.
/// </summary>
/// <param name="Id">The server-defined registration identifier.</param>
/// <param name="Method">The capability method being unregistered.</param>
public readonly record struct CapabilityUnregistrationPayload(
	[property: JsonPropertyName("id")] string? Id,
	[property: JsonPropertyName("method")] string? Method);

/// <summary>
/// Reads capability unregistration payloads while tolerating the historical misspelled property name.
/// </summary>
internal sealed class CapabilityUnregistrationParamsJsonConverter : JsonConverter<CapabilityUnregistrationParams>
{
	public override CapabilityUnregistrationParams Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		using JsonDocument document = JsonDocument.ParseValue(ref reader);
		JsonElement root = document.RootElement;

		if (root.ValueKind is not JsonValueKind.Object)
			throw new JsonException("Capability unregistration payload must be a JSON object.");

		CapabilityUnregistrationPayload[]? unregistrations = null;

		if (root.TryGetProperty("unregistrations", out JsonElement correctedProperty))
			unregistrations = correctedProperty.Deserialize<CapabilityUnregistrationPayload[]>(options);
		else if (root.TryGetProperty("unregisterations", out JsonElement legacyProperty))
			unregistrations = legacyProperty.Deserialize<CapabilityUnregistrationPayload[]>(options);

		return new CapabilityUnregistrationParams(unregistrations);
	}

	public override void Write(Utf8JsonWriter writer, CapabilityUnregistrationParams value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();
		// The peer still expects the historical misspelling on write, so keep emitting it even
		// though the reader accepts both forms for compatibility.
		writer.WritePropertyName("unregisterations");
		JsonSerializer.Serialize(writer, value.Unregistrations, options);
		writer.WriteEndObject();
	}
}
