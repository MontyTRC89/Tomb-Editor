using System.Text.Json;
using System.Text.Json.Serialization;

namespace TombLib.Scripting.Lua.LanguageServer;

/// <summary>
/// Represents the typed result of the LSP initialize request.
/// </summary>
public sealed record LuaInitializeResponse
{
	[JsonPropertyName("capabilities")]
	public LuaServerCapabilities? Capabilities { get; init; }
}

/// <summary>
/// Represents the subset of LuaLS capabilities consumed by the host Lua IntelliSense provider.
/// </summary>
public sealed record LuaServerCapabilities
{
	[JsonPropertyName("textDocumentSync")]
	public LuaTextDocumentSyncCapability? TextDocumentSync { get; init; }

	[JsonPropertyName("completionProvider")]
	public LuaCompletionProviderCapability? CompletionProvider { get; init; }

	[JsonPropertyName("referencesProvider")]
	public LuaSupportedCapability? ReferencesProvider { get; init; }

	[JsonPropertyName("renameProvider")]
	public LuaSupportedCapability? RenameProvider { get; init; }

	[JsonPropertyName("documentFormattingProvider")]
	public LuaSupportedCapability? DocumentFormattingProvider { get; init; }

	[JsonPropertyName("semanticTokensProvider")]
	public LuaSemanticTokensProviderCapability? SemanticTokensProvider { get; init; }
}

public sealed record LuaCompletionProviderCapability
{
	[JsonPropertyName("resolveProvider")]
	public bool? ResolveProvider { get; init; }
}

public sealed record LuaSemanticTokensProviderCapability
{
	[JsonPropertyName("full")]
	public LuaSemanticTokensFullCapability? Full { get; init; }

	[JsonPropertyName("legend")]
	public LuaSemanticTokensLegendCapability? Legend { get; init; }
}

public sealed record LuaSemanticTokensLegendCapability
{
	[JsonPropertyName("tokenTypes")]
	public string[]? TokenTypes { get; init; }

	[JsonPropertyName("tokenModifiers")]
	public string[]? TokenModifiers { get; init; }
}

[JsonConverter(typeof(LuaSupportedCapabilityJsonConverter))]
public readonly record struct LuaSupportedCapability(bool IsSupported);

[JsonConverter(typeof(LuaTextDocumentSyncCapabilityJsonConverter))]
public readonly record struct LuaTextDocumentSyncCapability(LuaTextDocumentSyncKind Kind);

[JsonConverter(typeof(LuaSemanticTokensFullCapabilityJsonConverter))]
public readonly record struct LuaSemanticTokensFullCapability(bool SupportsDelta);

/// <summary>
/// Deserializes LSP capability fields that may be advertised as either booleans or objects.
/// </summary>
public sealed class LuaSupportedCapabilityJsonConverter : JsonConverter<LuaSupportedCapability>
{
	public override LuaSupportedCapability Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		return reader.TokenType switch
		{
			JsonTokenType.True => new LuaSupportedCapability(true),
			JsonTokenType.False => new LuaSupportedCapability(false),
			JsonTokenType.StartObject => ReadObject(ref reader),
			JsonTokenType.Null => default,
			_ => ReadUnsupported(ref reader)
		};
	}

	public override void Write(Utf8JsonWriter writer, LuaSupportedCapability value, JsonSerializerOptions options)
		=> throw new NotSupportedException();

	private static LuaSupportedCapability ReadObject(ref Utf8JsonReader reader)
	{
		using JsonDocument ignored = JsonDocument.ParseValue(ref reader);
		return new LuaSupportedCapability(true);
	}

	private static LuaSupportedCapability ReadUnsupported(ref Utf8JsonReader reader)
	{
		using JsonDocument ignored = JsonDocument.ParseValue(ref reader);
		return new LuaSupportedCapability(false);
	}
}

/// <summary>
/// Deserializes the LSP text-document sync capability from either numeric or object form.
/// </summary>
public sealed class LuaTextDocumentSyncCapabilityJsonConverter : JsonConverter<LuaTextDocumentSyncCapability>
{
	public override LuaTextDocumentSyncCapability Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		switch (reader.TokenType)
		{
			case JsonTokenType.Number:
				return reader.TryGetInt32(out int rawSyncKind)
					? new LuaTextDocumentSyncCapability(ParseTextDocumentSyncKind(rawSyncKind))
					: new LuaTextDocumentSyncCapability(LuaTextDocumentSyncKind.None);

			case JsonTokenType.StartObject:
				return ReadObject(ref reader);

			case JsonTokenType.Null:
				return default;

			default:
				using (JsonDocument ignored = JsonDocument.ParseValue(ref reader))
				{ }

				return new LuaTextDocumentSyncCapability(LuaTextDocumentSyncKind.None);
		}
	}

	public override void Write(Utf8JsonWriter writer, LuaTextDocumentSyncCapability value, JsonSerializerOptions options)
		=> throw new NotSupportedException();

	private static LuaTextDocumentSyncCapability ReadObject(ref Utf8JsonReader reader)
	{
		using JsonDocument document = JsonDocument.ParseValue(ref reader);
		JsonElement root = document.RootElement;

		if (!root.TryGetProperty("change", out JsonElement changeElement)
			|| !changeElement.TryGetInt32(out int rawSyncKind))
		{
			return new LuaTextDocumentSyncCapability(LuaTextDocumentSyncKind.None);
		}

		return new LuaTextDocumentSyncCapability(ParseTextDocumentSyncKind(rawSyncKind));
	}

	private static LuaTextDocumentSyncKind ParseTextDocumentSyncKind(int rawSyncKind) => rawSyncKind switch
	{
		0 => LuaTextDocumentSyncKind.None,
		1 => LuaTextDocumentSyncKind.Full,
		2 => LuaTextDocumentSyncKind.Incremental,
		_ => LuaTextDocumentSyncKind.None
	};
}

/// <summary>
/// Deserializes the semantic-token full capability and whether delta refresh is supported.
/// </summary>
public sealed class LuaSemanticTokensFullCapabilityJsonConverter : JsonConverter<LuaSemanticTokensFullCapability>
{
	public override LuaSemanticTokensFullCapability Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		return reader.TokenType switch
		{
			JsonTokenType.True => new LuaSemanticTokensFullCapability(false),
			JsonTokenType.False => new LuaSemanticTokensFullCapability(false),
			JsonTokenType.StartObject => ReadObject(ref reader),
			JsonTokenType.Null => default,
			_ => ReadUnsupported(ref reader)
		};
	}

	public override void Write(Utf8JsonWriter writer, LuaSemanticTokensFullCapability value, JsonSerializerOptions options)
		=> throw new NotSupportedException();

	private static LuaSemanticTokensFullCapability ReadObject(ref Utf8JsonReader reader)
	{
		using JsonDocument document = JsonDocument.ParseValue(ref reader);
		JsonElement root = document.RootElement;

		bool supportsDelta = root.TryGetProperty("delta", out JsonElement deltaElement)
			&& deltaElement.ValueKind == JsonValueKind.True;

		return new LuaSemanticTokensFullCapability(supportsDelta);
	}

	private static LuaSemanticTokensFullCapability ReadUnsupported(ref Utf8JsonReader reader)
	{
		using JsonDocument ignored = JsonDocument.ParseValue(ref reader);
		return new LuaSemanticTokensFullCapability(false);
	}
}
