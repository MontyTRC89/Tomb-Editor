using System.Text.Json;
using System.Text.Json.Serialization;

namespace TombLib.Scripting.Lua.LanguageServer;

/// <summary>
/// Represents a completion response that may arrive either as an item array or as an LSP completion list.
/// </summary>
[JsonConverter(typeof(LuaCompletionResponseJsonConverter))]
public sealed record LuaCompletionResponse(LuaCompletionItemPayload[]? Items);

/// <summary>
/// Represents a single typed completion item returned by LuaLS.
/// </summary>
public sealed record LuaCompletionItemPayload
{
	[JsonPropertyName("label")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? Label { get; init; }

	[JsonPropertyName("kind")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public int? Kind { get; init; }

	[JsonPropertyName("detail")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? Detail { get; init; }

	[JsonPropertyName("documentation")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public JsonElement? Documentation { get; init; }

	[JsonPropertyName("insertText")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? InsertText { get; init; }

	[JsonPropertyName("insertTextFormat")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public int? InsertTextFormat { get; init; }

	[JsonPropertyName("filterText")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? FilterText { get; init; }

	[JsonPropertyName("preselect")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public bool? Preselect { get; init; }

	[JsonPropertyName("textEdit")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public LuaCompletionTextEditPayload? TextEdit { get; init; }
}

/// <summary>
/// Represents the supported completion text-edit shapes returned by LuaLS.
/// </summary>
public sealed record LuaCompletionTextEditPayload
{
	[JsonPropertyName("newText")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? NewText { get; init; }

	[JsonPropertyName("range")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public LuaProtocolRangePayload? Range { get; init; }

	[JsonPropertyName("insert")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public LuaProtocolRangePayload? Insert { get; init; }

	[JsonPropertyName("replace")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public LuaProtocolRangePayload? Replace { get; init; }
}

/// <summary>
/// Deserializes completion responses from either LSP array or completion-list form.
/// </summary>
public sealed class LuaCompletionResponseJsonConverter : JsonConverter<LuaCompletionResponse>
{
	public override LuaCompletionResponse? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType == JsonTokenType.Null)
			return new LuaCompletionResponse(null);

		using JsonDocument document = JsonDocument.ParseValue(ref reader);
		JsonElement root = document.RootElement;
		LuaCompletionItemPayload[]? items = null;

		if (root.ValueKind == JsonValueKind.Array)
		{
			items = JsonSerializer.Deserialize<LuaCompletionItemPayload[]>(root.GetRawText(), options);
		}
		else if (root.ValueKind == JsonValueKind.Object
			&& root.TryGetProperty("items", out JsonElement itemsElement)
			&& itemsElement.ValueKind == JsonValueKind.Array)
		{
			items = JsonSerializer.Deserialize<LuaCompletionItemPayload[]>(itemsElement.GetRawText(), options);
		}

		return new LuaCompletionResponse(items);
	}

	public override void Write(Utf8JsonWriter writer, LuaCompletionResponse value, JsonSerializerOptions options)
		=> throw new NotSupportedException();
}
