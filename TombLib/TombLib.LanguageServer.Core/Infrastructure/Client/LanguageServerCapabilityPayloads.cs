using System.Text.Json;
using System.Text.Json.Serialization;

namespace TombLib.LanguageServer.Core;

/// <summary>
/// Represents the typed result of the LSP initialize request.
/// </summary>
public sealed record InitializeResponse
{
	/// <summary>
	/// Gets the server capabilities advertised during initialization.
	/// </summary>
	[JsonPropertyName("capabilities")]
	public ServerCapabilities? Capabilities { get; init; }
}

/// <summary>
/// Represents the subset of server capabilities consumed by the host provider.
/// </summary>
public sealed record ServerCapabilities
{
	/// <summary>
	/// Gets the text-document synchronization capability.
	/// </summary>
	[JsonPropertyName("textDocumentSync")]
	public TextDocumentSyncCapability? TextDocumentSync { get; init; }

	/// <summary>
	/// Gets the completion provider capability.
	/// </summary>
	[JsonPropertyName("completionProvider")]
	public CompletionProviderCapability? CompletionProvider { get; init; }

	/// <summary>
	/// Gets the references provider capability.
	/// </summary>
	[JsonPropertyName("referencesProvider")]
	public SupportedCapability? ReferencesProvider { get; init; }

	/// <summary>
	/// Gets the rename provider capability.
	/// </summary>
	[JsonPropertyName("renameProvider")]
	public SupportedCapability? RenameProvider { get; init; }

	/// <summary>
	/// Gets the document-formatting provider capability.
	/// </summary>
	[JsonPropertyName("documentFormattingProvider")]
	public SupportedCapability? DocumentFormattingProvider { get; init; }

	/// <summary>
	/// Gets the semantic-tokens provider capability.
	/// </summary>
	[JsonPropertyName("semanticTokensProvider")]
	public SemanticTokensProviderCapability? SemanticTokensProvider { get; init; }
}

/// <summary>
/// Represents completion-specific capabilities advertised by the server.
/// </summary>
public sealed record CompletionProviderCapability
{
	/// <summary>
	/// Gets a value indicating whether completion-item resolve is supported.
	/// </summary>
	[JsonPropertyName("resolveProvider")]
	public bool? ResolveProvider { get; init; }
}

/// <summary>
/// Represents semantic-tokens capabilities advertised by the server.
/// </summary>
public sealed record SemanticTokensProviderCapability
{
	/// <summary>
	/// Gets the semantic-tokens full-refresh capability.
	/// </summary>
	[JsonPropertyName("full")]
	public SemanticTokensFullCapability? Full { get; init; }

	/// <summary>
	/// Gets the semantic-tokens legend advertised by the server.
	/// </summary>
	[JsonPropertyName("legend")]
	public SemanticTokensLegendCapability? Legend { get; init; }
}

/// <summary>
/// Represents the semantic-tokens legend advertised by the server.
/// </summary>
public sealed record SemanticTokensLegendCapability
{
	/// <summary>
	/// Gets the token type names advertised by the server.
	/// </summary>
	[JsonPropertyName("tokenTypes")]
	public string[]? TokenTypes { get; init; }

	/// <summary>
	/// Gets the token modifier names advertised by the server.
	/// </summary>
	[JsonPropertyName("tokenModifiers")]
	public string[]? TokenModifiers { get; init; }
}

/// <summary>
/// Represents a capability that may be advertised as either a boolean or an object.
/// </summary>
/// <param name="IsSupported">Whether the capability is supported.</param>
[JsonConverter(typeof(SupportedCapabilityJsonConverter))]
public readonly record struct SupportedCapability(bool IsSupported);

/// <summary>
/// Represents the text-document synchronization capability advertised by the server.
/// </summary>
/// <param name="Kind">The negotiated synchronization kind.</param>
[JsonConverter(typeof(TextDocumentSyncCapabilityJsonConverter))]
public readonly record struct TextDocumentSyncCapability(TextDocumentSyncKind Kind);

/// <summary>
/// Represents the semantic-tokens full capability advertised by the server.
/// </summary>
/// <param name="IsSupported">Whether full semantic-token requests are supported.</param>
/// <param name="SupportsDelta">Whether delta refresh is supported.</param>
[JsonConverter(typeof(SemanticTokensFullCapabilityJsonConverter))]
public readonly record struct SemanticTokensFullCapability(bool IsSupported, bool SupportsDelta);

