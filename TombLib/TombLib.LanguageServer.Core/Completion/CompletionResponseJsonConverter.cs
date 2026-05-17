using NLog;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TombLib.LanguageServer.Core;

/// <summary>
/// Deserializes completion responses from either LSP array or completion-list form.
/// </summary>
public sealed class CompletionResponseJsonConverter : JsonConverter<CompletionResponse>
{
	private static readonly Logger Log = LogManager.GetCurrentClassLogger();

	/// <summary>
	/// Reads a completion response from either array or completion-list wire form.
	/// </summary>
	/// <param name="reader">The JSON reader positioned at the response payload.</param>
	/// <param name="typeToConvert">The target type being deserialized.</param>
	/// <param name="options">The serializer options used for nested deserialization.</param>
	/// <returns>The parsed completion response.</returns>
	public override CompletionResponse? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType == JsonTokenType.Null)
			return new CompletionResponse(null);

		using JsonDocument document = JsonDocument.ParseValue(ref reader);
		JsonElement root = document.RootElement;
		IReadOnlyList<CompletionItemPayload>? items = null;
		bool isIncomplete = false;

		if (root.ValueKind == JsonValueKind.Array)
		{
			items = root.Deserialize<CompletionItemPayload[]>(options);
		}
		else if (root.ValueKind == JsonValueKind.Object)
		{
			bool hasSupportedItemsShape = false;

			if (root.TryGetProperty("items", out JsonElement itemsElement))
			{
				if (itemsElement.ValueKind == JsonValueKind.Array)
				{
					items = itemsElement.Deserialize<CompletionItemPayload[]>(options);
					hasSupportedItemsShape = true;
				}
				else if (itemsElement.ValueKind == JsonValueKind.Null)
				{
					items = null;
					hasSupportedItemsShape = true;
				}
				else
				{
					Log.Warn("Ignoring malformed completion-list payload because 'items' had unsupported JSON kind {Kind}.", itemsElement.ValueKind);
				}
			}
			else
			{
				Log.Warn("Ignoring malformed completion-list payload because the 'items' property was missing.");
			}

			if (hasSupportedItemsShape
				&& root.TryGetProperty("isIncomplete", out JsonElement isIncompleteElement)
				&& (isIncompleteElement.ValueKind == JsonValueKind.True || isIncompleteElement.ValueKind == JsonValueKind.False))
			{
				isIncomplete = isIncompleteElement.GetBoolean();
			}
		}

		return new CompletionResponse(items, isIncomplete);
	}

	public override void Write(Utf8JsonWriter writer, CompletionResponse value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();
		writer.WriteBoolean("isIncomplete", value.IsIncomplete);
		writer.WritePropertyName("items");

		if (value.Items is null)
			writer.WriteNullValue();
		else
			JsonSerializer.Serialize(writer, value.Items, options);

		writer.WriteEndObject();
	}
}
