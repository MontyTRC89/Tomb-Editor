using System.Text.Json;
using System.Text.Json.Serialization;

namespace TombLib.LanguageServer.Core;

/// <summary>
/// Deserializes LSP capability fields that may be advertised as either booleans or objects.
/// </summary>
public sealed class SupportedCapabilityJsonConverter : JsonConverter<SupportedCapability>
{
	public override SupportedCapability Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		return reader.TokenType switch
		{
			JsonTokenType.True => new SupportedCapability(true),
			JsonTokenType.False => new SupportedCapability(false),
			JsonTokenType.StartObject => ReadObject(ref reader),
			JsonTokenType.Null => default,
			_ => ReadUnsupported(ref reader)
		};
	}

	public override void Write(Utf8JsonWriter writer, SupportedCapability value, JsonSerializerOptions options)
		=> throw new NotSupportedException();

	private static SupportedCapability ReadObject(ref Utf8JsonReader reader)
	{
		using JsonDocument ignored = JsonDocument.ParseValue(ref reader);
		return new SupportedCapability(true);
	}

	private static SupportedCapability ReadUnsupported(ref Utf8JsonReader reader)
	{
		using JsonDocument ignored = JsonDocument.ParseValue(ref reader);
		return new SupportedCapability(false);
	}
}

/// <summary>
/// Deserializes the LSP text-document sync capability from either numeric or object form.
/// </summary>
public sealed class TextDocumentSyncCapabilityJsonConverter : JsonConverter<TextDocumentSyncCapability>
{
	private readonly record struct TextDocumentSyncCapabilityObject(
		[property: JsonPropertyName("change")] JsonElement Change);

	public override TextDocumentSyncCapability Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		switch (reader.TokenType)
		{
			case JsonTokenType.Number:
				return reader.TryGetInt32(out int rawSyncKind)
					? new TextDocumentSyncCapability(ParseTextDocumentSyncKind(rawSyncKind))
					: new TextDocumentSyncCapability(TextDocumentSyncKind.None);

			case JsonTokenType.StartObject:
				return ReadObject(ref reader, options);

			case JsonTokenType.Null:
				return default;

			default:
				using (JsonDocument ignored = JsonDocument.ParseValue(ref reader))
				{ }

				return new TextDocumentSyncCapability(TextDocumentSyncKind.None);
		}
	}

	public override void Write(Utf8JsonWriter writer, TextDocumentSyncCapability value, JsonSerializerOptions options)
		=> throw new NotSupportedException();

	private static TextDocumentSyncCapability ReadObject(ref Utf8JsonReader reader, JsonSerializerOptions options)
	{
		TextDocumentSyncCapabilityObject payload = JsonSerializer.Deserialize<TextDocumentSyncCapabilityObject>(ref reader, options);

		if (payload.Change.ValueKind != JsonValueKind.Number
			|| !payload.Change.TryGetInt32(out int rawSyncKind))
		{
			return new TextDocumentSyncCapability(TextDocumentSyncKind.None);
		}

		return new TextDocumentSyncCapability(ParseTextDocumentSyncKind(rawSyncKind));
	}

	private static TextDocumentSyncKind ParseTextDocumentSyncKind(int rawSyncKind) => rawSyncKind switch
	{
		0 => TextDocumentSyncKind.None,
		1 => TextDocumentSyncKind.Full,
		2 => TextDocumentSyncKind.Incremental,
		_ => TextDocumentSyncKind.None
	};
}

/// <summary>
/// Deserializes the semantic token full capability and whether delta refresh is supported.
/// </summary>
public sealed class SemanticTokensFullCapabilityJsonConverter : JsonConverter<SemanticTokensFullCapability>
{
	private readonly record struct SemanticTokensFullCapabilityObject(
		[property: JsonPropertyName("delta")] JsonElement Delta);

	public override SemanticTokensFullCapability Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		return reader.TokenType switch
		{
			JsonTokenType.True => new SemanticTokensFullCapability(true, false),
			JsonTokenType.False => new SemanticTokensFullCapability(false, false),
			JsonTokenType.StartObject => ReadObject(ref reader, options),
			JsonTokenType.Null => default,
			_ => ReadUnsupported(ref reader)
		};
	}

	public override void Write(Utf8JsonWriter writer, SemanticTokensFullCapability value, JsonSerializerOptions options)
		=> throw new NotSupportedException();

	private static SemanticTokensFullCapability ReadObject(ref Utf8JsonReader reader, JsonSerializerOptions options)
	{
		SemanticTokensFullCapabilityObject payload = JsonSerializer.Deserialize<SemanticTokensFullCapabilityObject>(ref reader, options);
		bool supportsDelta = payload.Delta.ValueKind == JsonValueKind.True;

		return new SemanticTokensFullCapability(true, supportsDelta);
	}

	private static SemanticTokensFullCapability ReadUnsupported(ref Utf8JsonReader reader)
	{
		using JsonDocument ignored = JsonDocument.ParseValue(ref reader);
		return new SemanticTokensFullCapability(false, false);
	}
}
