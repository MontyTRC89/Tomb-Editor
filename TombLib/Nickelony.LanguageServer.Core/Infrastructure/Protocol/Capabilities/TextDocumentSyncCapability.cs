using System.Text.Json.Serialization;

namespace Nickelony.LanguageServer.Core;

/// <summary>
/// Represents the text-document synchronization capability advertised by the server.
/// </summary>
/// <param name="Kind">The negotiated synchronization kind.</param>
[JsonConverter(typeof(TextDocumentSyncCapabilityJsonConverter))]
public readonly record struct TextDocumentSyncCapability(TextDocumentSyncKind Kind);