/// <summary>
/// Deserializes LSP capability fields that may be advertised as either booleans or objects.
/// </summary>
public sealed class SupportedCapabilityJsonConverter : JsonConverter<SupportedCapability>
{
	/// <summary>
	/// Reads a supported-capability value from boolean or object wire form.
	/// </summary>
	/// <param name="reader">The JSON reader positioned at the capability payload.</param>
	/// <param name="typeToConvert">The target type being deserialized.</param>
	/// <param name="options">The serializer options used for nested deserialization.</param>
	/// <returns>The parsed supported-capability value.</returns>
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

	/// <summary>
	/// Treats an object-form capability as supported.
	/// </summary>
	private static SupportedCapability ReadObject(ref Utf8JsonReader reader)
	{
		using JsonDocument ignored = JsonDocument.ParseValue(ref reader);
		return new SupportedCapability(true);
	}

	/// <summary>
	/// Consumes an unsupported capability shape and reports it as unsupported.
	/// </summary>
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

	/// <summary>
	/// Reads a text-document synchronization capability from numeric or object wire form.
	/// </summary>
	/// <param name="reader">The JSON reader positioned at the capability payload.</param>
	/// <param name="typeToConvert">The target type being deserialized.</param>
	/// <param name="options">The serializer options used for nested deserialization.</param>
	/// <returns>The parsed text-document synchronization capability.</returns>
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

	/// <summary>
	/// Reads the object-form text-document synchronization capability.
	/// </summary>
	private static TextDocumentSyncCapability ReadObject(ref Utf8JsonReader reader, JsonSerializerOptions options)
	{
		TextDocumentSyncCapabilityObject payload = JsonSerializer.Deserialize<TextDocumentSyncCapabilityObject>(ref reader, options);

		if (payload.Change.ValueKind != JsonValueKind.Number
			|| !payload.Change.TryGetInt32(out int rawSyncKind))
			return new TextDocumentSyncCapability(TextDocumentSyncKind.None);

		return new TextDocumentSyncCapability(ParseTextDocumentSyncKind(rawSyncKind));
	}

	/// <summary>
	/// Maps the raw protocol sync integer to the local synchronization kind enum.
	/// </summary>
	private static TextDocumentSyncKind ParseTextDocumentSyncKind(int rawSyncKind) => rawSyncKind switch
	{
		0 => TextDocumentSyncKind.None,
		1 => TextDocumentSyncKind.Full,
		2 => TextDocumentSyncKind.Incremental,
		_ => TextDocumentSyncKind.None
	};
}

/// <summary>
/// Deserializes the semantic-token full capability and whether delta refresh is supported.
/// </summary>
public sealed class SemanticTokensFullCapabilityJsonConverter : JsonConverter<SemanticTokensFullCapability>
{
	private readonly record struct SemanticTokensFullCapabilityObject(
		[property: JsonPropertyName("delta")] JsonElement Delta);

	/// <summary>
	/// Reads a semantic-tokens full capability from boolean or object wire form.
	/// </summary>
	/// <param name="reader">The JSON reader positioned at the capability payload.</param>
	/// <param name="typeToConvert">The target type being deserialized.</param>
	/// <param name="options">The serializer options used for nested deserialization.</param>
	/// <returns>The parsed semantic-tokens full capability.</returns>
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

	/// <summary>
	/// Reads the object-form semantic-tokens full capability.
	/// </summary>
	private static SemanticTokensFullCapability ReadObject(ref Utf8JsonReader reader, JsonSerializerOptions options)
	{
		SemanticTokensFullCapabilityObject payload = JsonSerializer.Deserialize<SemanticTokensFullCapabilityObject>(ref reader, options);
		bool supportsDelta = payload.Delta.ValueKind == JsonValueKind.True;

		return new SemanticTokensFullCapability(true, supportsDelta);
	}

	/// <summary>
	/// Consumes an unsupported semantic-tokens capability shape and reports it as unsupported.
	/// </summary>
	private static SemanticTokensFullCapability ReadUnsupported(ref Utf8JsonReader reader)
	{
		using JsonDocument ignored = JsonDocument.ParseValue(ref reader);
		return new SemanticTokensFullCapability(false, false);
	}
}
