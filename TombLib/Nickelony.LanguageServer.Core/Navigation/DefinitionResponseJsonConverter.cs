using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nickelony.LanguageServer.Core;

/// <summary>
/// Deserializes definition responses from LSP location and location-link payloads.
/// </summary>
public sealed class DefinitionResponseJsonConverter : JsonConverter<DefinitionResponse>
{
	/// <summary>
	/// Lists the candidate property names that may carry the target range in LSP definition payloads.
	/// </summary>
	private static readonly string[] RangeProperties =
	[
		"targetSelectionRange",
		"targetRange",
		"range"
	];

	public override DefinitionResponse Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType == JsonTokenType.Null)
			return new DefinitionResponse([]);

		using JsonDocument document = JsonDocument.ParseValue(ref reader);
		JsonElement root = document.RootElement;

		if (root.ValueKind == JsonValueKind.Array)
		{
			var targets = new List<DefinitionTargetResponse>();

			foreach (JsonElement definitionElement in root.EnumerateArray())
			{
				if (TryParseDefinitionTarget(definitionElement, out DefinitionTargetResponse target))
					targets.Add(target);
			}

			return new DefinitionResponse([.. targets]);
		}

		return TryParseDefinitionTarget(root, out DefinitionTargetResponse targetResponse)
			? new DefinitionResponse([targetResponse])
			: new DefinitionResponse([]);
	}

	public override void Write(Utf8JsonWriter writer, DefinitionResponse value, JsonSerializerOptions options)
	{
		writer.WriteStartArray();

		for (int i = 0; i < value.Targets.Count; i++)
			WriteDefinitionTarget(writer, value.Targets[i]);

		writer.WriteEndArray();
	}

	private static void WriteDefinitionTarget(Utf8JsonWriter writer, DefinitionTargetResponse target)
	{
		writer.WriteStartObject();

		if (target.Uri is not null)
			writer.WriteString("uri", target.Uri);
		else
			writer.WriteNull("uri");

		writer.WritePropertyName("range");
		writer.WriteStartObject();
		WriteZeroBasedPosition(writer, "start", target.LineNumber, target.ColumnNumber);
		WriteZeroBasedPosition(writer, "end", target.LineNumber, target.ColumnNumber);
		writer.WriteEndObject();

		writer.WriteEndObject();
	}

	private static void WriteZeroBasedPosition(Utf8JsonWriter writer, string propertyName, int lineNumber, int columnNumber)
	{
		writer.WritePropertyName(propertyName);
		writer.WriteStartObject();
		writer.WriteNumber("line", Math.Max(0, lineNumber - 1));
		writer.WriteNumber("character", Math.Max(0, columnNumber - 1));
		writer.WriteEndObject();
	}

	/// <summary>
	/// Parses one usable definition target from a definition payload element.
	/// </summary>
	/// <param name="definitionElement">The definition payload element.</param>
	/// <param name="target">Receives the parsed definition target.</param>
	/// <returns><see langword="true"/> when a usable target was found.</returns>
	private static bool TryParseDefinitionTarget(JsonElement definitionElement, out DefinitionTargetResponse target)
	{
		target = default;

		if (definitionElement.ValueKind != JsonValueKind.Object)
			return false;

		string? uri = definitionElement.TryGetProperty("targetUri", out JsonElement targetUriElement)
			? targetUriElement.GetString()
			: definitionElement.TryGetProperty("uri", out JsonElement uriElement)
				? uriElement.GetString()
				: null;

		if (string.IsNullOrWhiteSpace(uri)
			|| !Uri.TryCreate(uri, UriKind.Absolute, out _)
			|| !TryGetOneBasedPosition(definitionElement, out int lineNumber, out int columnNumber))
		{
			return false;
		}

		target = new DefinitionTargetResponse(uri, lineNumber, columnNumber);
		return true;
	}

	/// <summary>
	/// Extracts the first usable one-based position from a definition payload.
	/// </summary>
	/// <param name="definitionElement">The definition payload element.</param>
	/// <param name="lineNumber">Receives the one-based line number.</param>
	/// <param name="columnNumber">Receives the one-based column number.</param>
	/// <returns><see langword="true"/> when a usable position was found.</returns>
	private static bool TryGetOneBasedPosition(JsonElement definitionElement, out int lineNumber, out int columnNumber)
	{
		lineNumber = 1;
		columnNumber = 1;

		for (int i = 0; i < RangeProperties.Length; i++)
		{
			string rangeProperty = RangeProperties[i];

			if (definitionElement.TryGetProperty(rangeProperty, out JsonElement rangeElement)
				&& rangeElement.TryGetProperty("start", out JsonElement startElement)
				&& startElement.ValueKind == JsonValueKind.Object
				&& TryGetOneBasedPositionFromStartElement(startElement, out lineNumber, out columnNumber))
			{
				return true;
			}
		}

		return false;
	}

	/// <summary>
	/// Extracts a valid one-based position from an LSP start-position payload.
	/// </summary>
	/// <param name="startElement">The protocol start-position element.</param>
	/// <param name="lineNumber">Receives the one-based line number.</param>
	/// <param name="columnNumber">Receives the one-based column number.</param>
	/// <returns><see langword="true"/> when the position is present and non-negative.</returns>
	private static bool TryGetOneBasedPositionFromStartElement(JsonElement startElement, out int lineNumber, out int columnNumber)
	{
		lineNumber = 1;
		columnNumber = 1;

		if (!startElement.TryGetProperty("line", out JsonElement lineElement)
			|| !lineElement.TryGetInt32(out int parsedLine)
			|| parsedLine < 0)
		{
			return false;
		}

		if (!startElement.TryGetProperty("character", out JsonElement characterElement)
			|| !characterElement.TryGetInt32(out int parsedCharacter)
			|| parsedCharacter < 0)
		{
			return false;
		}

		lineNumber = parsedLine + 1;
		columnNumber = parsedCharacter + 1;
		return true;
	}
}
