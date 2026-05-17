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
