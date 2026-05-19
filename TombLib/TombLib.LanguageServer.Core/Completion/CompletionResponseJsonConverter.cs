using NLog;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace TombLib.LanguageServer.Core;

/// <summary>
/// Deserializes completion responses from either LSP array or completion-list form.
/// </summary>
public sealed class CompletionResponseJsonConverter : JsonConverter<CompletionResponse>
{
	private static readonly Logger Log = LogManager.GetCurrentClassLogger();

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
					items = root.TryGetProperty("itemDefaults", out JsonElement itemDefaultsElement)
						&& itemDefaultsElement.ValueKind == JsonValueKind.Object
							? DeserializeCompletionListItems(itemsElement, itemDefaultsElement, options)
							: itemsElement.Deserialize<CompletionItemPayload[]>(options);

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

	private static IReadOnlyList<CompletionItemPayload> DeserializeCompletionListItems(
		JsonElement itemsElement,
		JsonElement itemDefaultsElement,
		JsonSerializerOptions options)
	{
		var items = new List<CompletionItemPayload>();

		foreach (JsonElement itemElement in itemsElement.EnumerateArray())
			items.Add(DeserializeCompletionListItem(itemElement, itemDefaultsElement, options));

		return items;
	}

	private static CompletionItemPayload DeserializeCompletionListItem(
		JsonElement itemElement,
		JsonElement itemDefaultsElement,
		JsonSerializerOptions options)
	{
		if (itemElement.ValueKind is not JsonValueKind.Object)
			return itemElement.Deserialize<CompletionItemPayload>(options) ?? new CompletionItemPayload();

		JsonNode? itemNode = JsonNode.Parse(itemElement.GetRawText());

		if (itemNode is not JsonObject itemObject)
			return itemElement.Deserialize<CompletionItemPayload>(options) ?? new CompletionItemPayload();

		ApplyCompletionItemDefaults(itemObject, itemDefaultsElement);
		return itemObject.Deserialize<CompletionItemPayload>(options) ?? new CompletionItemPayload();
	}

	private static void ApplyCompletionItemDefaults(JsonObject itemObject, JsonElement itemDefaultsElement)
	{
		ApplyDefaultPropertyIfMissing(itemObject, itemDefaultsElement, "commitCharacters");
		ApplyDefaultPropertyIfMissing(itemObject, itemDefaultsElement, "data");
		ApplyDefaultPropertyIfMissing(itemObject, itemDefaultsElement, "insertTextFormat");
		ApplyDefaultPropertyIfMissing(itemObject, itemDefaultsElement, "insertTextMode");
		ApplyDefaultEditRangeIfMissing(itemObject, itemDefaultsElement);
	}

	private static void ApplyDefaultPropertyIfMissing(JsonObject itemObject, JsonElement itemDefaultsElement, string propertyName)
	{
		if (itemObject.TryGetPropertyValue(propertyName, out JsonNode? existingValue) && existingValue is not null)
			return;

		if (!itemDefaultsElement.TryGetProperty(propertyName, out JsonElement defaultValue))
			return;

		itemObject[propertyName] = JsonNode.Parse(defaultValue.GetRawText());
	}

	private static void ApplyDefaultEditRangeIfMissing(JsonObject itemObject, JsonElement itemDefaultsElement)
	{
		if (itemObject.TryGetPropertyValue("textEdit", out JsonNode? existingTextEdit) && existingTextEdit is not null)
			return;

		if (!itemDefaultsElement.TryGetProperty("editRange", out JsonElement editRangeElement))
			return;

		string? newText = GetDefaultTextEditNewText(itemObject);

		if (string.IsNullOrEmpty(newText))
			return;

		JsonObject? textEditObject = CreateDefaultTextEdit(editRangeElement, newText);

		if (textEditObject is null)
			return;

		itemObject["textEdit"] = textEditObject;
	}

	private static string? GetDefaultTextEditNewText(JsonObject itemObject)
	{
		return TryGetStringPropertyValue(itemObject, "textEditText")
			?? TryGetStringPropertyValue(itemObject, "insertText")
			?? TryGetStringPropertyValue(itemObject, "label");
	}

	private static string? TryGetStringPropertyValue(JsonObject itemObject, string propertyName)
	{
		if (!itemObject.TryGetPropertyValue(propertyName, out JsonNode? propertyValue)
			|| propertyValue is not JsonValue jsonValue
			|| !jsonValue.TryGetValue(out string? value)
			|| string.IsNullOrEmpty(value))
		{
			return null;
		}

		return value;
	}

	private static JsonObject? CreateDefaultTextEdit(JsonElement editRangeElement, string newText)
	{
		var textEditObject = new JsonObject
		{
			["newText"] = newText
		};

		if (LooksLikeProtocolRange(editRangeElement))
		{
			textEditObject["range"] = JsonNode.Parse(editRangeElement.GetRawText());
			return textEditObject;
		}

		if (!TryGetObjectProperty(editRangeElement, "insert", out JsonElement insertRange)
			|| !TryGetObjectProperty(editRangeElement, "replace", out JsonElement replaceRange))
		{
			return null;
		}

		textEditObject["insert"] = JsonNode.Parse(insertRange.GetRawText());
		textEditObject["replace"] = JsonNode.Parse(replaceRange.GetRawText());
		return textEditObject;
	}

	private static bool LooksLikeProtocolRange(JsonElement element)
		=> element.ValueKind == JsonValueKind.Object
			&& element.TryGetProperty("start", out _)
			&& element.TryGetProperty("end", out _);

	private static bool TryGetObjectProperty(JsonElement element, string propertyName, out JsonElement propertyValue)
	{
		if (element.TryGetProperty(propertyName, out propertyValue) && propertyValue.ValueKind == JsonValueKind.Object)
			return true;

		propertyValue = default;
		return false;
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
