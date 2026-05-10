using System.Text.Json;
using System.Text.Json.Serialization;

namespace TombLib.Scripting.Lua.LanguageServer;

/// <summary>
/// Represents the first usable definition target returned by LuaLS.
/// </summary>
[JsonConverter(typeof(LuaDefinitionResponseJsonConverter))]
public readonly record struct LuaDefinitionResponse(string? Uri, int LineNumber, int ColumnNumber);

/// <summary>
/// Deserializes definition responses from LSP location and location-link payloads.
/// </summary>
public sealed class LuaDefinitionResponseJsonConverter : JsonConverter<LuaDefinitionResponse>
{
	private static readonly string[] RangeProperties =
	[
		"targetSelectionRange",
		"targetRange",
		"range"
	];

	public override LuaDefinitionResponse Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType == JsonTokenType.Null)
			return default;

		using JsonDocument document = JsonDocument.ParseValue(ref reader);
		JsonElement root = document.RootElement;
		JsonElement definitionElement = root;

		if (root.ValueKind == JsonValueKind.Array)
		{
			definitionElement = root.EnumerateArray().FirstOrDefault();

			if (definitionElement.ValueKind == JsonValueKind.Undefined)
				return default;
		}

		if (definitionElement.ValueKind != JsonValueKind.Object)
			return default;

		string? uri = definitionElement.TryGetProperty("targetUri", out JsonElement targetUriElement)
			? targetUriElement.GetString()
			: definitionElement.TryGetProperty("uri", out JsonElement uriElement)
				? uriElement.GetString()
				: null;

		if (string.IsNullOrWhiteSpace(uri) || !TryGetStartElement(definitionElement, out JsonElement startElement))
			return default;

		int lineNumber = startElement.TryGetProperty("line", out JsonElement lineElement)
			&& lineElement.TryGetInt32(out int parsedLine) ? parsedLine + 1 : 1;

		int columnNumber = startElement.TryGetProperty("character", out JsonElement characterElement)
			&& characterElement.TryGetInt32(out int parsedCharacter) ? parsedCharacter + 1 : 1;

		return new LuaDefinitionResponse(uri, lineNumber, columnNumber);
	}

	public override void Write(Utf8JsonWriter writer, LuaDefinitionResponse value, JsonSerializerOptions options)
		=> throw new NotSupportedException();

	private static bool TryGetStartElement(JsonElement definitionElement, out JsonElement startElement)
	{
		for (int i = 0; i < RangeProperties.Length; i++)
		{
			string rangeProperty = RangeProperties[i];

			if (definitionElement.TryGetProperty(rangeProperty, out JsonElement rangeElement)
				&& rangeElement.TryGetProperty("start", out startElement)
				&& startElement.ValueKind == JsonValueKind.Object)
			{
				return true;
			}
		}

		startElement = default;
		return false;
	}
}
